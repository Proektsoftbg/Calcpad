using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Calcpad.OpenXml
{
    internal static class StyleDefinitionsWriter
    {
        internal static void CreateStyleDefinitionsPart(MainDocumentPart mainPart)
        {
            var styleDefinitions = AddStyleDefinitionsPart(mainPart);
            AddDocDefaults(styleDefinitions);
            AddParagraphStyles(styleDefinitions);
            AddTableStyles(styleDefinitions);
        }

        private static StyleDefinitionsPart AddStyleDefinitionsPart(MainDocumentPart mainPart)
        {
            var part = mainPart.AddNewPart<StyleDefinitionsPart>();
            var root = new Styles();
            root.Save(part);
            part.Styles.DocDefaults = new DocDefaults();
            return part;
        }

        private static void AddDocDefaults(StyleDefinitionsPart styleDefinitionsPart)
        {
            styleDefinitionsPart.Styles.DocDefaults.RunPropertiesDefault = new RunPropertiesDefault()
            {
                RunPropertiesBaseStyle = new RunPropertiesBaseStyle
                {
                    RunFonts = new RunFonts()
                    {
                        Ascii = "Calibri",
                        HighAnsi = "Calibri"
                    },
                    FontSize = new FontSize
                    {
                        Val = "24"
                    }
                }
            };
        }

        private static void AddParagraphStyles(StyleDefinitionsPart styleDefinitionsPart)
        {
            var styles = styleDefinitionsPart.Styles;
            string[] fontSizes = ["48", "40", "36", "32", "28", "26"];
            for (int i = 0; i < 6; ++i)
            {
                var id = 'h' + (i + 1).ToString();
                styles.AppendChild(new Style()
                {
                    Type = StyleValues.Paragraph,
                    StyleId = id,
                    StyleName = new StyleName() { Val = id },
                    BasedOn = new BasedOn() { Val = "Normal" },
                    NextParagraphStyle = new NextParagraphStyle() { Val = "Normal" },
                    StyleRunProperties = new StyleRunProperties()
                    {
                        FontSize = new FontSize() { Val = fontSizes[i] },
                        Bold = new Bold()
                    }
                });
            }
        }

        private static void AddTableStyles(StyleDefinitionsPart styleDefinitionsPart)
        {
            styleDefinitionsPart.Styles.AppendChild(new Style()
            {
                Type = StyleValues.Table,
                StyleId = "bcpd",
                StyleName = new StyleName() { Val = "Calcpad Bordered" },
                StyleTableCellProperties = new StyleTableCellProperties()
                {
                    TableCellMargin = new TableCellMargin()
                    {
                        LeftMargin = new LeftMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "68"
                        },
                        RightMargin = new RightMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "68"
                        },
                        TopMargin = new TopMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "34"
                        },
                        BottomMargin = new BottomMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "34"
                        }
                    },
                    TableCellVerticalAlignment = new TableCellVerticalAlignment()
                    {
                        Val = TableVerticalAlignmentValues.Center
                    }
                }
            });
            styleDefinitionsPart.Styles.AppendChild(new Style()
            {
                Type = StyleValues.Table,
                StyleId = "cpd",
                StyleName = new StyleName() { Val = "Calcpad" },
                StyleTableCellProperties = new StyleTableCellProperties()
                {
                    TableCellMargin = new TableCellMargin()
                    {
                        LeftMargin = new LeftMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "0"
                        },
                        RightMargin = new RightMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "0"
                        },
                        TopMargin = new TopMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "0"
                        },
                        BottomMargin = new BottomMargin()
                        {
                            Type = TableWidthUnitValues.Dxa,
                            Width = "0"
                        }
                    },
                    TableCellVerticalAlignment = new TableCellVerticalAlignment()
                    {
                        Val = TableVerticalAlignmentValues.Top
                    }
                }
            });
        }
    }
}
