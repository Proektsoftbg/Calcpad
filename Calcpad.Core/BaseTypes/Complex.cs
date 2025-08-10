using System;

namespace Calcpad.Core
{
    internal readonly struct Complex : IEquatable<Complex>
    {
        private static readonly double Log2Inv = 1d / Math.Log(2d);
        private static readonly double Log10Inv = 1d / Math.Log(10d);
        private const double TrigMin = -1e8;
        private const double TrigMax = 1e8;
        private const double Eps = 1e-12;
        private static readonly Random Rand = new();

        private readonly double _a;
        private readonly double _b;

        internal static readonly Complex Zero = new(0d);
        internal static readonly Complex One = new(1d);
        internal static readonly Complex ImaginaryOne = new(0d, 1d);
        internal static readonly Complex NaN = new(double.NaN);
        internal static readonly Complex PositiveInfinity = new(double.PositiveInfinity);
        internal static readonly Complex NegativeInfinity = new(double.NegativeInfinity);
        internal static readonly Complex ComplexInfinity = new(double.PositiveInfinity, double.PositiveInfinity);

        public Complex(double real, double imaginary)
        {
            _a = real;
            _b = imaginary;
        }

        public Complex(double real)
        {
            _a = real;
            _b = 0d;
        }

        internal enum Types
        {
            Real = 1,
            Imaginary = 2,
            Complex = 3
        }

        internal static Types GetType(double a, double b)
        {
            if (b == 0)
                return Types.Real;

            if (a == 0)
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

        internal Types Type() => GetType(_a, _b);
        internal bool IsReal => GetType(_a, _b) == Types.Real;
        internal bool IsImaginary => GetType(_a, _b) == Types.Imaginary;
        internal bool IsComplex => GetType(_a, _b) == Types.Complex;
        internal double Re => _a;
        internal double Im => _b;
        internal Complex Real => new(_a);
        internal Complex Imaginary => new(0d, _b);
        internal double Phase
        {
            get
            {
                if (_b == 0d)
                    return _a >= 0d ? 0d : Math.PI;

                return Math.Atan2(_b, _a);
            }
        }

        internal double NormalPhase
        {
            get
            {
                var phi = Math.Atan2(_b, _a);
                if (phi < 0d)
                    return phi + 2d * Math.PI;

                return phi;
            }
        }

        internal Complex Conjugate => new(_a, -_b);

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
            if (d == 0)
            {
                if (c == 0 && (a != 0 || b != 0))
                    return ComplexInfinity;

                return new(a / c, b / c);
            }
            else if ((double.IsInfinity(c) || double.IsInfinity(d)) &&
                    !(double.IsInfinity(a) || double.IsInfinity(b)))
                return Zero;

            double e, f;
            if (Math.Abs(d) < Math.Abs(c))
            {
                e = d / c;
                f = 1d / (c + d * e);
                return new((a + b * e) * f, (b - a * e) * f);
            }
            e = c / d;
            f = 1d / (d + c * e);
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
            if (b == 0)
            {
                if (a == 0 && (left != 0))
                    return PositiveInfinity;

                return new(left / a);
            }
            else if ((double.IsInfinity(a) || double.IsInfinity(b)) &&
                    !double.IsInfinity(left))
                return Zero;

            double e, f;
            if (Math.Abs(b) < Math.Abs(a))
            {
                e = b / a;
                f = 1d / (a + b * e);
                return new(left * f, -left * e * f);
            }
            e = a / b;
            f = 1d / (b + a * e);
            return new(left * e * f, -left * f);
        }

        public static Complex IntDiv(in Complex left, in Complex right) =>
            right.IsReal && right._a != 0 ?
            new(Math.Truncate(left._a / right._a), Math.Truncate(left._b / right._a)) :
            NaN;

        public static Complex operator %(in Complex left, in Complex right) =>
            right.IsReal ?
            new(left._a % right._a, left._b % right._a) :
            NaN;

        public static double operator ==(in Complex left, in Complex right) =>
            AlmostEquals(left, right) ? 1d : 0d;

        public static double operator !=(in Complex left, in Complex right) =>
            AlmostEquals(left, right) ? 0d : 1d;

        public static double operator <(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a < right._a && !left._a.AlmostEquals(right._a) ? 1d : 0d :
            double.NaN;

        public static double operator >(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a > right._a && !left._a.AlmostEquals(right._a) ? 1d : 0d :
            double.NaN;

        public static double operator <=(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a <= right._a || left._a.AlmostEquals(right._a) ? 1d : 0d :
            double.NaN;

        public static double operator >=(in Complex left, in Complex right) =>
            left.IsReal && right.IsReal ?
            left._a >= right._a || left._a.AlmostEquals(right._a) ? 1d : 0d :
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
            left._a.Equals(right._a) && left._b.Equals(right._b);

        private static bool AlmostEquals(in Complex left, in Complex right)
        {
            var leftType = left.Type();
            var rightType = right.Type();
            if (leftType == Types.Real && rightType == Types.Real)
                return left._a.AlmostEquals(right._a);

            if (leftType == Types.Imaginary && rightType == Types.Imaginary)
                return left._b.AlmostEquals(right._b);

            return left._a.AlmostEquals(right._a) && left._b.AlmostEquals(right._b);
        }

        public override string ToString() => IsReal ?
            new TextWriter(new() { Decimals = 15 }, false).FormatReal(_a, null, false) :
            new TextWriter(new() { Decimals = 15 }, false).FormatComplex(_a, _b, null);

        public override int GetHashCode() =>
            IsReal ? _a.GetHashCode() : HashCode.Combine(_a, _b);

        internal static Complex Abs(in Complex value) => Abs(value._a, value._b);

        internal static double Abs(double a, double b)
        {
            if (double.IsInfinity(a) || double.IsInfinity(b))
                return double.PositiveInfinity;

            var c = Math.Abs(a);
            if (b == 0d)
                return c;

            var d = Math.Abs(b);
            if (c > d)
            {
                var rc = d / c;
                return c * Math.Sqrt(1d + rc * rc);
            }
            var rd = c / d;
            return d * Math.Sqrt(1d + rd * rd);
        }

        internal static Complex Sign(in Complex value) =>
            value.IsReal ? Math.Sign(value._a) : double.NaN;

        private static void CheckTrigScope(double angle, string func)
        {
            if (angle < TrigMin || angle > TrigMax)
                throw Exceptions.ArgumentOutOfRange(func);
        }

        internal static double RealRandom(in double value) =>
            Rand.NextDouble() * value;

        internal static double RealSin(double value)
        {
            CheckTrigScope(value, "sin");
            return Math.Sin(value);
        }

        internal static double RealCos(double value)
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
            return new Complex(ta, thb) / new Complex(1d, -ta * thb);
        }

        internal static Complex Cot(in Complex value)
        {
            var ta = Math.Tan(value._a);
            if (value._b == 0d)
                return 1d / ta;

            var thb = Math.Tanh(value._b);
            return new Complex(1d, -ta * thb) / new Complex(ta, thb);
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
            return tb == 0d ?
                tha :
                new Complex(tha, tb) / new Complex(1d, tha * tb);
        }

        internal static Complex Coth(in Complex value)
        {
            var tha = Math.Tanh(value._a);
            var tb = Math.Tan(value._b);
            return tb == 0d ?
                1d / tha :
                new Complex(1d, tha * tb) / new Complex(tha, tb);
        }

        internal static Complex Asin(in Complex value) =>
            -ImaginaryOne * Log(ImaginaryOne * value + Sqrt(One - value * value));

        internal static Complex Acos(in Complex value) =>
            -ImaginaryOne * Log(value + ImaginaryOne * Sqrt(One - value * value));

        internal static Complex Atan(in Complex value) =>
            value.Equals(ImaginaryOne) ? new(0d, double.PositiveInfinity) :
            value.Equals(-ImaginaryOne) ? new(0d, double.NegativeInfinity) :
            -ImaginaryOne / 2d * Log((ImaginaryOne - value) / (ImaginaryOne + value));

        internal static Complex Acot(in Complex value) =>
            value.Equals(ImaginaryOne) ? new(0d, double.NegativeInfinity) :
            value.Equals(-ImaginaryOne) ? new(0d, double.PositiveInfinity) :
            -ImaginaryOne / 2d * Log((value + ImaginaryOne) / (value - ImaginaryOne));

        internal static Complex Asinh(in Complex value) =>
            Log(value + Sqrt(value * value + One));

        internal static Complex Acosh(in Complex value) =>
            Log(value + Sqrt(value - One) * Sqrt(value + One));

        internal static Complex Atanh(in Complex value) =>
            Log((One + value) / (One - value)) / 2d;

        internal static Complex Acoth(in Complex value) =>
            Log((value + One) / (value - One)) / 2d;

        internal static Complex Pow(in Complex value, double power)
        {
            if (power == 0)
                return 1d;

            var isInteger = double.IsInteger(power);
            if (value._b == 0 && (value._a > 0 || isInteger))
            {
                return value._a == 0 ?
                    0d :
                    Math.Pow(value._a, power);
            }
            if (power == 2d)
                return value * value;

            if (isInteger && power > 0 && power < 6)
            {
                var result = value;
                for (var i = 1; i < power; ++i)
                    result *= value;

                if (power < 0)
                    return 1d / result;

                return result;
            }
            var r = Abs(value._a, value._b);
            var theta = power * value.NormalPhase;
            var t = Math.Pow(r, power);
            return new(t * Math.Cos(theta), t * Math.Sin(theta));
        }

        internal static Complex Pow(in Complex value, in Complex power)
        {
            var c = power._a;
            var d = power._b;
            if (d == 0d)
                return Pow(value, c);

            var a = value._a;
            var b = value._b;
            if (a == 0d && b == 0d)
                return Zero;

            var r = Abs(value._a, value._b);
            var phi = value.NormalPhase;
            var theta = c * phi + d * Math.Log(r);
            var t = Math.Pow(r, c) * Math.Exp(-d * phi);
            return new(t * Math.Cos(theta), t * Math.Sin(theta));
        }

        internal static Complex Log(in Complex value) =>
            new(Math.Log(Abs(value._a, value._b)), value.Phase);

        internal static Complex Log10(in Complex value) =>
            new(Math.Log(Abs(value._a, value._b)) * Log10Inv, value.Phase * Log10Inv);

        internal static Complex Log2(in Complex value) =>
            new(Math.Log(Abs(value._a, value._b)) * Log2Inv, value.Phase * Log2Inv);

        internal static Complex Exp(in Complex value)
        {
            var r = Math.Exp(value._a);
            return new(r * Math.Cos(value._b), r * Math.Sin(value._b));
        }

        internal static Complex Sqrt(in Complex value)
        {
            var r = Math.Sqrt(Abs(value._a, value._b));
            var theta = value.NormalPhase / 2d;
            return new(r * Math.Cos(theta), r * Math.Sin(theta));
        }

        internal static Complex Cbrt(in Complex value)
        {
            var r = Math.Cbrt(Abs(value._a, value._b));
            var theta = value.NormalPhase / 3d;
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

        internal static string Format(in Complex c, int decimals, bool phasor, OutputWriter.OutputFormat mode)
        {
            var settings = new MathSettings() { Decimals = decimals };
            return mode switch
            {
                OutputWriter.OutputFormat.Text => new TextWriter(settings, phasor).FormatComplex(c._a, c._b, null),
                OutputWriter.OutputFormat.Html => new HtmlWriter(settings, phasor).FormatComplex(c._a, c._b, null),
                OutputWriter.OutputFormat.Xml => new XmlWriter(settings, phasor).FormatComplex(c._a, c._b, null),
                _ => "undefined format"
            };
        }
    }
}
