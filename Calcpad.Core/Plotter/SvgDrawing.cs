using System;
using System.IO;
using System.Text;

namespace Calcpad.Core
{
    internal class SvgDrawing
    {
        private const int Decimals = 2;
        private readonly StringBuilder _sb;
        internal double Width { get; }
        internal double Height { get; }
        internal double ScaleFactor { get; set; }
        internal SvgDrawing(double width, double height, double scaleFactor)
        {
            Width = Math.Round(width);
            Height = Math.Round(height);
            ScaleFactor = scaleFactor;
            var k = 1.35 * scaleFactor;
            var _svgTag = $"<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\" viewbox=\" 0 0 {Width} {Height}\" style=\"width: {width/k}pt; height: {height/k}pt;\">";
            _sb = new StringBuilder(_svgTag);
            _sb.AppendLine();
            AddStyle();
        }

        private void AddClass(string svgClass)
        {
            if (svgClass.Length > 0)
                _sb.Append("\" class=\"" + svgClass);

            _sb.Append("\" />");
            _sb.AppendLine();
        }

        internal void DrawLine(double x1, double y1, double x2, double y2, string svgClass = "")
        {
            _sb.Append("<line x1=\"" + Math.Round(x1, Decimals));
            _sb.Append("\" y1=\"" + Math.Round(y1, Decimals));
            _sb.Append("\" x2=\"" + Math.Round(x2, Decimals));
            _sb.Append("\" y2=\"" + Math.Round(y2, Decimals));
            AddClass(svgClass);
        }

        internal void DrawRectangle(double x, double y, double w, double h, string svgClass = "")
        {
            _sb.Append("<rect x=\"" + Math.Round(x, Decimals));
            _sb.Append("\" y=\"" + Math.Round(y, Decimals));
            _sb.Append("\" width=\"" + Math.Round(w, Decimals));
            _sb.Append("\" height=\"" + Math.Round(h, Decimals));
            AddClass(svgClass);
        }

        internal void FillRectangle(double x, double y, double w, double h, string color = "White")
        {
            _sb.Append("<rect x=\"" + Math.Round(x, Decimals));
            _sb.Append("\" y=\"" + Math.Round(y, Decimals));
            _sb.Append("\" width=\"" + Math.Round(w, Decimals));
            _sb.Append("\" height=\"" + Math.Round(h, Decimals));
            _sb.Append("\" fill=\"" + color + "\" stroke=\"none\" />");
            _sb.AppendLine();
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
            _sb.Append("<circle cx=\"" + Math.Round(x, Decimals));
            _sb.Append("\" cy=\"" + Math.Round(y, Decimals));
            _sb.Append("\" r=\"" + Math.Round(r, Decimals));
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
            _sb.Append(" points=\"" + points[0]);
            for (int i = 1; i < nPoints; ++i)
                _sb.Append(" " + points[i]);
        }

        internal void DrawText(string text, double x, double y, string svgClass = "")
        {
            _sb.Append("<text x=\"" + Math.Round(x, Decimals));
            _sb.Append("\" y=\"" + Math.Round(y, Decimals));
            if (svgClass.Length > 0)
                _sb.Append("\" class=\"" + svgClass);
            _sb.Append("\">" + text + " </text>");
            _sb.AppendLine();
        }

        internal void DrawImage(double x, double y, double w, double h, string src)
        {
            _sb.Append("<image x=\"" + Math.Round(x, Decimals));
            _sb.Append("\" y=\"" + Math.Round(y, Decimals));
            _sb.Append("\" width=\"" + Math.Round(w, Decimals));
            _sb.Append("\" height=\"" + Math.Round(h, Decimals));
            _sb.Append("\" xlink:href=\"" + src + "\" />");
            _sb.AppendLine();
        }

        public override string ToString()
        {
            return _sb + "</svg>\n";
        }

        private void AddStyle()
        {
            var sf2 = 1.5 * ScaleFactor;
            _sb.AppendLine("<style type=\"text/css\">");
            _sb.AppendLine($".PlotGrid {{fill:none; stroke-width:{ScaleFactor}; stroke:rgba(0, 0, 0, 0.08);}}");
            _sb.AppendLine($".PlotFrame {{fill:none; stroke-width:{ScaleFactor}; stroke:rgba(0, 0, 0, 0.24);}}");
            _sb.AppendLine($".PlotAxis {{fill:none; stroke-width:{ScaleFactor}; stroke:Black;}}");
            _sb.AppendLine($".PlotSeries1 {{fill:none; stroke-width:{sf2}; stroke:Red;}}");
            _sb.AppendLine($".PlotSeries2 {{fill:none; stroke-width:{sf2}; stroke:Green;}}");
            _sb.AppendLine($".PlotSeries3 {{fill:none; stroke-width:{sf2}; stroke:Blue;}}");
            _sb.AppendLine($".PlotSeries4 {{fill:none; stroke-width:{sf2}; stroke:Goldenrod;}}");
            _sb.AppendLine($".PlotSeries5 {{fill:none; stroke-width:{sf2}; stroke:Magenta;}}");
            _sb.AppendLine($".PlotSeries6 {{fill:none; stroke-width:{sf2}; stroke:DarkCyan;}}");
            _sb.AppendLine($".PlotSeries7 {{fill:none; stroke-width:{sf2}; stroke:Purple;}}");
            _sb.AppendLine($".PlotSeries8 {{fill:none; stroke-width:{sf2}; stroke:DarkOrange;}}");
            _sb.AppendLine($".PlotSeries9 {{fill:none; stroke-width:{sf2}; stroke:Maroon;}}");
            _sb.AppendLine($".PlotSeries10 {{fill:none; stroke-width:{sf2}; stroke:YellowGreen;}}");
            _sb.AppendLine("circle.PlotSeries1 {fill:Red;}");
            _sb.AppendLine("circle.PlotSeries2 {fill:Green;}");
            _sb.AppendLine("circle.PlotSeries3 {fill:Blue;}");
            _sb.AppendLine("circle.PlotSeries4 {fill:Goldenrod;}");
            _sb.AppendLine("circle.PlotSeries5 {fill:Magenta;}");
            _sb.AppendLine("circle.PlotSeries6 {fill:DarkCyan;}");
            _sb.AppendLine("circle.PlotSeries7 {fill:Purple;}");
            _sb.AppendLine("circle.PlotSeries8 {fill:DarkOrange;}");
            _sb.AppendLine("circle.PlotSeries9 {fill:Maroon;}");
            _sb.AppendLine("circle.PlotSeries10 {fill:YellowGreen;}");
            _sb.AppendLine(".PlotFill1 {stroke:none; fill:Red; fill-opacity:0.050;}");
            _sb.AppendLine(".PlotFill2 {stroke:none; fill:Green; fill-opacity:0.045;}");
            _sb.AppendLine(".PlotFill3 {stroke:none; fill:Blue; fill-opacity:0.040;}");
            _sb.AppendLine(".PlotFill4 {stroke:none; fill:Goldenrod; fill-opacity:0.035;}");
            _sb.AppendLine(".PlotFill5 {stroke:none; fill:Magenta; fill-opacity:0.030;}");
            _sb.AppendLine(".PlotFill6 {stroke:none; fill:DarkCyan; fill-opacity:0.025;}");
            _sb.AppendLine(".PlotFill7 {stroke:none; fill:Purple; fill-opacity:0.020;}");
            _sb.AppendLine(".PlotFill8 {stroke:none; fill:DarkOrange; fill-opacity:0.020;}");
            _sb.AppendLine(".PlotFill9 {stroke:none; fill:Maroon; fill-opacity:0.020;}");
            _sb.AppendLine(".PlotFill10 {stroke:none; fill:YellowGreen; fill-opacity:0.020;}");
            _sb.AppendLine($".plot text {{fill:Black; font-family:'Segoe UI', Sans; font-size:{12*ScaleFactor}px}}");
            _sb.AppendLine(".plot text.left {text-anchor: start;}");
            _sb.AppendLine(".plot text.middle {text-anchor: middle;}");
            _sb.AppendLine(".plot text.end {text-anchor: end;}");
            _sb.AppendLine("</style>");
        }

        internal void Save(string fileName)
        {
            using var sr = new StreamWriter(fileName);
            sr.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sr.Write(_sb.ToString());
            sr.WriteLine("</svg>");
        }
    }
}
