using System.Collections.Generic;
using System;

namespace Calcpad.Core
{
    internal interface IScalarValue : IValue
    {
        internal abstract double Re { get;}
        internal abstract double Im { get;}
        internal abstract Unit Units { get;}
        internal abstract bool IsUnit { get; }
        internal abstract bool IsReal { get; }
        internal abstract bool IsComplex { get; }
        internal abstract Complex Complex { get; }
        internal abstract bool IsComposite();
        internal abstract RealValue AsReal();
        internal abstract ComplexValue AsComplex();

        public static IScalarValue operator -(IScalarValue a)
        {
            if (a is RealValue ra) return -ra;
            if (a is ComplexValue ca) return -ca;
            Throw.InvalidOperand($"-{a}");
            return null;
        }

        public static IScalarValue operator +(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra + rb;
                if (b is ComplexValue cb) return (ComplexValue)ra + cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca + (ComplexValue)rb;
                if (b is ComplexValue cb) return ca + cb;
            }
            Throw.InvalidOperand($"{a} + {b}");
            return null;
        }

        public static IScalarValue operator -(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra - rb;
                if (b is ComplexValue cb) return (ComplexValue)ra - cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca - (ComplexValue)rb;
                if (b is ComplexValue cb) return ca - cb;
            }
            Throw.InvalidOperand($"{a} - {b}");
            return null;
        }

        public static IScalarValue operator *(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra * rb;
                if (b is ComplexValue cb) return (ComplexValue)ra * cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca * (ComplexValue)rb;
                if (b is ComplexValue cb) return ca * cb;
            }
            Throw.InvalidOperand($"{a} * {b}");
            return null;
        }

        public static IScalarValue operator *(IScalarValue a, double b)
        {
            if (a is RealValue ra) return ra * b;
            if (a is ComplexValue ca) return ca * b;
            Throw.InvalidOperand($"{a} * {b}");
            return null;
        }

        public static IScalarValue operator /(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra / rb;
                if (b is ComplexValue cb) return (ComplexValue)ra / cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca / (ComplexValue)rb;
                if (b is ComplexValue cb) return ca / cb;
            }
            Throw.InvalidOperand($"{a} / {b}");
            return null;
        }

        public static IScalarValue operator %(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra % rb;
                if (b is ComplexValue cb) return (ComplexValue)ra % cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca % (ComplexValue)rb;
                if (b is ComplexValue cb) return ca % cb;
            }
            Throw.InvalidOperand($"{a} % {b}");
            return null;
        }

        public static IScalarValue operator <(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra < rb;
                if (b is ComplexValue cb) return (ComplexValue)ra < cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca < (ComplexValue)rb;
                if (b is ComplexValue cb) return ca < cb;
            }
            Throw.InvalidOperand($"{a} < {b}");
            return null;
        }

        public static IScalarValue operator >(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra > rb;
                if (b is ComplexValue cb) return (ComplexValue)ra > cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca > (ComplexValue)rb;
                if (b is ComplexValue cb) return ca > cb;
            }
            Throw.InvalidOperand($"{a} > {b}");
            return null;
        }

        public static IScalarValue operator <=(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra <= rb;
                if (b is ComplexValue cb) return (ComplexValue)ra <= cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca <= (ComplexValue)rb;
                if (b is ComplexValue cb) return ca <= cb;
            }
            Throw.InvalidOperand($"{a} ≤ {b}");
            return null;
        }

        public static IScalarValue operator >=(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra >= rb;
                if (b is ComplexValue cb) return (ComplexValue)ra >= cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca >= (ComplexValue)rb;
                if (b is ComplexValue cb) return ca >= cb;
            }
            Throw.InvalidOperand($"{a} ≥ {b}");
            return null;
        }

        internal static IScalarValue Equal(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra == rb;
                if (b is ComplexValue cb) return (ComplexValue)ra == cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca == (ComplexValue)rb;
                if (b is ComplexValue cb) return ca == cb;
            }
            Throw.InvalidOperand($"{a} ≡ {b}");
            return null;
        }

        internal static IScalarValue NotEqual(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra != rb;
                if (b is ComplexValue cb) return (ComplexValue)ra != cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca != (ComplexValue)rb;
                if (b is ComplexValue cb) return ca != cb;
            }
            Throw.InvalidOperand($"{a} ≠ {b}");
            return null;
        }

        public static IScalarValue operator &(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra & rb;
                if (b is ComplexValue cb) return (ComplexValue)ra & cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca & (ComplexValue)rb;
                if (b is ComplexValue cb) return ca & cb;
            }
            Throw.InvalidOperand($"{a} ∧ {b}");
            return null;
        }

        public static IScalarValue operator |(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra | rb;
                if (b is ComplexValue cb) return (ComplexValue)ra | cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca | (ComplexValue)rb;
                if (b is ComplexValue cb) return ca | cb;
            }
            Throw.InvalidOperand($"{a} ∨ {b}");
            return null;
        }

        public static IScalarValue operator ^(IScalarValue a, IScalarValue b)
        {
            if (a is RealValue ra)
            {
                if (b is RealValue rb) return ra ^ rb;
                if (b is ComplexValue cb) return (ComplexValue)ra ^ cb;
            }
            else if (a is ComplexValue ca)
            {
                if (b is RealValue rb) return ca ^ (ComplexValue)rb;
                if (b is ComplexValue cb) return ca ^ cb;
            }
            Throw.InvalidOperand($"{a} ⊕ {b}");
            return null;
        }

        internal static IScalarValue EvaluateFunction(Calculator calc, long index, in IScalarValue a) =>
            calc.EvaluateFunction(index, a); 

        internal static IScalarValue EvaluateOperator(Calculator calc, long index, in IScalarValue a, in IScalarValue b) =>
            calc.EvaluateOperator(index, a, b);

        internal static IScalarValue EvaluateFunction2(Calculator calc, long index, in IScalarValue a, in IScalarValue b) =>
            calc.EvaluateFunction2(index, a, b);

        internal static int AsInt(IScalarValue scalar, Throw.Items item = Throw.Items.Argument)
        {
            if (scalar.IsReal && scalar.Units is null)
            {
                var d = scalar.Re;
                if (d > 0 && d <= int.MaxValue && d.AlmostEquals(Math.Truncate(d)))
                    return (int)d;
            }
            Throw.MustBePositiveIntegerException(item);
            return 1;
        }
    }
}
