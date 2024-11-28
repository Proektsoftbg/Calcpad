using System;

namespace Calcpad.Core
{
    internal class ComplexCalculator : Calculator
    {
        private static readonly Operator[] Operators =
            [
                Pow,
                Divide,
                IntDiv,
                Remainder,
                Multiply,
                Subtract,
                Add,
                LessThan,
                GreaterThan,
                LessThanOrEqual,
                GreaterThanOrEqual,
                Equal,
                NotEqual,
                (in Value a, in Value b) => a & b,
                (in Value a, in Value b) => a | b,
                (in Value a, in Value b) => a ^ b,
                (in Value _, in Value b) => b
            ];
        private readonly Function[] _functions;
        private readonly Operator[] Functions2;
        private static readonly Func<Value[], Value>[] MultiFunctions =
            [
                Min,
                Max,
                Sum,
                SumSq,
                Srss,
                Average,
                Product,
                Mean,
                Switch,
                And,
                Or,
                Xor,
                Gcd,
                Lcm,
            ];

        internal override int Degrees
        {
            set => _degrees = value;
        }

        public ComplexCalculator()
        {
            _functions =
            [
                Sin,      // 0
                Cos,      // 1
                Tan,      // 2
                Csc,      // 3
                Sec,      // 4
                Cot,      // 5
                Asin,     // 6
                Acos,     // 7
                Atan,     // 8
                Acsc,     // 9
                Asec,     //10
                Acot,     //11
                Sinh,     //12
                Cosh,     //13
                Tanh,     //14
                Csch,     //15
                Sech,     //16
                Coth,     //17
                Asinh,    //18
                Acosh,    //19
                Atanh,    //20
                Acsch,    //21
                Asech,    //22
                Acoth,    //23
                Log10,    //24
                Log,      //25
                Log2,     //26
                Exp,      //27
                Abs,      //28
                Sign,     //29
                Sqrt,     //30
                Sqrt,     //31
                Cbrt,     //32
                Round,    //33
                Floor,    //34
                Ceiling,  //35
                Truncate, //36
                Real,     //37
                Imaginary,//38
                Phase,    //39
                Random,   //40
                Fact,     //41
                Negate,   //42
                Not,      //43
                Timer     //44  
            ];

            Functions2 =
            [
                Atan2,
                Root,
                Mod,
                MandelbrotSet
            ];
        }

        internal override Value EvaluateOperator(long index, in Value a, in Value b) => Operators[index](a, b);
        internal override Value EvaluateFunction(long index, in Value a) => _functions[index](a);
        internal override Value EvaluateFunction2(long index, in Value a, in Value b) => Functions2[index](a, b);
        internal override IValue EvaluateFunction3(long index, in IValue a, in IValue b, in IValue c) => Functions3[index](a, b, c);
        internal override Value EvaluateMultiFunction(long index, Value[] a) => MultiFunctions[index](a);
        internal override Value EvaluateInterpolation(long index, Value[] a) => Interpolations[index](a);
        internal override Operator GetOperator(long index) => Operators[index];
        internal override Function GetFunction(long index) => _functions[index];
        internal override Operator GetFunction2(long index) => Functions2[index];
        internal override Function3 GetFunction3(long index) => Functions3[index];
        internal override Func<Value[], Value> GetMultiFunction(long index) => MultiFunctions[index];

        private static Value Fact(in Value value)
        {
            if (!(value.IsReal))
                Throw.FactorialArgumentComplexException();

            if (value.Units is not null)
                Throw.FactorialArgumentUnitlessException();

            return new(Fact(value.Re));
        }

        private static Value Real(in Value value) => new(value.Re, value.Units);
        private static Value Imaginary(in Value value) => new(value.Im, value.Units);
        private static Value Phase(in Value value) => new(value.Complex.Phase);
        internal static Value Negate(in Value value) => new(-value.Re, -value.Im, value.Units, value.IsUnit);

        internal static Value Add(in Value a, in Value b) =>
            new(
                a.Complex + b.Complex * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        internal static Value Subtract(in Value a, in Value b) =>
            new(
                a.Complex - b.Complex * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        internal static Value Multiply(in Value a, in Value b)
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

        private static Value Divide(in Value a, in Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d, b.IsUnit);
            var c = a.Complex / b.Complex * d;
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        private static Value Remainder(in Value a, in Value b)
        {
            if (b.Units is not null)
                Throw.RemainderUnitsException(Unit.GetText(a.Units), Unit.GetText(b.Units));

            return new(a.Complex % b.Complex, a.Units);
        }

        private static Value IntDiv(in Value a, in Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            var c = Complex.IntDiv(a.Complex * d, b.Complex);
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        private static Value Equal(in Value a, in Value b) =>
            new(
                a.Complex == b.Complex * Unit.Convert(a.Units, b.Units, '≡')
            );

        private static Value NotEqual(in Value a, in Value b) =>
            new(
                a.Complex != b.Complex * Unit.Convert(a.Units, b.Units, '≠')
            );

        private static Value LessThan(in Value a, in Value b) =>
            new(
                a.Complex < b.Complex * Unit.Convert(a.Units, b.Units, '<')
            );

        private static Value GreaterThan(in Value a, in Value b) =>
            new(
                a.Complex > b.Complex * Unit.Convert(a.Units, b.Units, '>')
            );

        private static Value LessThanOrEqual(in Value a, in Value b) =>
            new(
                a.Complex <= b.Complex * Unit.Convert(a.Units, b.Units, '≤')
            );

        private static Value GreaterThanOrEqual(in Value a, in Value b) =>
            new(
                a.Complex >= b.Complex * Unit.Convert(a.Units, b.Units, '≥')
            );

        internal static Value Abs(in Value value) =>
           new(Complex.Abs(value.Complex), value.Units);

        private static Value Sign(in Value value) =>
            new(Complex.Sign(value.Complex));

        private Value Sin(in Value value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new(Complex.Sin(FromAngleUnits(value)));
        }

        private Value Cos(in Value value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new(Complex.Cos(FromAngleUnits(value)));
        }

        private Value Tan(in Value value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new(Complex.Tan(FromAngleUnits(value)));
        }

        private Value Csc(in Value value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new(1d / Complex.Sin(FromAngleUnits(value)));
        }

        private Value Sec(in Value value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new(1d / Complex.Cos(FromAngleUnits(value)));
        }

        private Value Cot(in Value value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new(Complex.Cot(FromAngleUnits(value)));
        }

        private static Value Sinh(in Value value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new(Complex.Sinh(value.Complex));
        }

        private static Value Cosh(in Value value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new(Complex.Cosh(value.Complex));
        }

        private static Value Tanh(in Value value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new(Complex.Tanh(value.Complex));
        }

        private static Value Csch(in Value value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new(1d / Complex.Sinh(value.Complex));
        }

        private static Value Sech(in Value value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new(1d / Complex.Cosh(value.Complex));
        }

        private static Value Coth(in Value value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new(Complex.Coth(value.Complex));
        }

        private Value Asin(in Value value)
        {
            CheckFunctionUnits("asin", value.Units);
            return ToAngleUnits(Complex.Asin(value.Complex));
        }

        private Value Acos(in Value value)
        {
            CheckFunctionUnits("acos", value.Units);
            return ToAngleUnits(Complex.Acos(value.Complex));
        }

        private Value Atan(in Value value)
        {
            CheckFunctionUnits("atan", value.Units);
            return ToAngleUnits(Complex.Atan(value.Complex));
        }

        private Value Acsc(in Value value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return value.Equals(Value.Zero) ?
                Value.ComplexInfinity :
                ToAngleUnits(Complex.Asin(1d / value.Complex));
        }

        private Value Asec(in Value value)
        {
            CheckFunctionUnits("asec", value.Units);
            return value.Equals(Value.Zero) ?
                Value.ComplexInfinity :
                ToAngleUnits(Complex.Acos(1d / value.Complex));
        }

        private Value Acot(in Value value)
        {
            CheckFunctionUnits("acot", value.Units);
            return ToAngleUnits(Complex.Acot(value.Complex));
        }

        private static Value Asinh(in Value value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new(Complex.Asinh(value.Complex));
        }

        private static Value Acosh(in Value value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new(Complex.Acosh(value.Complex));
        }

        private static Value Atanh(in Value value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new(Complex.Atanh(value.Complex));
        }

        private static Value Acsch(in Value value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return value.Equals(Value.Zero) ?
                Value.ComplexInfinity :
                new(Complex.Asinh(1d / value.Complex));
        }

        private static Value Asech(in Value value)
        {
            CheckFunctionUnits("asech", value.Units);
            return value.Equals(Value.Zero) ?
                Value.ComplexInfinity :
                new(Complex.Acosh(1d / value.Complex));
        }

        private static Value Acoth(in Value value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new(Complex.Acoth(value.Complex));
        }

        private static Value Pow(in Value value, in Value power)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Pow(value.Complex * u.GetDimensionlessFactor(), power.Complex));

            var result = Complex.Pow(value.Complex, power.Complex);
            var unit = Unit.Pow(u, power, value.IsUnit);
            return new(result, unit, value.IsUnit);
        }

        private static Value Log(in Value value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new(Complex.Log(value.Complex));
        }

        private static Value Log10(in Value value)
        {
            CheckFunctionUnits("log", value.Units);
            return new(Complex.Log10(value.Complex));
        }

        private static Value Log2(in Value value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new(Complex.Log2(value.Complex));
        }

        private static Value Exp(in Value value)
        {
            CheckFunctionUnits("exp", value.Units);
            return new(Complex.Exp(value.Complex));
        }

        internal static Value Sqrt(in Value value)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Sqrt(value.Complex * u.GetDimensionlessFactor()));

            var result = Complex.Sqrt(value.Complex);
            if (u is null)
                return new(result);

            var unit = Unit.Root(u, 2, value.IsUnit);
            return new(result, unit);
        }

        private static Value Cbrt(in Value value)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Cbrt(value.Complex * u.GetDimensionlessFactor()));

            var result = Complex.Cbrt(value.Complex);
            if (u is null)
                return new(result);

            var unit = Unit.Root(u, 3, value.IsUnit);
            return new(result, unit);
        }

        private static Value Root(in Value value, in Value root)
        {
            var n = GetRoot(root);

            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Pow(value.Complex * u.GetDimensionlessFactor(), 1d / n));

            var result = Complex.Pow(value.Complex, 1d / n);

            if (u is null)
                return new(result);

            var unit = Unit.Root(u, n, value.IsUnit);
            return new(result, unit);
        }

        private static Value Round(in Value value) => new(Complex.Round(value.Complex), value.Units);
        private static Value Floor(in Value value) => new(Complex.Floor(value.Complex), value.Units);
        private static Value Ceiling(in Value value) => new(Complex.Ceiling(value.Complex), value.Units);
        private static Value Truncate(in Value value) => new(Complex.Truncate(value.Complex), value.Units);
        private static Value Random(in Value value) => new(Complex.Random(value.Complex), value.Units);
        private Value Atan2(in Value a, in Value b) =>
            ToAngleUnits(Complex.Atan2(b.Complex * Unit.Convert(a.Units, b.Units, ','), a.Complex));

        private static bool AreAllReal(Value[] v)
        {
            for (int i = 0, len = v.Length; i < len; ++i)
            {
                if (!v[i].IsReal)
                    return false;
            }
            return true;
        }

        private new static Value Min(Value[] v) =>
            AreAllReal(v) ?
                Calculator.Min(v) :
                new(double.NaN, v[0].Units);

        private new static Value Max(Value[] v) =>
            AreAllReal(v) ?
                Calculator.Max(v) :
                new(double.NaN, v[0].Units);

        private static Value Sum(Value[] v)
        {
            var result = v[0].Complex;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
                result += v[i].Complex * Unit.Convert(u, v[i].Units, ',');

            return new(result, u);
        }

        private static Value SumSq(Value[] v)
        {
            var result = v[0].Complex;
            var u = v[0].Units;
            result *= result;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Complex * Unit.Convert(u, v[i].Units, ',');
                result += b * b;
            }
            return new(result, u is null ? null : u * u);
        }

        private static Value Srss(Value[] v)
        {
            var result = v[0].Complex;
            var u = v[0].Units;
            result *= result;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Complex * Unit.Convert(u, v[i].Units, ',');
                result += b * b;
            }
            return new(Complex.Sqrt(result), u);
        }

        private static Value Average(Value[] v)
        {
            var result = v[0].Complex;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
                result += v[i].Complex * Unit.Convert(u, v[i].Units, ',');

            return new(result / v.Length, u);
        }

        private static Value Product(Value[] v)
        {
            var result = v[0].Complex;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                u = Unit.Multiply(u, v[i].Units, out var b);
                result *= v[i].Complex * b;
            }
            return new(result, u);
        }

        private static Value Mean(Value[] v)
        {
            var result = v[0].Complex;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                ref var value = ref v[i];
                u = Unit.Multiply(u, value.Units, out var b);
                result *= value.Complex * b;
            }
            result = Complex.Pow(result, 1.0 / v.Length);
            if (u is null)
                return new(result);

            u = Unit.Root(u, v.Length);
            return new(result, u);
        }

        private new static Value Gcd(Value[] v) =>
            AreAllReal(v) ? Gcd(v) : new(double.NaN, v[0].Units);

        private new static Value Lcm(Value[] v) =>
            AreAllReal(v) ? Lcm(v) : new(double.NaN, v[0].Units);

        private Complex FromAngleUnits(in Value value)
        {
            if (value.Units is null)
                return value.Complex * ToRad[_degrees];

            return value.Complex * value.Units.ConvertTo(AngleUnits[1]);
        }

        private Value ToAngleUnits(Complex value) =>
            _returnAngleUnits ?
            new(value * FromRad[_degrees], AngleUnits[_degrees]) :
            new(value * FromRad[_degrees]);
    }
}
