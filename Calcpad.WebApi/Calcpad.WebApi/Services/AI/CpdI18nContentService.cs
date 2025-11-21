using System.Web;
using Calcpad.Document;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Models;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Utils.Web.Service;
using HtmlAgilityPack;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace Calcpad.WebApi.Services.AI
{
    public class CpdI18nContentService(AppSettings<TranslationConfig> config, MongoDBContext db)
        : IScopedService
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
            var extractedElements = ExtractI18nElements(doc);
            foreach (var element in extractedElements)
            {
                i18nKeys.Add(element.Item2);
            }

            return [.. i18nKeys];
        }

        public List<Tuple<HtmlNode, string>> ExtractI18nElements(HtmlDocument doc)
        {
            var results = new List<Tuple<HtmlNode, string>>();
            // Extract text from specific tags
            var textTags = new[]
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

                    foreach (var child in node.ChildNodes)
                    {
                        if (child.NodeType != HtmlNodeType.Text)
                            continue;

                        // trim and add to list
                        var text = HttpUtility.HtmlDecode(child.InnerText).Trim(trimeChars);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            results.Add(new Tuple<HtmlNode, string>(child, text));
                        }
                    }
                }
            }

            // remove special fields
            var ignoreFields = new string[] { " ", "#loop", "#continue", "#read" };
            var temps = results.Where(x => !ignoreFields.Contains(x.Item2));

            // ignore pure ASCII if configured
            if (config.Value.IgnorePureASCCI)
            {
                temps = temps.Where(x => x.Item2.Any(c => c > 127));
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

            // find langs
            // langs can be from source file or cpd file
            var (cpdFile, sourceFile) = await GetSelfAndSourceCpdFile(uniqueId);

            var lanQuery = db.AsQueryable<CalcpadLangModel>().Where(x => x.Lang == lang);
            if (sourceFile == null)
            {
                lanQuery = lanQuery.Where(x => x.UniqueId == uniqueId);
            }
            else
            {
                lanQuery = lanQuery.Where(x =>
                    new List<string> { sourceFile.UniqueId, cpdFile.UniqueId }.Contains(x.UniqueId)
                );
            }
            var langs = await lanQuery.ToListAsync();
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

            var i18nElements = ExtractI18nElements(doc);
            foreach (var element in i18nElements)
            {
                var node = element.Item1;
                var text = element.Item2;
                if (langDict.TryGetValue(text, out CalcpadLangModel? langModel))
                {
                    // replace node value
                    if (node.NodeType == HtmlNodeType.Text)
                    {
                        node.ParentNode.ReplaceChild(
                            HtmlTextNode.CreateNode(HttpUtility.HtmlEncode(langModel.Value)),
                            node
                        );
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }
    }
}
