using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    internal abstract class Calculator
    {
        internal delegate Value Operator(in Value a, in Value b);
        internal delegate Value Function(in Value a);

        private const double DeltaPlus = 1 + 1e-15, DeltaMinus = 1 - 1e-15;
        private static readonly double Log2Inv = 1 / Math.Log(2);

        private static readonly double[] Factorial;
        internal const char NegChar = '‐'; //hyphen, not minus "-"
        protected int _degrees;
        protected bool _returnAngleUnits;

        protected static readonly double[] FromRad =
        [
            180.0 / Math.PI,
            1.0,
            200.0 / Math.PI
        ];

        protected static readonly double[] ToRad =
        [
            Math.PI / 180.0,
            1.0,
            Math.PI / 200.0
        ];

        protected static Unit[] AngleUnits =
        [
            Unit.Get("deg"),
            Unit.Get("rad"),
            Unit.Get("grad")
        ];

        internal abstract int Degrees { set; }
        internal bool ReturnAngleUnits { set => _returnAngleUnits = value; }

        //                                              ^  ÷  \  ⦼  *  -  +  <  >  ≤  ≥  ≡  ≠  ∧ ∨  ⊕  =
        internal static readonly int[] OperatorOrder = [0, 3, 3, 3, 3, 4, 5, 6, 6, 6, 6, 6, 6, 7, 8, 8, 9];

        internal static readonly FrozenDictionary<char, int> OperatorIndex =
        new Dictionary<char, int>()
        {
            { '^', 0 },
            { '/', 1 },
            { '÷', 1 },
            { '\\', 2 },
            { '⦼', 3 },
            { '*', 4 },
            { '-', 5 },
            { '+', 6 },
            { '<', 7 },
            { '>', 8 },
            { '≤', 9 },
            { '≥', 10 },
            { '≡', 11 },
            { '≠', 12 },
            { '∧', 13 },
            { '∨', 14 },
            { '⊕', 15 },
            { '=', 16 }
        }.ToFrozenDictionary();

        internal static readonly FrozenDictionary<string, int> FunctionIndex =
        new Dictionary<string, int>()
        {
            { "sin", 0 },
            { "cos", 1 },
            { "tan", 2 },
            { "csc", 3 },
            { "sec", 4 },
            { "cot", 5 },
            { "asin", 6 },
            { "acos", 7 },
            { "atan", 8 },
            { "acsc", 9 },
            { "asec", 10 },
            { "acot", 11 },
            { "sinh", 12 },
            { "cosh", 13 },
            { "tanh", 14 },
            { "csch", 15 },
            { "sech", 16 },
            { "coth", 17 },
            { "asinh", 18 },
            { "acosh", 19 },
            { "atanh", 20 },
            { "acsch", 21 },
            { "asech", 22 },
            { "acoth", 23 },
            { "log", 24 },
            { "ln", 25 },
            { "log_2", 26 },
            { "exp", 27 },
            { "abs", 28 },
            { "sign", 29 },
            { "sqr", 30 },
            { "sqrt", 31 },
            { "cbrt", 32 },
            { "round", 33 },
            { "floor", 34 },
            { "ceiling", 35 },
            { "trunc", 36 },
            { "re", 37 },
            { "im", 38 },
            { "phase", 39 },
            { "random", 40 },
            { "fact", 41 },
            { "‐", 42 },
            { "not", 43},
            { "timer", 44 }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function2Index =
        new Dictionary<string, int>()
        {
            { "atan2", 0 },
            { "root", 1 },
            { "mod", 2 },
            { "mandelbrot", 3 }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> MultiFunctionIndex =
        new Dictionary<string, int>()
        {
            { "min", 0 },
            { "max", 1 },
            { "sum", 2 },
            { "sumsq", 3 },
            { "srss", 4 },
            { "average", 5 },
            { "product", 6 },
            { "mean", 7 },
            { "switch", 8 },
            { "take", 9 },
            { "line", 10 },
            { "spline", 11 },
            { "and", 12 },
            { "or", 13 },
            { "xor", 14 },
            { "gcd", 15 },
            { "lcm", 16 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static bool IsOperator(char name) => OperatorIndex.ContainsKey(name);
        internal static bool IsFunction(string name) => FunctionIndex.ContainsKey(name);
        internal static bool IsFunction2(string name) => Function2Index.ContainsKey(name);
        internal static bool IsMultiFunction(string name) => MultiFunctionIndex.ContainsKey(name);
        internal abstract Value EvaluateOperator(int index, in Value a, in Value b);
        internal abstract Value EvaluateFunction(int index, in Value a);
        internal abstract Value EvaluateFunction2(int index, in Value a, in Value b);
        internal abstract Value EvaluateMultiFunction(int index, Value[] a);
        internal abstract Operator GetOperator(int index);
        internal abstract Function GetFunction(int index);
        internal abstract Operator GetFunction2(int index);
        internal abstract Func<Value[], Value> GetMultiFunction(int index);

        internal static readonly int PowerIndex = OperatorIndex['^'];
        internal static readonly int SqrIndex = FunctionIndex["sqr"];
        internal static readonly int SqrtIndex = FunctionIndex["sqrt"];
        internal static readonly int CbrtIndex = FunctionIndex["cbrt"];
        internal static readonly int RootIndex = Function2Index["root"];

        static Calculator()
        {
            Factorial = new double[171];
            Factorial[0] = 1;
            for (int i = 1; i < 171; ++i)
                Factorial[i] = Factorial[i - 1] * i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void CheckFunctionUnits(string func, Unit unit)
        {
            if (unit is not null && !unit.IsAngle)
                Throw.InvalidUnitsFunctionException(func, Unit.GetText(unit));
        }

        protected static int GetRoot(in Value root)
        {
            if (root.Units is not null)
                Throw.RootUnitlessException();

            if (!root.IsReal)
                Throw.RootComplexException();

            var n = (int)root.Re;
            if (n < 2 || n != root.Re)
                Throw.RootIntegerException();

            return n;
        }

        protected static double Fact(double value)
        {
            if (value < 0 || value > 170)
                Throw.FactorialArgumentOutOfRangeException();

            var i = (int)value;

            if (i != value)
                Throw.FactorialArgumentPositiveIntegerException();

            return Factorial[i];
        }


        protected static Value Min(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Re * Unit.Convert(u, v[i].Units, ',');
                if (b < result)
                    result = b;
            }
            return new(result, u);
        }

        protected static Value Max(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Re * Unit.Convert(u, v[i].Units, ',');
                if (b > result)
                    result = b;
            }
            return new(result, u);
        }

        protected static Value Switch(Value[] v)
        {
            for (int i = 0, len = v.Length - 1; i < len; i += 2)
            {
                if (Math.Abs(v[i].Re) >= Value.LogicalZero)
                    return v[i + 1];
            }
            if (v.Length % 2 != 0)
                return v[^1];

            return Value.NaN;
        }

        protected static Value Take(Value[] v) => Take(v[0], v.AsSpan(1));
        internal static Value Take(Value x, ReadOnlySpan<Value> y)
        {
            var d = Math.Round(x.Re, MidpointRounding.AwayFromZero);
            if (!double.IsNormal(d) || d < DeltaMinus || d > y.Length * DeltaPlus)
                return Value.NaN;

            return y[(int)d - 1];
        }

        protected static Value Line(Value[] v) => Line(v[0], v.AsSpan(1));
        internal static Value Line(Value x, ReadOnlySpan<Value> y)
        {
            var d = x.Re;
            if (!double.IsNormal(d) || d < DeltaMinus || d > y.Length * DeltaPlus)
                return Value.NaN;

            var i = (int)Math.Floor(d);
            if (i == d || d >= y.Length)
                return y[i - 1];

            return y[i - 1] + (y[i] - y[i - 1]) * (d - i);
        }

        protected static Value Spline(Value[] v) => Spline(v[0], v.AsSpan(1));
        internal static Value Spline(Value x, ReadOnlySpan<Value> y)
        {
            var d = x.Re;
            if (!double.IsNormal(d) || d < DeltaMinus || d > y.Length * DeltaPlus)
                return Value.NaN;

            var i = (int)Math.Floor(d) - 1;
            if (i == d || d >= y.Length)
                return y[i];

            var u = y[0].Units;
            var y0 = y[i].Re * Unit.Convert(u, y[i].Units, ',');
            var y1 = y[i + 1].Re * Unit.Convert(u, y[i + 1].Units, ',');
            var dy = y1 - y0;
            var a = dy;
            var b = dy;
            dy = Math.Sign(dy);
            if (i > 0)
            {
                var y2 = y[i - 1].Re * Unit.Convert(u, y[i - 1].Units, ',');
                a = (y1 - y2) * (Math.Sign(y0 - y2) == dy ? 0.5 : 0.25);
            }
            if (i < y.Length - 2)
            {
                var y2 = y[i + 2].Re * Unit.Convert(u, y[i + 2].Units, ',');
                b = (y2 - y0) * (Math.Sign(y2 - y1) == dy ? 0.5 : 0.25);
            }
            if (i == 0)
                a += (a - b) / 2;

            if (i == y.Length - 2)
                b += (b - a) / 2;

            var t = d - i - 1d;
            d = y0 + ((y1 - y0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return new(d, u);
        }

        protected static Value And(Value[] v)
        {
            for (int i = 0, len = v.Length; i < len; ++i)
                if (Math.Abs(v[i].Re) < Value.LogicalZero)
                    return Value.Zero;

            return Value.One;
        }

        protected static Value Or(Value[] v)
        {
            for (int i = 0, len = v.Length; i < len; ++i)
                if (Math.Abs(v[i].Re) >= Value.LogicalZero)
                    return Value.One;

            return Value.Zero;
        }

        protected static Value Xor(Value[] v)
        {
            var b = Math.Abs(v[0].Re) >= Value.LogicalZero;
            for (int i = 1, len = v.Length; i < len; ++i)
                b = b != Math.Abs(v[i].Re) >= Value.LogicalZero;

            return b ? Value.One : Value.Zero;
        }

        protected static Value Not(in Value value) => 
            Math.Abs(value.Re) < Value.LogicalZero ? Value.One : Value.Zero;

        protected static Value Mod(in Value a, in Value b) => a % b;

        protected static Value MandelbrotSet(in Value a, in Value b) =>
            new(
                MandelbrotSet(
                    a.Re, b.Re * Unit.Convert(a.Units, b.Units, ',')
                ),
                a.Units
            );

        private static double MandelbrotSet(double x, double y)
        {
            if (x > -1.25 && x < 0.375)
            {
                if (x < -0.75)
                {
                    if (y > -0.25 && y < 0.25)
                    {
                        double x1 = x + 1d,
                            y2 = y * y,
                            x2 = x1 * x1;
                        if (x2 + y2 <= 0.0625)
                            return double.NaN;
                    }
                }
                else if (y > -0.65 && y < 0.65)
                {
                    double x1 = x - 0.25,
                        y2 = y * y,
                        q = x1 * x1 + y2;
                    if (q * (q + x1) <= 0.25 * y2)
                        return double.NaN;
                }
            }
            double re = x, im = y;
            for (int i = 0; i <= 1000; ++i)
            {
                double reSq = re * re, imSq = im * im;
                if (reSq + imSq > 4)
                {
                    var logZn = Math.Log(im * im + re * re) / 2d;
                    var nu = Math.Log(logZn * Log2Inv) * Log2Inv;
                    return (1.01 - Math.Pow(i + 1d - nu, 0.001)) * 1000;
                }
                im = 2d * re * im + y;
                re = reSq - imSq + x;
            }
            return double.NaN;
        }


        protected static Value Gcd(Value[] v)
        {
            var a = AsLong(v[0].Re);
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = AsLong(v[i].Re * Unit.Convert(u, v[i].Units, ','));
                a = Gcd(a, b);
            }
            return new(a);
        }

        protected static Value Lcm(Value[] v)
        {
            var a = AsLong(v[0].Re);
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = AsLong(v[i].Re * Unit.Convert(u, v[i].Units, ','));
                a = a * b / Gcd(a, b);
            }
            return new(a);
        }

        private static long AsLong(double d)
        {
            var c = Math.Abs(d);
            if (c > long.MaxValue || c != Math.Truncate(c))
                Throw.BothValuesIntegerException();
            return (long)c;
        }

        private static long Gcd(long a, long b)
        {
            if (a == 0) return b;
            if (b == 0) return a;
            int k;
            for (k = 0; ((a | b) & 1) == 0; ++k)
            {
                a >>= 1;
                b >>= 1;
            }
            while ((a & 1) == 0)
                a >>= 1;

            do
            {
                while ((b & 1) == 0)
                    b >>= 1;

                if (a > b)
                    (b, a) = (a, b);

                b -= a;
            } while (b != 0);
            return a << k;
        }
        private static readonly long Ticks = DateTime.Now.Ticks;
        protected static Value Timer(in Value _) => new((DateTime.Now.Ticks - Ticks) / 10000000.0, Unit.Get("s"));
    }
}
