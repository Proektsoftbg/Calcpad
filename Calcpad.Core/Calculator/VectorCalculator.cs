using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Calcpad.Core
{
    internal class VectorCalculator
    {
        internal delegate IValue VectorFunction(in IValue a);
        internal delegate IValue VectorFunction2(in IValue a, in IValue b);
        internal delegate IValue VectorFunction3(in IValue a, in IValue b, in IValue c);

        private readonly Calculator _calc;
        private readonly VectorFunction[] VectorFunctions;
        private readonly VectorFunction2[] VectorFunctions2;
        private readonly VectorFunction3[] VectorFunctions3;
        private readonly Func<IValue[], IValue>[] VectorMultiFunctions;
        private readonly Func<Vector, RealValue>[] MultiFunctions;
        private readonly Func<RealValue, Vector, RealValue>[] Interpolations;

        internal static readonly FrozenDictionary<string, int> FunctionIndex =
        new Dictionary<string, int>()
        {
            { "vector", 0 },
            { "len", 1 },
            { "size", 2 },
            { "sort", 3 },
            { "rsort", 4 },
            { "order", 5 },
            { "revorder", 6 },
            { "reverse", 7 },
            { "norm", 8 },
            { "norm_2", 8 },
            { "norm_e", 8 },
            { "norm_1", 9 },
            { "norm_i", 10 },
            { "unit", 11 },
            { "vector_hp", 12 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function2Index =
        new Dictionary<string, int>()
        {
            { "resize", 0 },
            { "fill", 1 },
            { "first", 2 },
            { "last", 3 },
            { "extract", 4 },
            { "dot", 5 },
            { "cross", 6 },
            { "norm_p", 7 }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> Function3Index =
        new Dictionary<string, int>()
        {
            { "slice", 0 },
            { "range", 1 },
            { "range_hp", 2 },
            { "search", 3 },
            { "count", 4 },
            { "find", 5 },
            { "find_eq", 5 },
            { "find_ne", 6 },
            { "find_lt", 7 },
            { "find_le", 8 },
            { "find_gt", 9 },
            { "find_ge", 10 },
            { "lookup", 11 },
            { "lookup_eq", 11 },
            { "lookup_ne", 12 },
            { "lookup_lt", 13 },
            { "lookup_le", 14 },
            { "lookup_gt", 15 },
            { "lookup_ge", 16 }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenDictionary<string, int> MultiFunctionIndex =
        new Dictionary<string, int>()
        {
            { "join", 0 },
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

        internal static readonly FrozenSet<string> ValueResultFunctions =
        new HashSet<string>()
        {
            "count",
            "dot",
            "len",
            "last",
            "norm",
            "norm1",
            "normp",
            "normi",
            "search"

        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        internal static bool IsVectorResultFunction(string name) =>
            !ValueResultFunctions.Contains(name);

        internal VectorCalculator(Calculator calc)
        {
            _calc = calc;
            VectorFunctions = [
                Create,
                Length,
                Size,
                Sort,
                Rsort,
                Order,
                RevOrder,
                Reverse,
                Norm,
                L1Norm,
                InfNorm,
                Unit,
                CreateHp
            ];

            VectorFunctions2 = [
                Resize,
                Fill,
                First,
                Last,
                Extract,
                Dot,
                Cross,
                LpNorm
            ];

            VectorFunctions3 = [
                Slice,
                Range,
                RangeHp,
                Search,
                Count,
                Find_EQ,
                Find_NE,
                Find_LT,
                Find_LE,
                Find_GT,
                Find_GE,
                Lookup_EQ,
                Lookup_NE,
                Lookup_LT,
                Lookup_LE,
                Lookup_GT,
                Lookup_GE
           ];

            VectorMultiFunctions = [
                Join,
            ];

            MultiFunctions = [
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

            Interpolations = [
                Take,
                Line,
                Spline,
            ];
        }

        internal Calculator Calculator => _calc;
        internal static bool IsFunction(string name) => FunctionIndex.ContainsKey(name);
        internal static bool IsFunction2(string name) => Function2Index.ContainsKey(name);
        internal static bool IsFunction3(string name) => Function3Index.ContainsKey(name);
        internal static bool IsMultiFunction(string name) => MultiFunctionIndex.ContainsKey(name);

        internal IValue EvaluateVectorFunction(long index, in IValue a) =>
            VectorFunctions[index](a);

        internal IValue EvaluateVectorFunction2(long index, in IValue a, in IValue b) =>
            VectorFunctions2[index](a, b);

        internal IValue EvaluateVectorFunction3(long index, in IValue a, in IValue b, in IValue c) =>
            VectorFunctions3[index](a, b, c);

        internal IValue EvaluateVectorMultiFunction(long index, IValue[] a) =>
            VectorMultiFunctions[index](a);

        internal VectorFunction GetFunction(long index) =>
            VectorFunctions[index];

        internal VectorFunction2 GetFunction2(long index) =>
            VectorFunctions2[index];

        internal VectorFunction3 GetFunction3(long index) =>
            VectorFunctions3[index];

        internal Func<IValue[], IValue> GetMultiFunction(long index) =>
            VectorMultiFunctions[index];

        internal Vector EvaluateOperator(long index, Vector a, in RealValue b) =>
            a is HpVector hp_a ? 
            HpVector.EvaluateOperator(_calc.GetOperator(index), hp_a, b, index) :
            Vector.EvaluateOperator(_calc.GetOperator(index), a, b, index);

        internal Vector EvaluateOperator(long index, in RealValue a, Vector b) =>
            b is HpVector hp_b ?
            HpVector.EvaluateOperator(_calc.GetOperator(index), a, hp_b, index) :
            Vector.EvaluateOperator(_calc.GetOperator(index), a, b, index);

        internal Vector EvaluateOperator(long index, Vector a, Vector b) =>
            a is HpVector hp_a && b is HpVector hp_b && 
            (index != Calculator.PowerIndex || hp_a.Units == null) ?
            HpVector.EvaluateOperator(_calc.GetOperator(index), hp_a, hp_b, index) :
            Vector.EvaluateOperator(_calc.GetOperator(index), a, b, index);

        internal Vector EvaluateFunction(long Index, Vector a) =>
            a is HpVector hp_a ?
            HpVector.EvaluateFunction(_calc.GetFunction(Index), hp_a) :
            Vector.EvaluateFunction(_calc.GetFunction(Index), a);

        internal Vector EvaluateFunction2(long index, Vector a, in RealValue b) =>
            a is HpVector hp_a ?
            HpVector.EvaluateOperator(_calc.GetFunction2(index), hp_a, b, -index - 1) :
            Vector.EvaluateOperator(_calc.GetFunction2(index), a, b, -index - 1);

        internal Vector EvaluateFunction2(long index, in RealValue a, Vector b) =>
            b is HpVector hp_b ?
            HpVector.EvaluateOperator(_calc.GetFunction2(index), a, hp_b, -index - 1) :
            Vector.EvaluateOperator(_calc.GetFunction2(index), a, b, -index - 1);

        internal Vector EvaluateFunction2(long index, Vector a, Vector b) =>
            a is HpVector hp_a && b is HpVector hp_b ?
            HpVector.EvaluateOperator(_calc.GetFunction2(index), hp_a, hp_b, -index - 1) :
            Vector.EvaluateOperator(_calc.GetFunction2(index), a, b, -index - 1);

        internal IValue EvaluateMultiFunction(long index, Vector a) => MultiFunctions[index](a);

        internal IValue EvaluateInterpolation(long index, RealValue a, Vector b) => Interpolations[index](a, b);

        private static Vector Create(in IValue length)
        {
            var n = IValue.AsInt(length);
            if (n > 100)
                return new LargeVector(n);

            return new(n);
        }

        private static HpVector CreateHp(in IValue length) => 
            new(IValue.AsInt(length), null);

        private static IValue Length(in IValue vector) =>
            new RealValue(IValue.AsVector(vector).Length);

        private static IValue Size(in IValue vector) =>
            new RealValue(IValue.AsVector(vector).Size);

        private static Vector Sort(in IValue vector) =>
            IValue.AsVector(vector).Sort();

        private static Vector Rsort(in IValue vector) =>
            IValue.AsVector(vector).Sort(true);

        private static Vector Order(in IValue vector) =>
            IValue.AsVector(vector).Order();

        private static Vector RevOrder(in IValue vector) =>
            IValue.AsVector(vector).Order(true);

        private static Vector Reverse(in IValue vector) =>
            IValue.AsVector(vector).Reverse();

        private static IValue Norm(in IValue vector) =>
            IValue.AsVector(vector).Norm();

        private static IValue L1Norm(in IValue vector) =>
            IValue.AsVector(vector).L1Norm();

        private static IValue InfNorm(in IValue vector) =>
            IValue.AsVector(vector).InfNorm();

        private static Vector Unit(in IValue vector) =>
            IValue.AsVector(vector).Normalize();

        private static IValue LpNorm(in IValue vector, in IValue p) =>
            IValue.AsVector(vector).LpNorm(IValue.AsInt(p));

        private static Vector Resize(in IValue vector, in IValue length)
        {
            var vec = IValue.AsVector(vector);
            vec.Resize(IValue.AsInt(length));
            vec.Change();
            return vec;
        }

        private static Vector Fill(in IValue vector, in IValue value)
        {

            var vec = IValue.AsVector(vector);
            vec.Fill(IValue.AsReal(value));
            vec.Change();
            return vec;
        }

        private static Vector First(in IValue vector, in IValue length) =>
            IValue.AsVector(vector).First(IValue.AsInt(length));

        private static Vector Last(in IValue vector, in IValue length) =>
            IValue.AsVector(vector).Last(IValue.AsInt(length));

        private static Vector Extract(in IValue vector, in IValue indexes) =>
            IValue.AsVector(vector).Extract(IValue.AsVector(indexes));

        private static IValue Dot(in IValue a, in IValue b)
        {
            var va = IValue.AsVector(a);
            var vb = IValue.AsVector(b);
            if (va is HpVector hp_a && vb is HpVector hp_b)
                return HpVector.DotProduct(hp_a, hp_b);

            return Vector.DotProduct(va, vb);
        }

        private static Vector Cross(in IValue a, in IValue b)
        {
            var va = IValue.AsVector(a);
            var vb = IValue.AsVector(b);
            if (va is HpVector hp_a && vb is HpVector hp_b)
                return HpVector.CrossProduct(hp_a, hp_b);

            return Vector.CrossProduct(va, vb);
        }


        private static Vector Slice(in IValue vector, in IValue n1, in IValue n2) =>
            IValue.AsVector(vector).Slice(IValue.AsInt(n1), IValue.AsInt(n2));

        private static IValue Search(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).Search(IValue.AsReal(value), IValue.AsInt(start));

        private static Vector Find_EQ(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).FindAll(IValue.AsReal(value), IValue.AsInt(start), Vector.Relation.Equal);

        private static Vector Find_NE(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).FindAll(IValue.AsReal(value), IValue.AsInt(start), Vector.Relation.NotEqual);

        private static Vector Find_LT(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).FindAll(IValue.AsReal(value), IValue.AsInt(start), Vector.Relation.LessThan);

        private static Vector Find_LE(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).FindAll(IValue.AsReal(value), IValue.AsInt(start), Vector.Relation.LessOrEqual);

        private static Vector Find_GT(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).FindAll(IValue.AsReal(value), IValue.AsInt(start), Vector.Relation.GreaterThan);

        private static Vector Find_GE(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).FindAll(IValue.AsReal(value), IValue.AsInt(start), Vector.Relation.GreaterOrEqual);

        private static IValue Count(in IValue vector, in IValue value, in IValue start) =>
            IValue.AsVector(vector).Count(IValue.AsReal(value), IValue.AsInt(start));

        private static Vector Range(in IValue start, in IValue end, in IValue step) =>
            Vector.Range(IValue.AsReal(start), IValue.AsReal(end), IValue.AsReal(step));

        private static Vector RangeHp(in IValue start, in IValue end, in IValue step) =>
            HpVector.Range(IValue.AsReal(start), IValue.AsReal(end), IValue.AsReal(step));

        private static Vector Lookup_EQ(in IValue x, in IValue y, in IValue value) =>
            Lookup(x, y, value, Vector.Relation.Equal);

        private static Vector Lookup_NE(in IValue x, in IValue y, in IValue value) =>
            Lookup(x, y, value, Vector.Relation.NotEqual);

        private static Vector Lookup_LT(in IValue x, in IValue y, in IValue value) =>
            Lookup(x, y, value, Vector.Relation.LessThan);

        private static Vector Lookup_LE(in IValue x, in IValue y, in IValue value) =>
            Lookup(x, y, value, Vector.Relation.LessOrEqual);

        private static Vector Lookup_GT(in IValue x, in IValue y, in IValue value) =>
            Lookup(x, y, value, Vector.Relation.GreaterThan);

        private static Vector Lookup_GE(in IValue x, in IValue y, in IValue value) =>
            Lookup(x, y, value, Vector.Relation.GreaterOrEqual);

        private static Vector Lookup(in IValue x, in IValue y, in IValue value, Vector.Relation rel)
        {
            var vecX = IValue.AsVector(x);
            var vecY = IValue.AsVector(y);
            var val = IValue.AsReal(value);
            if (vecX is HpVector hpVec)
                return hpVec.Lookup(vecY, val, rel);

            return vecX.Lookup(vecY, val, rel);
        }

        private static Vector Join(IValue[] items) => Vector.Join(items);
        private static RealValue Min(Vector v) => v.Min();
        private static RealValue Max(Vector v) => v.Max();
        private static RealValue Sum(Vector v) => v.Sum();
        private static RealValue SumSq(Vector v) => v.SumSq();
        private static RealValue Srss(Vector v) => v.Srss();
        private static RealValue Average(Vector v) => v.Average();
        private static RealValue Product(Vector v) => v.Product();
        private static RealValue Mean(Vector v) => v.Mean();
        private static RealValue Switch(Vector v) => v[0];
        private static RealValue And(Vector v) => v.And();
        private static RealValue Or(Vector v) => v.Or();
        private static RealValue Xor(Vector v) => v.Xor();
        private static RealValue Gcd(Vector v) => v.Gcd();
        private static RealValue Lcm(Vector v) => v.Lcm();
        private static RealValue Take(RealValue x, Vector v) => v.Take(x);
        private static RealValue Line(RealValue x, Vector v) => v.Line(x);
        private static RealValue Spline(RealValue x, Vector v) => v.Spline(x);

        internal static IValue GetElement(IValue vector, IValue ii)
        {
            var vec = IValue.AsVector(vector, Exceptions.Items.IndexTarget);
            var val = IValue.AsValue(ii, Exceptions.Items.Index);
            var i = (int)val.Re;
            if (i < 1 || i > vec.Length)
                throw Exceptions.IndexOutOfRange(i.ToString());

            return vec[i - 1];
        }

        internal static IValue SetElement(IValue vector, IValue ii, IValue value)
        {
            var vec = IValue.AsVector(vector, Exceptions.Items.IndexTarget);
            var val = IValue.AsValue(ii, Exceptions.Items.Index);
            var i = (int)val.Re;
            if (i < 1 || i > vec.Length)
                throw Exceptions.IndexOutOfRange(i.ToString());

            var real = IValue.AsReal(value, Exceptions.Items.Value);
            vec[i - 1] = real;
            vec.Change();
            return value;
        }
    }
}