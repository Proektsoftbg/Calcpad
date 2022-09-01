using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Calcpad.Core
{
    internal class ChartPlotter : Plotter
    {
        public ChartPlotter(MathParser parser, PlotSettings settings) : base(parser, settings) { }
        private const float GDILimit = 1e8f;
        private Variable _var;

        private void GetImageSize()
        {
            Left = (int)(1.5 * Margin);
            Right = (int)(0.75 * Margin);
            Width = (int)(Parser.PlotWidth * ScreenScaleFactor) + Left + Right;
            Height = (int)(Parser.PlotHeight * ScreenScaleFactor) + 2 * Margin;
        }

        internal string Plot(Func<Value>[] fx, Func<Value>[] fy, Variable var, double start, double end, Unit u)
        {
            _var = var;
            GetImageSize();
            var charts = new Chart[fx.Length];
            IEnumerable<Node> points;
            Box limits = new();
            var n = Settings.IsAdaptive ? 2 : 1;
            double x0 = 0.0, y0 = 0.0, xs = 0.0, ys = 0.0;
            for (int k = 0; k < n; ++k)
            {
                limits = new();
                for (int i = 0, len = fx.Length; i < len; ++i)
                {
                    if (k == 0)
                        points = Calculate(fx[i], fy[i], start, end, u);
                    else
                        points = CalculateAdaptive(charts[i], fx[i], fy[i], xs == 0.0 ? 1.0 : ys / xs);

                    charts[i] = new Chart(points.Count());
                    foreach (var p in points)
                        charts[i].AddPoint(p.X, p.Y, p.Z);

                    limits.Expand(charts[i].Bounds);
                }
                limits = FixLimits(limits);
                if (limits.Width == 0 || !double.IsNormal(limits.Width) || ys == limits.Height)
                {
                    if (start.EqualsBinary(end))
                    {
                        var padding = double.IsNormal(limits.Height) ?
                            Math.Max(limits.Height, 0.1) :
                            0.1;
                        var left = start - padding;
                        var right = end + padding;
                        limits = new Box(left, left, right, right);
                    }
                    else
                        limits = new Box(start, start, end, end);

                    xs = (Width - Left - Right) / limits.Width;
                    x0 = Left - (int)(limits.Left * xs);
                    ys = (Height - 2 * Margin) / limits.Height;
                    y0 = Height - Margin + (int)(limits.Bottom * ys);
                    break;
                }
                xs = (Width - Left - Right) / limits.Width;
                x0 = Left - (int)(limits.Left * xs);
                ys = (Height - 2 * Margin) / limits.Height;
                y0 = Height - Margin + (int)(limits.Bottom * ys);
            }
            if (Settings.VectorGraphics)
            {
                GetSvgPoints(charts, x0, y0, xs, ys);
                return DrawSvg(charts, x0, y0, xs, ys, limits);
            }
            GetPngPoints(charts, x0, y0, xs, ys);
            return DrawPng(charts, x0, y0, xs, ys, limits);
        }

        private static Box FixLimits(Box limits)
        {
            var left = limits.Left;
            var right = limits.Right;
            var bottom = limits.Bottom;
            var top = limits.Top;
            FixBounds(ref left, ref right);
            FixBounds(ref bottom, ref top);
            if (bottom > 0 && bottom < 0.35 * top)
                bottom = 0;
            else if (top < 0 && bottom > 0.35 * top)
                top = 0;
            return new Box(left, bottom, right, top);
        }

        private Node[] Calculate(Func<Value> fx, Func<Value> fy, double start, double end, Unit u)
        {
            var n = Settings.IsAdaptive ? 31 : 512;
            if (n < 2) n = 2;
            var s = (end - start) / n;
            var points = new Node[n + 1];
            var t = start;
            _var.SetValue(t, u);
            for (int i = 0; i <= n; ++i)
            {
                points[i] = CalculatePoint(fx, fy, t);
                t += s;
            }
            return points;
        }

        private LinkedList<Node> CalculateAdaptive(Chart chart, Func<Value> fx, Func<Value> fy, double kxy)
        {
            const double tol = 0.996;
            var points = new LinkedList<Node>(chart.GetPoints());
            if (chart.PointCount < 4)
                return points;

            for (int i = 0; i < 5; ++i)
            {
                var n1 = points.First;
                var n2 = n1.Next;
                var n3 = n2.Next;
                var p1 = n1.ValueRef;
                p1 = new Node(p1.X, p1.Y * kxy, p1.Z);
                var p2 = n2.ValueRef;
                p2 = new Node(p2.X, p2.Y * kxy, p2.Z);
                var v1 = Vector(p1, p2);
                var added = false;
                while (n3 is not null)
                {
                    var p3 = n3.ValueRef;
                    p3 = new Node(p3.X, p3.Y * kxy, p3.Z);
                    var v2 = Vector(p2, p3);
                    var d = Math.Abs(v1.X * v2.X + v1.Y * v2.Y);
                    if (d < tol)
                    {
                        var p = CalculatePoint(fx, fy, (p2.Z + p3.Z) / 2);
                        if (double.IsNaN(p.Y))
                            added = false;
                        else
                        {
                            points.AddAfter(n2, p);
                            if (!added)
                            {
                                p = CalculatePoint(fx, fy, (p1.Z + p2.Z) / 2);
                                if (!double.IsNaN(p.Y))
                                    points.AddBefore(n2, p);
                            }
                            added = true;
                        }
                    }
                    else
                        added = false;

                    v1 = v2;
                    p1 = p2;
                    p2 = p3;
                    n2 = n3;
                    n3 = n3.Next;
                }
            }
            return points;
        }

        private static Node Vector(Node p1, Node p2)
        {
            double x = p2.X - p1.X;
            double y = p2.Y - p1.Y;
            double l = Math.Sqrt(x * x + y * y);
            if (l == 0)
                return new Node();

            return new Node(x / l, y / l);
        }

        Node CalculatePoint(Func<Value> fx, Func<Value> fy, double t)
        {
            _var.SetNumber(t);
            var vx = fx();
            //Parser.CheckReal(vx);
            var vy = fy();
            //Parser.CheckReal(vy);
            return new Node(vx.Number.Re, vy.Number.Re, t);
        }

        private static void GetPngPoints(Chart[] charts, double x0, double y0, double xs, double ys)
        {
            for (int i = 0, len = charts.Length; i < len; ++i)
                charts[i].GetPngPoints(x0, y0, xs, ys);
        }

        private static void GetSvgPoints(Chart[] charts, double x0, double y0, double xs, double ys)
        {
            for (int i = 0, len = charts.Length; i < len; ++i)
                charts[i].GetSvgPoints(x0, y0, xs, ys);
        }

        private string DrawPng(Chart[] charts, double x0, double y0, double xs, double ys, Box bounds)
        {
            var canvas = new Bitmap(Width, Height);
            var g = Graphics.FromImage(canvas);
            var penWidth = 2f * (float)ScreenScaleFactor;
            var dotRadius = 2f * penWidth;
            var dotSize = new SizeF(2f * dotRadius, 2f * dotRadius);
            Pen[] chartPens =
            {
                new(Color.Red, penWidth),
                new(Color.Green, penWidth),
                new(Color.Blue, penWidth),
                new(Color.Goldenrod, penWidth),
                new(Color.Magenta, penWidth),
                new(Color.DarkCyan, penWidth),
                new(Color.Purple, penWidth),
                new(Color.DarkOrange, penWidth),
                new(Color.Maroon, penWidth),
                new(Color.YellowGreen, penWidth)
            };
            foreach (Pen pen in chartPens)
            {
                pen.LineJoin = LineJoin.Round;
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
            }
            DrawGridPng(g, x0, y0, xs, ys, bounds);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var penNo = 0;
            foreach (var c in charts)
            {
                if (c.PointCount <= 0)
                    continue;

                if (c.Bounds.Width == 0 && c.Bounds.Height == 0)
                {
                    var p = new PointF(c.PngPoints[0].X - dotRadius, c.PngPoints[0].Y - dotRadius);
                    var rect = new RectangleF(p, dotSize);
                    g.FillEllipse(chartPens[penNo].Brush, rect);
                }
                else
                {
                    //g.DrawCurve(chartPens[penNo], c.PngPoints, 0.25f);
                    g.DrawLines(chartPens[penNo], c.PngPoints);
                    //foreach (var p in c.PngPoints)
                    //  g.FillEllipse(chartPens[penNo].Brush, p.X - 6, p.Y - 6, 12, 12);
                }

                penNo++;
                if (penNo >= chartPens.Length)
                    penNo = 0;
            }
            string src;
            if (string.IsNullOrEmpty(Settings.ImagePath))
                src = ImageToBase64(canvas);
            else
                src = Settings.ImageUri + PngToFile(canvas, Settings.ImagePath);

            for (int j = 0, len = chartPens.Length; j < len; ++j)
                chartPens[j].Dispose();

            g.Dispose();
            canvas.Dispose();
            return HtmlImg(src);
        }

        private string DrawSvg(Chart[] charts, double x0, double y0, double xs, double ys, Box bounds)
        {
            var svgDrawing = new SvgDrawing(Width, Height);
            DrawGridSvg(svgDrawing, x0, y0, xs, ys, bounds);
            var penNo = 1;
            foreach (var chart in charts)
            {
                if (chart.PointCount > 0)
                {
                    if (chart.Bounds.Width == 0 && chart.Bounds.Height == 0)
                        svgDrawing.DrawCircle(chart.SvgPoints[0].X, chart.SvgPoints[0].Y, 4.0, "PlotFunction" + penNo);
                    else
                        svgDrawing.DrawPolyline(chart.SvgPoints, "PlotFunction" + penNo);

                    penNo++;
                    if (penNo > 10)
                        penNo = 1;
                }
            }
            if (Settings.ImagePath.Length != 0)
                return HtmlImg(Settings.ImageUri + SvgToFile(svgDrawing, Settings.ImagePath));

            double d = 0.75 / ScreenScaleFactor;
            double dw = Math.Round(d * Width);
            double dh = Math.Round(d * Height);
            return $"<div class=\"plot\" style=\"width:{dw}pt; height:{dh}pt;\">{svgDrawing}</div>";
        }
        private struct Chart
        {
            private readonly Node[] _points;
            internal PointF[] PngPoints;
            internal SvgPoint[] SvgPoints;
            internal int PointCount;
            internal Box Bounds;
            internal Chart(int size)
            {
                _points = new Node[size];
                PngPoints = Array.Empty<PointF>();
                SvgPoints = Array.Empty<SvgPoint>();
                PointCount = 0;
                Bounds = new Box();
            }

            internal void AddPoint(double x, double y, double z)
            {
                if (double.IsNaN(x) || double.IsInfinity(x) || (double.IsNaN(y) || double.IsInfinity(y)))
                    return;

                _points[PointCount] = new Node(x, y, z);
                Bounds.Expand(_points[PointCount]);
                ++PointCount;
            }

            internal IEnumerable<Node> GetPoints() => _points.Take(PointCount);

            internal void GetPngPoints(double x0, double y0, double xs, double ys)
            {
                PngPoints = new PointF[PointCount];
                for (int i = 0; i < PointCount; ++i)
                {
                    var x = (float)(x0 + _points[i].X * xs);
                    if (Math.Abs(x) > GDILimit)
                        x = (float)Math.CopySign(GDILimit, x);

                    var y = (float)(y0 - _points[i].Y * ys);
                    if (Math.Abs(y) > GDILimit)
                        y = (float)Math.CopySign(GDILimit, y);

                    PngPoints[i] = new PointF(x, y);
                }
            }

            internal void GetSvgPoints(double x0, double y0, double xs, double ys)
            {
                SvgPoints = new SvgPoint[PointCount];
                for (int i = 0; i < PointCount; ++i)
                    SvgPoints[i] = new SvgPoint(x0 + _points[i].X * xs, y0 - _points[i].Y * ys);
            }
        }
    }
}
