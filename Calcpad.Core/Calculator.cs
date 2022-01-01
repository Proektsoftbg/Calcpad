using System;
using System.Collections.Generic;

namespace Calcpad.Core
{
    internal abstract class Calculator
    {
        internal const char NegChar = '‐'; //hyphen, not minus "-"
        protected const double Deg2Rad = Math.PI / 180.0;
        protected const double Rad2deg = 180.0 / Math.PI;
        internal delegate Value Operator(Value a, Value b);
        internal delegate Value Function(in Value a);
        internal delegate Value MultiFunction(Value[] a);

        internal abstract bool Degrees { set; }
        //                                               ^  ÷  \  %  *  -  +  <  >  ≤  ≥  ≡  ≠  =
        internal static readonly int[] OperatorOrder = { 0, 3, 3, 3, 3, 4, 5, 6, 6, 6, 6, 6, 6, 7 };
        internal static Dictionary<char, int> OperatorIndex { get; } = new()
        {
            { '^', 0 },
            { '/', 1 },
            { '÷', 1 },
            { '\\', 2 },
            { '%', 3 },
            { '*', 4 },
            { '-', 5 },
            { '+', 6 },
            { '<', 7 },
            { '>', 8 },
            { '≤', 9 },
            { '≥', 10 },
            { '≡', 11 },
            { '≠', 12 },
            { '=', 13 }
        };

        internal static readonly Dictionary<string, int> FunctionIndex = new()
        {
            {"sin",      0},
            {"cos",      1},
            {"tan",      2},
            {"csc",      3},
            {"sec",      4},
            {"cot",      5},
            {"asin",     6},
            {"acos",     7},
            {"atan",     8},
            {"acsc",     9},
            {"asec",    10},
            {"acot",    11},
            {"sinh",    12},
            {"cosh",    13},
            {"tanh",    14},
            {"csch",    15},
            {"sech",    16},
            {"coth",    17},
            {"asinh",   18},
            {"acosh",   19},
            {"atanh",   20},
            {"acsch",   21},
            {"asech",   22},
            {"acoth",   23},
            {"log",     24},
            {"ln",      25},
            {"log_2",   26},
            {"abs",     27},
            {"sign",    28},
            {"sqr",     29},
            {"sqrt",    30},
            {"cbrt",    31},
            {"round",   32},
            {"floor",   33},
            {"ceiling", 34},
            {"trunc",   35},
            {"re",      36},
            {"im",      37},
            {"phase",   38},
            {"random",  39},
            {"fact",    40},
            { "‐",      41}
        };

        internal static readonly Dictionary<string, int> Function2Index = new ()
        {
            {"atan2", 0},
            {"root", 1},
            {"mandelbrot", 2}
        };

        internal static readonly Dictionary<string, int> MultiFunctionIndex = new()
        {
            { "min", 0 },
            { "max", 1 },
            { "sum", 2 },
            { "sumsq", 3 },
            { "srss", 4 },
            { "average", 5 },
            { "product", 6 },
            { "mean", 7 },
            { "switch", 8 },
            { "take", 9 },
            { "line", 10 },
            { "spline", 11 }
        };

        internal static bool IsOperator(char name) => OperatorIndex.ContainsKey(name);
        internal static bool IsFunction(string name) => FunctionIndex.ContainsKey(name);
        internal static bool IsFunction2(string name) => Function2Index.ContainsKey(name);
        internal static bool IsMultiFunction(string name) => MultiFunctionIndex.ContainsKey(name);
        internal abstract Value EvaluateOperator(int index, Value a, Value b);
        internal abstract Value EvaluateFunction(int index, Value a);
        internal abstract Value EvaluateFunction2(int index, Value a, Value b);
        internal abstract Value EvaluateMultiFunction(int index, Value[] a);
        internal abstract Operator GetOperator(int index);
        internal abstract Function GetFunction(int index);
        internal abstract Operator GetFunction2(int index);
        internal abstract MultiFunction GetMultiFunction(int index);

        internal static readonly int PowerIndex = OperatorIndex['^'];
        internal static readonly int SqrIndex = FunctionIndex["sqr"];
        internal static readonly int SqrtIndex = FunctionIndex["sqrt"];
        internal static readonly int CbrtIndex = FunctionIndex["cbrt"];
        internal static readonly int RootIndex = Function2Index["root"];
    }

    internal class RealCalculator : Calculator
    {
        private static readonly Operator[] Operators;
        private static readonly Function[] DegFunctions, RadFunctions;
        private Function[] _functions;
        private static readonly Operator[] Functions2;
        private static readonly MultiFunction[] MultiFunctions;

        internal override bool Degrees
        {
            set => _functions = value ? DegFunctions : RadFunctions;
        }
        static RealCalculator()
        {
            Operators = new Operator[]
            {
                Value.UnitPow,
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

            RadFunctions = new Function[]
            {
                Value.Sin,      // 0
                Value.Cos,      // 1
                Value.Tan,      // 2
                Value.Csc,      // 3
                Value.Sec,      // 4
                Value.Cot,      // 5
                Value.Asin,     // 6
                Value.Acos,     // 7
                Value.Atan,     // 8
                Value.Acsc,     // 9
                Value.Asec,     //10
                Value.Acot,     //11
                Value.Sinh,     //12
                Value.Cosh,     //13
                Value.Tanh,     //14
                Value.Csch,     //15
                Value.Sech,     //16
                Value.Coth,     //17
                Value.Asinh,    //18
                Value.Acosh,    //19
                Value.Atanh,    //20
                Value.Acsch,    //21
                Value.Asech,    //22
                Value.Acoth,    //23
                Value.Log10,    //24
                Value.Log,      //25
                Value.Log2,     //26
                Value.Abs,      //27
                Value.Sign,     //28
                Value.UnitSqrt, //29
                Value.UnitSqrt, //30
                Value.UnitCbrt, //31
                Value.Round,    //32
                Value.Floor,    //33
                Value.Ceiling,  //34
                Value.Truncate, //35
                Value.Real,     //36
                Value.Imaginary,//37
                Value.Phase,    //38
                Value.Random,   //39
                Value.Fact,     //40
                Value.ComplexNegate //41
            };

            var n = RadFunctions.Length;
            DegFunctions = new Function[n];
            for (int i = 12; i < n; ++i)
            {
                DegFunctions[i] = RadFunctions[i];
            }
            DegFunctions[FunctionIndex["sin"]] = (in Value x) => Value.Sin(x * Deg2Rad);
            DegFunctions[FunctionIndex["cos"]] = (in Value x) => Value.Cos(x * Deg2Rad);
            DegFunctions[FunctionIndex["tan"]] = (in Value x) => Value.Tan(x * Deg2Rad);
            DegFunctions[FunctionIndex["csc"]] = (in Value x) => Value.Csc(x * Deg2Rad);
            DegFunctions[FunctionIndex["sec"]] = (in Value x) => Value.Sec(x * Deg2Rad);
            DegFunctions[FunctionIndex["cot"]] = (in Value x) => Value.Cot(x * Deg2Rad);
            DegFunctions[FunctionIndex["asin"]] = (in Value x) => Value.Asin(x) * Rad2deg;
            DegFunctions[FunctionIndex["acos"]] = (in Value x) => Value.Acos(x) * Rad2deg;
            DegFunctions[FunctionIndex["atan"]] = (in Value x) => Value.Atan(x) * Rad2deg;
            DegFunctions[FunctionIndex["acsc"]] = (in Value x) => Value.Acsc(x) * Rad2deg;
            DegFunctions[FunctionIndex["asec"]] = (in Value x) => Value.Asec(x) * Rad2deg;
            DegFunctions[FunctionIndex["acot"]] = (in Value x) => Value.Acot(x) * Rad2deg;
            DegFunctions[FunctionIndex["phase"]] = (in Value x) => Value.Phase(x) * Rad2deg;
            

            Functions2 = new Operator []
            {
                Value.Atan2,
                Value.UnitRoot,
                Value.MandelbrotSet
            };

            MultiFunctions = new MultiFunction[]
            {
                Value.Min,
                Value.Max,
                Value.Sum,
                Value.SumSq,
                Value.Srss,
                Value.Average,
                Value.Product,
                Value.Mean,
                Value.Switch,
                Value.Take,
                Value.Line,
                Value.Spline
            };
        }

        internal RealCalculator()
        {
            _functions = DegFunctions;
        }

        internal override Value EvaluateOperator(int index, Value a, Value b) => Operators[index](a, b);
        internal override Value EvaluateFunction(int index, Value a) => _functions[index](a);
        internal override Value EvaluateFunction2(int index, Value a, Value b) => Functions2[index](a, b);
        internal override Value EvaluateMultiFunction(int index, Value[] a) => MultiFunctions[index](a);
        internal override Operator GetOperator(int index) => index == PowerIndex ? Value.Pow : Operators[index];

        internal override Function GetFunction(int index)
        {
            if (index == SqrIndex || index == SqrtIndex)
                return Value.Sqrt;

            if (index == CbrtIndex)
                return Value.Cbrt;

            return _functions[index];
        }
            
        internal override Operator GetFunction2(int index) => index == RootIndex ? Value.Root : Functions2[index];
        internal override MultiFunction GetMultiFunction(int index) => MultiFunctions[index];
    }

    internal class ComplexCalculator : Calculator
    {
        private static readonly Operator[] Operators;
        private static readonly Function[] DegFunctions, RadFunctions;
        private static readonly Operator[] Functions2;
        private static readonly MultiFunction[] MultiFunctions;
        private Function[] _functions;

        internal override bool Degrees
        {
            set => _functions = value ? DegFunctions : RadFunctions;
        }

        static ComplexCalculator()
        {
            Operators = new Operator[]
            {
                Value.ComplexPow,
                Value.ComplexDivide,
                Value.ComplexIntDiv,
                Value.ComplexReminder,
                Value.ComplexMultiply,
                Value.ComplexSubtract,
                Value.ComplexAdd,
                Value.ComplexLessThan,
                Value.ComplexGreaterThan,
                Value.ComplexLessThanOrEqual,
                Value.ComplexGreaterThanOrEqual,
                Value.ComplexEqual,
                Value.ComplexNotEqual,
                (_, b) => b
            };

            RadFunctions = new Function[]
            {
                Value.ComplexSin,      // 0
                Value.ComplexCos,      // 1
                Value.ComplexTan,      // 2
                Value.ComplexCsc,      // 3
                Value.ComplexSec,      // 4
                Value.ComplexCot,      // 5
                Value.ComplexAsin,     // 6
                Value.ComplexAcos,     // 7
                Value.ComplexAtan,     // 8
                Value.ComplexAcsc,     // 9
                Value.ComplexAsec,     //10
                Value.ComplexAcot,     //11
                Value.ComplexSinh,     //12
                Value.ComplexCosh,     //13
                Value.ComplexTanh,     //14
                Value.ComplexCsch,     //15
                Value.ComplexSech,     //16
                Value.ComplexCoth,     //17
                Value.ComplexAsinh,    //18
                Value.ComplexAcosh,    //19
                Value.ComplexAtanh,    //20
                Value.ComplexAcsch,    //21
                Value.ComplexAsech,    //22
                Value.ComplexAcoth,    //23
                Value.ComplexLog10,    //24
                Value.ComplexLog,      //25
                Value.ComplexLog2,     //26
                Value.ComplexAbs,      //27
                Value.ComplexSign,     //28
                Value.ComplexSqrt,     //29
                Value.ComplexSqrt,     //30
                Value.ComplexCbrt,     //31
                Value.ComplexRound,    //32
                Value.ComplexFloor,    //33
                Value.ComplexCeiling,  //34
                Value.ComplexTruncate, //35
                Value.Real,            //36
                Value.Imaginary,       //37
                Value.Phase,           //38
                Value.ComplexRandom,   //39
                Value.Fact,            //40
                Value.ComplexNegate    //41
            };

            var n = RadFunctions.Length;
            DegFunctions = new Function[n];
            for (int i = 12; i < n; ++i)
            {
                DegFunctions[i] = RadFunctions[i];
            }
            DegFunctions[FunctionIndex["sin"]]   = (in Value z) => Value.ComplexSin(z * Deg2Rad);
            DegFunctions[FunctionIndex["cos"]]   = (in Value z) => Value.ComplexCos(z * Deg2Rad);
            DegFunctions[FunctionIndex["tan"]]   = (in Value z) => Value.ComplexTan(z * Deg2Rad);
            DegFunctions[FunctionIndex["csc"]]   = (in Value z) => Value.ComplexCsc(z * Deg2Rad);
            DegFunctions[FunctionIndex["sec"]]   = (in Value z) => Value.ComplexSec(z * Deg2Rad);
            DegFunctions[FunctionIndex["cot"]]   = (in Value z) => Value.ComplexCot(z * Deg2Rad);
            DegFunctions[FunctionIndex["asin"]]  = (in Value z) => Value.ComplexAsin(z) * Rad2deg;
            DegFunctions[FunctionIndex["acos"]]  = (in Value z) => Value.ComplexAcos(z) * Rad2deg;
            DegFunctions[FunctionIndex["atan"]]  = (in Value z) => Value.ComplexAtan(z) * Rad2deg;
            DegFunctions[FunctionIndex["acsc"]]  = (in Value z) => Value.ComplexAcsc(z) * Rad2deg;
            DegFunctions[FunctionIndex["asec"]]  = (in Value z) => Value.ComplexAsec(z) * Rad2deg;
            DegFunctions[FunctionIndex["acot"]]  = (in Value z) => Value.ComplexAcot(z) * Rad2deg;
            DegFunctions[FunctionIndex["phase"]] = (in Value z) => Value.Phase(z) * Rad2deg;

            Functions2 = new Operator[]
            {
                Value.ComplexAtan2,
                Value.ComplexRoot,
                Value.MandelbrotSet
            };

            MultiFunctions = new MultiFunction[]
            {
                Value.ComplexMin,
                Value.ComplexMax,
                Value.ComplexSum,
                Value.ComplexSumSq,
                Value.ComplexSrss,
                Value.ComplexAverage,
                Value.ComplexProduct,
                Value.ComplexMean,
                Value.Switch,
                Value.Take,
                Value.Line,
                Value.Spline            
            };
        }

        internal ComplexCalculator()
        {
            _functions = DegFunctions;
        }

        internal override Value EvaluateOperator(int index, Value a, Value b) => Operators[index](a, b);
        internal override Value EvaluateFunction(int index, Value a) =>  _functions[index](a);
        internal override Value EvaluateFunction2(int index, Value a, Value b) => Functions2[index](a, b);
        internal override Value EvaluateMultiFunction(int index, Value[] a) => MultiFunctions[index](a);
        internal override Operator GetOperator(int index) => Operators[index];
        internal override Function GetFunction(int index) => _functions[index];
        internal override Operator GetFunction2(int index) => Functions2[index];
        internal override MultiFunction GetMultiFunction(int index) => MultiFunctions[index];
    }
}
