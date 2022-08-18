using System;

namespace Calcpad.Core
{
    internal class ComplexCalculator : Calculator
    {
        private static readonly Func<Value, Value, Value>[] Operators;
        private static readonly Func<Value, Value>[] Functions;
        private static readonly Func<Value, Value, Value>[] Functions2;
        private static readonly Func<Value[], Value>[] MultiFunctions;

        internal override int Degrees
        {
            set => _degrees = value;
        }

        static ComplexCalculator()
        {
            Operators = new Func<Value, Value, Value>[]
            {
                Pow,
                Divide,
                IntDiv,
                Reminder,
                Multiply,
                Subtract,
                Add,
                LessThan,
                GreaterThan,
                LessThanOrEqual,
                GreaterThanOrEqual,
                Equal,
                NotEqual,
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
                Abs,      //27
                Sign,     //28
                Sqrt,     //29
                Sqrt,     //30
                Cbrt,     //31
                Round,    //32
                Floor,    //33
                Ceiling,  //34
                Truncate, //35
                Real,     //36
                Imaginary,//37
                Phase,    //38
                Random,   //39
                Fact,     //40
                Negate    //41
            };

            Functions2 = new Func<Value, Value, Value>[]
            {
                Atan2,
                Root,
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
        internal override Func<Value, Value, Value> GetOperator(int index) => Operators[index];
        internal override Func<Value, Value> GetFunction(int index) => Functions[index];
        internal override Func<Value, Value, Value> GetFunction2(int index) => Functions2[index];
        internal override Func<Value[], Value> GetMultiFunction(int index) => MultiFunctions[index];

        private static Value Fact(Value value)
        {
            if (!(value.Number.IsReal))
#if BG
                throw new MathParser.MathParserException("Аргументът на функцията n! не може да е комплексно число.");
#else
                throw new MathParser.MathParserException("The argument of the n! function cannot be complex.");
#endif
            if (value.Units is not null)
#if BG
                throw new MathParser.MathParserException("Аргументът на функцията n! трябва да е бездименсионен.");
#else
                throw new MathParser.MathParserException("The argument of the n! function must be unitless.");
#endif
            return new(Fact(value.Number.Re));
        }

        private static Value Real(Value value) => new(value.Number.Real, value.Units);
        private static Value Imaginary(Value value) => new(value.Number.Imaginary, value.Units);
        private static Value Phase(Value value) => new(value.Number.Phase);
        private static Value Negate(Value a) => new(-a.Number, a.Units, a.IsUnit);

        private static Value Add(Value a, Value b) =>
            new(
                a.Number + b.Number * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        private static Value Subtract(Value a, Value b) =>
            new(
                a.Number - b.Number * Unit.Convert(a.Units, b.Units, '+'),
                a.Units
            );

        private static Value Multiply(Value a, Value b)
        {
            var uc = Unit.Multiply(a.Units, b.Units, out var d, b.IsUnit);
            var c = a.Number * b.Number * d;
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        private static Value Divide(Value a, Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d, b.IsUnit);
            var c = a.Number / b.Number * d;
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        private static Value Reminder(Value a, Value b)
        {
            if (b.Units is not null)
                throw new MathParser.MathParserException(
#if BG
                    $"Не мога да изчисля остатъка: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". Делителя трябва да е бездименсионен."
#else
                    $"Cannot evaluate reminder: \"{Unit.GetText(a.Units)}  %  {Unit.GetText(b.Units)}\". Denominator must be unitless."
#endif
                );
            return new(a.Number % b.Number, a.Units);
        }

        private static Value IntDiv(Value a, Value b)
        {
            var uc = Unit.Divide(a.Units, b.Units, out var d);
            var c = Complex.IntDiv(a.Number * d, b.Number);
            return new(c, uc, a.IsUnit && b.IsUnit);
        }

        private static Value Equal(Value a, Value b) =>
            new(
                a.Number == b.Number * Unit.Convert(a.Units, b.Units, '≡')
            );

        private static Value NotEqual(Value a, Value b) =>
            new(
                a.Number != b.Number * Unit.Convert(a.Units, b.Units, '≠')
            );

        private static Value LessThan(Value a, Value b) =>
            new(
                a.Number < b.Number * Unit.Convert(a.Units, b.Units, '<')
            );

        private static Value GreaterThan(Value a, Value b) =>
            new(
                a.Number > b.Number * Unit.Convert(a.Units, b.Units, '>')
            );

        private static Value LessThanOrEqual(Value a, Value b) =>
            new(
                a.Number <= b.Number * Unit.Convert(a.Units, b.Units, '≤')
            );

        private static Value GreaterThanOrEqual(Value a, Value b) =>
            new(
                a.Number >= b.Number * Unit.Convert(a.Units, b.Units, '≥')
            );

        private static Value Abs(Value value) =>
           new(Complex.Abs(value.Number), value.Units);

        private static Value Sign(Value value) =>
            new(Complex.Sign(value.Number));

        private static Value Sin(Value value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new(Complex.Sin(FromAngleUnits(value)));
        }

        private static Value Cos(Value value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new(Complex.Cos(FromAngleUnits(value)));
        }

        private static Value Tan(Value value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new(Complex.Tan(FromAngleUnits(value)));
        }

        private static Value Csc(Value value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new(1.0 / Complex.Sin(FromAngleUnits(value)));
        }

        private static Value Sec(Value value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new(1.0 / Complex.Cos(FromAngleUnits(value)));
        }

        private static Value Cot(Value value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new(Complex.Cot(FromAngleUnits(value)));
        }

        private static Value Sinh(Value value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new(Complex.Sinh(value.Number));
        }

        private static Value Cosh(Value value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new(Complex.Cosh(value.Number));
        }

        private static Value Tanh(Value value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new(Complex.Tanh(value.Number));
        }

        private static Value Csch(Value value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new(1 / Complex.Sinh(value.Number));
        }

        private static Value Sech(Value value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new(1 / Complex.Cosh(value.Number));
        }

        private static Value Coth(Value value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new(Complex.Coth(value.Number));
        }

        private static Value Asin(Value value)
        {
            CheckFunctionUnits("asin", value.Units);
            return ToAngleUnits(Complex.Asin(value.Number));
        }

        private static Value Acos(Value value)
        {
            CheckFunctionUnits("acos", value.Units);
            return ToAngleUnits(Complex.Acos(value.Number));
        }

        private static Value Atan(Value value)
        {
            CheckFunctionUnits("atan", value.Units);
            return ToAngleUnits(Complex.Atan(value.Number));
        }

        private static Value Acsc(Value value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return ToAngleUnits(Complex.Asin(1 / value.Number));
        }

        private static Value Asec(Value value)
        {
            CheckFunctionUnits("asec", value.Units);
            return ToAngleUnits(Complex.Acos(1 / value.Number));
        }

        private static Value Acot(Value value)
        {
            CheckFunctionUnits("acot", value.Units);
            return ToAngleUnits(Complex.Atan(1 / value.Number));
        }

        private static Value Asinh(Value value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new(Complex.Asinh(value.Number));
        }

        private static Value Acosh(Value value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new(Complex.Acosh(value.Number));
        }

        private static Value Atanh(Value value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new(Complex.Atanh(value.Number));
        }

        private static Value Acsch(Value value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return new(Complex.Asinh(1 / value.Number));
        }

        private static Value Asech(Value value)
        {
            CheckFunctionUnits("asech", value.Units);
            return new(Complex.Acosh(1 / value.Number));
        }

        private static Value Acoth(Value value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new(Complex.Acoth(value.Number));
        }

        private static Value Pow(Value value, Value power)
        {
            var result = Complex.Pow(value.Number, power.Number);
            var unit = Unit.Pow(value.Units, power, value.IsUnit);
            return new(result, unit, value.IsUnit);
        }

        private static Value Log(Value value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new(Complex.Log(value.Number));
        }

        private static Value Log10(Value value)
        {
            CheckFunctionUnits("log", value.Units);
            return new(Complex.Log10(value.Number));
        }

        private static Value Log2(Value value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new(Complex.Log2(value.Number));
        }

        private static Value Sqrt(Value value)
        {
            var result = Complex.Sqrt(value.Number);
            if (value.Units is null)
                return new(result);

            var unit = Unit.Root(value.Units, 2, value.IsUnit);
            return new(result, unit);
        }

        private static Value Cbrt(Value value)
        {
            var result = Complex.Cbrt(value.Number);
            if (value.Units is null)
                return new(result);

            var unit = Unit.Root(value.Units, 3, value.IsUnit);
            return new(result, unit);
        }

        private static Value Root(Value value, Value root)
        {
            var n = GetRoot(root);
            var result = Complex.Pow(value.Number, 1.0 / n);

            if (value.Units is null)
                return new(result);

            var unit = Unit.Root(value.Units, n, value.IsUnit);
            return new(result, unit);
        }

        private static Value Round(Value value) => new(Complex.Round(value.Number), value.Units);
        private static Value Floor(Value value) => new(Complex.Floor(value.Number), value.Units);
        private static Value Ceiling(Value value) => new(Complex.Ceiling(value.Number), value.Units);
        private static Value Truncate(Value value) => new(Complex.Truncate(value.Number), value.Units);
        private static Value Random(Value value) => new(Complex.Random(value.Number), value.Units);
        private static Value Atan2(Value a, Value b) =>
            new(
                Complex.Atan2(b.Number * Unit.Convert(a.Units, b.Units, ','), a.Number),
                a.Units
            );

        private static bool AreAllReal(Value[] v)
        {
            for (int i = 0, len = v.Length; i < len; ++i)
            {
                if (!v[i].Number.IsReal)
                    return false;
            }
            return true;
        }

        private static Value MandelbrotSet(Value a, Value b) =>
            new(
                MandelbrotSet(
                    a.Number.Re, b.Number.Re * Unit.Convert(a.Units, b.Units, ',')
                ),
                a.Units
            );

        private static new Value Min(Value[] v) =>
            AreAllReal(v) ?
                Calculator.Min(v) :
                new(double.NaN, v[0].Units);

        private static new Value Max(Value[] v) =>
            AreAllReal(v) ?
                Calculator.Max(v) :
                new(double.NaN, v[0].Units);

        private static Value Sum(Value[] v)
        {
            var result = v[0].Number;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
                result += v[i].Number * Unit.Convert(u, v[i].Units, ',');

            return new(result, u);
        }

        private static Value SumSq(Value[] v)
        {
            var result = v[0].Number;
            var u = v[0].Units;
            result *= result;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Number * Unit.Convert(u, v[i].Units, ',');
                result += b * b;
            }
            return new(result, u * u);
        }

        private static Value Srss(Value[] v)
        {
            var result = v[0].Number;
            var u = v[0].Units;
            result *= result;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                var b = v[i].Number * Unit.Convert(u, v[i].Units, ',');
                result += b * b;
            }
            return new(Complex.Sqrt(result), u);
        }

        private static Value Average(Value[] v)
        {
            var result = v[0].Number;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
                result += v[i].Number * Unit.Convert(u, v[i].Units, ',');

            return new(result / v.Length, u);
        }

        private static Value Product(Value[] v)
        {
            var result = v[0].Number;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                u = Unit.Multiply(u, v[i].Units, out var b);
                result *= v[i].Number * b;
            }
            return new(result, u);
        }

        private static Value Mean(Value[] v)
        {
            var result = v[0].Number;
            var u = v[0].Units;
            for (int i = 1, len = v.Length; i < len; ++i)
            {
                u = Unit.Multiply(u, v[i].Units, out var b, v[i].IsUnit);
                result *= v[i].Number * b;
            }
            return new(Complex.Pow(result, 1.0 / v.Length), Unit.Root(u, v.Length));
        }

        private static Complex FromAngleUnits(Value value)
        {
            if (value.Units is null)
                return value.Number * _toRad[_degrees];

            return value.Number * value.Units.ConvertTo(_angleUnits[1]);
        }

        private static Value ToAngleUnits(Complex value) =>
            _returnAngleUnits ? 
            new(value * _fromRad[_degrees], _angleUnits[_degrees]) :
            new(value * _fromRad[_degrees], null);
    }
}
