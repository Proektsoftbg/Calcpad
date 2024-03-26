using System;

namespace Calcpad.Core
{
    internal readonly struct Value : IEquatable<Value> 
    {
        internal const double LogicalZero = 1e-12;
        internal readonly double Re;
        internal readonly double Im;
        internal readonly Unit Units;
        internal readonly bool IsUnit;
        internal static readonly Value Zero = new(0d);
        internal static readonly Value One = new(1d);
        internal static readonly Value NaN = new(double.NaN);
        internal static readonly Value PositiveInfinity = new(double.PositiveInfinity);
        internal static readonly Value NegativeInfinity = new(double.NegativeInfinity);
        internal static readonly Value ComplexInfinity = new(double.PositiveInfinity, double.PositiveInfinity, null);

        internal Value(double re, double im, Unit units)
        {
            Re = re;
            Im = im;
            Units = units;
        }

        internal Value(in Complex number, Unit units) : this(number.Re, number.Im, units) { }

        internal Value(double number)
        {
            Re = number;
        }

        internal Value(double number, Unit units)
        {
            Re = number;
            Units = units;
        }

        internal Value(in Complex number)
        {
            Re = number.Re;
            Im = number.Im;
        }

        internal Value(Unit units)
        {
            Re = 1d;
            Units = units;
            IsUnit = true;
        }

        internal Value(double re, double im, Unit units, bool isUnit) : this(re, im, units)
        {
            IsUnit = isUnit;
        }

        internal Value(in Complex number, Unit units, bool isUnit) : this(number.Re, number.Im, units)
        {
            IsUnit = isUnit;
        }

        public override int GetHashCode() => HashCode.Combine(Re, Im, Units);

        public override bool Equals(object obj)
        {
            if (obj is Value v)
                return Equals(v);

            return false;
        }

        internal Complex Complex => new(Re, Im);

        internal bool IsComposite() => Unit.IsComposite(Re, Units);

        public static Value operator -(Value a) => new(-a.Re, a.Units, a.IsUnit);

        public static Value operator +(Value a, Value b) =>
            new(
                a.Re + b.Re * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        public static Value operator -(Value a, Value b) =>
            new(
                a.Re - b.Re * Unit.Convert(a.Units, b.Units, '-'),
                a.Units
            );

        public static Value operator *(Value a, Value b)
        {
            if (a.Units is null)
                return new(a.Re * b.Re, b.Units);

            var uc = Unit.Multiply(a.Units, b.Units, out var d);
            return new(a.Re * b.Re * d, uc);
        }

        public static Value Multiply(in Value a, in Value b)
        {
            if (a.Units is null)
                return new(a.Re * b.Re, b.Units);

            var uc = Unit.Multiply(a.Units, b.Units, out var d, b.IsUnit);
            var isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(a.Re * b.Re * d, uc, isUnit);
        }

        public static Value operator /(Value a, Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            return new(a.Re / b.Re * d, uc);
        }

        public static Value Divide(in Value a, in Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d, b.IsUnit);
            var isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(a.Re / b.Re * d, uc, isUnit);
        }

        public static Value operator *(Value a, double b) =>
            new(a.Re * b, a.Units);

        public static Value operator %(Value a, Value b)
        {
            if (b.Units is not null)
                Throw.CannotEvaluateRemainderException(Unit.GetText(a.Units), Unit.GetText(b.Units));

            return new(a.Re % b.Re, a.Units);
        }

        public static Value IntDiv(in Value a, in Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            bool isUnit = a.IsUnit && b.IsUnit && uc is not null;
            var c = b.Re == 0d ?
                double.NaN :
                Math.Truncate(a.Re / b.Re * d);
            return new(c, uc, isUnit);
        }

        public static Value operator ==(Value a, Value b) =>
            a.Re.EqualsBinary(b.Re * Unit.Convert(a.Units, b.Units, '≡')) ? One : Zero;

        public static Value operator !=(Value a, Value b) =>
            a.Re.EqualsBinary(b.Re * Unit.Convert(a.Units, b.Units, '≠')) ? Zero : One;

        public static Value operator <(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '<');
            return c < d && !c.EqualsBinary(d) ? One : Zero;
        }

        public static Value operator >(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '>');
            return c > d && !c.EqualsBinary(d) ? One : Zero;
        }

        public static Value operator <=(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '≤');
            return c <= d || c.EqualsBinary(d) ? One : Zero;
        }

        public static Value operator >=(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '≥');
            return c >= d || c.EqualsBinary(d) ? One : Zero;
        }

        public static Value operator &(Value a, Value b) =>
            Math.Abs(a.Re) < LogicalZero || Math.Abs(b.Re) < LogicalZero ? Zero : One;

        public static Value operator |(Value a, Value b) =>
            Math.Abs(a.Re) >= LogicalZero || Math.Abs(b.Re) >= LogicalZero ? One : Zero;

        public static Value operator ^(Value a, Value b) =>
            (Math.Abs(a.Re) >= LogicalZero) != (Math.Abs(b.Re) >= LogicalZero) ? One : Zero;


        public bool Equals(Value other)
        {
            if (Units is null)
                return other.Units is null &&
                    Re.Equals(other.Re) &&
                    Im.Equals(other.Im);

            if (other.Units is null)
                return false;

            return Re.Equals(other.Re) &&
                Im.Equals(other.Im) &&
                Units.Equals(other.Units);
        }

        internal bool IsReal => Complex.GetType(Re, Im) == Complex.Types.Real;
        internal bool IsComplex => Complex.GetType(Re, Im) == Complex.Types.Complex;
    }
}