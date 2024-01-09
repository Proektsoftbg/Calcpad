using System;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    internal class RealCalculator : Calculator
    {
        private static readonly Func<Value, Value, Value>[] Operators;
        private readonly Func<Value, Value>[] _functions;
        private readonly Func<Value, Value, Value>[] Functions2;
        private static readonly Func<Value[], Value>[] MultiFunctions;
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
                (a) => -a, //42
                Not,      //43
                Timer     //44
            ];

            Functions2 =
            [
                Atan2,
                UnitRoot,
                Mod,
                MandelbrotSet
            ];
        }

        static RealCalculator()
        {
            Operators =
            [
                UnitPow,
                Value.Divide,
                Value.IntDiv,
                (a, b) => a % b,
                Value.Multiply,
                (a, b) => a - b,
                (a, b) => a + b,
                (a, b) => a < b,
                (a, b) => a > b,
                (a, b) => a <= b,
                (a, b) => a >= b,
                (a, b) => a == b,
                (a, b) => a != b,
                (a, b) => a & b,
                (a, b) => a | b,
                (a, b) => a ^ b,
                (_, b) => b
            ];

            MultiFunctions =
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
                Take,
                Line,
                Spline,
                And,
                Or,
                Xor,
                Gcd,
                Lcm,
            ];
        }

        internal override Value EvaluateOperator(int index, Value a, Value b) => Operators[index](a, b);
        internal override Value EvaluateFunction(int index, Value a) => _functions[index](a);
        internal override Value EvaluateFunction2(int index, Value a, Value b) => Functions2[index](a, b);
        internal override Value EvaluateMultiFunction(int index, Value[] a) => MultiFunctions[index](a);
        internal override Func<Value, Value, Value> GetOperator(int index) => index == PowerIndex ? Pow : Operators[index];

        internal override Func<Value, Value> GetFunction(int index)
        {
            if (index == SqrIndex || index == SqrtIndex)
                return Sqrt;

            if (index == CbrtIndex)
                return Cbrt;

            return _functions[index];
        }


        public static Value Fact(Value a)
        {
            if (a.Units is not null)
                Throw.FactorialArgumentUnitlessException();

            return new(Fact(a.Re));
        }

        internal override Func<Value, Value, Value> GetFunction2(int index) => index == RootIndex ? Root : Functions2[index];
        internal override Func<Value[], Value> GetMultiFunction(int index) => MultiFunctions[index];

        private static Value Real(Value value) => value;
        private static Value Imaginary(Value value) => new(0.0);
        private static Value Phase(Value value) => new(value.Complex.Phase);
        private static Value Abs(Value value) => new(Math.Abs(value.Re), value.Units);

        private static Value Sign(Value value) => new(Math.Sign(value.Re));

        private Value Sin(Value value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new(Complex.RealSin(FromAngleUnits(value)));
        }

        private Value Cos(Value value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new(Complex.RealCos(FromAngleUnits(value)));
        }

        private Value Tan(Value value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new(Math.Tan(FromAngleUnits(value)));
        }

        private Value Csc(Value value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new(1 / Math.Sin(FromAngleUnits(value)));
        }

        private Value Sec(Value value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new(1 / Math.Cos(FromAngleUnits(value)));
        }

        private Value Cot(Value value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new(1 / Math.Tan(FromAngleUnits(value)));
        }

        private static Value Sinh(Value value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new(Math.Sinh(value.Re));
        }

        private static Value Cosh(Value value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new(Math.Cosh(value.Re));
        }

        private static Value Tanh(Value value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new(Math.Tanh(value.Re));
        }

        private static Value Csch(Value value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new(1 / Math.Sinh(value.Re));
        }

        private static Value Sech(Value value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new(1 / Math.Cosh(value.Re));
        }

        private static Value Coth(Value value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new(1 / Math.Tanh(value.Re));
        }

        private Value Asin(Value value)
        {
            CheckFunctionUnits("asin", value.Units);
            return ToAngleUnits(Math.Asin(value.Re));
        }

        private Value Acos(Value value)
        {
            CheckFunctionUnits("acos", value.Units);
            return ToAngleUnits(Math.Acos(value.Re));
        }

        private Value Atan(Value value)
        {
            CheckFunctionUnits("atan", value.Units);
            return ToAngleUnits(Math.Atan(value.Re));
        }

        private Value Acsc(Value value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return value.Re == 0d ?
                Value.PositiveInfinity :
                ToAngleUnits(Math.Asin(1d / value.Re));
        }

        private Value Asec(Value value)
        {
            CheckFunctionUnits("asec", value.Units);
            return value.Re == 0d ?
                Value.PositiveInfinity :
                ToAngleUnits(Math.Acos(1d / value.Re));
        }

        private Value Acot(Value value)
        {
            CheckFunctionUnits("acot", value.Units);
            return ToAngleUnits(Math.Atan(1d / value.Re));
        }

        private static Value Asinh(Value value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new(Math.Asinh(value.Re));
        }

        private static Value Acosh(Value value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new(Math.Acosh(value.Re));
        }

        private static Value Atanh(Value value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new(Math.Atanh(value.Re));
        }

        private static Value Acsch(Value value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return new(Math.Asinh(1d / value.Re));
        }

        private static Value Asech(Value value)
        {
            CheckFunctionUnits("asech", value.Units);
            return new(Math.Acosh(1d / value.Re));
        }

        private static Value Acoth(Value value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new(Math.Atanh(1 / value.Re));
        }

        private static Value Log(Value value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new(Math.Log(value.Re));
        }

        private static Value Log10(Value value)
        {
            CheckFunctionUnits("log", value.Units);
            return new(Math.Log10(value.Re));
        }

        private static Value Log2(Value value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new(Math.Log2(value.Re));
        }

        private static Value Exp(Value value)
        {
            CheckFunctionUnits("exp", value.Units);
            return new(Math.Exp(value.Re));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Value Pow(Value value, Value power, bool isUnit) =>
            new(
                Math.Pow(value.Re, power.Re),
                Unit.Pow(value.Units, power, isUnit),
                isUnit
            );

        private static Value Pow(Value value, Value power) =>
            Pow(value, power, false);

        private static Value UnitPow(Value value, Value power) =>
            Pow(value, power, value.IsUnit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Value Sqrt(Value value, bool isUnit)
        {
            var result = Math.Sqrt(value.Re);
            return value.Units is null ?
                new(result) :
                new(result, Unit.Root(value.Units, 2, isUnit), isUnit);
        }

        private static Value Sqrt(Value value) =>
            Sqrt(value, false);

        private static Value UnitSqrt(Value value) =>
            Sqrt(value, value.IsUnit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Value Cbrt(Value value, bool isUnit)
        {
            var result = Math.Cbrt(value.Re);
            return value.Units is null ?
                new(result) :
                new(result, Unit.Root(value.Units, 3, isUnit), isUnit);
        }

        private static Value Cbrt(Value value) =>
            Cbrt(value, false);

        private static Value UnitCbrt(Value value) =>
            Cbrt(value, value.IsUnit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Value Root(Value value, Value root, bool isUnit)
        {
            var n = GetRoot(root);
            var result = int.IsOddInteger(n) && value.Re < 0 ?
                -Math.Pow(-value.Re, 1d / n) :
                Math.Pow(value.Re, 1d / n);

            return value.Units is null ?
                new(result) :
                new(result, Unit.Root(value.Units, n, isUnit), isUnit);
        }

        private static Value Root(Value value, Value root) =>
            Root(value, root, false);

        private static Value UnitRoot(Value value, Value root) =>
            Root(value, root, value.IsUnit);

        private static Value Round(Value value) =>
            new(Math.Round(value.Re, MidpointRounding.AwayFromZero), value.Units);

        private static Value Floor(Value value) =>
            new(Math.Floor(value.Re), value.Units);

        private static Value Ceiling(Value value) =>
            new(Math.Ceiling(value.Re), value.Units);

        private static Value Truncate(Value value) =>
            new(Math.Truncate(value.Re), value.Units);

        private static Value Random(Value value) =>
            new(Complex.RealRandom(value.Re), value.Units);

        private Value Atan2(Value a, Value b) =>
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

        private static Value Srss(Value[] v)
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
