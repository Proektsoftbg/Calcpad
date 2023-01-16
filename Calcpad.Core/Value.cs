using System;

namespace Calcpad.Core
{
    internal readonly struct Value : IEquatable<Value>
    {
        internal readonly double Re;
        internal readonly double Im;
        internal readonly Unit Units;
        internal readonly bool IsUnit;
        internal static readonly Value Zero;
        internal static readonly Value One = new(1.0);

        internal Value(double re, double im, Unit units)
        {
            Re = re;
            Im = im;
            Units = units;
            IsUnit = false;
        }

        internal Value(in Complex number, Unit units) : this(number.Re, number.Im, units) { }

        internal Value(double number)
        {
            Re = number;
            Im = 0.0;
            Units = null;
            IsUnit = false;
        }

        internal Value(double number, Unit units)
        {
            Re = number;
            Im = 0.0;
            Units = units;
            IsUnit = false;
        }

        internal Value(in Complex number)
        {
            Re = number.Re;
            Im = number.Im;
            Units = null;
            IsUnit = false;
        }

        internal Value(Unit units)
        {
            Re = 1.0;
            Im = 0.0;
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
            if (a.Units is null && b.IsUnit)
                return new(a.Re * b.Re, b.Units);

            var uc = Unit.Multiply(a.Units, b.Units, out var d);
            return new(a.Re * b.Re * d, uc);
        }

        public static Value Multiply(Value a, Value b)
        {
            if (a.Units is null && b.IsUnit)
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

        public static Value Divide(Value a, Value b)
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
                throw new MathParser.MathParserException(
#if BG
                    $"Не мога да изчисля остатъка: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". Делителя трябва да е бездименсионен."
#else
                    $"Cannot evaluate reminder: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". The denominator must be unitless."
#endif
            );
            return new(a.Re % b.Re, a.Units);
        }

        public static Value IntDiv(Value a, Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            bool isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(Math.Truncate(a.Re * d / b.Re), uc, isUnit);
        }

        public static Value operator ==(Value a, Value b) =>
            new(
                a.Re.EqualsBinary(b.Re * Unit.Convert(a.Units, b.Units, '≡')) ? 1.0 : 0.0
            );

        public static Value operator !=(Value a, Value b) =>
            new(
                a.Re.EqualsBinary(b.Re * Unit.Convert(a.Units, b.Units, '≠')) ? 0.0 : 1.0
            );

        public static Value operator <(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '<');
            return new(
                c < d && !c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public static Value operator >(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '>');
            return new(
                c > d && !c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public static Value operator <=(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '≤');
            return new(
                c <= d || c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public static Value operator >=(Value a, Value b)
        {
            var c = a.Re;
            var d = b.Re * Unit.Convert(a.Units, b.Units, '≥');
            return new(
                c >= d || c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public bool Equals(Value other)
        {
            if (Units is null)
                return other.Units is null &&
                    Re.Equals(other.Re) &&
                    Im.Equals(other.Im); ;

            if (other.Units is null)
                return false;

            return Re.Equals(other.Re) &&
                Im.Equals(other.Im) &&
                (ReferenceEquals(Units, other.Units) || Units.Equals(other.Units));
        }

        internal bool IsReal => Complex.Type(Re, Im) == Complex.Types.Real;
        internal bool IsComplex => Complex.Type(Re, Im) == Complex.Types.Complex;
    }
}