using System;
using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Core
{
    internal interface IValue
    {
        public static IValue operator -(IValue a)
        {
            return a switch
            {
                RealValue ra => -ra,
                HpVector hp_va => -hp_va,
                HpMatrix hp_ma => -hp_ma,
                Vector va => -va,
                Matrix ma => -ma,
                ComplexValue ca => -ca,
                _ => throw Exceptions.InvalidOperand($"-{a}"),
            };
        }

        public static IValue operator +(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra + rb;
                if (b is HpVector hp_vb) return hp_vb + ra;
                if (b is HpMatrix hp_mb) return hp_mb + ra;
                if (b is Vector vb) return vb + ra;
                if (b is Matrix mb) return mb + ra;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va + hp_rb;
                    if (b is HpVector hp_vb) return hp_va + hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va + hp_mb;
                }
                if (b is RealValue rb) return va + rb;
                if (b is Vector vb) return va + vb;
                if (b is Matrix mb) return va + mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma + hp_rb;
                    if (b is HpVector hp_vb) return hp_ma + hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma + hp_mb;
                }
                if (b is RealValue rb) return ma + rb;
                if (b is Vector vb) return ma + vb;
                if (b is Matrix mb) return ma + mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() + sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} + {b}");
        }

        public static IValue operator -(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra - rb;
                if (b is HpVector hp_vb) return ra - hp_vb;
                if (b is HpMatrix hp_mb) return ra - hp_mb;
                if (b is Vector vb) return ra - vb;
                if (b is Matrix mb) return ra - mb;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va - hp_rb;
                    if (b is HpVector hp_vb) return hp_va - hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va - hp_mb;
                }
                if (b is RealValue rb) return va - rb;
                if (b is Vector vb) return va - vb;
                if (b is Matrix mb) return va - mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma - hp_rb;
                    if (b is HpVector hp_vb) return hp_ma - hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma - hp_mb;
                }
                if (b is RealValue rb) return ma - rb;
                if (b is Vector vb) return ma - vb;
                if (b is Matrix mb) return ma - mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() - sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} - {b}");
        }

        public static IValue operator *(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra * rb;
                if (b is HpVector hp_vb) return hp_vb * ra;
                if (b is HpMatrix hp_mb) return hp_mb * ra;
                if (b is Vector vb) return vb * ra;
                if (b is Matrix mb) return mb * ra;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va * hp_rb;
                    if (b is HpVector hp_vb) return hp_va * hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va * hp_mb;
                }
                if (b is RealValue rb) return va * rb;
                if (b is Vector vb) return va * vb;
                if (b is Matrix mb) return va * mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma * hp_rb;
                    if (b is HpVector hp_vb)
                    {
                        var hp_cm = new HpColumnMatrix(hp_vb);
                        var c = hp_ma * hp_cm;
                        return c.RowCount == 1 ? c[0, 0] : c.ColRef();
                    }
                    if (b is HpMatrix hp_mb) return hp_ma * hp_mb;
                }
                if (b is RealValue rb) return ma * rb;
                if (b is Vector vb)
                {
                    var cm = new ColumnMatrix(vb);
                    var c = ma * cm;
                    return c.RowCount == 1 ? c[0, 0] : c.ColRef();
                }
                if (b is Matrix mb) return ma * mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() * sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a}*{b}");
        }

        public static IValue operator /(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra / rb;
                if (b is HpVector hp_vb) return ra / hp_vb;
                if (b is HpMatrix hp_mb) return ra / hp_mb;
                if (b is Vector vb) return ra / vb;
                if (b is Matrix mb) return ra / mb;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va / hp_rb;
                    if (b is HpVector hp_vb) return hp_va / hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va / hp_mb;
                }
                if (b is RealValue rb) return va / rb;
                if (b is Vector vb) return va / vb;
                if (b is Matrix mb) return va / mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma / hp_rb;
                    if (b is HpVector hp_vb) return hp_ma / hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma / hp_mb;
                }
                if (b is RealValue rb) return ma / rb;
                if (b is Vector vb) return ma / vb;
                if (b is Matrix mb) return ma / mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() / sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a}/{b}");
        }

        public static IValue operator %(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra % rb;
                if (b is HpVector hp_vb) return hp_vb % ra;
                if (b is HpMatrix hp_mb) return hp_mb % ra;
                if (b is Vector vb) return ra % vb;
                if (b is Matrix mb) return ra % mb;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va % hp_rb;
                    if (b is HpVector hp_vb) return hp_va % hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va % hp_mb;
                }
                if (b is RealValue rb) return va % rb;
                if (b is Vector vb) return va % vb;
                if (b is Matrix mb) return va % mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma % hp_rb;
                    if (b is HpVector hp_vb) return hp_ma % hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma % hp_mb;
                }
                if (b is RealValue rb) return ma % rb;
                if (b is Vector vb) return ma % vb;
                if (b is Matrix mb) return ma % mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() % sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a}%{b}");
        }

        public static IValue operator <(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra < rb;
                if (b is HpVector hp_vb) return ra < hp_vb;
                if (b is HpMatrix hp_mb) return ra < hp_mb;
                if (b is Vector vb) return ra < vb;
                if (b is Matrix mb) return ra < mb;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va < hp_rb;
                    if (b is HpVector hp_vb) return hp_va < hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va < hp_mb;
                }
                if (b is RealValue rb) return va < rb;
                if (b is Vector vb) return va < vb;
                if (b is Matrix mb) return va < mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma < hp_rb;
                    if (b is HpVector hp_vb) return hp_ma < hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma < hp_mb;
                }
                if (b is RealValue rb) return ma < rb;
                if (b is Vector vb) return ma < vb;
                if (b is Matrix mb) return ma < mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() < sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} < {b}");
        }

        public static IValue operator >(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra > rb;
                if (b is HpVector hp_vb) return ra > hp_vb;
                if (b is HpMatrix hp_mb) return ra > hp_mb;
                if (b is Vector vb) return ra > vb;
                if (b is Matrix mb) return ra > mb;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va > hp_rb;
                    if (b is HpVector hp_vb) return hp_va > hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va > hp_mb;
                }
                if (b is RealValue rb) return va > rb;
                if (b is Vector vb) return va > vb;
                if (b is Matrix mb) return va > mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma > hp_rb;
                    if (b is HpVector hp_vb) return hp_ma > hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma > hp_mb;
                }
                if (b is RealValue rb) return ma > rb;
                if (b is Vector vb) return ma > vb;
                if (b is Matrix mb) return ma > mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() > sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} > {b}");
        }

        public static IValue operator <=(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra <= rb;
                if (b is HpVector hp_vb) return ra <= hp_vb;
                if (b is HpMatrix hp_mb) return ra <= hp_mb;
                if (b is Vector vb) return ra <= vb;
                if (b is Matrix mb) return ra <= mb;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va <= hp_rb;
                    if (b is HpVector hp_vb) return hp_va <= hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va <= hp_mb;
                }
                if (b is RealValue rb) return va <= rb;
                if (b is Vector vb) return va <= vb;
                if (b is Matrix mb) return va <= mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma <= hp_rb;
                    if (b is HpVector hp_vb) return hp_ma <= hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma <= hp_mb;
                }
                if (b is RealValue rb) return ma <= rb;
                if (b is Vector vb) return ma <= vb;
                if (b is Matrix mb) return ma <= mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() <= sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} ≤ {b}");
        }

        public static IValue operator >=(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra >= rb;
                if (b is HpVector hp_vb) return ra >= hp_vb;
                if (b is HpMatrix hp_mb) return ra >= hp_mb;
                if (b is Vector vb) return ra >= vb;
                if (b is Matrix mb) return ra >= mb;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va >= hp_rb;
                    if (b is HpVector hp_vb) return hp_va >= hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va >= hp_mb;
                }
                if (b is RealValue rb) return va >= rb;
                if (b is Vector vb) return va >= vb;
                if (b is Matrix mb) return va >= mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma >= hp_rb;
                    if (b is HpVector hp_vb) return hp_ma >= hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma >= hp_mb;
                }
                if (b is RealValue rb) return ma >= rb;
                if (b is Vector vb) return ma >= vb;
                if (b is Matrix mb) return ma >= mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() >= sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} ≥ {b}");
        }

        internal static IValue Equal(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra == rb;
                if (b is HpVector hp_vb) return hp_vb == ra;
                if (b is HpMatrix hp_mb) return hp_mb == ra;
                if (b is Vector vb) return vb == ra;
                if (b is Matrix mb) return mb == ra;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va == hp_rb;
                    if (b is HpVector hp_vb) return hp_va == hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va == hp_mb;
                }
                if (b is RealValue rb) return va == rb;
                if (b is Vector vb) return va == vb;
                if (b is Matrix mb) return va == mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma == hp_rb;
                    if (b is HpVector hp_vb) return hp_ma == hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma == hp_mb;
                }
                if (b is RealValue rb) return ma == rb;
                if (b is Vector vb) return ma == vb;
                if (b is Matrix mb) return ma == mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() == sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} ≡ {b}");
        }

        internal static IValue NotEqual(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra != rb;
                if (b is HpVector hp_vb) return hp_vb != ra;
                if (b is HpMatrix hp_mb) return hp_mb != ra;
                if (b is Vector vb) return vb != ra;
                if (b is Matrix mb) return mb != ra;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va != hp_rb;
                    if (b is HpVector hp_vb) return hp_va != hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va != hp_mb;
                }
                if (b is RealValue rb) return va != rb;
                if (b is Vector vb) return va != vb;
                if (b is Matrix mb) return va != mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma != hp_rb;
                    if (b is HpVector hp_vb) return hp_ma != hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma != hp_mb;
                }
                if (b is RealValue rb) return ma != rb;
                if (b is Vector vb) return ma != vb;
                if (b is Matrix mb) return ma != mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() != sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a} ≠ {b}");
        }

        public static IValue operator &(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra & rb;
                if (b is HpVector hp_vb) return hp_vb & ra;
                if (b is HpMatrix hp_mb) return hp_mb & ra;
                if (b is Vector vb) return vb & ra;
                if (b is Matrix mb) return mb & ra;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va & hp_rb;
                    if (b is HpVector hp_vb) return hp_va & hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va & hp_mb;
                }
                if (b is RealValue rb) return va & rb;
                if (b is Vector vb) return va & vb;
                if (b is Matrix mb) return va & mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma & hp_rb;
                    if (b is HpVector hp_vb) return hp_ma & hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma & hp_mb;
                }
                if (b is RealValue rb) return ma & rb;
                if (b is Vector vb) return ma & vb;
                if (b is Matrix mb) return ma & mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() & sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a}∧{b}.");
        }

        public static IValue operator |(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra | rb;
                if (b is HpVector hp_vb) return hp_vb | ra;
                if (b is HpMatrix hp_mb) return hp_mb | ra;
                if (b is Vector vb) return vb | ra;
                if (b is Matrix mb) return mb | ra;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va | hp_rb;
                    if (b is HpVector hp_vb) return hp_va | hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va | hp_mb;
                }
                if (b is RealValue rb) return va | rb;
                if (b is Vector vb) return va | vb;
                if (b is Matrix mb) return va | mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma | hp_rb;
                    if (b is HpVector hp_vb) return hp_ma | hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma | hp_mb;
                }
                if (b is RealValue rb) return ma | rb;
                if (b is Vector vb) return ma | vb;
                if (b is Matrix mb) return ma | mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() | sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a}∨{b}");
        }

        public static IValue operator ^(IValue a, IValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra ^ rb;
                if (b is HpVector hp_vb) return hp_vb ^ ra;
                if (b is HpMatrix hp_mb) return hp_mb ^ ra;
                if (b is Vector vb) return vb ^ ra;
                if (b is Matrix mb) return mb ^ ra;
            }
            else if (a is Vector va)
            {
                if (va is HpVector hp_va)
                {
                    if (b is RealValue hp_rb) return hp_va ^ hp_rb;
                    if (b is HpVector hp_vb) return hp_va ^ hp_vb;
                    if (b is HpMatrix hp_mb) return hp_va ^ hp_mb;
                }
                if (b is RealValue rb) return va ^ rb;
                if (b is Vector vb) return va ^ vb;
                if (b is Matrix mb) return va ^ mb;
            }
            else if (a is Matrix ma)
            {
                if (ma is HpMatrix hp_ma)
                {
                    if (b is RealValue hp_rb) return hp_ma ^ hp_rb;
                    if (b is HpVector hp_vb) return hp_ma ^ hp_vb;
                    if (b is HpMatrix hp_mb) return hp_ma ^ hp_mb;
                }
                if (b is RealValue rb) return ma ^ rb;
                if (b is Vector vb) return ma ^ vb;
                if (b is Matrix mb) return ma ^ mb;
            }
            if (a is IScalarValue sa && b is IScalarValue sb)
                return sa.AsComplex() ^ sb.AsComplex();

            throw Exceptions.InvalidOperand($"{a}⊕{b}");
        }

        internal static IValue Phasor(IValue a, IValue b)
        {
            if (a is IScalarValue sa && b is IScalarValue sb)
                return ComplexCalculator.Phasor(sa.AsComplex(), sb.AsComplex());

            throw Exceptions.InvalidOperand($"{a} ≠ {b}");
        }

        internal static IValue EvaluateFunction(MatrixCalculator calc, long index, in IValue a)
        {
            if (a is IScalarValue scalar)
                return calc.Calculator.EvaluateFunction(index, scalar);
            if (a is Vector vector)
                return calc.VectorCalculator.EvaluateFunction(index, vector);
            if (a is Matrix matrlix)
                return calc.EvaluateFunction(index, matrlix);

            throw Exceptions.InvalidArgument($"{a}");
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
                    {
                        if (c.RowCount == 1)
                            return c[0, 0];
                        if (c is HpColumnMatrix hp_cm)
                            return hp_cm.ColRef();
                        if (c is ColumnMatrix cm)
                            return cm.ColRef();

                        return c.Col(1);
                    }
                    return c;
                }
                if (b is Matrix mb)
                    return calc.EvaluateOperator(index, ma, mb);
            }
            throw Exceptions.InvalidOperand($"{a}; {b}");
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
            throw Exceptions.InvalidArgument($"{a}; {b}");
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
                throw Exceptions.CannotInterpolateWithNonScalarValue();

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
                    valList.AddRange(vector.Values.Cast<IScalarValue>());
                else if (ival is Matrix matrix)
                    valList.AddRange(matrix.Values.Cast<IScalarValue>());
                else
                    throw Exceptions.InvalidArgument($"{ival}");
            }
            return valList.ToArray();
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
                    throw Exceptions.InvalidArgument($"{ival}");
            }
            return valList.ToArray();
        }

        internal static IScalarValue AsValue(IValue ival, Exceptions.Items item = Exceptions.Items.Argument)
        {
            if (ival is IScalarValue scalar)
                return scalar;

            throw Exceptions.MustBeScalar(item);
        }

        internal static RealValue AsReal(IValue ival, Exceptions.Items item = Exceptions.Items.Argument)
        {
            if (ival is RealValue real)
                return real;

            if (ival is ComplexValue complex)
                return (RealValue)complex;

            throw Exceptions.MustBeScalar(item);
        }

        internal static int AsInt(IValue ivalue, Exceptions.Items item = Exceptions.Items.Argument)
        {
            var value = AsValue(ivalue, item);
            if (value.IsReal && value.Units is null)
            {
                var d = value.Re;
                if (d > 0 && d <= int.MaxValue && d.AlmostEquals(Math.Truncate(d)))
                    return (int)d;
            }
            throw Exceptions.MustBePositiveInteger(item);
        }

        internal static Vector AsVector(IValue ivalue, Exceptions.Items item = Exceptions.Items.Argument)
        {
            if (ivalue is Vector vec)
                return vec;

            throw Exceptions.MustBeVector(item);
        }

        internal static Matrix AsMatrix(IValue ivalue, Exceptions.Items item = Exceptions.Items.Argument)
        {
            if (ivalue is Matrix m)
                return m;

            if (ivalue is Vector vec)
            {
                if (vec is HpVector hp_vec)
                    return new HpColumnMatrix(hp_vec);

                return new ColumnMatrix(vec);
            }

            throw Exceptions.MustBeMatrix(item);
        }

        internal static HpVector AsVectorHp(IValue ivalue, Exceptions.Items item = Exceptions.Items.Argument)
        {
            if (ivalue is HpVector vec)
                return vec;

            throw Exceptions.MustBeHpVector(item);
        }

        internal static HpMatrix AsMatrixHp(IValue ivalue, Exceptions.Items item = Exceptions.Items.Argument)
        {
            if (ivalue is HpMatrix m)
                return m;

            if (ivalue is HpVector vec)
                return new HpColumnMatrix(vec);

            throw Exceptions.MustBeHpMatrix(item);
        }
    }
}