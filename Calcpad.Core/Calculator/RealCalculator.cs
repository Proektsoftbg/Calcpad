using System;
using System.Linq;

namespace Calcpad.Core
{
    internal class RealCalculator : Calculator
    {
        private static readonly Operator<RealValue>[] _operators =
            [
                UnitPow,
                RealValue.Divide,
                RealValue.IntDiv,
                (in RealValue a, in RealValue b) => a % b,
                RealValue.Multiply,
                (in RealValue a, in RealValue b) => a - b,
                (in RealValue a, in RealValue b) => a + b,
                (in RealValue a, in RealValue b) => a < b,
                (in RealValue a, in RealValue b) => a > b,
                (in RealValue a, in RealValue b) => a <= b,
                (in RealValue a, in RealValue b) => a >= b,
                (in RealValue a, in RealValue b) => a == b,
                (in RealValue a, in RealValue b) => a != b,
                (in RealValue a, in RealValue b) => a & b,
                (in RealValue a, in RealValue b) => a | b,
                (in RealValue a, in RealValue b) => a ^ b,
                (in RealValue _, in RealValue b) => b
            ];
        private readonly Function<RealValue>[] _functions;
        private readonly Operator<RealValue>[] _functions2;
        private static readonly Func<IScalarValue[], IScalarValue>[] MultiFunctions =
            [
                (IScalarValue[] values) => Min(values),
                (IScalarValue[] values) => Max(values),
                Sum,
                SumSq,
                Srss,
                Average,
                Product,
                Mean,
                Switch,
                (IScalarValue[] values) => And(values),
                (IScalarValue[] values) => Or(values),
                (IScalarValue[] values) => Xor(values),
                (IScalarValue[] values) => Gcd(values),
                (IScalarValue[] values) => Lcm(values),
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
                (in RealValue a) => -a, //42
                (in RealValue a) => Not(a),      //43
                Timer     //44
            ];

            _functions2 =
            [
                Atan2,
                UnitRoot,
                (in RealValue a, in RealValue b) => a % b,
                (in RealValue a, in RealValue b) => MandelbrotSet(a, b)
            ];
        }

        internal override IScalarValue EvaluateOperator(long index, in IScalarValue a, in IScalarValue b) => _operators[index](a.AsReal(), b.AsReal());
        internal override IScalarValue EvaluateFunction(long index, in IScalarValue a) => _functions[index](a.AsReal());
        internal override IScalarValue EvaluateFunction2(long index, in IScalarValue a, in IScalarValue b) => _functions2[index](a.AsReal(), b.AsReal());
        internal override IValue EvaluateFunction3(long index, in IValue a, in IValue b, in IValue c) => Functions3[index](a, b, c);
        internal override IScalarValue EvaluateMultiFunction(long index, IScalarValue[] a) => MultiFunctions[index](a);
        internal override IScalarValue EvaluateInterpolation(long index, IScalarValue[] a) => Interpolations[index](a);
        internal override Operator<RealValue> GetOperator(long index) => index == PowerIndex ? Pow : _operators[index];
        internal override Function<RealValue> GetFunction(long index)
        {
            if (index == SqrIndex || index == SqrtIndex)
                return Sqrt;

            if (index == CbrtIndex)
                return Cbrt;

            return _functions[index];
        }
        internal override Operator<RealValue> GetFunction2(long index) => index == RootIndex ? Root : _functions2[index];
        internal override Function3 GetFunction3(long index) => Functions3[index];
        internal override Func<IScalarValue[], IScalarValue> GetMultiFunction(long index) => MultiFunctions[index];

        public static RealValue Fact(in RealValue a)
        {
            if (a.Units is not null)
                Throw.FactorialArgumentUnitlessException();

            return new(Fact(a.D));
        }

        private static RealValue Real(in RealValue value) => value;
        private static RealValue Imaginary(in RealValue _) => RealValue.Zero;
        private static RealValue Phase(in RealValue value) => new(value.D >= 0d ? 0d : Math.PI);
        internal static RealValue Abs(in RealValue value) => new(Math.Abs(value.D), value.Units);
        private static RealValue Sign(in RealValue value) => double.IsNaN(value.D) ? RealValue.NaN : new(Math.Sign(value.D));

        private RealValue Sin(in RealValue value)
        {
            CheckFunctionUnits("sin", value.Units);
            return new(Complex.RealSin(FromAngleUnits(value)));
        }

        private RealValue Cos(in RealValue value)
        {
            CheckFunctionUnits("cos", value.Units);
            return new(Complex.RealCos(FromAngleUnits(value)));
        }

        private RealValue Tan(in RealValue value)
        {
            CheckFunctionUnits("tan", value.Units);
            return new(Math.Tan(FromAngleUnits(value)));
        }

        private RealValue Csc(in RealValue value)
        {
            CheckFunctionUnits("csc", value.Units);
            return new(1 / Math.Sin(FromAngleUnits(value)));
        }

        private RealValue Sec(in RealValue value)
        {
            CheckFunctionUnits("sec", value.Units);
            return new(1 / Math.Cos(FromAngleUnits(value)));
        }

        private RealValue Cot(in RealValue value)
        {
            CheckFunctionUnits("cot", value.Units);
            return new(1 / Math.Tan(FromAngleUnits(value)));
        }

        private static RealValue Sinh(in RealValue value) /* Hyperbolic sin */
        {
            CheckFunctionUnits("sinh", value.Units);
            return new(Math.Sinh(value.D));
        }

        private static RealValue Cosh(in RealValue value)
        {
            CheckFunctionUnits("cosh", value.Units);
            return new(Math.Cosh(value.D));
        }

        private static RealValue Tanh(in RealValue value)
        {
            CheckFunctionUnits("tanh", value.Units);
            return new(Math.Tanh(value.D));
        }

        private static RealValue Csch(in RealValue value)
        {
            CheckFunctionUnits("csch", value.Units);
            return new(1 / Math.Sinh(value.D));
        }

        private static RealValue Sech(in RealValue value)
        {
            CheckFunctionUnits("sech", value.Units);
            return new(1 / Math.Cosh(value.D));
        }

        private static RealValue Coth(in RealValue value)
        {
            CheckFunctionUnits("coth", value.Units);
            return new(1 / Math.Tanh(value.D));
        }

        private RealValue Asin(in RealValue value)
        {
            CheckFunctionUnits("asin", value.Units);
            return ToAngleUnits(Math.Asin(value.D));
        }

        private RealValue Acos(in RealValue value)
        {
            CheckFunctionUnits("acos", value.Units);
            return ToAngleUnits(Math.Acos(value.D));
        }

        private RealValue Atan(in RealValue value)
        {
            CheckFunctionUnits("atan", value.Units);
            return ToAngleUnits(Math.Atan(value.D));
        }

        private RealValue Acsc(in RealValue value)
        {
            CheckFunctionUnits("acsc", value.Units);
            return value.D == 0d ?
                RealValue.PositiveInfinity :
                ToAngleUnits(Math.Asin(1d / value.D));
        }

        private RealValue Asec(in RealValue value)
        {
            CheckFunctionUnits("asec", value.Units);
            return value.D == 0d ?
                RealValue.PositiveInfinity :
                ToAngleUnits(Math.Acos(1d / value.D));
        }

        private RealValue Acot(in RealValue value)
        {
            CheckFunctionUnits("acot", value.Units);
            return ToAngleUnits(Math.Atan(1d / value.D));
        }

        private static RealValue Asinh(in RealValue value)
        {
            CheckFunctionUnits("asinh", value.Units);
            return new(Math.Asinh(value.D));
        }

        private static RealValue Acosh(in RealValue value)
        {
            CheckFunctionUnits("acosh", value.Units);
            return new(Math.Acosh(value.D));
        }

        private static RealValue Atanh(in RealValue value)
        {
            CheckFunctionUnits("atanh", value.Units);
            return new(Math.Atanh(value.D));
        }

        private static RealValue Acsch(in RealValue value)
        {
            CheckFunctionUnits("acsch", value.Units);
            return new(Math.Asinh(1d / value.D));
        }

        private static RealValue Asech(in RealValue value)
        {
            CheckFunctionUnits("asech", value.Units);
            return new(Math.Acosh(1d / value.D));
        }

        private static RealValue Acoth(in RealValue value)
        {
            CheckFunctionUnits("acoth", value.Units);
            return new(Math.Atanh(1 / value.D));
        }

        private static RealValue Log(in RealValue value)
        {
            CheckFunctionUnits("ln", value.Units);
            return new(Math.Log(value.D));
        }

        private static RealValue Log10(in RealValue value)
        {
            CheckFunctionUnits("log", value.Units);
            return new(Math.Log10(value.D));
        }

        private static RealValue Log2(in RealValue value)
        {
            CheckFunctionUnits("log_2", value.Units);
            return new(Math.Log2(value.D));
        }

        private static RealValue Exp(in RealValue value)
        {
            CheckFunctionUnits("exp", value.Units);
            return new(Math.Exp(value.D));
        }

        private static RealValue Pow(RealValue value, RealValue power, bool isUnit)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new RealValue(Math.Pow(value.D * u.GetDimensionlessFactor(), power.D));

            return new(
                Math.Pow(value.D, power.D),
                Unit.Pow(u, power, isUnit),
                isUnit
            );
        }

        internal static RealValue Pow(in RealValue value, in RealValue power) =>
            Pow(value, power, false);

        private static RealValue UnitPow(in RealValue value, in RealValue power) =>
            Pow(value, power, value.IsUnit);

        private static RealValue Sqrt(RealValue value, bool isUnit)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Math.Sqrt(value.D * u.GetDimensionlessFactor()));

            var result = Math.Sqrt(value.D);
            return u is null ?
                new(result) :
                new(result, Unit.Root(u, 2, isUnit), isUnit);
        }

        internal static RealValue Sqrt(in RealValue value) =>
            Sqrt(value, false);

        private static RealValue UnitSqrt(in RealValue value) =>
            Sqrt(value, value.IsUnit);

        private static RealValue Cbrt(RealValue value, bool isUnit)
        {
            var u = value.Units;
            if (u is not null && u.IsDimensionless)
                return new(Math.Cbrt(value.D * u.GetDimensionlessFactor()));

            var result = Math.Cbrt(value.D);
            return u is null ?
                new(result) :
                new(result, Unit.Root(u, 3, isUnit), isUnit);
        }

        private static RealValue Cbrt(in RealValue value) =>
            Cbrt(value, false);

        private static RealValue UnitCbrt(in RealValue value) =>
            Cbrt(value, value.IsUnit);

        private static RealValue Root(in RealValue value, in RealValue root, bool isUnit)
        {
            var n = GetRoot(root);
            var u = value.Units;
            var d = u is not null && u.IsDimensionless ?
                value.D * u.GetDimensionlessFactor() :
                value.D;

            var result = int.IsOddInteger(n) && d < 0 ?
                -Math.Pow(-d, 1d / n) :
                Math.Pow(d, 1d / n);

            return u is null ?
                new(result) :
                new(result, Unit.Root(u, n, isUnit), isUnit);
        }

        private static RealValue Root(in RealValue value, in RealValue root) =>
            Root(value, root, false);

        private static RealValue UnitRoot(in RealValue value, in RealValue root) =>
            Root(value, root, value.IsUnit);

        private static RealValue Round(in RealValue value) =>
            new(Math.Round(value.D, MidpointRounding.AwayFromZero), value.Units);

        private static RealValue Floor(in RealValue value) =>
            new(Math.Floor(value.D), value.Units);

        private static RealValue Ceiling(in RealValue value) =>
            new(Math.Ceiling(value.D), value.Units);

        private static RealValue Truncate(in RealValue value) =>
            new(Math.Truncate(value.D), value.Units);

        private static RealValue Random(in RealValue value) =>
            new(Complex.RealRandom(value.D), value.Units);

        private RealValue Atan2(in RealValue a, in RealValue b) =>
            ToAngleUnits(Math.Atan2(b.D * Unit.Convert(a.Units, b.Units, ','), a.D));

        private static IScalarValue Sum(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                result += value.Re * Unit.Convert(u, value.Units, ',');
            }
            return new RealValue(result, u);
        }

        private static IScalarValue SumSq(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            result *= result;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                var b = value.Re * Unit.Convert(u, value.Units, ',');
                result += b * b;
            }
            return new RealValue(result, u?.Pow(2f));
        }

        internal static IScalarValue Srss(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            result *= result;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                var b = value.Re * Unit.Convert(u, value.Units, ',');
                result += b * b;
            }
            return new RealValue(Math.Sqrt(result), u);
        }

        private static IScalarValue Average(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                result += value.Re * Unit.Convert(u, value.Units, ',');
            }
            return new RealValue(result / values.Length, u);
        }

        private static IScalarValue Product(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                u = Unit.Multiply(u, value.Units, out var b);
                result *= value.Re * b;
            }
            return new RealValue(result, u);
        }

        private static IScalarValue Mean(IScalarValue[] values)
        {
            ref var value = ref values[0];
            var result = value.Re;
            var u = value.Units;
            for (int i = 1, len = values.Length; i < len; ++i)
            {
                value = ref values[i];
                u = Unit.Multiply(u, value.Units, out var b);
                result *= value.Re * b;
            }
            result = Math.Pow(result, 1.0 / values.Length);
            if (u is null)
                return new  RealValue(result);

            u = Unit.Root(u, values.Length);
            return new RealValue(result, u);
        }

        protected static RealValue Not(in RealValue value) =>
            Math.Abs(value.D) < RealValue.LogicalZero ? RealValue.One : RealValue.Zero;

        private double FromAngleUnits(in RealValue value)
        {
            if (value.Units is null)
                return value.D * ToRad[_degrees];

            return value.D * value.Units.ConvertTo(AngleUnits[1]);
        }

        private RealValue ToAngleUnits(double value) =>
            _returnAngleUnits ?
            new(value * FromRad[_degrees], AngleUnits[_degrees]) :
            new(value * FromRad[_degrees]);

        protected static RealValue Timer(in RealValue _) => new(Timer(), Unit.Get("s"));
    }
}
