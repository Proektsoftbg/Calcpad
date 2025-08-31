using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Calcpad.Core
{
    internal class SvgDrawing
    {
        private readonly StringBuilder _sb;
        internal double Width { get; }
        internal double Height { get; }
        internal double ScaleFactor { get; set; }
        internal SvgDrawing(double width, double height, double scaleFactor, int seriesCount)
        {
            Width = width;
            Height = height;
            ScaleFactor = scaleFactor;
            var k = 1.35 * scaleFactor;
            var w = Convert(width);
            var h = Convert(height);
            var wk = Convert(width / k);
            var hk = Convert(height / k);
            var _svgTag = $"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" viewbox=\" 0 0 {w} {h}\" width=\"{w}px\" height=\"{h}px\" style=\"width:{wk}pt; height:{hk}pt;\" >";
            _sb = new StringBuilder(_svgTag);
            _sb.AppendLine();
            AddStyle(seriesCount);
        }

        private void AddClass(string svgClass)
        {
            if (svgClass.Length > 0)
                _sb.Append("\" class=\"" + svgClass);

            _sb.Append("\" />").AppendLine();
        }

        internal void DrawLine(double x1, double y1, double x2, double y2, string svgClass = "")
        {
            _sb.Append("<line x1=\"").Append(Convert(x1))
            .Append("\" y1=\"").Append(Convert(y1))
            .Append("\" x2=\"").Append(Convert(x2))
            .Append("\" y2=\"").Append(Convert(y2));
            AddClass(svgClass);
        }

        internal void DrawRectangle(double x, double y, double w, double h, string svgClass = "")
        {
            _sb.Append("<rect x=\"").Append(Convert(x))
            .Append("\" y=\"").Append(Convert(y))
            .Append("\" width=\"").Append(Convert(w))
            .Append("\" height=\"").Append(Convert(h));
            AddClass(svgClass);
        }

        internal void FillRectangle(double x, double y, double w, double h, string color = "White")
        {
            _sb.Append("<rect x=\"").Append(Convert(x))
            .Append("\" y=\"").Append(Convert(y))
            .Append("\" width=\"").Append(Convert(w))
            .Append("\" height=\"").Append(Convert(h))
            .Append("\" fill=\"").Append(color).Append("\" stroke=\"none\" />")
            .AppendLine();
        }


        internal void FillPolygon(SvgPoint[] points, string svgClass)
        {
            _sb.Append("<polygon");
            DrawPoints(points);
            _sb.Append("\" stroke-width=\"0");
            AddClass(svgClass);
        }

        internal void DrawCircle(double x, double y, double r, string svgClass = "")
        {
            _sb.Append("<circle cx=\"").Append(Convert(x))
            .Append("\" cy=\"").Append(Convert(y))
            .Append("\" r=\"").Append(Convert(r));
            AddClass(svgClass);
        }

        internal void DrawPolyline(SvgPoint[] points, string svgClass = "")
        {
            _sb.Append("<polyline");
            DrawPoints(points);
            AddClass(svgClass);
        }

        internal void DrawPoints(SvgPoint[] points)
        {
            var nPoints = points.Length;
            _sb.Append(" points=\"").Append(points[0]);
            for (int i = 1; i < nPoints; ++i)
                _sb.Append(' ').Append(points[i]);
        }

        internal void DrawText(string text, double x, double y, string svgClass = "")
        {
            _sb.Append("<text x=\"").Append(Convert(x))
            .Append("\" y=\"").Append(Convert(y));
            if (svgClass.Length > 0)
                _sb.Append("\" class=\"").Append(svgClass);
            _sb.Append("\">").Append(text).Append(" </text>");
            _sb.AppendLine();
        }

        internal void DrawImage(double x, double y, double w, double h, string src)
        {
            _sb.Append("<image x=\"").Append(Convert(x))
            .Append("\" y=\"").Append(Convert(y))
            .Append("\" width=\"").Append(Convert(w))
            .Append("\" height=\"").Append(Convert(h))
            .Append("\" href=\"").Append(src).Append("\" />")
            .AppendLine();
        }

        private static string Convert(double d) =>
            Math.Round(d, 2).ToString(CultureInfo.InvariantCulture);

        public override string ToString() => _sb + "</g></svg>\n";

        private void AddStyle(int n)
        {
            var sf2 = 1.5 * ScaleFactor;
            string[] colors = ["Tomato", "YellowGreen", "CornflowerBlue", "#f0d000", "MediumVioletRed", "MediumSpringGreen", "BlueViolet", "LightSalmon", "DeepPink", "DarkTurquoise"];
            string[] opacities = ["0.050", "0.045", "0.040", "0.035", "0.030", "0.025", "0.020", "0.020", "0.020", "0.020"];
            _sb.AppendLine("<style type=\"text/css\">")
            .AppendLine($".plot text {{fill:Black; font-family:'Segoe UI', Sans; font-size:{Convert(11 * ScaleFactor)}px}}")
            .AppendLine(".plot text.left {text-anchor: start;}")
            .AppendLine($".plot text.sm {{font-size:{Convert(11 * ScaleFactor)}px}}")
            .AppendLine(".plot text.mid {text-anchor: middle;}")
            .AppendLine(".plot text.end {text-anchor: end;}")
            .AppendLine($".plot .Grid, .plot .Frame, .plot .Axis{{fill:none; stroke-width:{Convert(ScaleFactor)};}}")
            .AppendLine(".plot .Grid {stroke:rgba(0, 0, 0, 0.08);}")
            .AppendLine(".plot .Frame {stroke:rgba(0, 0, 0, 0.24);}")
            .AppendLine(".plot .Axis {stroke: Black;}")
            .AppendLine($".plot polyline {{fill:none; stroke-width:{Convert(sf2)};}}")
            .AppendLine(".plot [class^=\"Fill\"] {stroke:none;}");
            for (int i = 1; i <= n; ++i)
            {
                var color = colors[i - 1];
                _sb.AppendLine($".Series{i} {{stroke:{color};}}")
                .AppendLine($"circle.Series{i} {{fill:{color};}}")
                .AppendLine($".Fill{i} {{fill:{color}; fill-opacity:{opacities[i - 1]};}}");
            }
            _sb.AppendLine("</style><g class=\"plot\">");
        }

        internal void Save(string fileName)
        {
            using var sr = new StreamWriter(fileName);
            sr.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sr.Write(_sb.ToString());
            sr.WriteLine("</g></svg>");
        }
    }
}
