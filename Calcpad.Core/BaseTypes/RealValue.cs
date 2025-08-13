using System;

namespace Calcpad.Core
{
    internal readonly struct RealValue : IEquatable<RealValue>, IComparable<RealValue>, IScalarValue
    {
        internal const double LogicalZero = 1e-12;
        internal readonly double D;
        internal readonly Unit Units;
        internal readonly bool IsUnit;
        internal static readonly RealValue Zero = new(0d);
        internal static readonly RealValue One = new(1d);
        internal static readonly RealValue NaN = new(double.NaN);
        internal static readonly RealValue PositiveInfinity = new(double.PositiveInfinity);
        internal static readonly RealValue NegativeInfinity = new(double.NegativeInfinity);
        double IScalarValue.Re => D;
        double IScalarValue.Im => 0d;
        Unit IScalarValue.Units => Units;
        bool IScalarValue.IsUnit => IsUnit;
        bool IScalarValue.IsReal => true;
        bool IScalarValue.IsComplex => false;
        Complex IScalarValue.Complex => new(D, 0d);
        bool IScalarValue.IsComposite() => Unit.IsComposite(D, Units);
        RealValue IScalarValue.AsReal() => this;
        ComplexValue IScalarValue.AsComplex() => new(D, Units, IsUnit);
        internal RealValue(double number)
        {
            D = number;
        }

        internal RealValue(double number, Unit units)
        {
            D = number;
            Units = units;
        }

        internal RealValue(Unit units)
        {
            D = 1d;
            Units = units;
            IsUnit = true;
        }

        internal RealValue(in double number, Unit units, bool isUnit) : this(number, units)
        {
            IsUnit = isUnit;
        }

        public override int GetHashCode() => HashCode.Combine(D, Units);

        public override bool Equals(object obj)
        {
            if (obj is RealValue real)
                return Equals(real);

            return false;
        }

        public bool Equals(RealValue other)
        {
            if (Units is null)
                return other.Units is null &&
                    D.Equals(other.D);

            if (other.Units is null)
                return false;

            return D.Equals(other.D) &&
                Units.Equals(other.Units);
        }

        internal bool AlmostEquals(in RealValue other)
        {
            if (ReferenceEquals(Units, other.Units))
                return D.AlmostEquals(other.D);

            if (!Units.IsConsistent(other.Units))
                return false;

            var d = Units.ConvertTo(other.Units);
            return D.AlmostEquals(other.D * d);
        }

        public int CompareTo(RealValue other)
        {
            var d = Unit.Convert(Units, other.Units, ',');
            return D.CompareTo(other.D * d);
        }

        public override string ToString()
        {
            if (IsUnit)
                return Units.Text;

            var s = Units is null ? string.Empty : Units.Text;
            return $"{D}{s}";
        }

        internal static RealValue Abs(RealValue value) => new(Math.Abs(value.D), value.Units);

        internal static RealValue Sqrt(RealValue value)
        {
            var u = value.Units;
            double d;
            if (u is not null && u.IsDimensionless)
            {
                d = value.D * u.GetDimensionlessFactor();
                return new(Math.Sqrt(d));
            }
            d = Math.Sqrt(value.D);
            return u is null ?
                new(d) :
                new(d, u.Pow(0.5f));
        }

        internal static RealValue Pow2(RealValue value)
        {
            var u = value.Units;
            double d;
            if (u is not null && u.IsDimensionless)
            {
                d = value.D * u.GetDimensionlessFactor();
                return new(d * d);
            }

            d = value.D;
            return u is null ?
                new(d * d) :
                new(d * d, u.Pow(2));
        }


        internal bool IsComposite() => Unit.IsComposite(D, Units);

        public static RealValue operator -(RealValue a) => new(-a.D, a.Units, a.IsUnit);

        public static RealValue operator +(RealValue a, RealValue b) =>

            new(
                a.D + b.D * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        public static RealValue operator -(RealValue a, RealValue b) =>
            new(
                a.D - b.D * Unit.Convert(a.Units, b.Units, '-'),
                a.Units
            );

        public static RealValue operator *(RealValue a, RealValue b)
        {
            if (a.Units is null)
            {
                if (b.Units is not null && b.Units.IsDimensionless && !b.IsUnit)
                    return new(a.D * b.D * b.Units.GetDimensionlessFactor(), null);

                return new(a.D * b.D, b.Units);
            }
            var uc = Unit.Multiply(a.Units, b.Units, out var d);
            return new(a.D * b.D * d, uc);
        }

        public static RealValue Multiply(in RealValue a, in RealValue b)
        {
            if (a.Units is null)
            {
                if (b.Units is not null && b.Units.IsDimensionless && !b.IsUnit)
                    return new(a.D * b.D * b.Units.GetDimensionlessFactor(), null);

                return new(a.D * b.D, b.Units);
            }
            var uc = Unit.Multiply(a.Units, b.Units, out var d, b.IsUnit);
            var isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(a.D * b.D * d, uc, isUnit);
        }

        public static RealValue operator /(RealValue a, RealValue b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            return new(a.D / b.D * d, uc);
        }

        public static RealValue Divide(in RealValue a, in RealValue b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d, b.IsUnit);
            var isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(a.D / b.D * d, uc, isUnit);
        }

        public static RealValue operator *(RealValue a, double b) =>
            new(a.D * b, a.Units);

        public static RealValue operator %(RealValue a, RealValue b)
        {
            if (b.Units is not null)
                throw Exceptions.CannotEvaluateRemainder(Unit.GetText(a.Units), Unit.GetText(b.Units));

            return new(a.D % b.D, a.Units);
        }

        internal static RealValue IntDiv(in RealValue a, in RealValue b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            bool isUnit = a.IsUnit && b.IsUnit && uc is not null;
            var c = b.D == 0d ?
                double.NaN :
                Math.Truncate(a.D / b.D * d);
            return new(c, uc, isUnit);
        }

        public static RealValue operator ==(RealValue a, RealValue b) =>
            a.D.AlmostEquals(b.D * Unit.Convert(a.Units, b.Units, '≡')) ? One : Zero;

        public static RealValue operator !=(RealValue a, RealValue b) =>
            a.D.AlmostEquals(b.D * Unit.Convert(a.Units, b.Units, '≠')) ? Zero : One;

        public static RealValue operator <(RealValue a, RealValue b)
        {
            var c = a.D;
            var d = b.D * Unit.Convert(a.Units, b.Units, '<');
            return c < d && !c.AlmostEquals(d) ? One : Zero;
        }

        public static RealValue operator >(RealValue a, RealValue b)
        {
            var c = a.D;
            var d = b.D * Unit.Convert(a.Units, b.Units, '>');
            return c > d && !c.AlmostEquals(d) ? One : Zero;
        }

        public static RealValue operator <=(RealValue a, RealValue b)
        {
            var c = a.D;
            var d = b.D * Unit.Convert(a.Units, b.Units, '≤');
            return c <= d || c.AlmostEquals(d) ? One : Zero;
        }

        public static RealValue operator >=(RealValue a, RealValue b)
        {
            var c = a.D;
            var d = b.D * Unit.Convert(a.Units, b.Units, '≥');
            return c >= d || c.AlmostEquals(d) ? One : Zero;
        }

        public static RealValue operator &(RealValue a, RealValue b) =>
            Math.Abs(a.D) < LogicalZero || Math.Abs(b.D) < LogicalZero ? Zero : One;

        public static RealValue operator |(RealValue a, RealValue b) =>
            Math.Abs(a.D) >= LogicalZero || Math.Abs(b.D) >= LogicalZero ? One : Zero;

        public static RealValue operator ^(RealValue a, RealValue b) =>
            (Math.Abs(a.D) >= LogicalZero) != (Math.Abs(b.D) >= LogicalZero) ? One : Zero;
    }
}