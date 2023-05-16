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

        internal enum Types
        {
            Real = 1,
            Imaginary = 2,
            Complex = 3
        }

        private static Types GetType(double a, double b)
        {
            if (b == 0.0)
                return Types.Real;

            if (a == 0.0)
                return Types.Imaginary;

            var re = Math.Abs(a);
            var im = Math.Abs(b);
            var d = (re + im) * Eps;
            if (im < d)
                return Types.Real;

            if (re < d)
                return Types.Imaginary;

            return Types.Complex;
        }

        internal static Types Type(double re, double im) => GetType(re, im);

        internal bool IsReal => GetType(_a, _b) == Types.Real;
        internal bool IsImaginary => GetType(_a, _b) == Types.Imaginary;
        internal bool IsComplex => GetType(_a, _b) == Types.Complex;
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
            new(left._a / right, left._b / right);

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

        public static Complex IntDiv(in Complex left, in Complex right) =>
            right.IsReal ?
            new Complex(Math.Truncate(left._a / right._a), Math.Truncate(left._b / right._a)) :
            double.NaN;

        public static Complex operator %(in Complex left, in Complex right) =>
            right.IsReal ?
            new Complex(left._a % right._a, left._b % right._a) :
            double.NaN;

        public static double operator ==(in Complex left, in Complex right) =>
            Equals(left, right) ? 1.0 : 0.0;

        public static double operator !=(in Complex left, in Complex right) =>
            Equals(left, right) ? 0.0 : 1.0;

        public static double operator <(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a < right._a && !left._a.EqualsBinary(right._a) ? 1.0 : 0.0 :
            double.NaN;

        public static double operator >(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a > right._a && !left._a.EqualsBinary(right._a) ? 1.0 : 0.0 :
            double.NaN;

        public static double operator <=(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a <= right._a || left._a.EqualsBinary(right._a) ? 1.0 : 0.0 :
            double.NaN;

        public static double operator >=(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a >= right._a || left._a.EqualsBinary(right._a) ? 1.0 : 0.0 :
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

        public override bool Equals(object obj) =>
            obj is Complex number && Equals(this, number);

        public bool Equals(Complex value) => Equals(this, value);

        private static bool Equals(in Complex left, in Complex right) =>
            left._a.EqualsBinary(right._a) && left._b.EqualsBinary(right._b);

        public override string ToString() => IsReal ?
            new TextWriter().FormatReal(_a, 15) :
            new TextWriter().FormatComplex(_a, _b, 15);

        public override int GetHashCode() =>
            IsReal ? _a.GetHashCode() : HashCode.Combine(_a, _b);


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
            Rand.NextDouble() * value._a;

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
            var c = Math.SinCos(a);
            return new(c.Sin * Math.Cosh(b), c.Cos * Math.Sinh(b));
        }

        internal static Complex Cos(in Complex value)
        {
            var a = value._a;
            var b = value._b;
            CheckTrigScope(a, "cos");
            var c = Math.SinCos(a);
            return new(c.Cos * Math.Cosh(b), -c.Sin * Math.Sinh(b));
        }

        internal static Complex Tan(in Complex value)
        {
            var ta = Math.Tan(value._a);
            if (value._b == 0d)
                return ta;

            var thb = Math.Tanh(value._b);
            return new Complex(ta, thb) / new Complex(1.0, -ta * thb);
        }

        internal static Complex Cot(in Complex value)
        {
            var ta = Math.Tan(value._a);
            if (value._b == 0d)
                return 1d / ta;

            var thb = Math.Tanh(value._b);
            return new Complex(1.0, -ta * thb) / new Complex(ta, thb);
        }

        internal static Complex Sinh(in Complex value) /* Hyperbolic sin */
        {
            var a = value._a;
            var b = value._b;
            CheckTrigScope(b, "sinh");
            var c = Math.SinCos(b);
            return new(Math.Sinh(a) * c.Cos, Math.Cosh(a) * c.Sin);
        }

        internal static Complex Cosh(in Complex value) /* Hyperbolic cos */
        {
            var a = value._a;
            var b = value._b;
            CheckTrigScope(b, "cosh");
            var c = Math.SinCos(b);
            return new(Math.Cosh(a) * c.Cos, Math.Sinh(a) * c.Sin);
        }

        internal static Complex Tanh(in Complex value)
        {
            var tha = Math.Tanh(value._a);
            var tb = Math.Tan(value._b);
            return tb == 0 ? 
                tha :
                new Complex(tha, tb) / new Complex(1.0, tha * tb);
        }

        internal static Complex Coth(in Complex value)
        {
            var tha = Math.Tanh(value._a);
            var tb = Math.Tan(value._b);
            return tb == 0 ?
                1d / tha :
                new Complex(1.0, tha * tb) / new Complex(tha, tb);
        }

        internal static Complex Asin(in Complex value) =>
            -ImaginaryOne * Log(ImaginaryOne * value + Sqrt(One - value * value));

        internal static Complex Acos(in Complex value) =>
            -ImaginaryOne * Log(value + ImaginaryOne * Sqrt(One - (value * value)));

        internal static Complex Atan(in Complex value) =>
            ImaginaryOne / 2.0 * Log((ImaginaryOne + value) / (ImaginaryOne - value));

        internal static Complex Acot(in Complex value) =>
            ImaginaryOne / 2.0 * Log((value - ImaginaryOne) / (value + ImaginaryOne));

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
                return 1.0;

            if (value._b == 0)
            {
                return value._a == 0 ?
                    0.0 :
                    Math.Pow(value._a, power);
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
            new(
                Math.Round(value._a, MidpointRounding.AwayFromZero), 
                Math.Round(value._b, MidpointRounding.AwayFromZero)
                );

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

        internal static string Format(in Complex c, int decimals, OutputWriter.OutputFormat mode)
        {
            return mode switch
            {
                OutputWriter.OutputFormat.Text => new TextWriter().FormatComplex(c._a, c._b, decimals),
                OutputWriter.OutputFormat.Html => new HtmlWriter().FormatComplex(c._a, c._b, decimals),
                OutputWriter.OutputFormat.Xml => new XmlWriter().FormatComplex(c._a, c._b, decimals),
                _ => "undefined format"
            };
        }
    }
}
