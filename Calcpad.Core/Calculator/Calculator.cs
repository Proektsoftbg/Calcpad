using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    internal abstract class Calculator
    {
        internal delegate T Function<T>(in T a) where T : IScalarValue;
        internal delegate T Operator<T>(in T a, in T b) where T : IScalarValue;
        internal delegate IValue Function3(in IValue a, in IValue b, in IValue c);

        internal const double DeltaPlus = 1 + 1e-14, DeltaMinus = 1 - 1e-14;
        internal const char NegChar = '‐'; //hyphen, not minus "-"
        private static readonly double Log2Inv = 1 / Math.Log(2);
        private static readonly double[] Factorial = InitFactorial();
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

        protected static readonly Unit[] AngleUnits =
        [
            Unit.Get("deg"),
            Unit.Get("rad"),
            Unit.Get("grad")
        ];

        protected Function3[] Functions3 =
        [
            If
        ];

        protected Func<IScalarValue[], IScalarValue>[] Interpolations =
        [
            Take,
            Line,
            Spline,
        ];

        internal abstract int Degrees { set; }
        internal bool ReturnAngleUnits { set => _returnAngleUnits = value; }

        // Negation = 1                                   ^  ÷  \  ⦼  *  -  +  <  >  ≤  ≥  ≡  ≠  ∧ ∨  ⊕  = ∠
        internal static readonly sbyte[] OperatorOrder = [0, 4, 4, 4, 4, 5, 6, 7, 7, 7, 7, 7, 7, 8, 9, 9, 10, 3];

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
            { '=', 16 },
            { '∠', 17 },

        }.ToFrozenDictionary();

        internal static readonly char[] Operators = ['^', '/', '\\', '⦼', '*', '-', '+', '<', '>', '≤', '≥', '≡', '≠', '∧', '∨', '⊕', '=', '∠'];

        private static readonly bool[] _isZeroPreservingOperator = [
            false,   // ^  0
            false,   // /÷ 1
            false,   // \  2
            false,   // ⦼  3
            true,    // *  4
            true,    // -  5
            true,    // +  6
            true,    // <  7
            true,    // >  8
            false,   // ≤  9
            false,   // ≥ 10
            false,   // ≡ 11
            true,    // ≠ 12
            true,    // ∧ 13
            true,    // ∨ 14
            true,    // ⊕ 15
            true,    // = 16
            true,    // ∠ 17
            ];

        internal static bool IsZeroPreservingOperator(long index) =>
             index < 0 ? index == -1 : _isZeroPreservingOperator[index];
        internal static char GetOperatorSymbol(long index) =>
            index < 0 ? '\0' : Operators[index];
        internal static bool OperatorRequireConsistentUnits(long index) => index > 4 && index < 13;

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
            { "conj", 40 },
            { "random", 41 },
            { "fact", 42 },
            { "‐", 43 },
            { "not", 44},
            { "timer", 45 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function2Index =
        new Dictionary<string, int>()
        {
            { "atan2", 0 },
            { "root", 1 },
            { "mod", 2 },
            { "mandelbrot", 3 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function3Index =
        new Dictionary<string, int>()
        {
            { "if", 0 },
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
            { "and", 9 },
            { "or", 10 },
            { "xor", 11 },
            { "gcd", 12 },
            { "lcm", 13 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> InterpolationIndex =
        new Dictionary<string, int>()
        {
            { "take", 0 },
            { "line", 1 },
            { "spline", 2 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly sbyte UnitMultOrder = (sbyte)(OperatorOrder[OperatorIndex['*']] - 2);
        internal static bool IsOperator(char name) => OperatorIndex.ContainsKey(name);
        internal static bool IsFunction(string name) => FunctionIndex.ContainsKey(name);
        internal static bool IsFunction2(string name) => Function2Index.ContainsKey(name);
        internal static bool IsFunction3(string name) => Function3Index.ContainsKey(name);
        internal static bool IsMultiFunction(string name) => MultiFunctionIndex.ContainsKey(name);
        internal static bool IsInterpolation(string name) => InterpolationIndex.ContainsKey(name);
        internal abstract IScalarValue EvaluateOperator(long index, in IScalarValue a, in IScalarValue b);
        internal abstract IScalarValue EvaluateFunction(long index, in IScalarValue a);
        internal abstract IScalarValue EvaluateFunction2(long index, in IScalarValue a, in IScalarValue b);
        internal abstract IValue EvaluateFunction3(long index, in IValue a, in IValue b, in IValue c);
        internal abstract IScalarValue EvaluateMultiFunction(long index, IScalarValue[] a);
        internal abstract IScalarValue EvaluateInterpolation(long index, IScalarValue[] a);
        internal abstract Operator<RealValue> GetOperator(long index);
        internal abstract Function<RealValue> GetFunction(long index);
        internal abstract Operator<RealValue> GetFunction2(long index);
        internal abstract Function3 GetFunction3(long index);
        internal abstract Func<IScalarValue[], IScalarValue> GetMultiFunction(long index);
        internal Func<IScalarValue[], IScalarValue> GetInterpolation(long index) => Interpolations[index];

        internal static readonly int PowerIndex = OperatorIndex['^'];
        internal static readonly int MultiplyIndex = OperatorIndex['*'];
        internal static readonly int AddIndex = OperatorIndex['+'];
        internal static readonly int SqrIndex = FunctionIndex["sqr"];
        internal static readonly int SqrtIndex = FunctionIndex["sqrt"];
        internal static readonly int CbrtIndex = FunctionIndex["cbrt"];
        internal static readonly int RootIndex = Function2Index["root"];
        internal static readonly int SrssIndex = MultiFunctionIndex["srss"];
        internal static readonly int SumIndex = MultiFunctionIndex["sum"];

        private static double[] InitFactorial()
        {
            var factorial = new double[171];
            factorial[0] = 1;
            for (int i = 1; i < 171; ++i)
                factorial[i] = factorial[i - 1] * i;

            return factorial;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void CheckFunctionUnits(string func, Unit unit)
        {
            if (unit is not null && !unit.IsAngle)
                throw Exceptions.InvalidUnitsFunction(func, Unit.GetText(unit));
        }

        protected static int GetRoot(in IScalarValue root)
        {
            if (root.Units is not null)
                throw Exceptions.RootUnitless();

            if (!root.IsReal)
                throw Exceptions.RootComplex();

            var n = (int)root.Re;
            if (n < 2 || n != root.Re)
                throw Exceptions.RootInteger();

            return n;
        }

        protected static IValue If(in IValue condition, in IValue valueIfTrue, in IValue valueIfFalse)
        {
            var value = IValue.AsValue(condition);
            return Math.Abs(value.Re) < ComplexValue.LogicalZero ?
                valueIfFalse :
                valueIfTrue;
        }

        protected static double Fact(double value)
        {
            if (value < 0 || value > 170)
                throw Exceptions.FactorialArgumentOutOfRange();

            var i = (int)value;
            if (i != value)
                throw Exceptions.FactorialArgumentPositiveInteger();

            return Factorial[i];
        }


        protected static IScalarValue Min(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                var b = value.Re * Unit.Convert(u, value.Units, ',');
                if (b < result)
                    result = b;
            }
            return new RealValue(result, u);
        }

        protected static IScalarValue Max(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                var b = value.Re * Unit.Convert(u, value.Units, ',');
                if (b > result)
                    result = b;
            }
            return new RealValue(result, u);
        }

        protected static IScalarValue Switch(IScalarValue[] v)
        {
            for (int i = 0, len = v.Length - 1; i < len; i += 2)
            {
                if (Math.Abs(v[i].Re) >= ComplexValue.LogicalZero)
                    return v[i + 1];
            }
            if (v.Length % 2 != 0)
                return v[^1];

            return RealValue.NaN;
        }

        protected static IScalarValue Take(IScalarValue[] values)
        {
            var x = values[0];
            ReadOnlySpan<IScalarValue> y = values.AsSpan(1..);
            var d = Math.Round(x.Re, MidpointRounding.AwayFromZero);
            if (!double.IsNormal(d) || d < DeltaMinus || d > y.Length * DeltaPlus)
                return RealValue.NaN;

            return y[(int)d - 1];
        }

        protected static IScalarValue Line(IScalarValue[] values)
        {
            var x = values[0];
            ReadOnlySpan<IScalarValue> y = values.AsSpan(1..);
            var d = x.Re;
            if (!double.IsNormal(d) || d < DeltaMinus || d > y.Length * DeltaPlus)
                return RealValue.NaN;

            var i = (int)Math.Floor(d);
            var y1 = y[i - 1];
            if (i == d || d >= y.Length)
                return y1;

            return y1 + (y[i] - y1) * (d - i);
        }

        protected static IScalarValue Spline(IScalarValue[] values)
        {
            var x = values[0];
            ReadOnlySpan<IScalarValue> y = values.AsSpan(1..);
            var d = x.Re;
            if (!double.IsNormal(d) || d < DeltaMinus || d > y.Length * DeltaPlus)
                return RealValue.NaN;

            var i = (int)Math.Floor(d) - 1;
            var v = y[i];
            if (i == d || d >= y.Length)
                return v;

            var u = v.Units;
            var y0 = v.Re;
            v = y[i + 1];
            var y1 = v.Re * Unit.Convert(u, v.Units, ',');
            var dy = y1 - y0;
            var a = dy;
            var b = dy;
            dy = Math.Sign(dy);
            if (i > 0)
            {
                v = y[i - 1];
                var y2 = v.Re * Unit.Convert(u, v.Units, ',');
                a = (y1 - y2) * (Math.Sign(y0 - y2) == dy ? 0.5 : 0.25);
            }
            if (i < y.Length - 2)
            {
                v = y[i + 2];
                var y2 = v.Re * Unit.Convert(u, v.Units, ',');
                b = (y2 - y0) * (Math.Sign(y2 - y1) == dy ? 0.5 : 0.25);
            }
            if (i == 0)
                a += (a - b) / 2;

            if (i == y.Length - 2)
                b += (b - a) / 2;

            var t = d - i - 1d;
            d = y0 + ((y1 - y0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return new RealValue(d, u);
        }

        protected static IScalarValue And(IScalarValue[] v)
        {
            for (int i = 0, len = v.Length; i < len; ++i)
                if (Math.Abs(v[i].Re) < RealValue.LogicalZero)
                    return RealValue.Zero;

            return RealValue.One;
        }

        protected static IScalarValue Or(IScalarValue[] v)
        {
            for (int i = 0, len = v.Length; i < len; ++i)
                if (Math.Abs(v[i].Re) >= RealValue.LogicalZero)
                    return RealValue.One;

            return RealValue.Zero;
        }

        protected static IScalarValue Xor(IScalarValue[] v)
        {
            var b = Math.Abs(v[0].Re) >= RealValue.LogicalZero;
            for (int i = 1, len = v.Length; i < len; ++i)
                b = b != Math.Abs(v[i].Re) >= RealValue.LogicalZero;

            return b ? RealValue.One : RealValue.Zero;
        }

        protected static RealValue MandelbrotSet(in IScalarValue a, in IScalarValue b) =>
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

        protected static RealValue Gcd(IScalarValue[] v)
        {
            var a = AsLong(v[0].Re);
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = AsLong(v[i].Re * Unit.Convert(u, v[i].Units, ','));
                a = Gcd(a, b);
            }
            return new RealValue(a);
        }

        protected static RealValue Lcm(IScalarValue[] v)
        {
            var a = AsLong(v[0].Re);
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = AsLong(v[i].Re * Unit.Convert(u, v[i].Units, ','));
                if (a == 0 && b == 0)
                    return RealValue.NaN;
                a = a * b / Gcd(a, b);
            }
            return new RealValue(a);
        }

        internal static long AsLong(double d)
        {
            var a = Math.Abs(d);
            if (a > long.MaxValue || a != Math.Truncate(a))
                throw Exceptions.BothValuesInteger();

            return (long)a;
        }

        internal static long Gcd(long a, long b)
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
        protected static double Timer() => (DateTime.Now.Ticks - Ticks) / 10000000.0;
    }
}
