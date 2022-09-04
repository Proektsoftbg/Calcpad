﻿using System;
using static Calcpad.Core.MathParser;

namespace Calcpad.Core
{
    internal enum QuadratureMethods
    {
        AdaptiveLobatto,
        TanhSinh
    }

    internal class Solver
    {
        private const double Limits = 1E6;
        internal double Precision = 1E-14;
        internal QuadratureMethods QuadratureMethod = QuadratureMethods.AdaptiveLobatto;
        internal Unit Units;
        internal Func<Value> Function;
        public Variable Variable;
        private const int n = 11;
        private static readonly int[] m = { 6, 7, 13, 26, 53, 106, 212, 423, 846, 1693, 3385 };
        private static readonly double[][] r = new double[n][];
        private static readonly double[][] w = new double[n][];

        static Solver()
        {
            GetTanhSinhAbscissasAndWeights();
        }

        private static void GetTanhSinhAbscissasAndWeights()
        {
            double t;
            var h = 2d;
            for (int i = 0; i < n; ++i)
            {
                h /= 2d;
                var eh = Math.Exp(h);
                t = eh;
                r[i] = new double[m[i]];
                w[i] = new double[m[i]];
                if (i > 0)
                    eh *= eh;

                for (int j = 0; j < m[i]; ++j)
                {
                    var u = Math.Exp(1d/t - t);
                    var d = 2d * u / (1d + u);
                    r[i][j] = d;
                    w[i][j] = (1d/t + t) * d / (1d + u);
                    t *= eh;
                }
            }
        }

        private double Fd(double x)
        {
            Variable.SetNumber(x);
            var value = Function();
            var result = value.Number;
            Units ??= value.Units;

            if (!result.IsReal)
#if BG
                throw new MathParserException($"Не мога да изчисля функцията %F за %V = {x}.");
#else
                throw new MathParserException($"Cannot evaluate the function %F for %V = {x}.");
#endif

            var d = result.Re;
            if (double.IsNaN(d) || double.IsInfinity(d))
#if BG
                throw new MathParserException($"Функцията %F не е дефинирана за %V = {x}.");
#else
                throw new MathParserException($"The function %F is not defined for %V = {x}.");
#endif
            return d;
        }

        private Complex Fc(Complex x)
        {
            Variable.SetNumber(x);
            var value = Function();
            var result = value.Number;
            Units ??= value.Units;

            if (double.IsNaN(result.Re) && double.IsNaN(result.Im))
#if BG      
                throw new MathParserException($"Не мога да изчисля функцията %F за %V = {x}.");
#else       
                throw new MathParserException($"Cannot evaluate the function %F for %V = {x}.");
#endif
            return result;
        }

        internal double ModAB(double left, double right, double target, out double err)
        {
            err = 0;
            var u = Variable.Value.Units;
            double x1 = Math.Min(left, right), y1 = Fd(x1) - target;
            if (Math.Abs(y1) <= Precision)
            {
                Units = u;
                return x1;
            }
            double x2 = Math.Max(left, right), y2 = Fd(x2) - target;
            if (Math.Abs(y2) <= Precision)
            {
                Units = u;
                return x2;
            }
            var nMax = -(int)(Math.Log2(Precision) / 2.0) + 1;
            double eps1 = Precision / 4, eps = Precision * (x2 - x1) / 2.0;
            if (Math.Abs(target) > 1)
                eps1 *= target;

            var side = 0;
            var ans = x1;
            var bisection = true;
            const double k = 0.25;
            const int n = 100;
            for (int i = 1; i <= n; ++i)
            {
                double x3, y3;
                if (bisection)
                {
                    x3 = (x1 + x2) / 2.0;
                    y3 = Fd(x3) - target;
                    var ym = (y1 + y2) / 2.0;
                    //Check if function is close to straight line
                    if (Math.Abs(ym - y3) < k * (Math.Abs(y3) + Math.Abs(ym)))
                        bisection = false;
                }
                else
                {
                    x3 = (x1 * y2 - y1 * x2) / (y2 - y1);
                    if (x3 < x1 - eps || x3 > x2 + eps)
                    {
                        err = 1;
                        Units = u;
                        return double.NaN;
                    }
                    y3 = Fd(x3) - target;
                }
                err = Math.Abs(y3);
                if (err < eps1 || Math.Abs(x3 - ans) < eps)
                {
                    Units = u;
                    return x3;
                }
                ans = x3;
                if (Math.Sign(y1) == Math.Sign(y3))
                {
                    if (side == 1)
                    {
                        var m = 1 - y3 / y1;
                        if (m <= 0)
                            y2 /= 2;
                        else
                            y2 *= m;
                    }
                    if (!bisection)
                        side = 1;

                    x1 = x3;
                    y1 = y3;
                }
                else
                {
                    if (side == -1)
                    {
                        var m = 1 - y3 / y2;
                        if (m <= 0)
                            y1 /= 2;
                        else
                            y1 *= m;
                    }
                    if (!bisection)
                        side = -1;

                    x2 = x3;
                    y2 = y3;
                }
                if (i % nMax == 0)
                    bisection = true;
            }
            Units = u;
            return ans;
        }

        internal double Root(double left, double right, double target)
        {
            Units = null;
            var x = ModAB(left, right, target, out var err);
            var eps = Math.Sqrt(Precision);
            if (target != 0)
                eps *= Math.Abs(target);

            if (err > eps)
                return double.NaN;

            return x;
        }

        internal double Find(double left, double right)
        {
            Units = null;
            return ModAB(left, right, 0.0, out _);
        }

        internal double Sup(double left, double right) => Extremum(left, right, false);

        internal double Inf(double left, double right) => Extremum(left, right, true);

        private double Extremum(double left, double right, bool isMin)
        {
            const double k = 0.618033988749897;
            var x1 = left;
            var x2 = right;
            var x3 = x2 - k * (x2 - x1);
            var x4 = x1 + k * (x2 - x1);
            var eps = Precision * (Math.Abs(x3) + Math.Abs(x4)) / 2.0;
            var tol2 = Precision * Precision;
            Units = null;
            while (Math.Abs(x2 - x1) > eps)
            {
                if (isMin == Fd(x3) < Fd(x4))
                {
                    x2 = x4;
                    x4 = x3;
                    x3 = x2 - k * (x2 - x1);
                }
                else
                {
                    x1 = x3;
                    x3 = x4;
                    x4 = x1 + k * (x2 - x1);
                }
                eps = Precision * (Math.Abs(x3) + Math.Abs(x4));
                if (eps < tol2)
                    eps = tol2;
            }
            return Fd((x1 + x2) / 2.0);
        }

        internal double Area(double left, double right)
        {
            double area;
            Units = null;
            if (Math.Abs(right - left) > 1e-14 * (Math.Abs(left) + Math.Abs(right)))
            {
                if (QuadratureMethod == QuadratureMethods.AdaptiveLobatto)
                    area = AdaptiveLobatto(left, right);
                else
                    area = TanhSinh(left, right);
            }
            else
            {
                var y = Fd((left + right) / 2.0);
                area = (right - left) * y;
            }
            var u = Variable.Value.Units;
            if (u is null)
                return area;

            if (Units is null)
            {
                Units = u;
                return area;
            }
            double factor = Unit.GetProductOrDivisionFactor(Units, u);
            Units *= u;
            return area * factor;
        }

        private double eps = 1e-14;
        private double AdaptiveLobatto(double left, double right)
        {
            Units = null;
            eps = Math.Max(Precision, 1e-14) / 2d; //Integration must be slightly more precise than differentiation, when used together
            return Lobatto(left, right, Fd(left), Fd(right), 1);
        }

        private readonly double alpha = Math.Sqrt(2.0 / 3.0);
        private readonly double beta = 1.0 / Math.Sqrt(5.0);
        private double Lobatto(double x1, double x3, double y1, double y3, int depth)
        {
            const double k1 = 1.0 / 1470.0;
            const double k2 = 1.0 / 6.0;
            var h = (x3 - x1) / 2.0;
            var x2 = (x1 + x3) / 2.0;
            var ah = alpha * h;
            var bh = beta * h;
            var x4 = x2 - ah;
            var x5 = x2 - bh;
            var x6 = x2 + bh;
            var x7 = x2 + ah;

            var y4 = Fd(x4);
            var y5 = Fd(x5);
            var y2 = Fd(x2);
            var y6 = Fd(x6);
            var y7 = Fd(x7);

            var a1 = h * k1 * (77.0 * (y1 + y3) + 432.0 * (y4 + y7) + 625.0 * (y5 + y6) + 672.0 * y2);
            var a2 = h * k2 * (y1 + y3 + 5.0 * (y5 + y6));
            
            if (depth == 1)
            {
                if (double.IsFinite(a1) && a1 > 1)
                    eps *= a1;
            }
            else if(Math.Abs(a1 - a2) < eps || depth > 15)
                return a1;

            depth++;
            return Lobatto(x1, x4, y1, y4, depth) +
                   Lobatto(x4, x5, y4, y5, depth) +
                   Lobatto(x5, x2, y5, y2, depth) +
                   Lobatto(x2, x6, y2, y6, depth) +
                   Lobatto(x6, x7, y6, y7, depth) +
                   Lobatto(x7, x3, y7, y3, depth);
        }

        private double TanhSinh(double left, double right)
        {
            var c = (left + right) / 2d;
            var d = (right - left) / 2d;
            var s = Fd(c);
            double err;
            var i = 0;
            eps = Math.Clamp(Precision * 0.1, 1e-15, 1e-8);
            var tol = 10.0 * Precision;
            do
            {
                double q, p = 0d, fp = 0d, fm = 0d;
                int j = 0;
                do
                {
                    var x = r[i][j] * d;
                    if (left + x > left)
                    {
                        var y = Fd(left + x);
                        if (double.IsFinite(y))
                            fp = y;
                    }
                    if (right - x < right)
                    {
                        var y = Fd(right - x);
                        if (double.IsFinite(y))
                            fm = y;
                    }
                    q = w[i][j] * (fp + fm);
                    p += q;
                    ++j;
                } while (Math.Abs(q) > eps * Math.Abs(p) && j < m[i]);
                err = 2d * s;
                s += p;
                err = Math.Abs(err - s);
                ++i;
            } while (err > tol * Math.Abs(s) && i < n);
            if (Math.Abs(s) > 1d)
                err /= Math.Abs(s);

            if (err > 10d * tol)
                return double.NaN;

            return d * s * Math.Pow(2d, 1d - i);
        }

        internal double Slope(double x) //Richardson extrapolation on a 2 node stencil
        {
            double delta = Math.Min(Math.Sqrt(Precision), 1e-3);
            double maxErr = Math.Max(50 * Precision, 1e-3);
            const int n = 7;
            var a = Math.Abs(x) < 1 ? 1 : x;
            var eps = Math.Cbrt(Math.BitIncrement(a) - a);
            var h = Math.Pow(2.0, n) * eps;
            var h2 = 2.0 * h;
            var r = new double[n];
            var err = delta / 2.0;
            Units = null;
            for (int i = 0; i < n; ++i)
            {
                var x1 = x - h;
                var x2 = x1 + h2;
                var d = 1.0;
                var r0 = r[0];
                r[i] = (Fd(x2) - Fd(x1)) / h2;
                for (int k = i - 1; k >= 0; --k)
                {
                    d *= 4.0;
                    var k1 = k + 1;
                    r[k] = r[k1] + (r[k1] - r[k]) / (d - 1);
                }
                if (i >= 1)
                {
                    err = Math.Abs(r[0]) <= delta ?
                          Math.Abs(r[0] - r0) :
                          Math.Abs((r[0] - r0) / r[0]);

                    if (err < delta)
                        break;
                }
                h2 = h;
                h = h2 / 2.0;
            }
            var u = Variable.Value.Units;
            double slope = err > maxErr ? double.NaN : r[0];
            if (u is null)
                return slope;

            if (Units is null)
            {
                Units = u;
                return slope;
            }
            double factor = Unit.GetProductOrDivisionFactor(Units, u, true);
            Units /= u;
            return slope * factor;
        }

        internal Complex Repeat(double start, double end)
        {
            GetBounds(start, end, out var n1, out var n2);
            var number = new Complex(0.0);
            Units = null;
            for (int i = n1; i <= n2; ++i)
            {
                number = Fc(i);
                if (IsInfinity(number))
                    break;
            }
            return number;
        }

        internal Complex Sum(double start, double end)
        {
            GetBounds(start, end, out var n1, out var n2);
            var number = Fc(n1);
            var units = Units;
            var hasUnits = units is not null;
            n1++;
            Units = null;
            for (int i = n1; i <= n2; ++i)
            {
                number += Fc(i);
                if (hasUnits && !Unit.IsConsistent(units, Units))
#if BG
                    throw new MathParserException($"Несъвместими мерни единици в $Sum: \"{Unit.GetText(units)}\" и \"{Unit.GetText(Units)}\".");
#else
                    throw new MathParserException($"Inconsistent units in $Sum: \"{Unit.GetText(units)}\" and \"{Unit.GetText(Units)}\".");
#endif
                if (IsInfinity(number))
                    break;
            }
            Units = units;
            return number;
        }

        internal Complex Product(double start, double end)
        {
            GetBounds(start, end, out var n1, out var n2);
            var number = Fc(n1);
            var units = Units;
            var hasUnits = units is not null;
            n1++;
            Units = null;
            for (int i = n1; i <= n2; ++i)
            {
                number *= Fc(i);
                if (hasUnits && Units is not null)
                    units *= Units;

                if (IsInfinity(number))
                    break;
            }
            Units = units;
            return number;
        }

        private static void GetBounds(double start, double end, out int n1, out int n2)
        {
            if (Math.Abs(start) > Limits || Math.Abs(end) > Limits)
#if BG
                throw new MathParserException($"Ограничението за брой итерации е надвишено: [{-Limits}; {Limits}].");
#else
                throw new MathParserException($"Iteration limits out of range: [{-Limits}; {Limits}].");
#endif

            n1 = (int)Math.Round(start);
            n2 = (int)Math.Round(end);
        }

        private static bool IsInfinity(Complex c) => double.IsInfinity(c.Re) || double.IsInfinity(c.Im);
    }
}