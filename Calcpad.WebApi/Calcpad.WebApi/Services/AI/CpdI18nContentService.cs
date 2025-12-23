using System.Text.RegularExpressions;
using System.Web;
using Calcpad.Document;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Models;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Utils.Web.Service;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.EMMA;
using HtmlAgilityPack;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace Calcpad.WebApi.Services.AI
{
    public partial class CpdI18nContentService(
        AppSettings<TranslationConfig> config,
        MongoDBContext db
    ) : IScopedService
    {
        /// <summary>
        /// Asynchronously extracts all internationalization (i18n) keys from the specified CPD file.
        /// </summary>
        /// <param name="cpdFileFullPath">The full path to the CPD file from which to extract i18n keys. Cannot be null or empty.</param>
        /// <returns>A list of strings containing all i18n keys found in the specified CPD file. Returns an empty list if the
        /// file does not exist or contains no i18n keys.</returns>
        public async Task<List<string>> GetCpdFileI18nKeys(string cpdFileFullPath)
        {
            if (!File.Exists(cpdFileFullPath))
                return [];

            var cpdExecutor = new CpdExecutor(cpdFileFullPath);
            var outputText = await cpdExecutor.RunCalculation([], false);

            if (string.IsNullOrEmpty(outputText))
                return [];

            return ExtractI18nKeysFromInputForm(outputText);
        }

        private List<string> ExtractI18nKeysFromInputForm(string inputFormHtml)
        {
            // extract texts from bellow:
            // 1. HtmlNodeType.Text in p、div、span、option、th、a tags
            // 2. image alt attributes
            // 3. title attributes
            // 4. button value attributes
            // 5. input placeholder attributes

            if (string.IsNullOrEmpty(inputFormHtml))
            {
                return [];
            }

            // Use HtmlAgilityPack to parse HTML
            var doc = new HtmlDocument();
            doc.LoadHtml(inputFormHtml);

            var i18nKeys = new HashSet<string>();
            var extractedElements = ExtractI18nElements(doc, true);
            foreach (var element in extractedElements)
            {
                foreach (var text in element.Item2)
                {
                    i18nKeys.Add(text);
                }
            }

            return [.. i18nKeys];
        }

        /// <summary>
        /// Extracts text elements from the specified HTML document that are relevant for internationalization (i18n)
        /// processing.
        /// </summary>
        /// <remarks>Text is extracted from a predefined set of HTML tags, including standard and SVG text
        /// elements. Elements with certain classes or values are filtered out to avoid non-translatable content. The
        /// extraction respects configuration settings, such as ignoring pure ASCII text. The method is intended to
        /// assist in identifying content that may require localization.</remarks>
        /// <param name="doc">The HTML document to analyze for i18n-relevant text elements.</param>
        /// <param name="isInputForm">Indicates whether the document represents an input form. true will extract label in svg by regex</param>
        /// <returns>A list of tuples, each containing an HTML node and an array of extracted text strings. Only text elements
        /// suitable for i18n are included; elements with ignored values or pure ASCII content may be excluded based on
        /// configuration.</returns>
        public List<Tuple<HtmlNode, string[]>> ExtractI18nElements(
            HtmlDocument doc,
            bool isInputForm
        )
        {
            var results = new List<Tuple<HtmlNode, string[]>>();
            // Extract text from specific tags
            var textTags = new List<string>()
            {
                "p",
                "div",
                "span",
                "option",
                "th",
                "a",
                "strong",
                "h1",
                "h2",
                "h3",
                "h4",
                "h5",
                "h6",
                "label"
            };
            if (!isInputForm)
            {
                // // svg text tags
                textTags.Add("text");
            }

            var trimeChars = new[]
            {
                ' ',
                '\u00A0', // non-breaking space
                '\u3000', // ideographic space
                '\n',
                '\r',
                '\t',
                ',',
                '，',
                ':',
                '：',
                '；'
            };

            IEnumerable<Tuple<HtmlNode, string[]>> FilterTextChildren(HtmlNode node)
            {
                return node
                    .ChildNodes.Select(child =>
                    {
                        if (child.NodeType != HtmlNodeType.Text)
                            return null;

                        // trim and add to list
                        var text = HttpUtility.HtmlDecode(child.InnerText).Trim(trimeChars);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            return new Tuple<HtmlNode, string[]>(child, [text]);
                        }

                        return null;
                    })
                    .Where(x => x != null)!;
            }

            foreach (var tag in textTags)
            {
                var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
                if (nodes == null)
                    continue;

                // get texts from HtmlNodeType.Text
                foreach (var node in nodes)
                {
                    // if span tag, only extract those with class "user-text" or no class
                    if (tag == "span")
                    {
                        var tagClass = node.GetClasses().ToList();
                        if (tagClass.Count > 0 && !tagClass.Contains("user-text"))
                        {
                            continue;
                        }
                    }

                    // if p tag, skip those contains span with value "#read"
                    if (tag == "p")
                    {
                        var spanNode = node.SelectSingleNode(".//span");
                        if (spanNode != null && spanNode.InnerText.Trim() == "#read")
                        {
                            continue;
                        }
                    }

                    results.AddRange(FilterTextChildren(node));
                }
            }

            // get all user-text elements
            var userTextNodes = doc.DocumentNode.SelectNodes(
                "//*[@class and contains(@class, 'user-text')]"
            );
            if (userTextNodes != null)
            {
                foreach (var node in userTextNodes)
                {
                    var children = FilterTextChildren(node);
                    foreach (var child in children)
                    {
                        // if already exists, skip
                        if (results.Any(x => x.Item2 == child.Item2))
                            continue;
                        results.Add(child);
                    }
                }
            }

            // because svg content are not rendered as normal html
            // so we need to extract text from svg text tags by regex
            if (isInputForm)
            {
                var svgNodes = doc.DocumentNode.SelectNodes("//svg");
                if (svgNodes != null)
                {
                    var regex = SvgTextRegex();
                    foreach (var node in svgNodes)
                    {
                        var matches = regex.Matches(node.OuterHtml);
                        if (matches != null)
                        {
                            foreach (Match child in matches)
                            {
                                results.Add(
                                    Tuple.Create<HtmlNode, string[]>(
                                        node,
                                        [
                                            HttpUtility.HtmlDecode(
                                                child.Groups[1].Value.Trim(trimeChars)
                                            )
                                        ]
                                    )
                                );
                            }
                        }
                    }
                }
            }

            // remove special fields
            var ignoreFields = new string[] { " ", "#loop", "#continue", "#read" };
            var temps = results.Where(x => !ignoreFields.Contains(x.Item2[0]));

            // ignore pure ASCII if configured
            if (config.Value.IgnorePureASCCI)
            {
                temps = temps.Where(x => x.Item2.Any(str => str.Any(c => c > 127)));
            }
            return [.. temps];
        }

        /// <summary>
        /// Retrieves the Calcpad file with the specified unique identifier and its associated source Calcpad file, if
        /// available.
        /// </summary>
        /// <remarks>The method returns null for both items in the tuple if no Calcpad file with the
        /// specified unique identifier exists. Only files marked as CPD are considered.</remarks>
        /// <param name="uniqueId">The unique identifier of the Calcpad file to retrieve. Cannot be null or empty.</param>
        /// <returns>A tuple containing the Calcpad file that matches the specified unique identifier and its source Calcpad
        /// file, if one exists. The second item in the tuple is null if the file has no source or the source file
        /// cannot be found.</returns>
        public async Task<Tuple<CalcpadFileModel, CalcpadFileModel?>> GetSelfAndSourceCpdFile(
            string uniqueId
        )
        {
            var cpdFile = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .Where(x => x.IsCpd == true)
                .FirstOrDefaultAsync();
            if (cpdFile == null)
            {
                return Tuple.Create<CalcpadFileModel, CalcpadFileModel?>(cpdFile, null);
            }

            CalcpadFileModel? sourceFile = null;
            if (!cpdFile.SourceId.IsEmpty())
            {
                sourceFile = await db.AsQueryable<CalcpadFileModel>()
                    .Where(x => x.Id == cpdFile.SourceId)
                    .Where(x => x.IsCpd == true)
                    .FirstOrDefaultAsync();
            }

            return Tuple.Create(cpdFile, sourceFile);
        }

        /// <summary>
        /// translate html content to target lang
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="htmlContent"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public async Task<string> TranslateHtmlContentToLang(
            string uniqueId,
            string htmlContent,
            string lang
        )
        {
            if (string.IsNullOrEmpty(htmlContent))
                return htmlContent;

            var cpdFile = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .Where(x => x.IsCpd == true)
                .FirstOrDefaultAsync();

            var cpdUids = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.GroupId == cpdFile.GroupId)
                .Where(x => x.IsCpd == true)
                .Select(x => x.UniqueId)
                .ToListAsync();

            var langs = await db.AsQueryable<CalcpadLangModel>()
                .Where(x => x.Lang == lang)
                .Where(x => cpdUids.Contains(x.UniqueId))
                .Where(x => x.IsDeleted == false)
                .ToListAsync();

            if (langs.Count == 0)
            {
                return htmlContent;
            }

            // convert langs to dictionary
            // if duplicated, use the latest version one
            // then if duplicated, use the one from cpd file
            var langDict = new Dictionary<string, CalcpadLangModel>();
            foreach (var langModel in langs.OrderBy(x => x.Version))
            {
                if (!langDict.TryGetValue(langModel.Key, out CalcpadLangModel? value))
                {
                    value = langModel;
                    langDict[langModel.Key] = value;
                    continue;
                }

                // if duplicated, use the latest version one
                if (langModel.Version > value.Version)
                {
                    langDict[langModel.Key] = langModel;
                    continue;
                }

                if (langModel.UniqueId == uniqueId)
                {
                    langDict[langModel.Key] = langModel;
                }
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var i18nElements = ExtractI18nElements(doc, false);
            foreach (var element in i18nElements)
            {
                var node = element.Item1;
                var texts = element.Item2;
                if (langDict.TryGetValue(texts[0], out CalcpadLangModel? langModel))
                {
                    // replace node value
                    if (node.NodeType == HtmlNodeType.Text)
                    {
                        node.ParentNode.ReplaceChild(
                            HtmlTextNode.CreateNode(
                                node.InnerText.Replace(
                                    texts[0],
                                    HttpUtility.HtmlEncode(langModel.Value)
                                )
                            ),
                            node
                        );
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        [GeneratedRegex(
            @">(.*?)</text>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.RightToLeft,
            "zh-CN"
        )]
        private static partial Regex SvgTextRegex();
    }
}
