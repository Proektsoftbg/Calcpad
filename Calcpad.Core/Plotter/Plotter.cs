using SkiaSharp;
using System;
using System.Globalization;
using System.IO;

namespace Calcpad.Core
{
    internal abstract class Plotter

    {
        private static readonly double[] Steps = [1.0, 2.0, 2.5, 5.0];
        protected readonly float ScreenScaleFactor;
        protected int Width = 500;
        protected int Height = 350;
        protected readonly int Margin;
        protected int Left;
        protected int Right;
        protected PlotSettings Settings;
        protected MathParser Parser;

        protected Plotter(MathParser parser, PlotSettings settings)
        {
            Settings = settings;
            Parser = parser;
            ScreenScaleFactor = 2*(float)settings.ScreenScaleFactor;
            Margin = (int)(30 * ScreenScaleFactor);
            Left = Margin;
            Right = Margin;
        }

        protected static void FixBounds(ref double min, ref double max)
        {
            if (min > max)
                (min, max) = (max, min);

            var d = Math.Max(Math.Abs(min), Math.Abs(max));
            d = d < 1 ? max - min : (max - min) / d;

            if (d >= 1e-14)
                return;

            var mid = (min + max) / 2.0;
            var span = Math.Pow(10, Math.Round(Math.Log10(Math.Abs(mid))) - 2);
            if (span != 0)
                mid = Math.Round(min / span) * span;

            if (d == 0.0)
                d = 0.1;

            if (d < span)
                d = span;

            min = mid - d;
            max = mid + d;
        }

        protected void DrawGridPng(SKCanvas canvas, double x0, double y0, double xs, double ys, Box bounds)
        {
            using var gridPen = CreateGridPen();
            using var axisPen = CreateAxisPen();
            using var textPen = CreateTextPen();
            using var framePen = CreateFramePen();
            var sz = new SKRect();
            textPen.MeasureText(" -0.12 ", ref sz);
            var tw = sz.Width / 5f;
            var th = sz.Height / 2f;
            float xn = Width - Right;
            float yn = Height - Margin;
            var maxSteps = (int)Math.Min(15d * yn / xn, (yn - Margin) / (2.5 * th));
            var delta = bounds.Top - bounds.Bottom;
            var stepY = GetGridStep(delta, maxSteps);
            if (stepY * ys > 8f * th && delta < 7 * stepY)
                stepY /= 2f;
            var tol = delta * 0.02;
            var yg = Math.Round(bounds.Bottom / stepY) * stepY;
            if (yg < bounds.Bottom - tol)
                yg += stepY;
            var max = bounds.Top + tol;
            var isScientific = Math.Abs(bounds.Top) + Math.Abs(bounds.Bottom) >= 20000;
            var a = 4f * ScreenScaleFactor;
            var xt = Left - tw / 2f - a / 2f;
            string s;
            textPen.TextAlign = SKTextAlign.Right;
            while ((yg < max) == (stepY > 0))
            {
                var y = (float)(y0 - yg * ys);
                if (Math.Abs(y) > 1e8)
                    break;

                if (yg >= bounds.Bottom && yg <= bounds.Top)
                    canvas.DrawLine(Left, y, xn, y, gridPen);

                if (Math.Abs(yg) < stepY * 1e-8)
                    s = "0";
                else if (isScientific)
                    s = yg.ToString("0.##E+0", CultureInfo.InvariantCulture);
                else
                    s = OutputWriter.FormatNumberHelper(yg, 2);

                canvas.DrawText(s, xt, y + th, textPen);
                yg += stepY;
            }
            var sx0 = OutputWriter.FormatNumberHelper(bounds.Left, 2);
            var sx1 = OutputWriter.FormatNumberHelper(bounds.Right, 2);
            var n = Math.Max(sx0.Length, sx1.Length) + 2;
            maxSteps = Math.Min(15, (int)((xn - Left) / (tw * n)));
            delta = bounds.Right - bounds.Left;
            var stepX = GetGridStep(delta, maxSteps);
            var midLine = stepX * xs > 1.5 * stepY * ys;
            if (midLine)
                stepX /= 2.0;
            tol = delta * 0.02;
            var xg = Math.Round((bounds.Left - tol) / stepX) * stepX;
            if (xg < bounds.Left - tol)
                xg += stepX;
            max = bounds.Right + tol;
            var yt = yn + 2f * th + a;
            isScientific = Math.Abs(bounds.Right) + Math.Abs(bounds.Left) >= 20000;
            if (midLine)
            {
                n = OutputWriter.FormatNumberHelper(Math.Round(max / stepX) * stepX, 2).Length + 2;
                if (tw * n < stepX * xs)
                    midLine = false;
            }
            textPen.TextAlign = SKTextAlign.Center;
            var even = true;
            while ((xg < max) == (stepX > 0))
            {
                var x = (float)(x0 + xg * xs);
                if (Math.Abs(x) > 1e8)
                    break;

                if (xg >= bounds.Left && xg <= bounds.Right)
                    canvas.DrawLine(x, Margin, x, yn, gridPen);

                if (even)
                {
                    if (Math.Abs(xg) < stepX * 1e-8)
                        s = "0";
                    else if (isScientific)
                        s = xg.ToString("0.##E+0", CultureInfo.InvariantCulture);
                    else
                        s = OutputWriter.FormatNumberHelper(xg, 2);

                    canvas.DrawText(s, x, yt, textPen);
                }
                if (midLine)
                    even = !even;
                xg += stepX;
            }
            textPen.TextAlign = SKTextAlign.Left;
            canvas.DrawRect(Left, Margin, xn - Left, yn - Margin, framePen);
            if (y0 >= Margin - 0.1 && y0 <= yn + 0.1)
            {
                canvas.DrawLine(Left, (float)y0, xn, (float)y0, axisPen);
                canvas.DrawText("x", xn + tw, (float)y0, textPen);
            }
            if (x0 >= Left - 0.1 && x0 <= xn + 0.1)
            {
                canvas.DrawLine((float)x0, Margin, (float)x0, yn, axisPen);
                canvas.DrawText("y", (float)x0 - tw / 2f, Margin - 2f * th, textPen);
            }
            var sy0 = OutputWriter.FormatNumberHelper(bounds.Bottom, 2);
            var sy1 = OutputWriter.FormatNumberHelper(bounds.Top, 2);
            textPen.TextAlign = SKTextAlign.Left;
            textPen.Color = SKColors.Gray;
            a /= 2f;
            canvas.DrawText($"[{sx0}; {sy0}]", a, Height - 1f * th - a, textPen);
            textPen.TextAlign = SKTextAlign.Right;
            canvas.DrawText($"[{sx1}; {sy1}]", Width - a, 2f * th + a, textPen);
        }

        protected SKPaint CreateGridPen() => new()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black.WithAlpha(20),
            StrokeWidth = ScreenScaleFactor,
            IsAntialias = true
        };

        protected SKPaint CreateFramePen() => new()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black.WithAlpha(62),
            StrokeWidth = ScreenScaleFactor,
            IsAntialias = true
        };

        protected SKPaint CreateAxisPen() => new()
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Black,
            StrokeWidth = ScreenScaleFactor,
            IsAntialias = true
        };


        protected SKPaint CreateTextPen() => new()
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = SKColors.Black,
            StrokeWidth = 0.5f,
            Typeface = SKTypeface.FromFamilyName("Segoe UI"),
            TextSize = 12f * ScreenScaleFactor,
            TextAlign = SKTextAlign.Left,
            IsAntialias = true,
            IsAutohinted = true
        };

        protected void DrawGridSvg(SvgDrawing canvas, double x0, double y0, double xs, double ys, Box bounds)
        {
            double tw = 5.5 * ScreenScaleFactor;
            double th = 4.5 * ScreenScaleFactor;
            double th05 = th / 2.0;
            double xn = Width - Right;
            double yn = Height - Margin;
            var maxSteps = (int)Math.Min(15d * yn / xn, (yn - Margin) / (2.5 * th));
            var delta = bounds.Top - bounds.Bottom;
            var stepY = GetGridStep(delta, maxSteps);
            if (stepY * ys > 8f * th && delta < 7 * stepY)
                stepY /= 2f;
            var tol = delta * 0.02;
            var yg = Math.Round(bounds.Bottom / stepY) * stepY;
            if (yg < bounds.Bottom - tol)
                yg += stepY;
            var max = bounds.Top + tol;
            var isScientific = Math.Abs(bounds.Top) + Math.Abs(bounds.Bottom) >= 20000;
            var a = 4f * ScreenScaleFactor;
            var xt = Left - tw / 2f - a / 2f;
            string s;
            canvas.DrawRectangle(Left, Margin, xn - Left, yn - Margin, "PlotFrame");
            while ((yg < max) == (stepY > 0))
            {
                var y = y0 - yg * ys;
                if (Math.Abs(y) > 1e8)
                    break;

                if (yg >= bounds.Bottom && yg <= bounds.Top)
                    canvas.DrawLine(Left, y, xn, y, "PlotGrid");

                if (Math.Abs(yg) < stepY * 1e-8)
                    s = "0";
                else if (isScientific)
                    s = yg.ToString("0.##E+0", CultureInfo.InvariantCulture);
                else
                    s = OutputWriter.FormatNumberHelper(yg, 2);

                canvas.DrawText(s, xt, y + th, "end");
                yg += stepY;
            }
            var sx0 = OutputWriter.FormatNumberHelper(bounds.Left, 2);
            var sx1 = OutputWriter.FormatNumberHelper(bounds.Right, 2);
            var n = Math.Max(sx0.Length, sx1.Length) + 2;
            maxSteps = Math.Min(15, (int)((xn - Left) / (tw * n)));
            delta = bounds.Right - bounds.Left;
            var stepX = GetGridStep(delta, maxSteps);
            var midLine = stepX * xs > 1.5 * stepY * ys;
            if (midLine)
                stepX /= 2.0;

            tol = delta * 0.02;
            var xg = Math.Round((bounds.Left - tol) / stepX) * stepX;
            if (xg < bounds.Left - tol)
                xg += stepX;

            max = bounds.Right + tol;
            var yt = yn + 2f * th + a;
            isScientific = Math.Abs(bounds.Right) + Math.Abs(bounds.Left) >= 20000;
            if (midLine)
            {
                n = OutputWriter.FormatNumberHelper(Math.Round(max / stepX) * stepX, 2).Length + 2;
                if (tw * n < stepX * xs)
                    midLine = false;
            }
            var even = true;
            while ((xg < max) == (stepX > 0))
            {
                var x = x0 + xg * xs;
                if (Math.Abs(x) > 1e8)
                    break;

                if (xg >= bounds.Left && xg <= bounds.Right)
                    canvas.DrawLine(x, Margin, x, yn, "PlotGrid");

                if (even)
                {
                    if (Math.Abs(xg) < stepX * 1e-8)
                        s = "0";
                    else if (isScientific)
                        s = xg.ToString("0.##E+0", CultureInfo.InvariantCulture);
                    else
                        s = OutputWriter.FormatNumberHelper(xg, 2);

                    canvas.DrawText(s, x, yt, "middle");
                }
                if (midLine)
                    even = !even;

                xg += stepX;
            }
            if (y0 >= Margin - 0.1 && y0 <= yn + 0.1)
            {
                canvas.DrawLine(Left, y0, xn, y0, "PlotAxis");
                canvas.DrawText("x", xn + tw, y0 - th);
            }
            if (x0 >= Left - 0.1 && x0 <= xn + 0.1)
            {
                canvas.DrawLine(x0, Margin, x0, yn, "PlotAxis");
                canvas.DrawText("y", x0, Margin - 2*th, "middle");
            }
            var sy0 = OutputWriter.FormatNumberHelper(bounds.Bottom, 2);
            var sy1 = OutputWriter.FormatNumberHelper(bounds.Top, 2);
            a /= 2f;
            canvas.DrawText($"[{sx0}; {sy0}]", a, Height - 1d * th - a);
            canvas.DrawText($"[{sx1}; {sy1}]", Width - a, 2d * th + a, "end");
        }

        protected string HtmlImg(string src)
        {
            double w = Math.Round(0.75 * Width / ScreenScaleFactor);
            return $"<img class=\"plot\" src=\"{src}\" alt=\"Plot\" style=\"width:{w}pt;\">";
        }

        protected static string PngToFile(SKBitmap bitmap, string imagePath)
        {
            var fileName = Path.GetRandomFileName();
            fileName = Path.ChangeExtension(fileName, "png");
            var fullPath = imagePath + fileName;
            try
            {
                using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var data = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);
                data.SaveTo(fs);
                return fileName;
            }
            catch
            {
                Throw.ErrorWritingPngFileException(fullPath);
                return null;
            }
        }

        protected static string SvgToFile(SvgDrawing drawing, string imagePath)
        {
            var fileName = Path.GetRandomFileName();
            fileName = Path.ChangeExtension(fileName, "svg");
            var fullPath = imagePath + fileName;
            try
            {
                drawing.Save(fullPath);
                return fileName;
            }
            catch
            {
                Throw.ErrorWritingSvgFileException(fullPath);
                return null;
            }
        }

        protected static string ImageToBase64(SKBitmap bitmap)
        {
            try
            {
                using var ms = new MemoryStream();
                bitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
                var imageBytes = ms.ToArray();
                var b64Str = Convert.ToBase64String(imageBytes);
                return "data:image/png;base64," + b64Str;
            }
            catch
            {
                Throw.ErrorConvertingPngToBase64Exception();
                return null;
            }
        }

        private static double GetGridStep(double span, int maxSteps)
        {
            var a = Math.Abs(span);
            var n = (int)Math.Round(Math.Log10(a));
            var k = Math.Pow(10.0, n);
            var st = a / k;
            var dMin = 10.0;
            if (maxSteps < 2)
                maxSteps = 2;

            if (maxSteps > 15)
                maxSteps = 15;

            var step = 2.5 * Math.Round(4.0 * st / maxSteps);
            for (int i = 0; i < 4; ++i)
            {
                var d = Math.Abs(st - Steps[i]);
                if (d < dMin && Math.Floor(10.0 * st / Steps[i]) <= maxSteps)
                {
                    dMin = d;
                    step = Steps[i];
                }
            }
            return Math.CopySign(step * (k / 10.0), span);
        }

        protected readonly struct Node
        {
            internal readonly double X;
            internal readonly double Y;
            internal readonly double Z;

            internal Node(double x, double y)
            {
                X = x;
                Y = y;
                Z = 0.0;
            }

            internal Node(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        protected class Box
        {
            internal double Left { get; private set; }
            internal double Right { get; private set; }
            internal double Top { get; private set; }
            internal double Bottom { get; private set; }
            internal double Width => Right - Left;
            internal double Height => Top - Bottom;
            internal Box()
            {
                Left = Bottom = double.MaxValue;
                Right = Top = double.MinValue;
            }

            internal Box(double left, double bottom, double right, double top)
            {
                Left = left;
                Bottom = bottom;
                Right = right;
                Top = top;
            }

            internal void Expand(Node node)
            {
                if (node.X < Left) Left = node.X;
                if (node.X > Right) Right = node.X;
                if (node.Y < Bottom) Bottom = node.Y;
                if (node.Y > Top) Top = node.Y;
            }

            internal void Expand(Box box)
            {
                if (box.Left < Left) Left = box.Left;
                if (box.Right > Right) Right = box.Right;
                if (box.Bottom < Bottom) Bottom = box.Bottom;
                if (box.Top > Top) Top = box.Top;
            }
        }
    }
}
