using System;

namespace Calcpad.Core
{
    internal readonly struct Value : IEquatable<Value>
    {
        internal readonly Complex Number;
        internal readonly Unit Units;
        internal readonly bool IsUnit;
        internal static readonly Value Zero;
        internal static readonly Value One = new(1.0);

        internal Value(in Complex number, Unit units)
        {
            Number = number;
            Units = units;
            IsUnit = false;
        }

        internal Value(double number)
        {
            Number = number;
            Units = null;
            IsUnit = false;
        }

        internal Value(double number, Unit units)
        {
            Number = number;
            Units = units;
            IsUnit = false;
        }

        internal Value(in Complex number)
        {
            Number = number;
            Units = null;
            IsUnit = false;
        }

        internal Value(Unit units)
        {
            Number = Complex.One;
            Units = units;
            IsUnit = true;
        }

        internal Value(in Complex number, Unit units, bool isUnit) : this(number, units)
        {
            IsUnit = isUnit;
        }

        public override int GetHashCode() => HashCode.Combine(Number, Units);

        public override bool Equals(object obj)
        {
            if (obj is Value v)
                return Equals(v);

            return false;
        }

        internal bool IsComposite() => Unit.IsComposite(Number.Re, Units);

        public static Value operator -(Value a) => new(-a.Number.Re, a.Units, a.IsUnit);

        public static Value operator +(Value a, Value b) =>
            new(
                a.Number.Re + b.Number.Re * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        public static Value operator -(Value a, Value b) =>
            new(
                a.Number.Re - b.Number.Re * Unit.Convert(a.Units, b.Units, '-'),
                a.Units
            );

        public static Value operator *(Value a, Value b)
        {
            if (a.Units is null && b.IsUnit)
                return new(a.Number, b.Units);

            var uc = Unit.Multiply(a.Units, b.Units, out var d);
            return new(a.Number.Re * b.Number.Re * d, uc);
        }

        public static Value Multiply(Value a, Value b)
        {
            if (a.Units is null && b.IsUnit)
                return new(a.Number, b.Units);

            var uc = Unit.Multiply(a.Units, b.Units, out var d, b.IsUnit);
            var isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(a.Number.Re * b.Number.Re * d, uc, isUnit);
        }

        public static Value operator /(Value a, Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            return new(a.Number.Re / b.Number.Re * d, uc);
        }

        public static Value Divide(Value a, Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d, b.IsUnit);
            var isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(a.Number.Re / b.Number.Re * d, uc, isUnit);
        }

        public static Value operator *(Value a, double b) =>
            new(a.Number * b, a.Units);

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
            return new(a.Number.Re % b.Number.Re, a.Units);
        }

        public static Value IntDiv(Value a, Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            bool isUnit = a.IsUnit && b.IsUnit && uc is not null;
            return new(Math.Truncate(a.Number.Re * d / b.Number.Re), uc, isUnit);
        }

        public static Value operator ==(Value a, Value b) =>
            new(
                a.Number.Re.EqualsBinary(b.Number.Re * Unit.Convert(a.Units, b.Units, '≡')) ? 1.0 : 0.0
            );

        public static Value operator !=(Value a, Value b) =>
            new(
                a.Number.Re.EqualsBinary(b.Number.Re * Unit.Convert(a.Units, b.Units, '≠')) ? 0.0 : 1.0
            );

        public static Value operator <(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * Unit.Convert(a.Units, b.Units, '<');
            return new(
                c < d && !c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public static Value operator >(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * Unit.Convert(a.Units, b.Units, '>');
            return new(
                c > d && !c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public static Value operator <=(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * Unit.Convert(a.Units, b.Units, '≤');
            return new(
                c <= d || c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public static Value operator >=(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * Unit.Convert(a.Units, b.Units, '≥');
            return new(
                c >= d || c.EqualsBinary(d) ? 1.0 : 0.0
            );
        }

        public bool Equals(Value other)
        {
            if (Units is null)
                return other.Units is null && Number.Equals(other.Number);

            if (other.Units is null)
                return false;

            return Number.Equals(other.Number) && Units.Equals(other.Units);
        }
    }
}