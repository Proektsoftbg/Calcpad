using Calcpad.Core;
using System.IO;
using System;
using Calcpad;

namespace PyCalcpad
{
    public class Parser
    {
        public Settings Settings;

        public string Parse(string code)
        {
            var parser = new ExpressionParser
            {
                Settings = ConvertSettings(Settings)
            };
            parser.Parse(code, true, false);
            return parser.HtmlResult;
        }

        public bool Convert(string inputFileName, string outputFileName)
        {
            if (OperatingSystem.IsWindows())
            {
                inputFileName = inputFileName.ToLower();
            }
            if (string.IsNullOrWhiteSpace(outputFileName))
                outputFileName = Path.ChangeExtension(inputFileName, ".html");
            else if (string.Equals(outputFileName, "html", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(outputFileName, "htm", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(outputFileName, "docx", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(outputFileName, "pdf", StringComparison.OrdinalIgnoreCase))
                outputFileName = Path.ChangeExtension(inputFileName, "." + outputFileName);

            var ext = Path.GetExtension(outputFileName);
            var path = Path.GetDirectoryName(inputFileName);
            if (!string.IsNullOrWhiteSpace(path))
                Directory.SetCurrentDirectory(path);

            var code = Reader.Read(inputFileName);
            var macroParser = new MacroParser
            {
                Include = Reader.Include
            };
            var hasMacroErrors = macroParser.Parse(code, out var unwrappedCode, null, 0, true);
            string htmlResult;
            Converter converter = new();
            if (hasMacroErrors)
            {
                htmlResult = Reader.CodeToHtml(unwrappedCode);
                converter.ToHtml(htmlResult, outputFileName);
                return true;
            }
            ExpressionParser parser = new()
            {
                Settings = ConvertSettings(Settings)
            };
            parser.Parse(unwrappedCode, true, ext == ".docx");
            htmlResult = parser.HtmlResult;

            if (ext == ".html" || ext == ".htm")
                converter.ToHtml(htmlResult, outputFileName);
            else if (ext == ".docx")
                converter.ToOpenXml(htmlResult, outputFileName, parser.OpenXmlExpressions);
            else if (ext == ".pdf")
                converter.ToPdf(htmlResult, outputFileName);
            else
                return false;

            return true;
        }

        private static Calcpad.Core.Settings ConvertSettings(Settings settings) =>
            new()
            {
                Units = settings.Units,
                Math = Calculator.ConvertMathSettings(settings.Math),
                Plot = ConvertPlotSettings(settings.Plot),
            };

        private static Calcpad.Core.PlotSettings ConvertPlotSettings(PlotSettings settings) =>
            new()
            {
                IsAdaptive = settings.IsAdaptive,
                ScreenScaleFactor = settings.ScreenScaleFactor,
                ImagePath = settings.ImagePath,
                ImageUri = settings.ImageUri,
                VectorGraphics = settings.VectorGraphics,
                ColorScale = (Calcpad.Core.PlotSettings.ColorScales)settings.ColorScale,
                SmoothScale = settings.SmoothScale,
                Shadows = settings.Shadows,
                LightDirection = (Calcpad.Core.PlotSettings.LightDirections)settings.LightDirection
            };
    }
}
