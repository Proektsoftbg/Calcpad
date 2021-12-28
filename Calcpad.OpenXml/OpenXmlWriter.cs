﻿using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using M = DocumentFormat.OpenXml.Math;

namespace Calcpad.OpenXml
{
    public class OpenXmlWriter
    {
        private readonly Queue<OpenXmlElement> _buffer = new();
        private readonly TableBuilder _tableBuilder = new();
        private string _url = string.Empty;

        public string Convert(string html, Stream stream, string url = "")
        {
            using var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
            {
                _url = url;
                WriteDocument(html, doc);
                return Validate(doc);
            }
        }

        public string Convert(string html, string path)
        {
            using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
            {
                WriteDocument(html, doc);
                return Validate(doc);
            }
        }

        private void WriteDocument(string html, WordprocessingDocument doc)
        {
            // Add a main document part. 
            var mainPart = doc.AddMainDocumentPart();
            //AddSettings(mainPart);
            StyleDefinitionsWriter.CreateStyleDefinitionsPart(mainPart);
            // Create the document structure and add text.
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            var mainNode = htmlDocument.DocumentNode.SelectSingleNode("//body");
            if (mainNode == null)
                mainNode = htmlDocument.DocumentNode;

            var body = (Body)ParseHtmlNode(mainNode, mainPart);
            AddSectionProperties(body);
            mainPart.Document = new Document()
            {
                Body = body
            };

        }

        private static string Validate(WordprocessingDocument doc)
        {
            //Validate document
            var validator = new OpenXmlValidator();
            var errors = validator.Validate(doc);
            var stringBuilder = new StringBuilder();
            var count = 0;
            foreach (var error in errors)
            {
                count++;
                stringBuilder.Append("\n-------------------------------------------");
                stringBuilder.Append("\nError: " + count);
                stringBuilder.Append("\nDescription: " + error.Description);
                stringBuilder.Append("\nErrorType: " + error.ErrorType);
                stringBuilder.Append("\nNode: " + error.Node);
                stringBuilder.Append("\nInnerXml: " + error.Node.InnerXml);
                stringBuilder.Append("\nInnerText: " + error.Node.InnerText);
                stringBuilder.Append("\nPath: " + error.Path.XPath);
                stringBuilder.Append("\nPart: " + error.Part.Uri);
            }
            if (count == 0)
                return string.Empty;

            stringBuilder.Append("\n-------------------------------------------");
            stringBuilder.Append("\nContent XML:\n");
            //stringBuilder.Append(doc.MainDocumentPart.Document.OuterXml);
#if BG
            return $"Грешки при валидиране на документ" + stringBuilder.ToString();
#else
            return $"Validation errors in document" + stringBuilder.ToString();
#endif
        }

        private OpenXmlElement ParseHtmlNode(HtmlNode domNode, MainDocumentPart mainPart)
        {
            var parentElement = Node2Element(domNode, mainPart);
            if (!(parentElement is null || parentElement is M.Paragraph))
            {
                if (parentElement is TableCell)
                    parentElement.AppendChild(AddCellParagraph());

                var childNodes = domNode.ChildNodes;
                var n = childNodes.Count;
                var count = _buffer.Count;
                for (var i = 0; i < n; i++)
                {
                    var childNode = childNodes[i];
                    var childElement = ParseHtmlNode(childNode, mainPart);
                    if (childElement != null)
                    {
                        if (parentElement is Run)
                        {
                            if (childElement is Text || childElement is Break)
                            {
                                var e = parentElement.CloneNode(false);
                                e.AppendChild(childElement);
                                _buffer.Enqueue(e);
                            }
                            else
                                _buffer.Enqueue(childElement);
                        }
                        else
                        {
                            if (_buffer.Count > 0)
                            {
                                if (IsRunParent(childElement))
                                {
                                    while (_buffer.Count > 0)
                                        AppendChild(childElement, _buffer.Dequeue());
                                }
                                else if (IsRunParent(parentElement))
                                {
                                    while (_buffer.Count > 0)
                                        AppendChild(parentElement, _buffer.Dequeue());

                                }
                            }
                            AppendChild(parentElement, childElement);
                        }
                    }
                }

                if (parentElement is Run)
                {
                    var i = 0;
                    foreach (var e in _buffer)
                    {
                        i++;
                        if (i > count && e is Run)
                            CopyRunProperties(parentElement, e);
                    }
                    if (n > 0)
                        return null;
                }
            }
            return parentElement;
            static bool IsRunParent(OpenXmlElement e) => !(e is M.Paragraph || e is Text || e is Break);
        }

        private static void CopyRunProperties(OpenXmlElement source, OpenXmlElement dest)
        {
            var sourceProps = source.GetFirstChild<RunProperties>();
            if (sourceProps is null)
                return;

            var destProps = dest.GetFirstChild<RunProperties>()
                            ?? dest.PrependChild(new RunProperties());
            destProps.RunFonts ??= sourceProps.RunFonts?.Clone() as RunFonts;
            destProps.Bold ??= sourceProps.Bold?.Clone() as Bold;
            destProps.Italic ??= sourceProps.Italic?.Clone() as Italic;
            destProps.Underline ??= sourceProps.Underline?.Clone() as Underline;
            destProps.Strike ??= sourceProps.Strike?.Clone() as Strike;
            destProps.DoubleStrike ??= sourceProps.DoubleStrike?.Clone() as DoubleStrike;
            destProps.Color ??= sourceProps.Color?.Clone() as Color;
            destProps.Caps ??= sourceProps.Caps?.Clone() as Caps;
            destProps.SmallCaps ??= sourceProps.SmallCaps?.Clone() as SmallCaps;
            destProps.Emphasis ??= sourceProps.Emphasis?.Clone() as Emphasis;
            destProps.FontSize ??= sourceProps.FontSize?.Clone() as FontSize;
            destProps.Highlight ??= sourceProps.Highlight?.Clone() as Highlight;
            destProps.Shading ??= sourceProps.Shading?.Clone() as Shading;
            destProps.Border ??= sourceProps.Border?.Clone() as Border;
            destProps.VerticalTextAlignment ??= sourceProps.VerticalTextAlignment?.Clone() as VerticalTextAlignment;
        }

        private void AppendChild(OpenXmlElement parentElement, OpenXmlElement childElement)
        {
            const string refShading = "008800";
            if (parentElement is Table && !(childElement is TableRow || childElement is CustomXmlBlock b && b.Element == "tbody") ||
                parentElement is TableRow && childElement is not TableCell)
                return;

            var cellPara = parentElement is TableCell ?
                parentElement.GetFirstChild<Paragraph>() ?? parentElement.AppendChild(new Paragraph()) : null;

            if (childElement is Break)
            {
                if (parentElement is Run)
                    parentElement.Append(childElement);
                else if (parentElement is Paragraph)
                    parentElement.Append(new Run(childElement.CloneNode(false)));
                else if (parentElement is TableCell)
                    cellPara.Append(new Run(childElement.CloneNode(false)));
                else
                    parentElement.Append(new Paragraph(new Run(childElement.CloneNode(false))));
            }
            else if (childElement is Text)
            {
                if (parentElement is Run)
                    parentElement.AppendChild(childElement);
                else if (parentElement is TableCell tc)
                {
                    var r = new Run(childElement);
                    if (tc.TableCellProperties.Shading != null)
                    {
                        r.RunProperties = new RunProperties(new Bold());
                    }
                    cellPara.AppendChild(r);
                }
                else if (parentElement is Body || parentElement is CustomXmlBlock)
                    parentElement.AppendChild(new Paragraph(new Run(childElement.CloneNode(false))));
                else if (parentElement is Paragraph p && p.ParagraphProperties?.Shading != null)
                {
                    parentElement.AppendChild(new Run(childElement.CloneNode(false))
                    {
                        RunProperties = new RunProperties(new Color() { Val = refShading })
                    });
                }
                else
                    parentElement.AppendChild(new Run(childElement.CloneNode(false)));
            }
            else if (parentElement is TableCell)
            {
                if (childElement is Paragraph)
                    parentElement.AppendChild(childElement);
                else
                    cellPara.AppendChild(childElement);
            }
            else if (childElement is Run || childElement is M.Paragraph)
            {
                if (parentElement is Paragraph p)
                {
                    if (p.ParagraphProperties?.Shading != null && childElement is Run r)
                    {
                        r.RunProperties ??= new RunProperties();
                        r.RunProperties.Color = new Color()
                        {
                            Val = refShading
                        };
                    }
                    parentElement.AppendChild(childElement);
                }
                else if (parentElement is Run)
                    _buffer.Enqueue(childElement);
                else
                    parentElement.AppendChild(new Paragraph(childElement.CloneNode(true)));
            }
            else if (childElement is Paragraph)
            {
                if (!IsBlankLine(childElement))
                    parentElement.AppendChild(childElement);
            }
            else if (childElement is CustomXmlElement)
                AppendGrandChildren(parentElement, childElement);
            else
                parentElement.AppendChild(childElement);
        }

        private void AppendGrandChildren(OpenXmlElement parentElement, OpenXmlElement childElement)
        {
            var hasGrandChildren = !(childElement is CustomXmlElement e && e.Element == "FOLD");
            do
            {
                var grandChild = childElement.FirstChild;
                if (grandChild is null)
                    hasGrandChildren = false;
                else
                {
                    grandChild.Remove();
                    AppendChild(parentElement, grandChild);
                }
            } while (hasGrandChildren);
        }

        private static bool IsBlankLine(OpenXmlElement element)
        {
            if (!element.HasChildren)
                return true;

            if (element.ChildElements.Count == 1 && element.FirstChild is Run r)
            {
                if (!r.HasChildren)
                    return true;

                if (r.ChildElements.Count == 1 && r.FirstChild is Text t)
                    if (string.IsNullOrWhiteSpace(t.Text))
                        return true;
            }
            return false;
        }

        private OpenXmlElement Node2Element(HtmlNode domNode, MainDocumentPart mainPart)
        {
            if (domNode.NodeType == HtmlNodeType.Text)
                return AddText(domNode);

            switch (domNode.Name.ToLowerInvariant())
            {
                case "#document":
                case "body":
                    return new Body();
                case "div":
                    return AddDiv(domNode);
                case "ol":
                case "ul":
                case "thead":
                case "tbody":
                    return new CustomXmlBlock() { Element = domNode.Name };
                case "small":
                    return new Run(new RunProperties(new FontSize() { Val = $"22" }));
                case "img":
                    try
                    {
                        var w = (int)(GetImgSize(domNode, "width"));
                        var h = (int)(GetImgSize(domNode, "height"));
                        var src = domNode.GetAttributeValue("src", string.Empty);
                        var alt = domNode.GetAttributeValue("alt", string.Empty);
                        return new Paragraph(ImageWriter.AddImage(src, _url, alt, w, h, mainPart));
                    }
                    catch (Exception e)
                    {
                        return new Paragraph(new Run(new Text($"Error adding image: {domNode.OuterHtml}! " + e.Message)));
                    }
                case "hr":
                    return AddHr(domNode);
                case "table":
                    _tableBuilder.IsBorderedTable = domNode.HasClass("bordered");
                    return _tableBuilder.AddTable();
                case "tr":
                    return _tableBuilder.AddTableRow();
                case "th":
                case "td":
                    var colSpan = domNode.GetAttributeValue("colspan", 1);
                    var rowSpan = domNode.GetAttributeValue("rowspan", 1);
                    return _tableBuilder.AddTableCell(domNode.Name == "th", colSpan, rowSpan);
                case "li":
                case "p":
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    return AddParagraph(domNode);
                case "br":
                    return new Break();
                case "a":
                    try
                    {
                        return AddHyperlink(domNode, mainPart, _url);
                    }
                    catch (Exception e)
                    {
                        return new Paragraph(new Run(new Text($"Error adding hyperlink: {domNode.OuterHtml}! " + e.Message)));
                    }
                case "span":
                    return AddSpan(domNode);
                default:
                    return AddRun(domNode);
            }
        }

        private static double GetImgSize(HtmlNode node, string direction)
        {
            var size = node.GetAttributeValue(direction, 0);
            if (size != 0)
                return size;

            string s = node.GetAttributeValue("style", "");
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            return HtmlStyleParser.GetParameter(s, direction);
        }

        private static OpenXmlElement AddDiv(HtmlNode domNode)
        {
            if (domNode.HasClass("fold"))
                return new CustomXmlBlock() { Element = "fold" };

            return new CustomXmlBlock() { Element = domNode.Name };
        }

        private static OpenXmlElement AddSpan(HtmlNode domNode)
        {
            if (domNode.HasClass("eq"))
            {
                var s = domNode.GetAttributeValue("data-xml", string.Empty);
                var oMath = new M.OfficeMath
                {
                    InnerXml = s.Replace("<<", "&lt;<")
                                .Replace(">>", ">&gt;")
                                .Replace("&quot;", "\"")
                };
                var oMathPara = new M.Paragraph()
                {
                    ParagraphProperties = new M.ParagraphProperties()
                    {
                        Justification = new M.Justification()
                        {
                            Val = M.JustificationValues.Left
                        }
                    }
                };
                oMathPara.AppendChild(oMath);
                return oMathPara;
            }
            if (domNode.HasClass("err"))
                return new Run(new RunProperties(new Color() { Val = "FF0000" }));

            return new Run();
        }

        private static OpenXmlElement AddHr(HtmlNode domNode)
        {
            const string HrColor = "888888";
            var name = domNode.ParentNode.Name;
            if (name == "p" || name[0] == 'h' && name.Length == 2)
            {
                return new Run(new Break(), new Text(new string('─', 55)), new Break())
                {
                    RunProperties = new RunProperties(new Color() { Val = HrColor })
                };
            }
            return new Paragraph()
            {
                ParagraphProperties = new ParagraphProperties()
                {
                    ParagraphBorders = new ParagraphBorders()
                    {
                        BottomBorder = new BottomBorder()
                        {
                            Size = 2,
                            Val = BorderValues.Single,
                            Color = HrColor
                        }
                    }
                }
            };
        }

        private readonly OnOffValue on = OnOffValue.FromBoolean(true);
        private OpenXmlElement AddRun(HtmlNode domNode)
        {
            var r = new Run();
            var rp = new RunProperties();
            r.RunProperties = rp;
            switch (domNode.Name.ToLowerInvariant())
            {
                case "var":
                case "em":
                case "i":
                    rp.Italic = new Italic()
                    {
                        Val = on
                    };
                    break;
                case "b":
                case "strong":
                    rp.Bold = new Bold()
                    {
                        Val = on
                    };
                    break;
                case "u":
                case "ins":
                    rp.Underline = new Underline()
                    {
                        Val = UnderlineValues.Single
                    };
                    break;
                case "s":
                case "del":
                case "strike":
                    rp.Strike = new Strike()
                    {
                        Val = on
                    };
                    break;
                case "sup":
                    rp.VerticalTextAlignment = new VerticalTextAlignment()
                    {
                        Val = VerticalPositionValues.Superscript
                    };
                    break;
                case "sub":
                    rp.VerticalTextAlignment = new VerticalTextAlignment()
                    {
                        Val = VerticalPositionValues.Subscript
                    };
                    break;
                case "mark":
                    rp.Highlight = new Highlight
                    {
                        Val = HighlightColorValues.Yellow
                    };
                    break;
                case "tt":
                case "kbd":
                case "code":
                case "samp":
                    rp.RunFonts = new RunFonts()
                    {
                        Ascii = "Consolas",
                        HighAnsi = "Consolas"
                    };
                    break;
                default:
                    return null;
            }
            if (domNode.HasClass("err"))
            {
                rp.Color = new Color()
                {
                    Val = "FF0000"
                };
            }
            return r;
        }

        private static OpenXmlElement AddText(HtmlNode domNode)
        {
            var s = HttpUtility.HtmlDecode(domNode.InnerText);
            var hasNewLine = s.Contains('\n');
            if (hasNewLine && string.IsNullOrWhiteSpace(s))
                return null;

            if (hasNewLine)
                s = s.Replace("\n", string.Empty);

            var t = new Text(s)
            {
                Space = SpaceProcessingModeValues.Preserve
            };
            return t;
        }

        private static void AddSectionProperties(Body body)
        {
            body.AppendChild(
                new SectionProperties(
                    new PageMargin()
                    {
                        Top = 850,
                        Right = 850,
                        Bottom = 850,
                        Left = 1134,
                        Header = 850,
                        Footer = 850,
                        Gutter = 0
                    }
                )
            );
        }

        private static Hyperlink AddHyperlink(HtmlNode domNode, MainDocumentPart mainPart, string url)
        {
            var href = domNode.GetAttributeValue("href", string.Empty);
            var hlink = new Hyperlink();
            if (!string.IsNullOrWhiteSpace(href))
            {
                var uriSuccess = Uri.TryCreate(href, UriKind.Absolute, out Uri uri);
                if (!uriSuccess && !string.IsNullOrWhiteSpace(url))
                    uriSuccess = Uri.TryCreate(url + href, UriKind.Absolute, out uri);

                if (uriSuccess)
                {
                    var hrel = mainPart.AddHyperlinkRelationship(uri, true);
                    hlink.Id = hrel.Id;
                    hlink.TargetFrame = "_blank";
                }
            }
            return hlink;
        }

        private static Paragraph AddCellParagraph()
        {
            return new()
            {
                ParagraphProperties = new ParagraphProperties()
                {
                    SpacingBetweenLines = new SpacingBetweenLines()
                    {
                        Before = "0",
                        After = "0",
                        LineRule = LineSpacingRuleValues.Exact
                    },
                    Justification = new Justification()
                    {
                        Val = JustificationValues.Center
                    }
                }
            };
        }

        private static OpenXmlElement AddParagraph(HtmlNode domNode)
        {
            var styleName = domNode.Name.ToLowerInvariant();
            var p = new Paragraph();
            if (styleName != "p")
            {
                p.ParagraphProperties = new ParagraphProperties()
                {
                    ParagraphStyleId = new ParagraphStyleId()
                    {
                        Val = styleName
                    }
                };
            }
            if (domNode.HasClass("ref"))
            {
                if (styleName == "p")
                    p.ParagraphProperties = new ParagraphProperties();

                p.ParagraphProperties.Justification = new Justification()
                {
                    Val = JustificationValues.Right
                };
                p.ParagraphProperties.SpacingBetweenLines = new SpacingBetweenLines()
                {
                    Before = "0",
                    After = "0",
                    LineRule = LineSpacingRuleValues.Exact
                };
                p.ParagraphProperties.Shading = new Shading()
                {
                    Fill = "F0FFF0",
                    Val = ShadingPatternValues.Clear
                };
            }
            return p;
        }
    }
}
