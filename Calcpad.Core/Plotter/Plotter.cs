using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;

namespace Calcpad.Core
{
    internal abstract class Plotter

    {
        private static readonly double[] Steps = { 1.0, 2.0, 2.5, 5.0 };
        protected readonly double ScreenScaleFactor;
        protected int Width = 500;
        protected int Height = 350;
        protected readonly int Margin = 30;
        protected int Left = 30;
        protected int Right = 30;
        protected PlotSettings Settings;
        protected MathParser Parser;

        protected Plotter(MathParser parser, PlotSettings settings)
        {
            Settings = settings;
            Parser = parser;
            ScreenScaleFactor = settings.ScreenScaleFactor;
            Margin = (int)(40 * ScreenScaleFactor);
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

            var mid = (min + max) / 2;
            var span = Math.Pow(10, Math.Round(Math.Log10(Math.Abs(mid))) - 2);
            if (span != 0)
                mid = Math.Round(min / span) * span;

            if (d < span)
                d = span;

            min = mid - d;
            max = mid + d;
        }

        protected void DrawGridPng(Graphics g, double x0, double y0, double xs, double ys, Box bounds)
        {
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            var penWidth = (float)ScreenScaleFactor;
            var gridPen = new Pen(Color.FromArgb(48, 0, 0, 0), penWidth);
            var axisPen = new Pen(Color.Black, penWidth);
            var b = Brushes.Black;
            var fh = 8.5f;
            var f = new Font("Arial", fh);
            var sz = g.MeasureString(" -0.12 ", f);
            var tw = sz.Width / 5;
            var th = sz.Height / 2 - 2;
            var th05 = th / 2;
            float xn = Width - Right;
            float yn = Height - Margin;
            var maxSteps = (int)Math.Min(15d * yn / xn, (yn - Margin) / (2.5 * th));
            var delta = bounds.Top - bounds.Bottom;
            var stepY = GetGridStep(delta, maxSteps);
            if (stepY * ys > 8 * th && delta < 7 * stepY)
                stepY /= 2;
            var tol = delta * 0.02;
            var yg = Math.Round(bounds.Bottom / stepY) * stepY;
            if (yg < bounds.Bottom - tol)
                yg += stepY;
            var max = bounds.Top + tol;
            var isScientific = Math.Abs(bounds.Top) + Math.Abs(bounds.Bottom) >= 20000;
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Far
            };
            var xt = Left - tw / 2;
            string s;
            while ((yg < max) == (stepY > 0))
            {
                var y = (float)(y0 - yg * ys);
                if (Math.Abs(y) > 1e8)
                    break;

                if (yg >= bounds.Bottom && yg <= bounds.Top)
                    g.DrawLine(gridPen, Left, y, xn, y);
                if (Math.Abs(yg) < stepY * 1e-8)
                    s = "0";
                else if (isScientific)
                    s = yg.ToString("0.##E+0", CultureInfo.InvariantCulture);
                else
                    s = OutputWriter.FormatNumberHelper(yg, 2);
                g.DrawString(s, f, b, xt, y - th, sf);
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
                stepX /= 2;
            tol = delta * 0.02;
            var xg = Math.Round((bounds.Left - tol) / stepX) * stepX;
            if (xg < bounds.Left - tol)
                xg += stepX;
            max = bounds.Right + tol;
            var yt = yn + th05 + 4;
            isScientific = Math.Abs(bounds.Right) + Math.Abs(bounds.Left) >= 20000;
            if (midLine)
            {
                n = OutputWriter.FormatNumberHelper(Math.Round(max / stepX) * stepX, 2).Length + 2;
                if (tw * n < stepX * xs)
                    midLine = false;
            }
            sf.Alignment = StringAlignment.Center;
            var even = true;
            while ((xg < max) == (stepX > 0))
            {
                var x = (float)(x0 + xg * xs);
                if (Math.Abs(x) > 1e8)
                    break;

                if (xg >= bounds.Left && xg <= bounds.Right)
                    g.DrawLine(gridPen, x, Margin, x, yn);
                if (even)
                {
                    if (Math.Abs(xg) < stepX * 1e-8)
                        s = "0";
                    else if (isScientific)
                        s = xg.ToString("0.##E+0", CultureInfo.InvariantCulture);
                    else
                        s = OutputWriter.FormatNumberHelper(xg, 2);
                    g.DrawString(s, f, b, x, yt, sf);
                }
                if (midLine)
                    even = !even;
                xg += stepX;
            }
            g.DrawRectangle(axisPen, Left, Margin, xn - Left, yn - Margin);
            if (y0 >= Margin && y0 <= yn)
            {
                g.DrawLine(axisPen, Left, (float)y0, xn, (float)y0);
                g.DrawString("x", f, b, xn + tw, (float)y0 - th);
            }
            if (x0 >= Left && x0 <= xn)
            {
                g.DrawLine(axisPen, (float)x0, Margin, (float)x0, yn);
                g.DrawString("y", f, b, (float)x0 - tw / 2, Margin - 3 * th);
            }
            var sy0 = OutputWriter.FormatNumberHelper(bounds.Bottom, 2);
            var sy1 = OutputWriter.FormatNumberHelper(bounds.Top, 2);
            sf.Alignment = StringAlignment.Near;
            g.DrawString($"[{sx0}; {sy0}]", f, b, 5, Height - 3f * th, sf);
            sf.Alignment = StringAlignment.Far;
            g.DrawString($"[{sx1}; {sy1}]", f, b, Width - 5, th, sf);
            axisPen.Dispose();
            gridPen.Dispose();
            f.Dispose();
        }

        protected void DrawGridSvg(SvgDrawing g, double x0, double y0, double xs, double ys, Box bounds)
        {
            const double th = 12;
            const double th05 = th / 2;
            const double tw = 6;
            double xn = Width - Right;
            double yn = Height - Margin;
            var maxSteps = (int)Math.Min(12.0 * yn / xn, (yn - Margin) / (2.5 * th));
            var delta = bounds.Top - bounds.Bottom;
            var stepY = GetGridStep(delta, maxSteps);
            var tol = delta * 0.01;
            var yg = Math.Round((bounds.Bottom - tol) / stepY) * stepY;
            var max = bounds.Top + tol;
            var xt = Margin - tw;
            while ((yg < max) == (stepY > 0))
            {
                var y = y0 - yg * ys;
                g.DrawLine(Margin, y, xn, y, "PlotGrid");
                var s = OutputWriter.FormatNumberHelper(yg, 2);
                g.DrawText(s, xt, y + th05, "end");
                yg += stepY;
            }
            var sx0 = OutputWriter.FormatNumberHelper(bounds.Left, 2);
            var sx1 = OutputWriter.FormatNumberHelper(bounds.Right, 2);
            var n = Math.Max(sx0.Length, sx1.Length) + 2;
            maxSteps = Math.Min(12, (int)((xn - Margin) / (tw * n)));
            delta = bounds.Right - bounds.Left;
            var stepX = GetGridStep(delta, maxSteps);
            var midLine = stepX * xs > 1.5 * stepY * ys;
            if (midLine)
                stepX /= 2;
            tol = delta * 0.01;
            var xg = Math.Round((bounds.Left - tol) / stepX) * stepX;
            max = bounds.Right + tol;
            var yt = yn + 1.5 * th;
            var even = true;
            while ((xg < max) == (stepX > 0))
            {
                double x = (float)(x0 + xg * xs);
                g.DrawLine(x, Margin, x, yn, "PlotGrid");
                if (even)
                {
                    var s = OutputWriter.FormatNumberHelper(xg, 2);
                    g.DrawText(s, x, yt, "middle");
                }
                if (midLine)
                    even = !even;
                xg += stepX;
            }
            g.DrawRectangle(Margin, Margin, xn - Margin, yn - Margin, "PlotAxis");
            if (y0 >= Margin && y0 <= yn)
            {
                g.DrawLine(Margin, y0, xn, y0, "PlotAxis");
                g.DrawText("x", xn + 2 * tw, y0 + th05);
            }
            if (x0 >= Margin && x0 <= xn)
            {
                g.DrawLine(x0, Margin, x0, yn, "PlotAxis");
                g.DrawText("y", x0 - tw, Margin - th);
            }
            var sy0 = OutputWriter.FormatNumberHelper(bounds.Bottom, 2);
            var sy1 = OutputWriter.FormatNumberHelper(bounds.Top, 2);
            g.DrawText($"({sx0}; {sy0})", 5, Height - th);
            g.DrawText($"({sx1}; {sy1})", Width - 2.5 * tw, 1.5f * th, "end");
        }

        protected string HtmlImg(string src)
        {
            double w = Math.Round(0.75 * Width / ScreenScaleFactor);
            return $"<img class=\"plot\" src=\"{src}\" alt=\"Plot\" style=\"width:{w}pt;\">";
        }

        protected static string PngToFile(Bitmap canvas, string imagePath)
        {
            var fileName = Path.GetRandomFileName();
            fileName = Path.ChangeExtension(fileName, "png");
            var fullPath = imagePath + fileName;
            try
            {
                canvas.Save(fullPath, ImageFormat.Png);
                return fileName;
            }
            catch
            {
#if BG
                throw new MathParser.MathParserException($"Грешка при запис на png файл като \"{fullPath}\".");
#else
                throw new MathParser.MathParserException($"Error writing a png file to \"{fullPath}\".");
#endif
            }
        }

        protected static string SvgToFile(SvgDrawing canvas, string imagePath)
        {
            var fileName = Path.GetRandomFileName();
            fileName = Path.ChangeExtension(fileName, "svg");
            var fullPath = imagePath + fileName;
            try
            {
                canvas.Save(fullPath);
                return fileName;
            }
            catch
            {
#if BG
                throw new MathParser.MathParserException($"Грешка при запис на svg файл като  \"{fullPath}\".");
#else
                throw new MathParser.MathParserException($"Error writing a svg file to \"{fullPath}\".");
#endif
            }
        }

        protected static string ImageToBase64(Bitmap canvas)
        {
            try
            {
                using var ms = new MemoryStream();
                canvas.Save(ms, ImageFormat.Png);
                var imageBytes = ms.ToArray();
                var b64Str = Convert.ToBase64String(imageBytes);
                return "data:image/png;base64," + b64Str;
            }
            catch
            {
#if BG
                throw new MathParser.MathParserException("Грешка при конвертиране на png към Base64.");
#else
                throw new MathParser.MathParserException("Error converting png to Base64.");
#endif
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
            return step * (k / 10.0) * Math.Sign(span);
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
