using System;

namespace Calcpad.Core
{
    internal class RealCalculator : Calculator
    {
        private static readonly Func<Value, Value, Value>[] Operators;
        private static readonly Func<Value, Value>[] Functions;
        private static readonly Func<Value, Value, Value>[] Functions2;
        private static readonly Func<Value[], Value>[] MultiFunctions;
        internal override int Degrees
        {
            set => _degrees = value;
        }

        static RealCalculator()
        {
            Operators = new Func<Value, Value, Value>[]
            {
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
                (_, b) => b
            };

            Functions = new Func<Value, Value>[]
            {
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
                (a) => -a //42
            };

            Functions2 = new Func<Value, Value, Value>[]
            {
                Atan2,
                UnitRoot,
                MandelbrotSet
            };

            MultiFunctions = new Func<Value[], Value>[]
            {
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
                Spline
            };
        }

        internal override Value EvaluateOperator(int index, Value a, Value b) => Operators[index](a, b);
        internal override Value EvaluateFunction(int index, Value a) => Functions[index](a);
        internal override Value EvaluateFunction2(int index, Value a, Value b) => Functions2[index](a, b);
        internal override Value EvaluateMultiFunction(int index, Value[] a) => MultiFunctions[index](a);
        internal override Func<Value, Value, Value> GetOperator(int index) => index == PowerIndex ? Pow : Operators[index];

        internal override Func<Value, Value> GetFunction(int index)
        {
            if (index == SqrIndex || index == SqrtIndex)
                return Sqrt;

            if (index == CbrtIndex)
                return Cbrt;

            return Functions[index];
        }


        public static Value Fact(Value a)
        {
            if (a.Units is not null)
#if BG
                throw new MathParser.MathParserException("Аргументът на функцията n! трябва да е бездименсионен.");
#else
                throw new MathParser.MathParserException("The argument of the n! function must be unitless.");
#endif

            return new(Fact(a.Re));
        }

        internal override Func<Value, Value, Value> GetFunction2(int index) => index == RootIndex ? Root : Functions2[index];
        internal override Func<Value[], Value> GetMultiFunction(int index) => MultiFunctions[index];

        private static Value Real(Value value) => value;
        private static Value Imaginary(Value value) => new(0.0);
        private static Value Phase(Value value) => new(value.Complex.Phase);
        private static Value Abs(Value value) => new(Math.Abs(value.Re), value.Units);

        private static Value Sign(Value value) => new(Math.Sign(value.Re));

        private static Value Sin(Value value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new(Complex.RealSin(FromAngleUnits(value)));
        }

        private static Value Cos(Value value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new(Complex.RealCos(FromAngleUnits(value)));
        }

        private static Value Tan(Value value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new(Math.Tan(FromAngleUnits(value)));
        }

        private static Value Csc(Value value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new(1 / Math.Sin(FromAngleUnits(value)));
        }

        private static Value Sec(Value value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new(1 / Math.Cos(FromAngleUnits(value)));
        }

        private static Value Cot(Value value)
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

        private static Value Asin(Value value)
        {
            CheckFunctionUnits("asin", value.Units);
            return ToAngleUnits(Math.Asin(value.Re));
        }

        private static Value Acos(Value value)
        {
            CheckFunctionUnits("acos", value.Units);
            return ToAngleUnits(Math.Acos(value.Re));
        }

        private static Value Atan(Value value)
        {
            CheckFunctionUnits("atan", value.Units);
            return ToAngleUnits(Math.Atan(value.Re));
        }

        private static Value Acsc(Value value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return ToAngleUnits(Math.Asin(1 / value.Re));
        }

        private static Value Asec(Value value)
        {
            CheckFunctionUnits("asec", value.Units);
            return ToAngleUnits(Math.Acos(1 / value.Re));
        }

        private static Value Acot(Value value)
        {
            CheckFunctionUnits("acot", value.Units);
            return ToAngleUnits(Math.Atan(1 / value.Re));
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
            return new(Math.Asinh(1 / value.Re));
        }

        private static Value Asech(Value value)
        {
            CheckFunctionUnits("asech", value.Units);
            return new(Math.Acosh(1 / value.Re));
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

        private static Value Pow(Value value, Value power) =>
            new(
                Math.Pow(value.Re, power.Re),
                Unit.Pow(value.Units, power)
            );

        private static Value UnitPow(Value value, Value power) =>
            new(
                Math.Pow(value.Re, power.Re),
                Unit.Pow(value.Units, power, value.IsUnit),
                value.IsUnit
            );

        private static Value Sqrt(Value value) => value.Units is null ?
                new(Math.Sqrt(value.Re)) :
                new(Math.Sqrt(value.Re), Unit.Root(value.Units, 2));

        private static Value UnitSqrt(Value value) => value.Units is null ?
                new(Math.Sqrt(value.Re)) :
                new(Math.Sqrt(value.Re), Unit.Root(value.Units, 2, value.IsUnit), value.IsUnit);

        private static Value Cbrt(Value value) =>
            value.Units is null ?
                new(Math.Cbrt(value.Re)) :
                new(Math.Cbrt(value.Re), Unit.Root(value.Units, 3));

        private static Value UnitCbrt(Value value) =>
            value.Units is null ?
                new(Math.Cbrt(value.Re)) :
                new(Math.Cbrt(value.Re), Unit.Root(value.Units, 3, value.IsUnit), value.IsUnit);

        private static Value Root(Value value, Value root)
        {
            var n = GetRoot(root);
            var result = Math.Pow(value.Re, 1.0 / n);
            return value.Units is null ?
                new(result) :
                new(result, Unit.Root(value.Units, n));
        }

        private static Value UnitRoot(Value value, Value root)
        {
            var n = GetRoot(root);
            var result = Math.Pow(value.Re, 1.0 / n);
            return value.Units is null ?
                new(result) :
                new(result, Unit.Root(value.Units, n, value.IsUnit), value.IsUnit);
        }

        private static Value Round(Value value) =>
            new(Math.Round(value.Re), value.Units);

        private static Value Floor(Value value) =>
            new(Math.Floor(value.Re), value.Units);

        private static Value Ceiling(Value value) =>
            new(Math.Ceiling(value.Re), value.Units);

        private static Value Truncate(Value value) =>
            new(Math.Truncate(value.Re), value.Units);

        private static Value Random(Value value) =>
            new(Complex.RealRandom(value.Re), value.Units);

        private static Value Atan2(Value a, Value b) =>
            new(
                Math.Atan2(b.Re * Unit.Convert(a.Units, b.Units, ','), a.Re)
            );

        private static Value MandelbrotSet(Value a, Value b) =>
            new(
                MandelbrotSet(
                    a.Re, b.Re * Unit.Convert(a.Units, b.Units, ',')
                ),
                a.Units
            );

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
                u = Unit.Multiply(u, v[i].Units, out var b, v[i].IsUnit);
                result *= v[i].Re * b;
            }
            if (u is not null)
                u = Unit.Root(u, v.Length);

            return new(Math.Pow(result, 1.0 / v.Length), u);
        }

        private static double FromAngleUnits(in Value value)
        {
            if (value.Units is null)
                return value.Re * _toRad[_degrees];

            return value.Re * value.Units.ConvertTo(_angleUnits[1]);
        }

        private static Value ToAngleUnits(double value) =>
            _returnAngleUnits ?
            new(value * _fromRad[_degrees], _angleUnits[_degrees]) :
            new(value * _fromRad[_degrees], null);
    }
}
