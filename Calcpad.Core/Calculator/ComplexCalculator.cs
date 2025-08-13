using System;

namespace Calcpad.Core
{
    internal class ComplexCalculator : Calculator
    {
        private static readonly Operator<ComplexValue>[] _operators =
            [
                Pow,
                (in ComplexValue a, in ComplexValue b) =>  a / b,
                ComplexValue.IntDiv,
                (in ComplexValue a, in ComplexValue b) => a % b,
                (in ComplexValue a, in ComplexValue b) => a * b,
                (in ComplexValue a, in ComplexValue b) => a - b,
                (in ComplexValue a, in ComplexValue b) => a + b,
                (in ComplexValue a, in ComplexValue b) => a < b,
                (in ComplexValue a, in ComplexValue b) => a > b,
                (in ComplexValue a, in ComplexValue b) => a <= b,
                (in ComplexValue a, in ComplexValue b) => a >= b,
                (in ComplexValue a, in ComplexValue b) => a == b,
                (in ComplexValue a, in ComplexValue b) => a != b,
                (in ComplexValue a, in ComplexValue b) => a & b,
                (in ComplexValue a, in ComplexValue b) => a | b,
                (in ComplexValue a, in ComplexValue b) => a ^ b,
                (in ComplexValue _, in ComplexValue b) => b, 
                (in ComplexValue a, in ComplexValue b) => Phasor(a, b),
            ];
        private readonly Function<ComplexValue>[] _functions;
        private readonly Operator<ComplexValue>[] Functions2;
        private static readonly Func<IScalarValue[], IScalarValue>[] MultiFunctions =
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
                Imaginary,//38s
                Phase,    //39
                Conjugate,//40
                Random,   //41
                Fact,     //42
                (in ComplexValue a) => -a,   //43
                Not,      //44
                Timer     //45  
            ];

            Functions2 =
            [
                Atan2,
                Root,
                (in ComplexValue a, in ComplexValue b) => a % b,
                (in ComplexValue a, in ComplexValue b) => MandelbrotSet(a, b)
            ];
        }

        internal override IScalarValue EvaluateOperator(long index, in IScalarValue a, in IScalarValue b) => _operators[index](a.AsComplex(), b.AsComplex());
        internal override IScalarValue EvaluateFunction(long index, in IScalarValue a) => _functions[index](a.AsComplex());
        internal override IScalarValue EvaluateFunction2(long index, in IScalarValue a, in IScalarValue b) => Functions2[index](a.AsComplex(), b.AsComplex());
        internal override IValue EvaluateFunction3(long index, in IValue a, in IValue b, in IValue c) => Functions3[index](a, b, c);
        internal override IScalarValue EvaluateMultiFunction(long index, IScalarValue[] a) => MultiFunctions[index](a);
        internal override IScalarValue EvaluateInterpolation(long index, IScalarValue[] a) => Interpolations[index](a);
        internal override Operator<RealValue> GetOperator(long index) => (in RealValue a, in RealValue b) => (RealValue)_operators[index](a, b);
        internal override Function<RealValue> GetFunction(long index) => (in RealValue a) => (RealValue)_functions[index](a);
        internal override Operator<RealValue> GetFunction2(long index) => (in RealValue a, in RealValue b) => (RealValue)_operators[index](a, b);
        internal override Function3 GetFunction3(long index) => Functions3[index];
        internal override Func<IScalarValue[], IScalarValue> GetMultiFunction(long index) => MultiFunctions[index];

        private static ComplexValue Fact(in ComplexValue value)
        {
            if (!(value.IsReal))
                throw Exceptions.FactorialArgumentComplex();

            if (value.Units is not null)
                throw Exceptions.FactorialArgumentUnitless();

            return new(Fact(value.A));
        }

        private static ComplexValue Real(in ComplexValue value) => new(value.A, value.Units);
        private static ComplexValue Imaginary(in ComplexValue value) => new(value.B, value.Units);
        internal static ComplexValue Abs(in ComplexValue value) =>
          new(Complex.Abs(value.Complex), value.Units);
        private static ComplexValue Phase(in ComplexValue value) => new(value.Complex.Phase);
        internal static ComplexValue Conjugate(in ComplexValue value) =>
           new(value.Complex.Conjugate, value.Units);

        private static ComplexValue Sign(in ComplexValue value) =>
            new(Complex.Sign(value.Complex));

        private ComplexValue Sin(in ComplexValue value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new(Complex.Sin(FromAngleUnits(value)));
        }

        private ComplexValue Cos(in ComplexValue value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new(Complex.Cos(FromAngleUnits(value)));
        }


        internal static ComplexValue Phasor(ComplexValue a, ComplexValue b)
        {
            CheckFunctionUnits("phasor", b.Units);
            var u = b.Units;
            var d = u is null ? 1d : u.ConvertTo(Unit.Get("rad"));
            var phi = b.A * d;
            var c = a.A * (Math.Cos(phi) + Complex.ImaginaryOne * Math.Sin(phi));
            return new ComplexValue(c, a.Units, a.IsUnit);
        }

        private ComplexValue Tan(in ComplexValue value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new(Complex.Tan(FromAngleUnits(value)));
        }

        private ComplexValue Csc(in ComplexValue value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new(1d / Complex.Sin(FromAngleUnits(value)));
        }

        private ComplexValue Sec(in ComplexValue value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new(1d / Complex.Cos(FromAngleUnits(value)));
        }

        private ComplexValue Cot(in ComplexValue value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new(Complex.Cot(FromAngleUnits(value)));
        }

        private static ComplexValue Sinh(in ComplexValue value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new(Complex.Sinh(value.Complex));
        }

        private static ComplexValue Cosh(in ComplexValue value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new(Complex.Cosh(value.Complex));
        }

        private static ComplexValue Tanh(in ComplexValue value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new(Complex.Tanh(value.Complex));
        }

        private static ComplexValue Csch(in ComplexValue value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new(1d / Complex.Sinh(value.Complex));
        }

        private static ComplexValue Sech(in ComplexValue value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new(1d / Complex.Cosh(value.Complex));
        }

        private static ComplexValue Coth(in ComplexValue value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new(Complex.Coth(value.Complex));
        }

        private ComplexValue Asin(in ComplexValue value)
        {
            CheckFunctionUnits("asin", value.Units);
            return ToAngleUnits(Complex.Asin(value.Complex));
        }

        private ComplexValue Acos(in ComplexValue value)
        {
            CheckFunctionUnits("acos", value.Units);
            return ToAngleUnits(Complex.Acos(value.Complex));
        }

        private ComplexValue Atan(in ComplexValue value)
        {
            CheckFunctionUnits("atan", value.Units);
            return ToAngleUnits(Complex.Atan(value.Complex));
        }

        private ComplexValue Acsc(in ComplexValue value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return value.IsZero ?
                ComplexValue.ComplexInfinity :
                ToAngleUnits(Complex.Asin(1d / value.Complex));
        }

        private ComplexValue Asec(in ComplexValue value)
        {
            CheckFunctionUnits("asec", value.Units);
            return value.IsZero ?
                ComplexValue.ComplexInfinity :
                ToAngleUnits(Complex.Acos(1d / value.Complex));
        }

        private ComplexValue Acot(in ComplexValue value)
        {
            CheckFunctionUnits("acot", value.Units);
            return ToAngleUnits(Complex.Acot(value.Complex));
        }

        private static ComplexValue Asinh(in ComplexValue value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new(Complex.Asinh(value.Complex));
        }

        private static ComplexValue Acosh(in ComplexValue value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new(Complex.Acosh(value.Complex));
        }

        private static ComplexValue Atanh(in ComplexValue value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new(Complex.Atanh(value.Complex));
        }

        private static ComplexValue Acsch(in ComplexValue value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return value.IsZero ?
                ComplexValue.ComplexInfinity :
                new(Complex.Asinh(1d / value.Complex));
        }

        private static ComplexValue Asech(in ComplexValue value)
        {
            CheckFunctionUnits("asech", value.Units);
            return value.IsZero ?
                ComplexValue.ComplexInfinity :
                new(Complex.Acosh(1d / value.Complex));
        }

        private static ComplexValue Acoth(in ComplexValue value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new(Complex.Acoth(value.Complex));
        }

        private static ComplexValue Pow(in ComplexValue value, in ComplexValue power)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Pow(value.Complex * u.GetDimensionlessFactor(), power.Complex));

            var result = Complex.Pow(value.Complex, power.Complex);
            var unit = Unit.Pow(u, power, value.IsUnit);
            return new(result, unit, value.IsUnit);
        }

        private static ComplexValue Log(in ComplexValue value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new(Complex.Log(value.Complex));
        }

        private static ComplexValue Log10(in ComplexValue value)
        {
            CheckFunctionUnits("log", value.Units);
            return new(Complex.Log10(value.Complex));
        }

        private static ComplexValue Log2(in ComplexValue value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new(Complex.Log2(value.Complex));
        }

        private static ComplexValue Exp(in ComplexValue value)
        {
            CheckFunctionUnits("exp", value.Units);
            return new(Complex.Exp(value.Complex));
        }

        internal static ComplexValue Sqrt(in ComplexValue value)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Sqrt(value.Complex * u.GetDimensionlessFactor()));

            var result = Complex.Sqrt(value.Complex);
            if (u is null)
                return new(result);

            var unit = Unit.Root(u, 2, value.IsUnit);
            return new(result, unit, value.IsUnit);
        }

        private static ComplexValue Cbrt(in ComplexValue value)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Cbrt(value.Complex * u.GetDimensionlessFactor()));

            var result = Complex.Cbrt(value.Complex);
            if (u is null)
                return new(result);

            var unit = Unit.Root(u, 3, value.IsUnit);
            return new(result, unit, value.IsUnit);
        }

        private static ComplexValue Root(in ComplexValue value, in ComplexValue root)
        {
            var n = GetRoot(root);

            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Complex.Pow(value.Complex * u.GetDimensionlessFactor(), 1d / n));

            var result = Complex.Pow(value.Complex, 1d / n);

            if (u is null)
                return new(result);

            var unit = Unit.Root(u, n, value.IsUnit);
            return new(result, unit, value.IsUnit);
        }

        private static ComplexValue Round(in ComplexValue value) => new(Complex.Round(value.Complex), value.Units);
        private static ComplexValue Floor(in ComplexValue value) => new(Complex.Floor(value.Complex), value.Units);
        private static ComplexValue Ceiling(in ComplexValue value) => new(Complex.Ceiling(value.Complex), value.Units);
        private static ComplexValue Truncate(in ComplexValue value) => new(Complex.Truncate(value.Complex), value.Units);
        private static ComplexValue Random(in ComplexValue value) => new(Complex.Random(value.Complex), value.Units);
        private ComplexValue Atan2(in ComplexValue a, in ComplexValue b) =>
            ToAngleUnits(Complex.Atan2(b.Complex * Unit.Convert(a.Units, b.Units, ','), a.Complex));

        private static bool AreAllReal(IScalarValue[] values)
        {
            for (int i = 0, len = values.Length; i < len; ++i)
            {
                if (!values[i].IsReal)
                    return false;
            }
            return true;
        }

        private new static IScalarValue Min(IScalarValue[] values) =>
            AreAllReal(values) ?
                Calculator.Min(values) :
                new RealValue(double.NaN, values[0].Units);

        private new static IScalarValue Max(IScalarValue[] values) =>
            AreAllReal(values) ?
                Calculator.Max(values) :
                new RealValue(double.NaN, values[0].Units);

        private static IScalarValue Sum(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Complex;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                result += value.Complex * Unit.Convert(u, value.Units, ',');
            }

            return new ComplexValue(result, u);
        }

        private static IScalarValue SumSq(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Complex;
            var u = value.Units;
            result *= result;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                var b = value.Complex * Unit.Convert(u, value.Units, ',');
                result += b * b;
            }
            return new ComplexValue(result, u?.Pow(2f));
        }

        private static IScalarValue Srss(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Complex;
            var u = value.Units;
            result *= result;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                var b = value.Complex * Unit.Convert(u, value.Units, ',');
                result += b * b;
            }
            return new ComplexValue(Complex.Sqrt(result), u);
        }

        private static IScalarValue Average(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Complex;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                result += value.Complex * Unit.Convert(u, value.Units, ',');
            }

            return new ComplexValue(result / values.Length, u);
        }

        private static IScalarValue Product(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Complex;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                u = Unit.Multiply(u, value.Units, out var b);
                result *= value.Complex * b;
            }
            return new ComplexValue(result, u);
        }

        private static IScalarValue Mean(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Complex;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                u = Unit.Multiply(u, value.Units, out var b);
                result *= value.Complex * b;
            }
            result = Complex.Pow(result, 1.0 / values.Length);
            if (u is null)
                return new ComplexValue(result);

            u = Unit.Root(u, values.Length);
            return new ComplexValue(result, u);
        }

        private new static IScalarValue Gcd(IScalarValue[] v) =>
            AreAllReal(v) ? Calculator.Gcd(v) : new(double.NaN, v[0].Units);

        private new static IScalarValue Lcm(IScalarValue[] v) =>
            AreAllReal(v) ? Calculator.Lcm(v) : new(double.NaN, v[0].Units);

        protected static ComplexValue Not(in ComplexValue value) =>
            Math.Abs(value.A) < RealValue.LogicalZero ? RealValue.One : RealValue.Zero;

        private Complex FromAngleUnits(in ComplexValue value)
        {
            if (value.Units is null)
                return value.Complex * ToRad[_degrees];

            return value.Complex * value.Units.ConvertTo(AngleUnits[1]);
        }

        private ComplexValue ToAngleUnits(Complex value) =>
            _returnAngleUnits ?
            new(value * FromRad[_degrees], AngleUnits[_degrees]) :
            new(value * FromRad[_degrees]);

        protected static ComplexValue Timer(in ComplexValue _) => new(Timer(), Unit.Get("s"));

    }
}
