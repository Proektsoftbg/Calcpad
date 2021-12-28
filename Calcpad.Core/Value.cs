using System;

namespace Calcpad.Core
{
    internal readonly struct Value : IEquatable<Value>
    {
        private static readonly char[] CompositeUnitChars = {'/', '*', '×', '^'};
        internal readonly Complex Number;
        internal readonly Unit Units;
        internal readonly bool IsUnit;
        internal static readonly Value Zero;

        internal Value(in Complex number, Unit units)
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

        private Value(in Complex number, Unit units, bool isUnit) : this(number, units)
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

        internal bool IsComposite() =>
            !Unit.IsNullOrEmpty(Units) && 
            (   
                Number.Re > 0.0 && 
                Number.Re != 1.0 || 
                Units.Text.IndexOfAny(CompositeUnitChars) >= 0.0
            );

        public static Value Fact(in Value a)
        {
            if (!Unit.IsNullOrEmpty(a.Units))
#if BG
                throw new MathParser.MathParserException("Аргументът на функцията n! трябва да е бездименсионен.");
#else
                throw new MathParser.MathParserException("The argument of the n! function must be unitless.");
#endif

            return new Value(Complex.Fact(a.Number));
        }

        public static Value operator -(Value a) => new(-a.Number.Re, a.Units, a.IsUnit);

        public static Value operator +(Value a, Value b) =>
            new(
                a.Number.Re + b.Number.Re * ConvertUnits(a.Units, b.Units, '+'),
                a.Units
            );

        public static Value operator -(Value a, Value b) =>
            new(
                a.Number.Re - b.Number.Re * ConvertUnits(a.Units, b.Units, '-'),
                a.Units
            );

        public static Value operator *(Value a, Value b)
        {
            if (a.Units is null && b.IsUnit)
                return new Value(a.Number, b.Units);

            var uc = MultiplyUnits(a.Units, b.Units, out var d);
            return new Value(a.Number.Re * b.Number.Re * d, uc);
        }

        public static Value Multiply(Value a, Value b)
        {
            if (a.Units is null && b.IsUnit)
                return new Value(a.Number, b.Units);

            var uc = MultiplyUnits(a.Units, b.Units, out var d, b.IsUnit);
            return new Value(a.Number.Re * b.Number.Re * d, uc, a.IsUnit && b.IsUnit);
        }

        public static Value operator /(Value a, Value b)
        {
            var uc = DivideUnits(a, b, out var d);
            return new Value(a.Number.Re / b.Number.Re * d, uc);
        }

        public static Value Divide(Value a, Value b)
        {
            var uc = DivideUnits(a, b, out var d, true);
            return new Value(a.Number.Re / b.Number.Re * d, uc, a.IsUnit && b.IsUnit);
        }

        public static Value operator *(Value a, double b) =>
            new(a.Number * b, a.Units);

        public static Value operator %(Value a, Value b)
        {
            if (!Unit.IsNullOrEmpty(b.Units))
                throw new MathParser.MathParserException(
#if BG
                    $"Не мога да изчисля остатъка: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". Делителя трябва да е бездименсионен."
#else
                    $"Cannot evaluate reminder: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". The denominator must be unitless."
#endif
            );
            return new Value(a.Number.Re % b.Number.Re, a.Units);
        }

        public static Value IntDiv(Value a, Value b)
        {
            var uc = DivideUnits(a, b, out var d);
            return new Value(Math.Truncate(a.Number.Re * d / b.Number.Re), uc, a.IsUnit && b.IsUnit);
        }

        public static Value operator ==(Value a, Value b) =>
            new(
                Complex.EqualsBinary(a.Number.Re, b.Number.Re * ConvertUnits(a.Units, b.Units, '≡')) ? 1.0 : 0.0
            );

        public static Value operator !=(Value a, Value b) =>
            new(
                Complex.EqualsBinary(a.Number.Re, b.Number.Re * ConvertUnits(a.Units, b.Units, '≡')) ? 0.0 : 1.0
            );

        public static Value operator <(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * ConvertUnits(a.Units, b.Units, '<');
            return new(
                c < d && !Complex.EqualsBinary(c, d) ? 1.0 : 0.0
            );        
        }

        public static Value operator >(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * ConvertUnits(a.Units, b.Units, '<');
            return new(
                c > d && !Complex.EqualsBinary(c, d) ? 1.0 : 0.0
            );
        }

        public static Value operator <=(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * ConvertUnits(a.Units, b.Units, '<');
            return new(
                c <= d || Complex.EqualsBinary(c, d) ? 1.0 : 0.0
            );
        }

        public static Value operator >=(Value a, Value b)
        {
            var c = a.Number.Re;
            var d = b.Number.Re * ConvertUnits(a.Units, b.Units, '<');
            return new(
                c >= d || Complex.EqualsBinary(c, d) ? 1.0 : 0.0
            );
        }

        internal static Value Abs(in Value value) =>
            new(Math.Abs(value.Number.Re), value.Units);

        internal static Value Sign(in Value value) =>
            new(Math.Sign(value.Number.Re));

        internal static Value Sin(in Value value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new Value(Complex.RealSin(value.Number.Re));
        }

        internal static Value Cos(in Value value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new Value(Complex.RealCos(value.Number.Re));
        }

        internal static Value Tan(in Value value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new Value(Math.Tan(value.Number.Re));
        }

        internal static Value Csc(in Value value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new Value(1 / Math.Sin(value.Number.Re));
        }

        internal static Value Sec(in Value value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new Value(1 / Math.Cos(value.Number.Re));
        }

        internal static Value Cot(in Value value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new Value(1 / Math.Tan(value.Number.Re));
        }

        internal static Value Sinh(in Value value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new Value(Math.Sinh(value.Number.Re));
        }

        internal static Value Cosh(in Value value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new Value(Math.Cosh(value.Number.Re));
        }

        internal static Value Tanh(in Value value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new Value(Math.Tanh(value.Number.Re));
        }

        internal static Value Csch(in Value value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new Value(1 / Math.Sinh(value.Number.Re));
        }

        internal static Value Sech(in Value value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new Value(1 / Math.Cosh(value.Number.Re));
        }

        internal static Value Coth(in Value value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new Value(1 / Math.Tanh(value.Number.Re));
        }

        internal static Value Asin(in Value value)
        {
            CheckFunctionUnits("asin", value.Units);
            return new Value(Math.Asin(value.Number.Re));
        }

        internal static Value Acos(in Value value)
        {
            CheckFunctionUnits("acos", value.Units);
            return new Value(Math.Acos(value.Number.Re));
        }

        internal static Value Atan(in Value value)
        {
            CheckFunctionUnits("atan", value.Units);
            return new Value(Math.Atan(value.Number.Re));
        }

        internal static Value Acsc(in Value value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return new Value(Math.Asin(1 / value.Number.Re));
        }

        internal static Value Asec(in Value value)
        {
            CheckFunctionUnits("asec", value.Units);
            return new Value(Math.Acos(1 / value.Number.Re));
        }

        internal static Value Acot(in Value value)
        {
            CheckFunctionUnits("acot", value.Units);
            return new Value(Math.Atan(1 / value.Number.Re));
        }

        internal static Value Asinh(in Value value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new Value(Math.Asinh(value.Number.Re));
        }

        internal static Value Acosh(in Value value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new Value(Math.Acosh(value.Number.Re));
        }

        internal static Value Atanh(in Value value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new Value(Math.Atanh(value.Number.Re));
        }

        internal static Value Acsch(in Value value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return new Value(Math.Asinh(1 / value.Number.Re));
        }

        internal static Value Asech(in Value value)
        {
            CheckFunctionUnits("asech", value.Units);
            return new Value(Math.Acosh(1 / value.Number.Re));
        }

        internal static Value Acoth(in Value value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new Value(Math.Atanh(1 / value.Number.Re));
        }

        internal static Value Log(in Value value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new Value(Math.Log(value.Number.Re));
        }

        internal static Value Log10(in Value value)
        {
            CheckFunctionUnits("log", value.Units);
            return new Value(Math.Log10(value.Number.Re));
        }

        internal static Value Log2(in Value value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new Value(Math.Log2(value.Number.Re));
        }

        internal static Value Pow(Value value, Value power) =>
            new(
                Math.Pow(value.Number.Re, power.Number.Re), 
                PowUnits(value.Units, power)
            );

        internal static Value UnitPow(Value value, Value power) =>
            new(
                Math.Pow(value.Number.Re, power.Number.Re),
                PowUnits(value.Units, power, value.IsUnit),
                value.IsUnit
            );

        internal static Value Sqrt(in Value value) => value.Units is null ?
                new (Math.Sqrt(value.Number.Re)) :
                new (Math.Sqrt(value.Number.Re), RootUnits(value.Units, 2));

        internal static Value UnitSqrt(in Value value) =>value.Units is null ?
                new (Math.Sqrt(value.Number.Re)) :
                new (Math.Sqrt(value.Number.Re), RootUnits(value.Units, 2, value.IsUnit), value.IsUnit);

        internal static Value Cbrt(in Value value) =>
            value.Units is null ?
                new (Math.Cbrt(value.Number.Re)) :
                new (Math.Cbrt(value.Number.Re), RootUnits(value.Units, 3));

        internal static Value UnitCbrt(in Value value) =>
            value.Units is null ?
                new (Math.Cbrt(value.Number.Re)) :
                new (Math.Cbrt(value.Number.Re), RootUnits(value.Units, 3, value.IsUnit), value.IsUnit);

        internal static Value Root(Value value, Value root)
        {
            var n = GetRoot(root);
            var result = Math.Pow(value.Number.Re, 1.0 / n);
            return value.Units is null ?
                new (result) :
                new (result, RootUnits(value.Units, n));
        }

        internal static Value UnitRoot(Value value, Value root)
        {
            var n = GetRoot(root);
            var result = Math.Pow(value.Number.Re, 1.0 / n);
            return value.Units is null ?
                new (result) :
                new (result, RootUnits(value.Units, n, value.IsUnit), value.IsUnit);
        }

        internal static Value Round(in Value value) =>
            new(Math.Round(value.Number.Re), value.Units);

        internal static Value Floor(in Value value) =>
            new(Math.Floor(value.Number.Re), value.Units);

        internal static Value Ceiling(in Value value) =>
            new(Math.Ceiling(value.Number.Re), value.Units);

        internal static Value Truncate(in Value value) =>
            new(Math.Truncate(value.Number.Re), value.Units);

        internal static Value Random(in Value value) =>
            new(Complex.RealRandom(value.Number.Re), value.Units);

        internal static Value Atan2(Value a, Value b) =>
            new(
                Math.Atan2(b.Number.Re * ConvertUnits(a.Units, b.Units, ','), a.Number.Re )
            );

        internal static Value MandelbrotSet(Value a, Value b) =>
            new(
                Complex.MandelbrotSet(
                    a.Number, b.Number * ConvertUnits(a.Units, b.Units, ',')
                )
            );

        internal static Value Min(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                var b = a[i].Number.Re * ConvertUnits(u, a[i].Units, ',');
                if (b < result)
                    result = b;
            }
            return new(result, u);
        }

        internal static Value Max(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                var b = a[i].Number.Re * ConvertUnits(u, a[i].Units, ',');
                if (b > result)
                    result = b;
            }
            return new(result, u);
        }

        internal static Value Sum(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
                result += a[i].Number.Re * ConvertUnits(u, a[i].Units, ',');

            return new(result, u);
        }

        internal static Value SumSq(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            result *= result;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                var b = a[i].Number.Re * ConvertUnits(u, a[i].Units, ',');
                result += b * b;
            }
            return new(result, u is null ? null : u * u);
        }

        internal static Value Srss(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            result *= result;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                var b = a[i].Number.Re * ConvertUnits(u, a[i].Units, ',');
                result += b * b;
            }
            return new(Math.Sqrt(result), u);
        }

        internal static Value Average(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
                result += a[i].Number.Re * ConvertUnits(u, a[i].Units, ',');

            return new(result / a.Length, u);
        }

        internal static Value Product(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                u = MultiplyUnits(u, a[i].Units, out var b);
                result *= a[i].Number.Re * b;
            }
            return new(result, u);
        }

        internal static Value Mean(Value[] a)
        {
            var result = a[0].Number.Re;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                u = MultiplyUnits(u, a[i].Units, out var b, a[i].IsUnit);
                result *= a[i].Number.Re * b;
            }
            if (u is not null)
                u = RootUnits(u, a.Length);

            return new(Math.Pow(result, 1.0 / a.Length), u);
        }

        internal static Value Real(in Value value) =>
            new(value.Number.Real, value.Units);

        internal static Value Imaginary(in Value value) =>
            new(value.Number.Imaginary, value.Units);

        internal static Value Phase(in Value value) =>
            new(value.Number.Phase);

        public static Value ComplexNegate(in Value a) => new(-a.Number, a.Units, a.IsUnit);

        public static Value ComplexAdd(Value a, Value b) =>
            new(
                a.Number + b.Number * ConvertUnits(a.Units, b.Units, '+'),
                a.Units
            );

        public static Value ComplexSubtract(Value a, Value b) =>
            new(
                a.Number - b.Number * ConvertUnits(a.Units, b.Units, '+'),
                a.Units
            );

        public static Value ComplexMultiply(Value a, Value b)
        {
            var uc = MultiplyUnits(a.Units, b.Units, out var d, b.IsUnit);
            var c = a.Number * b.Number * d;
            return new Value(c, uc, a.IsUnit && b.IsUnit);
        }

        public static Value ComplexDivide(Value a, Value b)
        {
            var uc = DivideUnits(a, b, out var d, true);
            var c = a.Number / b.Number * d;
            return new Value(c, uc, a.IsUnit && b.IsUnit);
        }

        public static Value ComplexReminder(Value a, Value b)
        {
            if (!Unit.IsNullOrEmpty(b.Units))
                throw new MathParser.MathParserException(
#if BG
                    $"Не мога да изчисля остатъка: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". Делителя трябва да е бездименсионен."
#else
                    $"Cannot evaluate reminder: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". Denominator must be unitless."
#endif
                );
            return new Value(a.Number % b.Number, a.Units);
        }

        public static Value ComplexIntDiv(Value a, Value b)
        {
            var uc = DivideUnits(a, b, out var d);
            var c = Complex.IntDiv(a.Number * d, b.Number);
            return new Value(c, uc, a.IsUnit && b.IsUnit);
        }

        public static Value ComplexEqual(Value a, Value b) =>
      new(
          a.Number == b.Number * ConvertUnits(a.Units, b.Units, '≡')
      );

        public static Value ComplexNotEqual(Value a, Value b) =>
            new(
                a.Number != b.Number * ConvertUnits(a.Units, b.Units, '≠')
            );

        public static Value ComplexLessThan(Value a, Value b) =>
            new(
                a.Number < b.Number * ConvertUnits(a.Units, b.Units, '<')
            );

        public static Value ComplexGreaterThan(Value a, Value b) =>
            new(
                a.Number > b.Number * ConvertUnits(a.Units, b.Units, '>')
            );

        public static Value ComplexLessThanOrEqual(Value a, Value b) =>
            new(
                a.Number <= b.Number * ConvertUnits(a.Units, b.Units, '≤')
            );

        public static Value ComplexGreaterThanOrEqual(Value a, Value b) =>
            new(
                a.Number >= b.Number * ConvertUnits(a.Units, b.Units, '≥')
            );

        internal static Value ComplexAbs(in Value value) =>
           new(Complex.Abs(value.Number), value.Units);

        internal static Value ComplexSign(in Value value) =>
            new(Complex.Sign(value.Number));

        internal static Value ComplexSin(in Value value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new Value(Complex.Sin(value.Number));
        }

        internal static Value ComplexCos(in Value value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new Value(Complex.Cos(value.Number));
        }

        internal static Value ComplexTan(in Value value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new Value(Complex.Tan(value.Number));
        }

        internal static Value ComplexCsc(in Value value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new Value(1 / Complex.Sin(value.Number));
        }

        internal static Value ComplexSec(in Value value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new Value(1 / Complex.Cos(value.Number));
        }

        internal static Value ComplexCot(in Value value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new Value(Complex.Cot(value.Number));
        }

        internal static Value ComplexSinh(in Value value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new Value(Complex.Sinh(value.Number));
        }

        internal static Value ComplexCosh(in Value value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new Value(Complex.Cosh(value.Number));
        }

        internal static Value ComplexTanh(in Value value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new Value(Complex.Tanh(value.Number));
        }

        internal static Value ComplexCsch(in Value value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new Value(1 / Complex.Sinh(value.Number));
        }

        internal static Value ComplexSech(in Value value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new Value(1 / Complex.Cosh(value.Number));
        }

        internal static Value ComplexCoth(in Value value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new Value(Complex.Coth(value.Number));
        }

        internal static Value ComplexAsin(in Value value)
        {
            CheckFunctionUnits("asin", value.Units);
            return new Value(Complex.Asin(value.Number));
        }

        internal static Value ComplexAcos(in Value value)
        {
            CheckFunctionUnits("acos", value.Units);
            return new Value(Complex.Acos(value.Number));
        }

        internal static Value ComplexAtan(in Value value)
        {
            CheckFunctionUnits("atan", value.Units);
            return new Value(Complex.Atan(value.Number));
        }

        internal static Value ComplexAcsc(in Value value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return new Value(Complex.Asin(1 / value.Number));
        }

        internal static Value ComplexAsec(in Value value)
        {
            CheckFunctionUnits("asec", value.Units);
            return new Value(Complex.Acos(1 / value.Number));
        }

        internal static Value ComplexAcot(in Value value)
        {
            CheckFunctionUnits("acot", value.Units);
            return new Value(Complex.Atan(1 / value.Number));
        }

        internal static Value ComplexAsinh(in Value value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new Value(Complex.Asinh(value.Number));
        }

        internal static Value ComplexAcosh(in Value value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new Value(Complex.Acosh(value.Number));
        }

        internal static Value ComplexAtanh(in Value value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new Value(Complex.Atanh(value.Number));
        }

        internal static Value ComplexAcsch(in Value value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return new Value(Complex.Asinh(1 / value.Number));
        }

        internal static Value ComplexAsech(in Value value)
        {
            CheckFunctionUnits("asech", value.Units);
            return new Value(Complex.Acosh(1 / value.Number));
        }

        internal static Value ComplexAcoth(in Value value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new Value(Complex.Acoth(value.Number));
        }

        internal static Value ComplexPow(Value value, Value power)
        {
            var result = Complex.Pow(value.Number, power.Number);
            var unit = PowUnits(value.Units, power, value.IsUnit);
            return new Value(result, unit, value.IsUnit);
        }

        internal static Value ComplexLog(in Value value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new Value(Complex.Log(value.Number));
        }

        internal static Value ComplexLog10(in Value value)
        {
            CheckFunctionUnits("log", value.Units);
            return new Value(Complex.Log10(value.Number));
        }

        internal static Value ComplexLog2(in Value value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new Value(Complex.Log2(value.Number));
        }

/*
        internal static Value ComplexExp(Value value)
        {
            CheckFunctionUnits("exp", value.Units);
            return new Value(Number.Exp(value.Number));
        }
*/

        internal static Value ComplexSqrt(in Value value)
        {
            var result = Complex.Sqrt(value.Number);
            if (value.Units is null)
                return new Value(result);

            var unit = RootUnits(value.Units, 2, value.IsUnit);
            return new Value(result, unit);
        }

        internal static Value ComplexCbrt(in Value value)
        {
            var result = Complex.Cbrt(value.Number);
            if (value.Units is null)
                return new Value(result);

            var unit = RootUnits(value.Units, 3, value.IsUnit);
            return new Value(result, unit);
        }

        internal static Value ComplexRoot(Value value, Value root)
        {
            var n = GetRoot(root);
            var result = Complex.Pow(value.Number, 1.0 / n);

            if (value.Units is null)
                return new Value(result);

            var unit = RootUnits(value.Units, n, value.IsUnit);
            return new Value(result, unit);
        }

        internal static Value ComplexRound(in Value value) =>
            new(Complex.Round(value.Number), value.Units);

        internal static Value ComplexFloor(in Value value) =>
            new(Complex.Floor(value.Number), value.Units);

        internal static Value ComplexCeiling(in Value value) =>
            new(Complex.Ceiling(value.Number), value.Units);

        internal static Value ComplexTruncate(in Value value) =>
            new(Complex.Truncate(value.Number), value.Units);

        internal static Value ComplexRandom(in Value value) =>
            new(Complex.Random(value.Number), value.Units);

        internal static Value ComplexAtan2(Value a, Value b) =>
            new(
                Complex.Atan2(b.Number * ConvertUnits(a.Units, b.Units, ','), a.Number),
                a.Units
            );

        private static bool AreAllReal(Value[] a)
        {
            for(int i = 0, n = a.Length; i < n; i++)
            {
                if (!a[i].Number.IsReal)
                    return false;
            }
            return true;
        }


        internal static Value ComplexMin(Value[] a) =>
            AreAllReal(a) ?
                Min(a) :
                new(double.NaN, a[0].Units);

        internal static Value ComplexMax(Value[] a) =>
            AreAllReal(a) ?
                Max(a) :
                new (double.NaN, a[0].Units);

        internal static Value ComplexSum(Value[] a)
        {
            var result = a[0].Number;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
                result += a[i].Number * ConvertUnits(u, a[i].Units, ',');

            return new(result, u);
        }

        internal static Value ComplexSumSq(Value[] a)
        {
            var result = a[0].Number;
            var u = a[0].Units;
            result *= result;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                var b = a[i].Number * ConvertUnits(u, a[i].Units, ',');
                result += b * b;
            }
            return new(result, u * u);
        }

        internal static Value ComplexSrss(Value[] a)
        {
            var result = a[0].Number;
            var u = a[0].Units;
            result *= result;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                var b = a[i].Number * ConvertUnits(u, a[i].Units, ',');
                result += b * b;
            }
            return new(Complex.Sqrt(result), u);
        }

        internal static Value ComplexAverage(Value[] a)
        {
            var result = a[0].Number;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
                result += a[i].Number * ConvertUnits(u, a[i].Units, ',');

            return new(result / a.Length, u);
        }

        internal static Value ComplexProduct(Value[] a)
        {
            var result = a[0].Number;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                u = MultiplyUnits(u, a[i].Units, out var b);
                result *= a[i].Number * b;
            }
            return new(result, u);
        }

        internal static Value ComplexMean(Value[] a)
        {
            var result = a[0].Number;
            var u = a[0].Units;
            for (int i = 1, n = a.Length; i < n; i++)
            {
                u = MultiplyUnits(u, a[i].Units, out var b, a[i].IsUnit);
                result *= a[i].Number * b;
            }
            return new(Complex.Pow(result, 1.0 / a.Length), RootUnits(u, a.Length));
        }

        internal static Value Switch(Value[] a)
        {
            for (int i = 0; i < a.Length - 1; i+=2)
            {
                if (!a[i].Number.Equals(Complex.Zero))
                    return a[i + 1];
            }
            if (a.Length % 2 != 0)
                return a[^1];

            return new Value(double.NaN);
        }

        internal static Value Take(Value[] a)
        {
            var d = Math.Round(a[0].Number.Re);
            if (d < 1 || d >= a.Length)
                return new Value(double.NaN);

            return a[(int)d];
        }

        private static Unit MultiplyUnits(Unit ua, Unit ub, out double d, bool updateText = false)
        {
            if (Unit.IsNullOrEmpty(ua))
            {
                d = 1d;
                return Unit.IsNullOrEmpty(ub) ? null : ub;
            }
            if (Unit.IsNullOrEmpty(ub))
            {
                d = 1d;
                return ua;
            }
            d = Unit.GetProductOrDivisionFactor(ua, ub);
            var uc = ua * ub;
            if (Unit.IsNullOrEmpty(uc))
                return null;

            if (updateText && !Unit.IsMultiple(ua, ub))
            {
                uc.Scale(d); 
                d = 1d;
                uc.Text = ua.Text + '·' + ub.Text;
            }
            return uc;
        }

        private static Unit DivideUnits(in Value a, in Value b, out double d, bool updateText = false)
        {
            d = 1d;
            Unit ua = a.Units, ub = b.Units;
            if (Unit.IsNullOrEmpty(ub))
                return Unit.IsNullOrEmpty(ua) ? null : ua;

            if (Unit.IsNullOrEmpty(ua))
                return ub.Pow(-1.0);

            d = Unit.GetProductOrDivisionFactor(ua, ub, true);
            var uc = ua / ub;   
            if (Unit.IsNullOrEmpty(uc))
                return null;

            if (updateText && b.IsUnit && 
                !(Unit.IsNullOrEmpty(ua) ||
                  Unit.IsNullOrEmpty(ub) ||
                  Unit.IsMultiple(ua, ub)))
            {
                uc.Scale(d);
                d = 1;
                uc.Text = ua.Text + '/' + ub.Text;
            }
            return uc;
        }

        private static int GetRoot(in Value root)
        {
            var d = root.Number.Re;
            var n = (int)d;
            if (n < 2 || n != d)
#if BG
                throw new MathParser.MathParserException("Коренният показател трябва да е цяло число > 1.");
#else
                throw new MathParser.MathParserException("Root degree must be integer > 1.");
#endif
            return n;
        }

        private static Unit PowUnits(Unit u, in Value power, bool updateText = false)
        {
            if (!Unit.IsNullOrEmpty(power.Units))
#if BG
                throw new MathParser.MathParserException("Степенният показател трябва да е бездименсионен.");
#else
                throw new MathParser.MathParserException("Power must be unitless.");
#endif
            if (u is null)
                return null;

            if (!power.Number.IsReal)
#if BG
                throw new MathParser.MathParserException("Не мога да повдигна мерни единици на комплексна степен.");
#else
                throw new MathParser.MathParserException("Units cannon be raised to complex power.");
#endif
            var result = u.Pow(power.Number.Re);
            var s = u.Text;
            if (updateText && !s.Contains('^'))
            {
                var ps = Complex.Format(power.Number, 2, OutputWriter.OutputFormat.Text);
                result.Text = 
                    s.IndexOfAny(CompositeUnitChars) >= 0.0 ?
                    $"({s})^{ps}":
                    s + '^' + ps;
            }
            return result;
        }

        private static Unit RootUnits(Unit u, int n, bool updateText = false)
        {
            var result = u.Pow(1.0 / n);
            var s = u.Text;
            if (updateText && !s.Contains('^'))
                result.Text = 
                    s.IndexOfAny(CompositeUnitChars) >= 0 ?
                    $"({s})^1/{n}":
                    s + $"^1/{n}";

            return result;
        }

        private static double ConvertUnits(Unit ua, Unit ub, char op)
        {
            if (!Unit.IsConsistent(ua, ub))
#if BG
                throw new MathParser.MathParserException($"Несъвместими мерни единици: \"{Unit.GetText(a)} {op} {Unit.GetText(b)}\".");
#else
                throw new MathParser.MathParserException($"Inconsistent units: \"{Unit.GetText(ua)} {op} {Unit.GetText(ub)}\".");
#endif
            return ua is null ? 1.0 : ub.ConvertTo(ua);
        }

        private static void CheckFunctionUnits(string func, Unit unit)
        {
            if (!Unit.IsNullOrEmpty(unit))
#if BG
                throw new MathParser.MathParserException($"Невалидни мерни единици за функция: \"{func}({Unit.GetText(unit)})\".");
#else
                throw new MathParser.MathParserException($"Invalid units for function: \"{func}({Unit.GetText(unit)})\".");
#endif
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