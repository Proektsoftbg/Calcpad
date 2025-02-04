using System;
using static Calcpad.Core.Throw;
using System.Numerics;

namespace Calcpad.Core
{
    internal readonly struct ComplexValue : IEquatable<ComplexValue>, IComparable<ComplexValue>, IScalarValue
    {
        internal const double LogicalZero = 1e-12;
        internal readonly double A;
        internal readonly double B;
        internal readonly Unit Units;
        internal readonly bool IsUnit;
        internal static readonly ComplexValue ComplexInfinity = new(double.PositiveInfinity, double.PositiveInfinity, null);
        double IScalarValue.Re => A;
        double IScalarValue.Im => B;
        Unit IScalarValue.Units => Units;
        bool IScalarValue.IsUnit => IsUnit;
        bool IScalarValue.IsReal => IsReal;
        bool IScalarValue.IsComplex => IsComplex;
        Complex IScalarValue.Complex => Complex;
        RealValue IScalarValue.AsReal() => new(A, Units);
        ComplexValue IScalarValue.AsComplex() => this;
        bool IScalarValue.IsComposite() => Unit.IsComposite(A, Units);

        internal bool IsReal => Complex.GetType(A, B) == Complex.Types.Real;
        internal bool IsComplex => Complex.GetType(A, B) == Complex.Types.Complex;
        internal Complex Complex => new(A, B);

        internal ComplexValue(double re, double im, Unit units)
        {
            A = re;
            B = im;
            Units = units;
        }

        internal ComplexValue(in Complex number, Unit units) : this(number.Re, number.Im, units) { }

        internal ComplexValue(double number)
        {
            A = number;
        }

        internal ComplexValue(double number, Unit units)
        {
            A = number;
            Units = units;
        }

        internal ComplexValue(in Complex number)
        {
            A = number.Re;
            B = number.Im;
        }

        internal ComplexValue(Unit units)
        {
            A = 1d;
            Units = units;
            IsUnit = true;
        }

        internal ComplexValue(double re, double im, Unit units, bool isUnit) : this(re, im, units)
        {
            IsUnit = isUnit;
        }

        internal ComplexValue(in Complex number, Unit units, bool isUnit) : this(number.Re, number.Im, units)
        {
            IsUnit = isUnit;
        }

        public override int GetHashCode() => HashCode.Combine(A, B, Units);

        public override bool Equals(object obj)
        {
            if (obj is ComplexValue complex)
                return Equals(complex);

            return false;
        }

        public bool Equals(ComplexValue other)
        {
            if (Units is null)
                return other.Units is null &&
                    A.Equals(other.A) &&
                    B.Equals(other.B);

            if (other.Units is null)
                return false;

            return A.Equals(other.A) &&
                B.Equals(other.B) &&
                Units.Equals(other.Units);
        }

        internal bool IsZero => A == 0d && B == 0d;

        internal bool AlmostEquals(in ComplexValue other)
        {
            if (ReferenceEquals(Units, other.Units))
                return A.AlmostEquals(other.A) && B.AlmostEquals(other.B);

            if (!Units.IsConsistent(other.Units))
                return false;

            var d = Units.ConvertTo(other.Units);
            return A.AlmostEquals(other.A * d) &&
                B.AlmostEquals(other.B * d);
        }

        //For complex numbers the real parts are ordered first and 
        //then the imaginary parts if real are euals (lexicographic ordering)   
        //Although it is not strictly correct mathematically, 
        //it is useful for practical sorting in many cases. 

        public int CompareTo(ComplexValue other)
        {
            var d = Unit.Convert(Units, other.Units, ',');
            var result = A.CompareTo(other.A * d);
            return result == 0 ? B.CompareTo(other.B * d) : result;
        }

        public override string ToString()
        {
            var s = Units is null ? string.Empty : " " + Units.Text;
            return $"{A}{B: + 0i; - 0i;#}{s}";
        }

        public static ComplexValue operator -(ComplexValue value) => new(-value.A, -value.B, value.Units, value.IsUnit);

        public static ComplexValue operator +(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex + b.Complex * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        public static ComplexValue operator -(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex - b.Complex * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        public static ComplexValue operator *(ComplexValue a, ComplexValue b)
        {
            if (a.Units is null)
            {
                if (b.Units is not null && b.Units.IsDimensionless && !b.IsUnit)
                    return new(a.Complex * b.Complex * b.Units.GetDimensionlessFactor(), null);

                return new(a.Complex * b.Complex, b.Units);
            }
            var uc = Unit.Multiply(a.Units, b.Units, out var d, b.IsUnit);
            var c = a.Complex * b.Complex * d;
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        public static ComplexValue operator *(ComplexValue a, double b) =>
            new(a.A * b, a.B * b, a.Units);

        public static ComplexValue operator /(ComplexValue a, ComplexValue b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d, b.IsUnit);
            var c = a.Complex / b.Complex * d;
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        public static ComplexValue operator %(ComplexValue a, ComplexValue b)
        {
            if (b.Units is not null)
                Throw.RemainderUnitsException(Unit.GetText(a.Units), Unit.GetText(b.Units));

            return new(a.Complex % b.Complex, a.Units);
        }

        internal static ComplexValue IntDiv(in ComplexValue a, in ComplexValue b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            var c = Complex.IntDiv(a.Complex * d, b.Complex);
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        public static ComplexValue operator ==(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex == b.Complex * Unit.Convert(a.Units, b.Units, '≡')
            );

        public static ComplexValue operator !=(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex != b.Complex * Unit.Convert(a.Units, b.Units, '≠')
            );

        public static ComplexValue operator <(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex < b.Complex * Unit.Convert(a.Units, b.Units, '<')
            );

        public static ComplexValue operator >(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex > b.Complex * Unit.Convert(a.Units, b.Units, '>')
            );

        public static ComplexValue operator <=(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex <= b.Complex * Unit.Convert(a.Units, b.Units, '≤')
            );

        public static ComplexValue operator  >=(ComplexValue a, ComplexValue b) =>
            new(
                a.Complex >= b.Complex * Unit.Convert(a.Units, b.Units, '≥')
            );

        public static RealValue operator &(ComplexValue a, ComplexValue b) =>
            Math.Abs(a.A) < LogicalZero || Math.Abs(b.A) < LogicalZero ? RealValue.Zero : RealValue.One;

        public static RealValue operator |(ComplexValue a, ComplexValue b) =>
            Math.Abs(a.A) >= LogicalZero || Math.Abs(b.A) >= LogicalZero ? RealValue.One : RealValue.Zero;

        public static RealValue operator ^(ComplexValue a, ComplexValue b) =>
            (Math.Abs(a.A) >= LogicalZero) != (Math.Abs(b.A) >= LogicalZero) ? RealValue.One : RealValue.Zero;

        public static implicit operator ComplexValue(RealValue value) => new(value.D, value.Units);
        public static explicit operator RealValue(ComplexValue value)
        {
            if (!value.IsReal)
                Throw.MustBeRealException(Throw.Items.Value);

            return new(value.A, value.Units);
        }
    }
}