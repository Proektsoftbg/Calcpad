using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Core
{
    internal class ChartPlotter(MathParser parser, PlotSettings settings) : Plotter(parser, settings)
    {
        private const float GDILimit = 1e8f;
        private Variable _var;
        private Unit _xUnits, _yUnits;

        private void GetImageSize()
        {
            Left = (int)(1.5 * Margin);
            Right = (int)(0.75 * Margin);
            Width = (int)(Parser.PlotWidth * ScreenScaleFactor) + Left + Right;
            Height = (int)(Parser.PlotHeight * ScreenScaleFactor) + 2 * Margin;
        }

        internal string Plot(Func<IValue>[] fx, Func<IValue>[] fy, Variable var, double start, double end, Unit u)
        {
            _var = var;
            _xUnits = null;
            _yUnits = null;
            GetImageSize();
            var charts = new Chart[fx.Length];
            Box limits = new();
            var n = Settings.IsAdaptive ? 2 : 1;
            double x0 = 0.0, y0 = 0.0, xs = 0.0, ys = 0.0;
            for (int k = 0; k < n; ++k)
            {
                limits = new();
                for (int i = 0, len = fx.Length; i < len; ++i)
                {
                    IEnumerable<Node> points;
                    if (k == 0)
                        points = Calculate(fx[i], fy[i], start, end, u);
                    else
                        points = CalculateAdaptive(charts[i], fx[i], fy[i], xs == 0.0 ? 1.0 : ys / xs);

                    charts[i] = new Chart(points.Count());
                    foreach (var p in points)
                        charts[i].AddPoint(p.X, p.Y, p.Z);

                    charts[i].Fill = fx[i] is null;
                    limits.Expand(charts[i].Bounds);
                }
                limits = FixLimits(limits);
                if (limits.Width == 0 || !double.IsNormal(limits.Width) || ys == limits.Height)
                {
                    if (start.AlmostEquals(end))
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

        private Node[] Calculate(Func<IValue> fx, Func<IValue> fy, double start, double end, Unit u)
        {
            var n = Settings.IsAdaptive ? 31 : 512;
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

        private LinkedList<Node> CalculateAdaptive(Chart chart, Func<IValue> fx, Func<IValue> fy, double kxy)
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

        Node CalculatePoint(Func<IValue> fx, Func<IValue> fy, double t)
        {
            Parser.BreakIfCanceled();
            _var.SetNumber(t);
            var x = ConvertValue(IValue.AsValue((fx?.Invoke() ?? _var.Value)), ref _xUnits);
            var y = ConvertValue(IValue.AsValue(fy()), ref _yUnits);
            return new Node(x, y, t);
        }

        static double ConvertValue(Value v, ref Unit u)
        {
            if (u is null)
                u = v.Units;
            else
            {
                if (!ReferenceEquals(u, v.Units))
                {
                    if (!Unit.IsConsistent(u, v.Units))
                        Throw.InconsistentUnitsException(Unit.GetText(u), Unit.GetText(v.Units));

                    return v.Re * v.Units.ConvertTo(u);
                }
            }
            return v.Re;
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
            using var bitmap = new SKBitmap(Width, Height);
            using var canvas = new SKCanvas(bitmap);
            var penWidth = 2f * ScreenScaleFactor;
            var dotRadius = 2f * penWidth;
            SKPaint[] chartPens =
            [
                new() { Color = SKColors.Red, StrokeWidth = penWidth },
                new() { Color = SKColors.Green, StrokeWidth = penWidth },
                new() { Color = SKColors.Blue, StrokeWidth = penWidth },
                new() { Color = SKColors.Goldenrod, StrokeWidth = penWidth },
                new() { Color = SKColors.Magenta, StrokeWidth = penWidth },
                new() { Color = SKColors.DarkCyan, StrokeWidth = penWidth },
                new() { Color = SKColors.Purple, StrokeWidth = penWidth },
                new() { Color = SKColors.DarkOrange, StrokeWidth = penWidth },
                new() { Color = SKColors.Maroon, StrokeWidth = penWidth },
                new() { Color = SKColors.YellowGreen, StrokeWidth = penWidth },
            ];
            SKPaint[] chartBrushes =
[
                new() { Color = chartPens[0].Color.WithAlpha(12) },
    new() { Color = chartPens[1].Color.WithAlpha(11) },
    new() { Color = chartPens[2].Color.WithAlpha(10) },
    new() { Color = chartPens[3].Color.WithAlpha(9) },
    new() { Color = chartPens[4].Color.WithAlpha(8) },
    new() { Color = chartPens[5].Color.WithAlpha(7) },
    new() { Color = chartPens[6].Color.WithAlpha(6) },
    new() { Color = chartPens[7].Color.WithAlpha(6) },
    new() { Color = chartPens[8].Color.WithAlpha(6) },
    new() { Color = chartPens[9].Color.WithAlpha(6) }
            ];
            foreach (var pen in chartPens)
            {
                pen.Style = SKPaintStyle.Stroke;
                pen.StrokeJoin = SKStrokeJoin.Round;
                pen.StrokeCap = SKStrokeCap.Round;
                pen.IsAntialias = true;
            }
            DrawGridPng(canvas, x0, y0, xs, ys, bounds);
            var penNo = 0;
            foreach (var c in charts)
            {
                if (c.PointCount <= 0)
                    continue;

                ref var pen = ref chartPens[penNo];
                if (c.Bounds.Width == 0 && c.Bounds.Height == 0)
                {

                    pen.Style = SKPaintStyle.StrokeAndFill;
                    canvas.DrawCircle(c.PngPoints[0], dotRadius, pen);
                    pen.Style = SKPaintStyle.Stroke;
                }
                else
                {
                    for (int i = 1, len = c.PngPoints.Length; i < len; ++i)
                        canvas.DrawLine(c.PngPoints[i - 1], c.PngPoints[i], pen);

                    if (c.Fill)
                    {
                        var yf = y0 - Math.Clamp(0, bounds.Bottom * ys, bounds.Top * ys);
                        FillChart(canvas, chartBrushes[penNo], (float)yf, c.PngPoints);
                    }
                }

                penNo++;
                if (penNo >= chartPens.Length)
                    penNo = 0;
            }
            string src;
            if (string.IsNullOrEmpty(Settings.ImagePath))
                src = ImageToBase64(bitmap);
            else
                src = Settings.ImageUri + PngToFile(bitmap, Settings.ImagePath);

            for (int j = 0, len = chartPens.Length; j < len; ++j)
            {
                chartPens[j].Dispose();
                chartBrushes[j].Dispose();
            }
            return HtmlImg(src);
        }

        private static void FillChart(SKCanvas canvas, SKPaint brush, float y0, SKPoint[] points)
        {
            var len = points.Length;
            var fillPoints = new SKPoint[points.Length + 2];
            fillPoints[0] = new(points[0].X, y0);
            fillPoints[^1] = new(points[^1].X, y0);
            for (int i = 0; i < len; ++i)
                fillPoints[i + 1] = points[i];

            using var path = new SKPath();
            path.AddPoly(fillPoints);
            canvas.DrawPath(path, brush);
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
            if (!string.IsNullOrEmpty(Settings.ImagePath))
                return HtmlImg(Settings.ImageUri + SvgToFile(svgDrawing, Settings.ImagePath));

            double d = 0.75 / ScreenScaleFactor;
            double dw = Math.Round(d * Width);
            double dh = Math.Round(d * Height);
            return $"<div class=\"plot\" style=\"width:{dw}pt; height:{dh}pt;\">{svgDrawing}</div>";
        }
        private struct Chart
        {
            private readonly Node[] _points;
            internal SKPoint[] PngPoints;
            internal SvgPoint[] SvgPoints;
            internal int PointCount;
            internal readonly Box Bounds;
            internal bool Fill;
            internal Chart(int size)
            {
                _points = new Node[size];
                PngPoints = [];
                SvgPoints = [];
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

            internal readonly IEnumerable<Node> GetPoints() => _points.Take(PointCount);

            internal void GetPngPoints(double x0, double y0, double xs, double ys)
            {
                PngPoints = new SKPoint[PointCount];
                for (int i = 0; i < PointCount; ++i)
                {
                    var x = (float)(x0 + _points[i].X * xs);
                    if (Math.Abs(x) > GDILimit)
                        x = (float)Math.CopySign(GDILimit, x);

                    var y = (float)(y0 - _points[i].Y * ys);
                    if (Math.Abs(y) > GDILimit)
                        y = (float)Math.CopySign(GDILimit, y);

                    PngPoints[i] = new SKPoint(x, y);
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