﻿using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Calcpad.Core
{
    internal class MapPlotter : Plotter
    {
        public MapPlotter(MathParser parser, PlotSettings settings) : base(parser, settings) { }

        private int _nx, _ny, size;
        private const int NColors = 12;
        private static readonly Node[] Colors = new Node[NColors];

        private struct Map
        {
            internal Node[,] Points;
            internal Node[,] Vertices;
            private readonly PointF[,] _pngPoints;
            private readonly SvgPoint[,] _svgPoints;
            private readonly int _nx;
            private readonly int _ny;
            internal double Min, Max;

            internal Map(Node[,] points, PlotSettings settings)
            {
                Points = points;
                Vertices = new Node[points.GetLength(0), points.GetLength(1)];
                _nx = points.GetLength(0);
                _ny = points.GetLength(1);
                _pngPoints = new PointF[_nx, _ny];
                _svgPoints = new SvgPoint[_nx, _ny];
                Min = double.MaxValue;
                Max = double.MinValue;
                GetBounds();
                if (settings.Shadows)
                    GetShadows();
            }

            private void GetBounds()
            {
                for (int i = 0; i < _nx; ++i)
                    for (int j = 0; j < _ny; ++j)
                    {
                        var z = Points[i, j].Z;
                        if (!(double.IsNaN(z) || double.IsInfinity(z)))
                        {
                            if (z < Min) Min = z;
                            if (z > Max) Max = z;
                        }
                    }
            }

            private void GetShadows()
            {
                double dMax = 0;
                for (int i = 1; i < _nx; ++i)
                    for (int j = 1; j < _ny; ++j)
                    {
                        var d = Math.Abs(Points[i, j].Z - Points[i - 1, j].Z);
                        if (d > dMax)
                            dMax = d;
                        d = Math.Abs(Points[i, j].Z - Points[i, j - 1].Z);
                        if (d > dMax)
                            dMax = d;
                    }
                int nx = _nx - 1, ny = _ny - 1;
                var dx = Math.Abs(Points[nx, 0].X - Points[0, 0].X) / nx;
                var dy = Math.Abs(Points[0, ny].Y - Points[0, 0].Y) / ny;
                double k = 1;
                if (dMax > 0)
                    k = Math.Sqrt(dx * dx + dy * dy) / (1.618 * dMax);

                for (int i = 0; i < _nx; ++i)
                {
                    var i1 = i == 0 ? 0 : i - 1;
                    var i2 = i == nx ? i : i + 1;
                    dx = k / (Points[i2, 0].X - Points[i1, 0].X);
                    for (int j = 0; j < _ny; ++j)
                    {
                        var j1 = j == 0 ? 0 : j - 1;
                        var j2 = j == ny ? j : j + 1;
                        dy = k / (Points[i, j2].Y - Points[i, j1].Y);
                        var x = (Points[i1, j].Z - Points[i2, j].Z) * dx;
                        var y = (Points[i, j1].Z - Points[i, j2].Z) * dy;
                        Vertices[i, j] = new Node(x, y); 
                    }
                }
            }

            internal void GetPngPoints(double x0, double y0, double xs, double ys)
            {
                for (int i = 0; i < _nx; ++i)
                    for (int j = 0; j < _ny; ++j)
                        _pngPoints[i, j] = new PointF((float)(x0 + Points[i, j].X * xs), (float)(y0 - Points[i, j].Y * ys));
            }

            internal void GetSvgPoints(double x0, double y0, double xs, double ys)
            {
                for (int i = 0; i < _nx; ++i)
                    for (int j = 0; j < _ny; ++j)
                        _svgPoints[i, j] = new SvgPoint((x0 + Points[i, j].X * xs), (y0 - Points[i, j].Y * ys));
            }
        }

        private void GetImageSize()
        {
            var w = (int)(Parser.PlotWidth * ScreenScaleFactor);
            var h = (int)(Parser.PlotHeight * ScreenScaleFactor);
            size = (int)(Parser.PlotStep * ScreenScaleFactor);
            var d = (int)Math.Sqrt(w * w + h * h);
            if (size == 0)
            {
                if (d >= 800)
                    size = 8;
                else if (d >= 400)
                    size = 4;
                else
                    size = 2;
            }

            _nx = w / size;
            _ny = h / size;
            w = _nx * size;
            h = _ny * size;

            Left = (int)(1.5 * Margin);
            Right = (int)(2.5 * Margin);
            Width = w + Left + Right;
            Height = h + 2 * Margin;
        }

        internal string Plot(Func<Value> function, Variable varX, Variable varY, double startX, double endX, double startY, double endY, Unit xUnits, Unit yUnits)
        {
            GetColorScale();
            GetImageSize();
            var m = new Map(Calculate(function, varX, varY, startX, endX, startY, endY, xUnits, yUnits), Settings);

            //Calculates upper and lower bounds along y
            FixBounds(ref m.Min, ref m.Max);
            //Calculates origin and scale to transform to bitmap coordinates
            var xs = (Width - Left - Right) / (endX - startX);
            var x0 = Left - (int)(startX * xs);
            var ys = (Height - 2 * Margin) / (endY - startY);
            var y0 = Height - Margin + (int)(startY * ys);

            var bounds = new Box(startX, startY, endX, endY);
            if (Settings.VectorGraphics)
            {
                m.GetSvgPoints(x0, y0, xs, ys);
                return DrawSvg(m, x0, y0, xs, ys, bounds);
            }
            else
            {
                m.GetPngPoints(x0, y0, xs, ys);
                return DrawPng(m, x0, y0, xs, ys, bounds);
            }
        }

        private Node[,] Calculate(Func<Value> function, Variable varX, Variable varY, double startX, double endX, double startY, double endY, Unit xUnits, Unit yUnits)
        {
            var n = new Node[_nx + 1, _ny + 1];
            var sx = (endX - startX) / _nx;
            var sy = (endY - startY) / _ny;
            var x = startX;
            double d = 1;
            if (yUnits is not null && Unit.IsConsistent(xUnits, yUnits))
                d = yUnits.ConvertTo(xUnits);

            //Unit zUnits = null;
            varX.SetUnits(xUnits);
            varY.SetUnits(yUnits);
            for (int i = 0; i <= _nx; ++i)
            {
                var number = Parser.MakeNumber(x);
                varX.SetNumber(number);
                var y = startY;
                for (int j = 0; j <= _ny; ++j)
                {
                    number = Parser.MakeNumber(y);
                    varY.SetNumber(number);
                    var value = function();
                    Parser.CheckReal(value);
                    var z = value.Number.Re;
                    n[i, j] = new Node(x, y * d, z);
                    y += sy;
                }
                x += sx;
            }
            return n;
        }

        private void GetColor(out byte red, out byte green, out byte blue, double value, double lightness)
        {
            var k = Settings.Shadows ? 85.0 + 170.0 * lightness : 255.0;
            if (Settings.SmoothScale)
            {
                GetRgb(out var dR, out var dG, out var dB, value);
                if (Settings.ColorScale == PlotSettings.ColorScales.Gray)
                    red = green = blue = (byte)dR;
                else
                {
                    red = (byte)(k * dR);
                    green = (byte)(k * dG);
                    blue = (byte)(k * dB);
                }
            }
            else
            {
                var i = (int)Math.Floor(value * NColors);
                if (i >= NColors)
                    i = NColors - 1;
                if (Settings.ColorScale == PlotSettings.ColorScales.Gray)
                    red = green = blue = (byte)Colors[i].X;
                else if (Settings.ColorScale == PlotSettings.ColorScales.None)
                    red = green = blue = (byte)k;
                else
                {
                    red = (byte)(k * Colors[i].X);
                    green = (byte)(k * Colors[i].Y);
                    blue = (byte)(k * Colors[i].Z);
                }
            }
        }

        private void GetColorScale()
        {
            for (int i = 0; i < NColors; ++i)
            {
                var value = (double)i / (NColors - 1);
                GetRgb(out var red, out var green, out var blue, value);
                Colors[i]= new Node( red, green, blue);
            }
        }

        private void GetRgb(out double red, out double green, out double blue, double value)
        {
            switch (Settings.ColorScale)
            {
                case PlotSettings.ColorScales.Gray:
                    red = green = blue = 85 + 170.0 * value;
                    break;
                case PlotSettings.ColorScales.Rainbow:
                    value *= 4;
                    var n = (int)Math.Floor(value);
                    value -= n;
                    switch (n)
                    {
                        case 0:
                            red = 0;
                            green = value;
                            blue = 1;
                            break;
                        case 1:
                            red = 0;
                            green = 1;
                            blue = 1 - value;
                            break;
                        case 2:
                            red = value;
                            green = 1;
                            blue = 0;
                            break;
                        case 3:
                            red = 1;
                            green = 1 - value;
                            blue = 0;
                            break;
                        default:
                            red = 1 - 0.5 * value;
                            green = blue = 0;
                            break;
                    }
                    break;
                case PlotSettings.ColorScales.Terrain:
                    value *= 3;
                    n = (int)Math.Floor(value);
                    value -= n;
                    switch (n)
                    {
                        case 0:
                            red = 0;
                            green = 0.5 * (1 + value);
                            blue = 0.5 * (1 - value);
                            break;
                        case 1:
                            red = value;
                            green = 1;
                            blue = 0;
                            break;
                        case 2:
                            red = 1;
                            green = 1 - 0.5 * value;
                            blue = 0;
                            break;
                        default:
                            red = 1;
                            green = 0.5 * (1 - value);
                            blue = 0;
                            break;
                    }
                    break;
                case PlotSettings.ColorScales.VioletToYellow:
                    red = 0.6 + 0.4 * value;
                    green = 0.2 + 0.8 * value;
                    blue = 0.8 - 0.8 * value;
                    break;
                case PlotSettings.ColorScales.GreenToYellow:
                    red = value;
                    green = 0.4 + 0.6 * value;
                    blue = 0.4 - 0.4 * value;
                    break;
                case PlotSettings.ColorScales.Blues:
                    red = 0.2 + 0.2 * value;
                    green = 0.2 + 0.8 * value;
                    blue = 0.8 + 0.2 * value;
                    break;
                default:
                    red = green = blue = 1;
                    break;
            }
        }

        private void DrawColorScalePng(Graphics g, Map m)
        {
            var x0 = Width - Right + Margin / 2;
            var y0 = Height - Margin;
            int w = Margin / 4;
            var h = Height - 2 * Margin;
            var n = NColors;
            if (Settings.SmoothScale)
                n = h / size + 1;
            var dh = (float)h / n;
            for (int i = 0; i < n; ++i)
            {
                GetColor(out var red, out var green, out var blue, (i + 0.5) / n, 1.0);
                Brush b = new SolidBrush(Color.FromArgb(255, red, green, blue));
                g.FillRectangle(b, x0, y0 - i * dh - dh, w, dh);
                b.Dispose();
            }
            g.DrawRectangle(Pens.Black, x0, Margin, w, h);
            var gridPen = new Pen(Color.FromArgb(48, 0, 0, 0));
            var f = new Font("Arial", 8);
            var sz = g.MeasureString("0", f);
            var th = sz.Height / 2;
            var dy = (m.Max - m.Min) / NColors;
            dh = (float)h / NColors;
            for (int i = 0; i <= NColors; ++i)
            {
                var y = y0 - i * dh;
                g.DrawLine(gridPen, x0, y, x0 + w, y);
                var s = OutputWriter.FormatNumberHelper(m.Min + i * dy, 2);
                g.DrawString(s, f, Brushes.Black, x0 + w + 5, y - th);
            }
            gridPen.Dispose();
            f.Dispose();
        }

        private void DrawColorScaleSvg(SvgDrawing g, Map m)
        {
            var x0 = Width - Right + 30;
            var y0 = Height - Margin;
            const int w = 15;
            var h = Height - 2 * Margin;
            var n = NColors;
            if (Settings.SmoothScale)
                n = h / size + 1;
            var dh = (double)h / n;
            const double th = 11, th05 = th / 2;
            for (int i = 0; i < n; ++i)
            {
                GetColor(out var red, out var green, out var blue, (i + 0.5) / n, 1.0);
                g.FillRectangle(x0, y0 - i * dh - dh, w, dh, $"#{red:x2}{green:x2}{blue:x2}");
            }
            g.DrawRectangle(x0, Margin, w, h, "PlotAxis");

            var dy = (m.Max - m.Min) / NColors;
            dh = (double)h / NColors;
            for (int i = 0; i <= NColors; ++i)
            {
                var y = y0 - i * dh;
                g.DrawLine(x0, y, x0 + w, y, "PlotGrid");
                var s = OutputWriter.FormatNumberHelper(m.Min + i * dy, 2);
                g.DrawText(s, x0 + w + 5, y + th05);
            }
        }

        private double[,,] Interpolate(Map m)
        {
            var delta = m.Max - m.Min;
            if (delta == 0.0)
                delta = 1.0;
            else
                delta = 1.0 / delta;
            var factor = 1.0 / size;
            int nxs = _nx * size, nys = _ny * size;
            var d = new double[nxs + 1, nys + 1, 3];
            int i0 = 0, i1 = 0;
            for (int i = 0; i <= nxs; i += size)
            {
                int j0 = 0, j1 = 0;
                for (int j = 0; j <= nys; j += size)
                {
                    d[i, j, 0] = (m.Points[i1, j1].Z - m.Min) * delta;
                    d[i, j, 1] = m.Vertices[i1, j1].X;
                    d[i, j, 2] = m.Vertices[i1, j1].Y;
                    if (j > 0 && size > 1)
                    {
                        var d0 = d[i, j0, 0];
                        var d1 = (d[i, j, 0] - d0) * factor;
                        for (int k = 1; k < size; k++)
                        {
                            d0 += d1;
                            d[i, j0 + k, 0] = d0;
                        }
                        if (Settings.Shadows)
                        {
                            for (int p = 1; p <= 2; p++)
                            {
                                d0 = d[i, j0, p];
                                d1 = (d[i, j, p] - d0) * factor;
                                for (int k = 1; k < size; k++)
                                {
                                    d0 += d1;
                                    d[i, j0 + k, p] = d0;
                                }
                            }
                        }
                    }
                    j0 = j;
                    j1++;
                }
                if (i > 0 && size > 1)
                {
                    for (int j = 0; j < nys; ++j)
                    {
                        var d0 = d[i0, j, 0];
                        var d1 = (d0 - d[i, j, 0]) * factor;
                        for (int k = 1; k < size; k++)
                        {
                            d0 -= d1;
                            d[i0 + k, j, 0] = d0;
                        }

                        if (Settings.Shadows)
                        {
                            for (int p = 1; p <= 2; p++)
                            {
                                d0 = d[i0, j, p];
                                d1 = (d0 - d[i, j, p]) * factor;
                                for (int k = 1; k < size; k++)
                                {
                                    d0 -= d1;
                                    d[i0 + k, j, p] = d0;
                                }
                            }
                        }
                    }
                }
                i0 = i;
                i1++;
            }
            return d;
        }

        private void SetBitmapBits(Bitmap canvas, double[,,] values)
        {
            Node light = new(), spec = new();
            double length, ks = 0.5;
            if (Settings.Shadows)
            {
                const double sqr2 = 0.70710678118654752440084436210485;
                const double sqr3 = 0.57735026918962576450914878050196;
                if (Settings.ColorScale <= PlotSettings.ColorScales.Terrain)
                    ks = 0.7;

                switch (Settings.LightDirection)
                {
                    case PlotSettings.LightDirections.North:     light = new(0, sqr2 ,sqr2); break;
                    case PlotSettings.LightDirections.East:      light = new(sqr2, 0, sqr2); break;
                    case PlotSettings.LightDirections.South:     light = new(0, -sqr2, sqr2); break;
                    case PlotSettings.LightDirections.West:      light = new(-sqr2, 0, sqr2); break;
                    case PlotSettings.LightDirections.NorthEast: light = new(sqr3, sqr3, sqr3); break;
                    case PlotSettings.LightDirections.SouthEast: light = new(sqr3, -sqr3, sqr3); break;
                    case PlotSettings.LightDirections.SouthWest: light = new(-sqr3, -sqr3, sqr3); break;
                    case PlotSettings.LightDirections.NorthWest: light = new(-sqr3, sqr3, sqr3); break;
                }
                var z = light.Z + 1;
                length = Math.Sqrt(light.X * light.X + light.Y * light.Y + z * z);
                spec = new(light.X / length, light.Y / length, z / length);
            }
            var w = canvas.Width;
            var h = canvas.Height;
            var nx = values.GetLength(0) - 1;
            var ny = values.GetLength(1) - 1;
            var mt = (h - ny + 1) / 2;
            var ml = (int)(mt * (double)Left / Margin);
            var r = new Rectangle(0, 0, w, h);
            var bmpData = canvas.LockBits(r, ImageLockMode.ReadOnly, canvas.PixelFormat);
            var n = 4 * w * h;
            var b = new byte[n];
            for (int i = 0; i < ny; ++i)
            {
                var iStr = (mt + i) * w + ml;
                for (int j = 0; j < nx; ++j)
                {
                    var n1 = ny - i - 1;
                    var d = values[j, n1, 0];
                    if (!double.IsInfinity(d) && !double.IsNaN(d))
                    {
                        double p = 0.0, s = 0.0;
                        if (Settings.Shadows)
                        {
                            var x = values[j, n1, 1];
                            var y = values[j, n1, 2];
                            length = Math.Sqrt(x * x + y * y + 1);
                            p = (x * light.X + y * light.Y + light.Z) / length;
                            if (Settings.ColorScale > PlotSettings.ColorScales.Gray)
                            {
                                length = (x * spec.X + y * spec.Y + spec.Z) / length;
                                if (Math.Abs(length) > 0.98)
                                    s = Math.Pow(length, 200) * ks;
                            }
                        }
                        GetColor(out var red, out var green, out var blue, values[j, n1, 0], p);
                        var jStr = 4 * (iStr + j);
                        b[jStr + 3] = (byte)(255 * (1 - s));
                        b[jStr + 2] = red;
                        b[jStr + 1] = green;
                        b[jStr] = blue;
                    }
                }
            }
            System.Runtime.InteropServices.Marshal.Copy(b, 0, bmpData.Scan0, n);
            canvas.UnlockBits(bmpData);
        }

        private string DrawPng(Map m, int x0, int y0, double xs, double ys, Box bounds)
        {
            var values = Interpolate(m);
            var canvas = new Bitmap(Width, Height);
            SetBitmapBits(canvas, values);
            var g = Graphics.FromImage(canvas);
            DrawGridPng(g, x0, y0, xs, ys, bounds);
            DrawColorScalePng(g, m);
            string src;
            if (string.IsNullOrEmpty(Settings.ImagePath))
                src = ImageToBase64(canvas);
            else
                src = Settings.ImageUri + PngToFile(canvas, Settings.ImagePath);

            g.Dispose();
            canvas.Dispose();
            return HtmlImg(src);
        }

        private string DrawSvg(Map m, double x0, double y0, double xs, double ys, Box bounds)
        {
            var g = new SvgDrawing(Width, Height);
            var values = Interpolate(m);
            var w = values.GetLength(0) - 1;
            var h = values.GetLength(1) - 1;
            string src;
            using (var canvas = new Bitmap(w, h))
            {
                SetBitmapBits(canvas, values);
                src = ImageToBase64(canvas);
            }
            g.DrawImage(Left, Margin, w, h, src);
            DrawGridSvg(g, x0, y0, xs, ys, bounds);
            DrawColorScaleSvg(g, m);
            if (string.IsNullOrEmpty(Settings.ImagePath))
            {
                double d = 0.75 / ScreenScaleFactor;
                double dw = Math.Round(d * Width);
                double dh = Math.Round(d * Height);
                return $"<div class=\"plot\" style=\"width:{dw}pt; height:{dh}pt;\">{g}</div>";
            }
            src = Settings.ImageUri + SvgToFile(g, Settings.ImagePath);
            return HtmlImg(src);
        }  
    }
}
