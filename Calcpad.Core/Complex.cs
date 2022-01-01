using System;

namespace Calcpad.Core
{
    internal readonly struct Complex : IEquatable<Complex>
    {
        private static readonly double Log2Inv = 1 / Math.Log(2);
        private static readonly double Log10Inv = 1 / Math.Log(10);
        private const double TrigMin = -1e8;
        private const double TrigMax = 1e8;
        private const double Eps = 1e-12;
        private static readonly Random Rand = new();

        private readonly double _a;
        private readonly double _b;

        internal static readonly Complex Zero = new(0.0);
        internal static readonly Complex One = new(1.0);
        internal static readonly Complex ImaginaryOne = new(0.0, 1.0);
        private static readonly double[] Factorial;


        static Complex()
        {
            Factorial = new double[171];
            Factorial[0] = 1;
            for (int i = 1; i < 171; ++i)
                Factorial[i] = Factorial[i - 1] * i;
        }

        public Complex(double real, double imaginary)
        {
            _a = real;
            _b = imaginary;
        }

        public Complex(double real)
        {
            _a = real;
            _b = 0.0;
        }

        private static byte GetComplexType(double a, double b)
        {
            if (b == 0)
                return 1;

            if (a == 0)
                return 2;

            var re = Math.Abs(a);
            var im = Math.Abs(b);
            var d = (re + im) * Eps;
            if (im < d)
                return 1;

            if (re < d)
                return 2;

            return 3;
        }

        internal bool IsReal =>  GetComplexType(_a, _b) < 2;
        internal bool IsImaginary => GetComplexType(_a, _b) == 2;
        internal bool IsComplex => GetComplexType(_a, _b) == 3;
        internal double Re => _a;
        internal double Im => _b;
        internal Complex Real => new(_a, 0);
        internal Complex Imaginary => new(0, _b);
        internal double Phase => Math.Atan2(_b, _a);

        public static Complex operator -(in Complex value) =>
            new(-value._a, -value._b);

        public static Complex operator +(in Complex left, in Complex right) =>
            new(left._a + right._a, left._b + right._b);

        public static Complex operator -(in Complex left, in Complex right) =>
            new(left._a - right._a, left._b - right._b);

        public static Complex operator *(in Complex left, in Complex right) =>
            new(
                left._a * right._a - left._b * right._b,
                left._b * right._a + left._a * right._b
            );

        public static Complex operator /(in Complex left, in Complex right) 
        {
            var a = left._a;
            var b = left._b;
            var c = right._a;
            var d = right._b;
            double e, f;
            if (Math.Abs(d) < Math.Abs(c))
            {
                e = d / c;
                f = 1.0 / (c + d * e);
                return new((a + b * e) * f, (b - a * e) * f);
            }
            e = c / d;
            f = 1.0 / (d + c * e);
            return new((b + a * e) * f, (-a + b * e) * f);
        }

        public static Complex operator *(in Complex left, double right) =>
            new(left._a * right, left._b * right);

        public static Complex operator *(double left, in Complex right) => right * left;

        public static Complex operator /(in Complex left, double right) =>
            new (left._a / right, left._b / right);

        public static Complex operator /(double left, in Complex right)
        {
            var a = right._a;
            var b = right._b;
            double e, f;
            if (Math.Abs(b) < Math.Abs(a))
            {
                e = b / a;
                f = 1.0 / (a + b * e);
                return new(left * f, -left * e * f);
            }
            e = a / b;
            f = 1.0 / (b + a * e);
            return new(left * e * f, -left * f);
        }

        public static Complex IntDiv(in Complex left, in Complex right)  =>
            right.IsReal ?
            new Complex(Math.Truncate(left._a / right._a), Math.Truncate(left._b / right._a)) :
            double.NaN;

        public static Complex operator %(in Complex left, in Complex right) =>
            right.IsReal ?
            new Complex(left._a % right._a, left._b % right._a):
            double.NaN;

        public static double operator ==(in Complex left, in Complex right) =>
            Equals(left, right) ? 1.0 : 0.0;

        public static double operator !=(in Complex left, in Complex right) =>
            Equals(left, right) ? 0.0 : 1.0;

        public static double operator <(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a < right._a && !EqualsBinary(left._a, right._a) ? 1.0 : 0.0 :
            double.NaN;

        public static double operator >(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a > right._a && !EqualsBinary(left._a, right._a) ? 1.0 : 0.0 :
            double.NaN;

        public static double operator <=(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a <= right._a || EqualsBinary(left._a, right._a) ? 1.0 : 0.0 :
            double.NaN;

        public static double operator >=(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a >= right._a || EqualsBinary(left._a, right._a) ? 1.0 : 0.0 :
            double.NaN;

        public static implicit operator Complex(short value) => new(value);
        public static implicit operator Complex(int value) => new(value);
        public static implicit operator Complex(long value) => new(value);
        public static implicit operator Complex(ushort value) => new(value);
        public static implicit operator Complex(uint value) => new(value);
        public static implicit operator Complex(ulong value) => new(value);
        public static implicit operator Complex(sbyte value) => new(value);
        public static implicit operator Complex(byte value) => new(value);
        public static implicit operator Complex(float value) => new(value);
        public static implicit operator Complex(double value) => new(value);
        public static implicit operator System.Numerics.Complex(in Complex value) => new(value._a, value._b);
        public static implicit operator Complex(System.Numerics.Complex value) => new(value.Real, value.Imaginary);

        internal static bool EqualsBinary(double d1, double d2)
        {
            var l1 = BitConverter.DoubleToInt64Bits(d1);
            var l2 = BitConverter.DoubleToInt64Bits(d2);

            if (l1 >> 63 != l2 >> 63)
                return d1.Equals(d2);

            return Math.Abs(l1 - l2) < 4;
        }

        public override bool Equals(object obj) => 
            obj is Complex number && Equals(this, number);

        public bool Equals(Complex value) => Equals(this, value);

        private static bool Equals(in Complex left, in Complex right) =>
            EqualsBinary(left._a, right._a) && EqualsBinary(left._b, right._b);

        public override string ToString() => IsReal ?
            new TextWriter().FormatReal(_a, 15) :
            new TextWriter().FormatComplex(this, 15);

        public override int GetHashCode() =>
            IsReal ? _a.GetHashCode() : HashCode.Combine(_a, _b);

        public static Complex Fact(in Complex value)
        {
            if (!(value.IsReal))
#if BG
                throw new MathParser.MathParserException("Аргументът на функцията n! не може да е имагинерно число.");
#else
                throw new MathParser.MathParserException("The argument of the n! function cannot be complex.");
#endif
            if (value.Re < 0 || value.Re > 170)
#if BG
                throw new MathParser.MathParserException("Аргументът e извън допустимите стойности за функцията n!");
#else
                throw new MathParser.MathParserException("Argument out of range for function n!");
#endif

            var i = (int)value.Re;

            if (i != value.Re)
#if BG
                throw new MathParser.MathParserException("Аргументът на функцията n! трябва да е цяло положително число.");
#else
                throw new MathParser.MathParserException("The argument of the n! function must be a positive integer.");
#endif

            return Factorial[i];
        }
        internal static Complex Abs(in Complex value) => Modulus(value);

        private static double Modulus(in Complex value)
        {
            if (double.IsInfinity(value._a) || double.IsInfinity(value._b))
                return double.PositiveInfinity;

            var c = Math.Abs(value._a);
            var d = Math.Abs(value._b);
            double r;
            if (c > d)
            {
                r = d / c;
                return c * Math.Sqrt(1.0 + r * r);
            }
            if (d == 0.0)
                return c;

            r = c / d;
            return d * Math.Sqrt(1.0 + r * r);
        }

        internal static Complex Sign(in Complex value) =>
            value.IsReal ? Math.Sign(value._a) : double.NaN;

        private static void CheckTrigScope(double angle, string func)
        {
            if (angle < TrigMin || angle > TrigMax)
#if BG
                throw new MathParser.MathParserException($"Аргументът е извън допустимия интервал за {func}(x).");
#else
                throw new MathParser.MathParserException($"Argument out of range for {func}(x).");
#endif
        }

        internal static Complex RealRandom(in Complex value) =>
            Rand.NextDouble() * value.Re;

        internal static Complex RealSin(double value)
        {
            CheckTrigScope(value, "sin");
            return Math.Sin(value);
        }

        internal static Complex RealCos(double value)
        {
            CheckTrigScope(value, "cos");
            return Math.Cos(value);
        }

        internal static Complex Sin(in Complex value)
        {
            var a = value._a;
            var b = value._b;
            CheckTrigScope(a, "sin");
            return new(Math.Sin(a) * Math.Cosh(b), Math.Cos(a) * Math.Sinh(b));
        }

        internal static Complex Cos(in Complex value)
        {
            var a = value._a;
            var b = value._b;
            CheckTrigScope(a, "cos");
            return new(Math.Cos(a) * Math.Cosh(b), -Math.Sin(a) * Math.Sinh(b));
        }

        internal static Complex Tan(in Complex value)
        {
            var ta = Math.Tan(value._a);
            var thb = Math.Tanh(value._b);
            return new Complex(ta, thb) / new Complex(1.0, -ta * thb);
        }

        internal static Complex Cot(in Complex value)
        {
            var ta = Math.Tan(value._a);
            var thb = Math.Tanh(value._b);
            return new Complex(1.0, -ta * thb) / new Complex(ta, thb);
        }

        internal static Complex Sinh(in Complex value) /* Hyperbolic sin */
        {
            var a = value._a;
            var b = value._b;
            CheckTrigScope(b, "sinh");
            return new(Math.Sinh(a) * Math.Cos(b), Math.Cosh(a) * Math.Sin(b));
        }

        internal static Complex Cosh(in Complex value) /* Hyperbolic cos */
        {
            var a = value._a;
            var b = value._b;
            CheckTrigScope(b, "cosh");
            return new(Math.Cosh(a) * Math.Cos(b), Math.Sinh(a) * Math.Sin(b));
        }

        internal static Complex Tanh(in Complex value)
        {
            var tha = Math.Tanh(value._a);
            var tb = Math.Tan(value._b);
            return new Complex(tha, tb) / new Complex(1.0, tha * tb);
        }

        internal static Complex Coth(in Complex value)
        {
            var tha = Math.Tanh(value._a);
            var tb = Math.Tan(value._b);
            return new Complex(1.0, tha * tb) / new Complex(tha, tb);
        }

        internal static Complex Asin(in Complex value) =>
            -ImaginaryOne * Log(ImaginaryOne * value + Sqrt(One - value * value));

        internal static Complex Acos(in Complex value) =>
            -ImaginaryOne * Log(value + ImaginaryOne * Sqrt(One - (value * value)));

        internal static Complex Atan(in Complex value) =>
            ImaginaryOne / 2.0 * Log((ImaginaryOne + value) / (ImaginaryOne - value));

        internal static Complex Asinh(in Complex value) =>
            Log(value + Sqrt(value * value + One));

        internal static Complex Acosh(in Complex value) =>
            Log(value + Sqrt(value - One) * Sqrt(value + One));

        internal static Complex Atanh(in Complex value) =>
            Log((One + value) / (One - value)) / 2.0;

        internal static Complex Acoth(in Complex value) =>
            Log((value + One) / (value - One)) / 2.0;

        internal static Complex Pow(in Complex value, double power)
        {
            if (power == 0)
                return new Complex(1.0, 0.0);

            if (value._b == 0)
            {
                return value._a == 0 ? 
                    new Complex(0.0, 0.0) : 
                    new Complex(Math.Pow(value._a, power), 0.0);
            }
            var r = Modulus(value);
            var theta = power * value.Phase;
            var t = Math.Pow(r, power);
            return new(t * Math.Cos(theta), t * Math.Sin(theta));
        }

        internal static Complex Pow(in Complex value, in Complex power)
        {
            var c = power._a;
            var d = power._b;
            if (d == 0)
                return Pow(value, c);

            var a = value._a;
            var b = value._b;
            if (a == 0 && b == 0)
                return new(0.0, 0.0);

            var r = Modulus(value);
            var phi = value.Phase;
            var theta = c * phi + d * Math.Log(r);
            var t = Math.Pow(r, c) * Math.Exp(-d * phi);
            return new(t * Math.Cos(theta), t * Math.Sin(theta));
        }

        internal static Complex Log(in Complex value) =>
            new(Math.Log(Modulus(value)), value.Phase);

/*
        internal static Number Log(Number value, double baseValue) =>
            Log(value) / Math.Log(baseValue);
*/

        internal static Complex Log10(in Complex value) => 
            new(Math.Log(Modulus(value)) * Log10Inv, value.Phase * Log10Inv);

        internal static Complex Log2(in Complex value) =>
            new(Math.Log(Modulus(value)) * Log2Inv, value.Phase * Log2Inv);

        internal static Complex Exp(in Complex value)
        {
            var r = Math.Exp(value._a);
            return new(r * Math.Cos(value._b), r * Math.Sin(value._b));
        }

        internal static Complex Sqrt(in Complex value)
        {
            var r = Math.Sqrt(Modulus(value));
            var theta = value.Phase / 2.0;
            return new(r * Math.Cos(theta), r * Math.Sin(theta));
        }

        internal static Complex Cbrt(in Complex value)
        {
            var r = Math.Cbrt(Modulus(value));
            var theta = value.Phase / 3.0;
            return new(r * Math.Cos(theta), r * Math.Sin(theta));
        }

        internal static Complex Round(in Complex value) =>
            new(Math.Round(value._a), Math.Round(value._b));

        internal static Complex Floor(in Complex value) =>
            new(Math.Floor(value._a), Math.Floor(value._b));

        internal static Complex Ceiling(in Complex value) =>
            new(Math.Ceiling(value._a), Math.Ceiling(value._b));

        internal static Complex Truncate(in Complex value) =>
            new(Math.Truncate(value._a), Math.Truncate(value._b));

        internal static Complex Random(in Complex value) =>
            new Complex(Rand.NextDouble(), Rand.NextDouble()) * value;

        internal static Complex Atan2(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
                Math.Atan2(left._a, right._a) :
                double.NaN;

        internal static Complex MandelbrotSet(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
                MandelbrotSet(left._a, right._a) :
                double.NaN; 

        internal static double MandelbrotSet(double x, double y)
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

        internal static string Format(in Complex c, int decimals, OutputWriter.OutputFormat mode)
        {
            return mode switch
            {
                OutputWriter.OutputFormat.Text => new TextWriter().FormatComplex(c, decimals),
                OutputWriter.OutputFormat.Html => new HtmlWriter().FormatComplex(c, decimals),
                OutputWriter.OutputFormat.Xml => new XmlWriter().FormatComplex(c, decimals),
                _ => "undefined format"
            };
        }
    }
}
