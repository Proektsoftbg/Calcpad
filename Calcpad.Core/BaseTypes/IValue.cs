using System;
using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Core
{
    internal interface IValue
    {
        public static IValue operator -(IValue a)
        {
            switch (a)
            {
                case RealValue ra: 
                    return -ra;
                case Vector va: 
                    return -va;
                case Matrix ma: 
                    return -ma;
                case ComplexValue ca: 
                    return -ca;
                default: 
                    Throw.InvalidOperand($"-{a}");
                    return null;
            }
        }

        public static IValue operator +(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra + rb;
                if (b is Vector vb) return vb + ra;
                if (b is Matrix mb) return mb + ra;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va + rb;
                if (b is Vector vb) return va + vb;
                if (b is Matrix mb) return va + mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma + rb;
                if (b is Vector vb) return ma + vb;
                if (b is Matrix mb) return ma + mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() + sb.AsComplex();

            Throw.InvalidOperand($"{a} + {b}");
            return null;
        }

        public static IValue operator -(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra - rb;
                if (b is Vector vb) return ra - vb;
                if (b is Matrix mb) return ra - mb;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va - rb;
                if (b is Vector vb) return va - vb;
                if (b is Matrix mb) return va - mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma - rb;
                if (b is Vector vb) return ma - vb;
                if (b is Matrix mb) return ma - mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() - sb.AsComplex();

            Throw.InvalidOperand($"{a} - {b}");
            return null;
        }

        public static IValue operator *(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra * rb;
                if (b is Vector vb) return vb * ra;
                if (b is Matrix mb) return mb * ra;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va * rb;
                if (b is Vector vb) return va * vb;
                if (b is Matrix mb) return va * mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma * rb;
                if (b is Vector vb)
                {
                    var c = ma * vb;
                    return c.RowCount == 1 ? c[0, 0] : c.Col(1);
                }
                if (b is Matrix mb) return ma * mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() * sb.AsComplex();

            Throw.InvalidOperand($"{a}*{b}");
            return null;
        }

        public static IValue operator /(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra / rb;
                if (b is Vector vb) return ra / vb;
                if (b is Matrix mb) return ra / mb;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va / rb;
                if (b is Vector vb) return va / vb;
                if (b is Matrix mb) return va / mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma / rb;
                if (b is Vector vb) return ma / vb;
                if (b is Matrix mb) return ma / mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() / sb.AsComplex();

            Throw.InvalidOperand($"{a}/{b}");
            return null;
        }

        public static IValue operator %(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra % rb;
                if (b is Vector vb) return ra % vb;
                if (b is Matrix mb) return ra % mb;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va % rb;
                if (b is Vector vb) return va % vb;
                if (b is Matrix mb) return va % mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma % rb;
                if (b is Vector vb) return ma % vb;
                if (b is Matrix mb) return ma % mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() % sb.AsComplex();

            Throw.InvalidOperand($"{a}%{b}");
            return null;
        }

        public static IValue operator <(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra < rb;
                if (b is Vector vb) return ra < vb;
                if (b is Matrix mb) return ra < mb;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va < rb;
                if (b is Vector vb) return va < vb;
                if (b is Matrix mb) return va < mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma < rb;
                if (b is Vector vb) return ma < vb;
                if (b is Matrix mb) return ma < mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() < sb.AsComplex();

            Throw.InvalidOperand($"{a} < {b}");
            return null;
        }

        public static IValue operator >(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra > rb;
                if (b is Vector vb) return ra > vb;
                if (b is Matrix mb) return ra > mb;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va > rb;
                if (b is Vector vb) return va > vb;
                if (b is Matrix mb) return va > mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma > rb;
                if (b is Vector vb) return ma > vb;
                if (b is Matrix mb) return ma > mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() > sb.AsComplex();

            Throw.InvalidOperand($"{a} > {b}");
            return null;
        }

        public static IValue operator <=(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra <= rb;
                if (b is Vector vb) return ra <= vb;
                if (b is Matrix mb) return ra <= mb;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va <= rb;
                if (b is Vector vb) return va <= vb;
                if (b is Matrix mb) return va <= mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma <= rb;
                if (b is Vector vb) return ma <= vb;
                if (b is Matrix mb) return ma <= mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() <= sb.AsComplex();

            Throw.InvalidOperand($"{a} ≤ {b}");
            return null;
        }

        public static IValue operator >=(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra >= rb;
                if (b is Vector vb) return ra >= vb;
                if (b is Matrix mb) return ra >= mb;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va >= rb;
                if (b is Vector vb) return va >= vb;
                if (b is Matrix mb) return va >= mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma >= rb;
                if (b is Vector vb) return ma >= vb;
                if (b is Matrix mb) return ma >= mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() >= sb.AsComplex();

            Throw.InvalidOperand($"{a} ≥ {b}");
            return null;
        }

        internal static IValue Equal(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra == rb;
                if (b is Vector vb) return vb == ra;
                if (b is Matrix mb) return mb == ra;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va == rb;
                if (b is Vector vb) return va == vb;
                if (b is Matrix mb) return va == mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma == rb;
                if (b is Vector vb) return ma == vb;
                if (b is Matrix mb) return ma == mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() == sb.AsComplex();

            Throw.InvalidOperand($"{a} ≡ {b}");
            return null;
        }

        internal static IValue NotEqual(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra != rb;
                if (b is Vector vb) return vb != ra;
                if (b is Matrix mb) return mb != ra;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va != rb;
                if (b is Vector vb) return va != vb;
                if (b is Matrix mb) return va != mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma != rb;
                if (b is Vector vb) return ma != vb;
                if (b is Matrix mb) return ma != mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() != sb.AsComplex();

            Throw.InvalidOperand($"{a} ≠ {b}");
            return null;
        }

        public static IValue operator &(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra & rb;
                if (b is Vector vb) return vb & ra;
                if (b is Matrix mb) return mb & ra;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va & rb;
                if (b is Vector vb) return va & vb;
                if (b is Matrix mb) return va & mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma & rb;
                if (b is Vector vb) return ma & vb;
                if (b is Matrix mb) return ma & mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() & sb.AsComplex();

            Throw.InvalidOperand($"{a}∧{b}.");
            return null;
        }

        public static IValue operator |(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra | rb;
                if (b is Vector vb) return vb | ra;
                if (b is Matrix mb) return mb | ra;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va | rb;
                if (b is Vector vb) return va | vb;
                if (b is Matrix mb) return va | mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma | rb;
                if (b is Vector vb) return ma | vb;
                if (b is Matrix mb) return ma | mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() | sb.AsComplex();

            Throw.InvalidOperand($"{a}∨{b}");
            return null;
        }

        public static IValue operator ^(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra ^ rb;
                if (b is Vector vb) return vb ^ ra;
                if (b is Matrix mb) return mb ^ ra;
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb) return va ^ rb;
                if (b is Vector vb) return va ^ vb;
                if (b is Matrix mb) return va ^ mb;
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb) return ma ^ rb;
                if (b is Vector vb) return ma ^ vb;
                if (b is Matrix mb) return ma ^ mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() ^ sb.AsComplex();

            Throw.InvalidOperand($"{a}⊕{b}");
            return null;
        }

        internal static IValue EvaluateFunction(MatrixCalculator calc, long index, in IValue a)
        {
            if (a is IScalarValue scalar)
                return calc.Calculator.EvaluateFunction(index, scalar);
            if (a is Vector vector)
                return calc.VectorCalculator.EvaluateFunction(index, vector);
            if (a is Matrix matrlix)
                return calc.EvaluateFunction(index, matrlix);

            Throw.InvalidArgument($"{a}");
            return null;
        }

        internal static IValue EvaluateOperator(MatrixCalculator calc, long index, in IValue a, in IValue b)
        {
            if (a is IScalarValue sa)
            {
                if (b is IScalarValue sb)
                    return calc.Calculator.EvaluateOperator(index, sa, sb);
                if (sa is RealValue ra)
                {
                    if (b is Vector vb)
                        return calc.VectorCalculator.EvaluateOperator(index, ra, vb);
                    if (b is Matrix mb)
                        return calc.EvaluateOperator(index, ra, mb);
                }
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb)
                    return calc.VectorCalculator.EvaluateOperator(index, va, rb);
                if (b is Vector vb)
                    return calc.VectorCalculator.EvaluateOperator(index, va, vb);
                if (b is Matrix mb)
                    return calc.EvaluateOperator(index, va, mb);

            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb)
                    return calc.EvaluateOperator(index, ma, rb);
                if (b is Vector vb)
                {
                    var c = calc.EvaluateOperator(index, ma, vb);
                    if (index == Calculator.MultiplyIndex)
                        return c.RowCount == 1 ? c[0, 0] : c.Col(1);

                    return c;
                }
                if (b is Matrix mb)
                    return calc.EvaluateOperator(index, ma, mb);
            }
            Throw.InvalidOperand($"{a}; {b}");
            return null;
        }

        internal static IValue EvaluateFunction2(MatrixCalculator calc, long index, in IValue a, in IValue b)
        {
            if (a is IScalarValue sa)
            {
                if (b is IScalarValue sb)
                    return calc.Calculator.EvaluateFunction2(index, sa, sb);
                if (sa is RealValue ra)
                {
                    if (b is Vector vb)
                        return calc.VectorCalculator.EvaluateFunction2(index, ra, vb);
                    if (b is Matrix mb)
                        return calc.EvaluateFunction2(index, ra, mb);
                }
            }
            else if (a is Vector va)
            {
                if (b is RealValue rb)
                    return calc.VectorCalculator.EvaluateFunction2(index, va, rb);
                if (b is Vector vb)
                    return calc.VectorCalculator.EvaluateFunction2(index, va, vb);
                if (b is Matrix mb)
                    return calc.EvaluateFunction2(index, va, mb);
            }
            else if (a is Matrix ma)
            {
                if (b is RealValue rb)
                    return calc.EvaluateFunction2(index, ma, rb);
                if (b is Vector vb)
                    return calc.EvaluateFunction2(index, ma, vb);
                if (b is Matrix mb)
                    return calc.EvaluateFunction2(index, ma, mb);
            }
            Throw.InvalidArgument($"{a}; {b}");
            return null;
        }

        internal static IValue EvaluateInterpolation(MatrixCalculator calc, long index, IValue[] values)
        {
            if (values[0] is RealValue x)
            {
                if (values.Length == 2 && values[1] is Vector vector)
                    return calc.VectorCalculator.EvaluateInterpolation(index, x, vector);
                if (values.Length == 3 && values[1] is RealValue y && values[2] is Matrix matrix)
                    return calc.EvaluateInterpolation(index, y, x, matrix);
            }
            else
                Throw.CannotInterpolateWithNonScalarValueException();

            return calc.Calculator.EvaluateInterpolation(index, ExpandValues(values));
        }

        internal static IValue EvaluateMultiFunction(MatrixCalculator calc, long index, IValue[] values)
        {
            if (values.Length == 1)
            {
                if (values[0] is Vector vector)
                    return calc.VectorCalculator.EvaluateMultiFunction(index, vector);
                if (values[0] is Matrix mirx)
                    return calc.EvaluateMultiFunction(index, mirx);
            }
            return calc.Calculator.EvaluateMultiFunction(index, ExpandValues(values));
        }

        internal static IScalarValue[] ExpandValues(IValue[] values)
        {
            var len = values.Length;
            var valList = new List<IScalarValue>(len);
            for (var i = 0; i < len; ++i)
            {
                var ival = values[i];
                if (ival is IScalarValue scalar)
                    valList.Add(scalar);
                else if (ival is Vector vector)
                    valList.AddRange(vector.Values.Cast<IScalarValue>().ToArray());
                else if (ival is Matrix matrix)
                    valList.AddRange(matrix.Values.Cast<IScalarValue>().ToArray());
                else
                    Throw.InvalidArgument($"{ival}");
            }
            return [.. valList];
        }


        internal static RealValue[] ExpandRealValues(IValue[] values)
        {
            var len = values.Length;
            var valList = new List<RealValue>(len);
            for (var i = 0; i < len; ++i)
            {
                var ival = values[i];
                if (ival is RealValue real)
                    valList.Add(real);
                else if (ival is Vector vector)
                    valList.AddRange(vector.Values);
                else if (ival is Matrix matrix)
                    valList.AddRange(matrix.Values);
                else
                    Throw.InvalidArgument($"{ival}");
            }
            return [.. valList];
        }

        internal static IScalarValue AsValue(IValue ival, Throw.Items item = Throw.Items.Argument)
        {
            if (ival is IScalarValue scalar)
                return scalar;
            else
                Throw.MustBeScalarException(item);

            return RealValue.NaN;
        }

        internal static RealValue AsReal(IValue ival, Throw.Items item = Throw.Items.Argument)
        {
            if (ival is RealValue real)
                return real;
            else if (ival is ComplexValue complex)
            {
                return (RealValue)complex;
            }
            else
                Throw.MustBeScalarException(item);

            return RealValue.NaN;
        }

        internal static int AsInt(IValue ivalue, Throw.Items item = Throw.Items.Argument)
        {
            var value = AsValue(ivalue, item);
            if (value.IsReal && value.Units is null)
            {
                var d = value.Re;
                if (d > 0 && d <= int.MaxValue && d.AlmostEquals(Math.Truncate(d)))
                    return (int)d;
            }
            Throw.MustBePositiveIntegerException(item);
            return 1;
        }

        internal static Vector AsVector(IValue ivalue, Throw.Items item = Throw.Items.Argument)
        {
            if (ivalue is Vector vec)
                return vec;
            else
                Throw.MustBeVectorException(item);

            return null;
        }

        internal static Matrix AsMatrix(IValue ivalue, Throw.Items item = Throw.Items.Argument)
        {
            if (ivalue is Matrix m)
                return m;
            else if (ivalue is Vector vec)
                return new ColumnMatrix(vec);
            else
                Throw.MustBeMatrixException(item);

            return null;
        }
    }
}