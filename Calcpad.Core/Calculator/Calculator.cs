using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    internal abstract class Calculator
    {
        private const double deltaPlus = 1 + 1e-15, deltaMinus = 1 - 1e-15;
        private static readonly double Log2Inv = 1 / Math.Log(2);

        private static readonly double[] Factorial;
        internal const char NegChar = '‐'; //hyphen, not minus "-"
        protected static int _degrees = 0;
        protected static bool _returnAngleUnits = false;
        protected static readonly double[] _toRad =
        {
            Math.PI / 180.0,
            1.0,
            Math.PI / 200.0
        };
        protected static readonly double[] _fromRad =
{
            180.0 / Math.PI,
            1.0,
            200.0 / Math.PI
        };
        protected static Unit[] _angleUnits =
        {
            Unit.Get("deg"),
            Unit.Get("rad"),
            Unit.Get("grad")
        };

        internal abstract int Degrees { set; }
        internal static bool ReturnAngleUnits { set => _returnAngleUnits = value; }

        //                                               ^  ÷  \  %  *  -  +  <  >  ≤  ≥  ≡  ≠  =
        internal static readonly int[] OperatorOrder = { 0, 3, 3, 3, 3, 4, 5, 6, 6, 6, 6, 6, 6, 7 };

        internal static readonly Dictionary<char, int> OperatorIndex = new()
        {
            { '^', 0 },
            { '/', 1 },
            { '÷', 1 },
            { '\\', 2 },
            { '%', 3 },
            { '*', 4 },
            { '-', 5 },
            { '+', 6 },
            { '<', 7 },
            { '>', 8 },
            { '≤', 9 },
            { '≥', 10 },
            { '≡', 11 },
            { '≠', 12 },
            { '=', 13 }
        };

        internal static readonly Dictionary<string, int> FunctionIndex = new(StringComparer.OrdinalIgnoreCase)
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
            { "‐", 42 }
        };

        internal static readonly Dictionary<string, int> Function2Index = new(StringComparer.OrdinalIgnoreCase)
        {
            { "atan2", 0 },
            { "root", 1 },
            { "mandelbrot", 2 }
        };

        internal static readonly Dictionary<string, int> MultiFunctionIndex = new(StringComparer.OrdinalIgnoreCase)
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
            { "spline", 11 }
        };

        internal static bool IsOperator(char name) => OperatorIndex.ContainsKey(name);
        internal static bool IsFunction(string name) => FunctionIndex.ContainsKey(name);
        internal static bool IsFunction2(string name) => Function2Index.ContainsKey(name);
        internal static bool IsMultiFunction(string name) => MultiFunctionIndex.ContainsKey(name);
        internal abstract Value EvaluateOperator(int index, Value a, Value b);
        internal abstract Value EvaluateFunction(int index, Value a);
        internal abstract Value EvaluateFunction2(int index, Value a, Value b);
        internal abstract Value EvaluateMultiFunction(int index, Value[] a);
        internal abstract Func<Value, Value, Value> GetOperator(int index);
        internal abstract Func<Value, Value> GetFunction(int index);
        internal abstract Func<Value, Value, Value> GetFunction2(int index);
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
#if BG
                throw new MathParser.MathParserException($"Невалидни мерни единици за функция: \"{func}({Unit.GetText(unit)})\".");
#else
                throw new MathParser.MathParserException($"Invalid units for function: \"{func}({Unit.GetText(unit)})\".");
#endif
        }


        protected static int GetRoot(Value root)
        {
            if (root.Units is not null)
#if BG
                throw new MathParser.MathParserException("Коренният показател трябва да е бездименсионен.");
#else
                throw new MathParser.MathParserException("Root index must be unitless.");
#endif
            if (!root.IsReal)
#if BG
                throw new MathParser.MathParserException("Коренният показател не може да е комплексно число.");
#else
                throw new MathParser.MathParserException("Root index cannot be complex number.");
#endif
            var n = (int)root.Re;
            if (n < 2 || n != root.Re)
#if BG
                throw new MathParser.MathParserException("Коренният показател трябва да е цяло число > 1.");
#else
                throw new MathParser.MathParserException("Root index must be integer > 1.");
#endif
            return n;
        }

        protected static Complex Fact(double value)
        {
            if (value < 0 || value > 170)
#if BG
                throw new MathParser.MathParserException("Аргументът e извън допустимите стойности за функцията n!.");
#else
                throw new MathParser.MathParserException("Argument out of range for function n!.");
#endif

            var i = (int)value;

            if (i != value)
#if BG
                throw new MathParser.MathParserException("Аргументът на функцията n! трябва да е цяло положително число.");
#else
                throw new MathParser.MathParserException("The argument of the n! function must be a positive integer.");
#endif

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
            for (int i = 0; i < v.Length - 1; i += 2)
            {
                if (Math.Abs(v[i].Re) >= 1e-12)
                    return v[i + 1];
            }
            if (v.Length % 2 != 0)
                return v[^1];

            return new(double.NaN);
        }

        protected static Value Take(Value[] v)
        {
            var x = Math.Round(v[0].Re);
            if (!double.IsNormal(x) || x < deltaMinus || x > v.Length * deltaPlus - 1.0)
                return new(double.NaN);

            return v[(int)x];
        }

        protected static Value Line(Value[] v)
        {
            var x = v[0].Re;
            if (!double.IsNormal(x) || x < deltaMinus || x > v.Length * deltaPlus - 1.0)
                return new(double.NaN);
            int i = (int)Math.Floor(x);
            if (i == x || x >= v.Length - 1)
                return v[i];

            return v[i] + (v[i + 1] - v[i]) * (x - i);
        }

        protected static Value Spline(Value[] v)
        {
            var x = v[0].Re;
            if (!double.IsNormal(x) || x < deltaMinus || x > v.Length * deltaPlus - 1.0)
                return new(double.NaN);
            int i = (int)Math.Floor(x);

            if (i == x || x >= v.Length - 1)
                return v[i];

            var u = v[1].Units;
            var y0 = v[i].Re * Unit.Convert(u, v[i].Units, ',');
            var y1 = v[i + 1].Re * Unit.Convert(u, v[i + 1].Units, ',');
            var d = y1 - y0;
            var a = d;
            var b = d;
            d = Math.Sign(d);
            if (i > 1)
            {
                var y2 = v[i - 1].Re * Unit.Convert(u, v[i - 1].Units, ',');
                a = (y1 - y2) * (Math.Sign(y0 - y2) == d ? 0.5 : 0.25);
            }
            if (i < v.Length - 2)
            {
                var y2 = v[i + 2].Re * Unit.Convert(u, v[i + 2].Units, ',');
                b = (y2 - y0) * (Math.Sign(y2 - y1) == d ? 0.5 : 0.25);
            }
            if (i == 1)
                a += (a - b) / 2;

            if (i == v.Length - 2)
                b += (b - a) / 2;

            var t = x - i;
            var y = y0 + ((y1 - y0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return new(y, u);
        }

        protected static double MandelbrotSet(double x, double y)
        {
            if (x > -1.25 && x < 0.375)
            {
                if (x < -0.75)
                {
                    if (y > -0.25 && y < 0.25)
                    {
                        double x1 = x + 1,
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
                    var logZn = Math.Log(im * im + re * re) / 2;
                    var nu = Math.Log(logZn * Log2Inv) * Log2Inv;
                    return (1.01 - Math.Pow(i + 1 - nu, 0.001)) * 1000;
                }
                im = 2 * re * im + y;
                re = reSq - imSq + x;
            }
            return double.NaN;
        }
    }
}
