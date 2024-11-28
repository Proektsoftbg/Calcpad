using System;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    internal class RealCalculator : Calculator
    {
        private static readonly Operator[] Operators =
            [
                UnitPow,
                Value.Divide,
                Value.IntDiv,
                (in Value a, in Value b) => a % b,
                Value.Multiply,
                (in Value a, in Value b) => a - b,
                (in Value a, in Value b) => a + b,
                (in Value a, in Value b) => a < b,
                (in Value a, in Value b) => a > b,
                (in Value a, in Value b) => a <= b,
                (in Value a, in Value b) => a >= b,
                (in Value a, in Value b) => a == b,
                (in Value a, in Value b) => a != b,
                (in Value a, in Value b) => a & b,
                (in Value a, in Value b) => a | b,
                (in Value a, in Value b) => a ^ b,
                (in Value _, in Value b) => b
            ];
        private readonly Function[] _functions;
        private readonly Operator[] _functions2;
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

        public RealCalculator()
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
                UnitSqrt, //30
                UnitSqrt, //31
                UnitCbrt, //32
                Round,    //33
                Floor,    //34
                Ceiling,  //35
                Truncate, //36
                Real,     //37
                Imaginary,//38
                Phase,    //39
                Random,   //40
                Fact,     //41
                (in Value a) => -a, //42
                Not,      //43
                Timer     //44
            ];

            _functions2 =
            [
                Atan2,
                UnitRoot,
                Mod,
                MandelbrotSet
            ];
        }

        internal override Value EvaluateOperator(long index, in Value a, in Value b) => Operators[index](a, b);
        internal override Value EvaluateFunction(long index, in Value a) => _functions[index](a);
        internal override Value EvaluateFunction2(long index, in Value a, in Value b) => _functions2[index](a, b);
        internal override IValue EvaluateFunction3(long index, in IValue a, in IValue b, in IValue c) => Functions3[index](a, b, c);
        internal override Value EvaluateMultiFunction(long index, Value[] a) => MultiFunctions[index](a);
        internal override Value EvaluateInterpolation(long index, Value[] a) => Interpolations[index](a);
        internal override Operator GetOperator(long index) => index == PowerIndex ? Pow : Operators[index];
        internal override Function GetFunction(long index)
        {
            if (index == SqrIndex || index == SqrtIndex)
                return Sqrt;

            if (index == CbrtIndex)
                return Cbrt;

            return _functions[index];
        }
        internal override Operator GetFunction2(long index) => index == RootIndex ? Root : _functions2[index];
        internal override Function3 GetFunction3(long index) => Functions3[index];
        internal override Func<Value[], Value> GetMultiFunction(long index) => MultiFunctions[index];

        public static Value Fact(in Value a)
        {
            if (a.Units is not null)
                Throw.FactorialArgumentUnitlessException();

            return new(Fact(a.Re));
        }

        private static Value Real(in Value value) => value;
        private static Value Imaginary(in Value _) => Value.Zero;
        private static Value Phase(in Value value) => new(value.Complex.Phase);
        internal static Value Abs(in Value value) => new(Math.Abs(value.Re), value.Units);
        private static Value Sign(in Value value) => double.IsNaN(value.Re) ? Value.NaN : new(Math.Sign(value.Re));

        private Value Sin(in Value value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new(Complex.RealSin(FromAngleUnits(value)));
        }

        private Value Cos(in Value value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new(Complex.RealCos(FromAngleUnits(value)));
        }

        private Value Tan(in Value value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new(Math.Tan(FromAngleUnits(value)));
        }

        private Value Csc(in Value value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new(1 / Math.Sin(FromAngleUnits(value)));
        }

        private Value Sec(in Value value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new(1 / Math.Cos(FromAngleUnits(value)));
        }

        private Value Cot(in Value value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new(1 / Math.Tan(FromAngleUnits(value)));
        }

        private static Value Sinh(in Value value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new(Math.Sinh(value.Re));
        }

        private static Value Cosh(in Value value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new(Math.Cosh(value.Re));
        }

        private static Value Tanh(in Value value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new(Math.Tanh(value.Re));
        }

        private static Value Csch(in Value value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new(1 / Math.Sinh(value.Re));
        }

        private static Value Sech(in Value value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new(1 / Math.Cosh(value.Re));
        }

        private static Value Coth(in Value value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new(1 / Math.Tanh(value.Re));
        }

        private Value Asin(in Value value)
        {
            CheckFunctionUnits("asin", value.Units);
            return ToAngleUnits(Math.Asin(value.Re));
        }

        private Value Acos(in Value value)
        {
            CheckFunctionUnits("acos", value.Units);
            return ToAngleUnits(Math.Acos(value.Re));
        }

        private Value Atan(in Value value)
        {
            CheckFunctionUnits("atan", value.Units);
            return ToAngleUnits(Math.Atan(value.Re));
        }

        private Value Acsc(in Value value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return value.Re == 0d ?
                Value.PositiveInfinity :
                ToAngleUnits(Math.Asin(1d / value.Re));
        }

        private Value Asec(in Value value)
        {
            CheckFunctionUnits("asec", value.Units);
            return value.Re == 0d ?
                Value.PositiveInfinity :
                ToAngleUnits(Math.Acos(1d / value.Re));
        }

        private Value Acot(in Value value)
        {
            CheckFunctionUnits("acot", value.Units);
            return ToAngleUnits(Math.Atan(1d / value.Re));
        }

        private static Value Asinh(in Value value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new(Math.Asinh(value.Re));
        }

        private static Value Acosh(in Value value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new(Math.Acosh(value.Re));
        }

        private static Value Atanh(in Value value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new(Math.Atanh(value.Re));
        }

        private static Value Acsch(in Value value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return new(Math.Asinh(1d / value.Re));
        }

        private static Value Asech(in Value value)
        {
            CheckFunctionUnits("asech", value.Units);
            return new(Math.Acosh(1d / value.Re));
        }

        private static Value Acoth(in Value value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new(Math.Atanh(1 / value.Re));
        }

        private static Value Log(in Value value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new(Math.Log(value.Re));
        }

        private static Value Log10(in Value value)
        {
            CheckFunctionUnits("log", value.Units);
            return new(Math.Log10(value.Re));
        }

        private static Value Log2(in Value value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new(Math.Log2(value.Re));
        }

        private static Value Exp(in Value value)
        {
            CheckFunctionUnits("exp", value.Units);
            return new(Math.Exp(value.Re));
        }

        private static Value Pow(Value value, Value power, bool isUnit)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Math.Pow(value.Re * u.GetDimensionlessFactor(), power.Re));

            return new(
                Math.Pow(value.Re, power.Re),
                Unit.Pow(u, power, isUnit),
                isUnit
            );
        }

        internal static Value Pow(in Value value, in Value power) =>
            Pow(value, power, false);

        private static Value UnitPow(in Value value, in Value power) =>
            Pow(value, power, value.IsUnit);

        private static Value Sqrt(Value value, bool isUnit)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Math.Sqrt(value.Re * u.GetDimensionlessFactor()));

            var result = Math.Sqrt(value.Re);
            return u is null ?
                new(result) :
                new(result, Unit.Root(u, 2, isUnit), isUnit);
        }

        internal static Value Sqrt(in Value value) =>
            Sqrt(value, false);

        private static Value UnitSqrt(in Value value) =>
            Sqrt(value, value.IsUnit);

        private static Value Cbrt(Value value, bool isUnit)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Math.Cbrt(value.Re * u.GetDimensionlessFactor()));

            var result = Math.Cbrt(value.Re);
            return u is null ?
                new(result) :
                new(result, Unit.Root(u, 3, isUnit), isUnit);
        }

        private static Value Cbrt(in Value value) =>
            Cbrt(value, false);

        private static Value UnitCbrt(in Value value) =>
            Cbrt(value, value.IsUnit);

        private static Value Root(in Value value, in Value root, bool isUnit)
        {
            var n = GetRoot(root);
            var u = value.Units;
            var d = u is not null && u.IsDimensionless ?
                value.Re * u.GetDimensionlessFactor() :
                value.Re;

            var result = int.IsOddInteger(n) && d < 0 ?
                -Math.Pow(-d, 1d / n) :
                Math.Pow(d, 1d / n);

            return u is null ?
                new(result) :
                new(result, Unit.Root(u, n, isUnit), isUnit);
        }

        private static Value Root(in Value value, in Value root) =>
            Root(value, root, false);

        private static Value UnitRoot(in Value value, in Value root) =>
            Root(value, root, value.IsUnit);

        private static Value Round(in Value value) =>
            new(Math.Round(value.Re, MidpointRounding.AwayFromZero), value.Units);

        private static Value Floor(in Value value) =>
            new(Math.Floor(value.Re), value.Units);

        private static Value Ceiling(in Value value) =>
            new(Math.Ceiling(value.Re), value.Units);

        private static Value Truncate(in Value value) =>
            new(Math.Truncate(value.Re), value.Units);

        private static Value Random(in Value value) =>
            new(Complex.RealRandom(value.Re), value.Units);

        private Value Atan2(in Value a, in Value b) =>
            ToAngleUnits(Math.Atan2(b.Re * Unit.Convert(a.Units, b.Units, ','), a.Re));

        private static Value Sum(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
                result += v[i].Re * Unit.Convert(u, v[i].Units, ',');

            return new(result, u);
        }

        private static Value SumSq(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            result *= result;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Re * Unit.Convert(u, v[i].Units, ',');
                result += b * b;
            }
            return new(result, u is null ? null : u * u);
        }

        internal static Value Srss(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            result *= result;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Re * Unit.Convert(u, v[i].Units, ',');
                result += b * b;
            }
            return new(Math.Sqrt(result), u);
        }

        private static Value Average(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
                result += v[i].Re * Unit.Convert(u, v[i].Units, ',');

            return new(result / v.Length, u);
        }

        private static Value Product(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                u = Unit.Multiply(u, v[i].Units, out var b);
                result *= v[i].Re * b;
            }
            return new(result, u);
        }

        private static Value Mean(Value[] v)
        {
            var result = v[0].Re;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                ref var value = ref v[i];
                u = Unit.Multiply(u, value.Units, out var b);
                result *= value.Re * b;
            }
            result = Math.Pow(result, 1.0 / v.Length);
            if (u is null)
                return new(result);

            u = Unit.Root(u, v.Length);
            return new(result, u);
        }

        private double FromAngleUnits(in Value value)
        {
            if (value.Units is null)
                return value.Re * ToRad[_degrees];

            return value.Re * value.Units.ConvertTo(AngleUnits[1]);
        }

        private Value ToAngleUnits(double value) =>
            _returnAngleUnits ?
            new(value * FromRad[_degrees], AngleUnits[_degrees]) :
            new(value * FromRad[_degrees]);
    }
}
