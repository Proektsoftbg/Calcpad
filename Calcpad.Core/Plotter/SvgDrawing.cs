using System;
using System.IO;
using System.Text;

namespace Calcpad.Core
{
    internal class SvgDrawing
    {
        private const int Decimals = 2;
        private readonly StringBuilder _sb;
        private readonly string _svgTag;
        internal double Width { get; }
        internal double Height { get; }
        internal SvgDrawing(double width, double height)
        {
            Width = width;
            Height = height;
            _svgTag = "<svg version=\"1.1\" xmlns=\"http://www.w3.org/2000/svg\" viewbox=\" 0 0 " + width + " " + height + "\">";
            _sb = new StringBuilder(1000);
            _sb.AppendLine();
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
            const double roughness = 0.001;
            var y1 = points[0].Y;
            var dyLim = (points[1].X - points[0].X) * roughness;
            var dy1 = points[1].Y - y1;
            var n = 0;
            var nPoints = points.Length;
            _sb.Append(" points=\"" + points[0]);
            for (int i = 1; i < nPoints; ++i)
            {
                var dy2 = (points[i].Y - y1) / (i - n);
                if (Math.Abs(dy2 - dy1) > dyLim)
                {
                    n = i - 1;
                    _sb.Append(" " + points[n]);
                    y1 = points[i].Y;
                    dy1 = dy2;
                    n = i;
                }
            }
            _sb.Append(" " + points[nPoints - 1]);
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
            return _svgTag + _sb + "</svg>\n";
        }

        internal void Save(string fileName)
        {
            using var sr = new StreamWriter(fileName);
            sr.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sr.WriteLine(_svgTag);
            sr.WriteLine("<style type=\"text/css\">");
            sr.WriteLine(".PlotGrid      {fill: none; stroke-width:1; stroke: Black; stroke-opacity: 0.15;}");
            sr.WriteLine(".PlotAxis      {fill: none; stroke-width:1; stroke: Black;}");
            sr.WriteLine(".PlotFunction1 {fill: none; stroke-width:2; stroke: Red;}");
            sr.WriteLine(".PlotFunction2 {fill: none; stroke-width:2; stroke: Green;}");
            sr.WriteLine(".PlotFunction3 {fill: none; stroke-width:2; stroke: Blue;}");
            sr.WriteLine(".PlotFunction4 {fill: none; stroke-width:2; stroke: Goldenrod;}");
            sr.WriteLine(".PlotFunction5 {fill: none; stroke-width:2; stroke: Magenta;}");
            sr.WriteLine(".PlotFunction6 {fill: none; stroke-width:2; stroke: DarkCyan;}");
            sr.WriteLine(".PlotFunction7 {fill: none; stroke-width:2; stroke: Purple;}");
            sr.WriteLine(".PlotFunction8 {fill: none; stroke-width:2; stroke: DarkOrange;}");
            sr.WriteLine(".PlotFunction9 {fill: none; stroke-width:2; stroke: Maroon;}");
            sr.WriteLine(".PlotFunction10{fill: none; stroke-width:2; stroke: YellowGreen;}");
            sr.WriteLine("text           {fill: Black; font-family: \"Arial\", Sans; font-size: 9pt;}");
            sr.WriteLine("text.left      {text-anchor: start;}");
            sr.WriteLine("text.middle    {text-anchor: middle;}");
            sr.WriteLine("text.end       {text-anchor: end;}");
            sr.WriteLine("</style>");
            sr.Write(_sb.ToString());
            sr.WriteLine("</svg>");
        }
    }
}
