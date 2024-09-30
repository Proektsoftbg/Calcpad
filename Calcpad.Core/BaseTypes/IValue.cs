using System;
using System.Collections.Generic;

namespace Calcpad.Core
{
    internal interface IValue
    {
        public static IValue operator -(IValue a)
        {
            if (a is Value value) return -value;
            if (a is Vector vector) return -vector;
            if (a is Matrix matrix) return -matrix;
            Throw.InvalidOperand($"-{a}");
            return null;
        }

        public static IValue operator +(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala + valb;
                if (b is Vector vecb) return vecb + vala;
                if (b is Matrix matb) return matb + vala;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca + valb;
                if (b is Vector vecb) return veca + vecb;
                if (b is Matrix matb) return veca + matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata + valb;
                if (b is Vector vecb) return mata + vecb;
                if (b is Matrix matb) return mata + matb;
            }
            Throw.InvalidOperand($"{a} + {b}");
            return null;
        }

        public static IValue operator -(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala - valb;
                if (b is Vector vecb) return vala - vecb;
                if (b is Matrix matb) return vala - matb;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca - valb;
                if (b is Vector vecb) return veca - vecb;
                if (b is Matrix matb) return veca - matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata - valb;
                if (b is Vector vecb) return mata - vecb;
                if (b is Matrix matb) return mata - matb;
            }
            Throw.InvalidOperand($"{a} - {b}");
            return null;
        }

        public static IValue operator *(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala * valb;
                if (b is Vector vecb) return vecb * vala;
                if (b is Matrix matb) return matb * vala;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca * valb;
                if (b is Vector vecb) return veca * vecb;
                if (b is Matrix matb) return veca * matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata * valb;
                if (b is Vector vecb)
                {
                    var c = mata * vecb;
                    return c.RowCount == 1 ? c[0, 0] : c.Col(1);
                }
                if (b is Matrix matb) return mata * matb;
            }
            Throw.InvalidOperand($"{a}*{b}");
            return null;
        }

        public static IValue operator /(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala / valb;
                if (b is Vector vecb) return vala / vecb;
                if (b is Matrix matb) return vala / matb;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca / valb;
                if (b is Vector vecb) return veca / vecb;
                if (b is Matrix matb) return veca / matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata / valb;
                if (b is Vector vecb) return mata / vecb;
                if (b is Matrix matb) return mata / matb;
            }
            Throw.InvalidOperand($"{a}/{b}");
            return null;
        }

        public static IValue operator %(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala % valb;
                if (b is Vector vecb) return vala % vecb;
                if (b is Matrix matb) return vala % matb;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca % valb;
                if (b is Vector vecb) return veca % vecb;
                if (b is Matrix matb) return veca % matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata % valb;
                if (b is Vector vecb) return mata % vecb;
                if (b is Matrix matb) return mata % matb;
            }
            Throw.InvalidOperand($"{a}%{b}");
            return null;
        }

        public static IValue operator <(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala < valb;
                if (b is Vector vecb) return vala < vecb;
                if (b is Matrix matb) return vala < matb;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca < valb;
                if (b is Vector vecb) return veca < vecb;
                if (b is Matrix matb) return veca < matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata < valb;
                if (b is Vector vecb) return mata < vecb;
                if (b is Matrix matb) return mata < matb;
            }
            Throw.InvalidOperand($"{a} < {b}");
            return null;
        }

        public static IValue operator >(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala > valb;
                if (b is Vector vecb) return vala > vecb;
                if (b is Matrix matb) return vala > matb;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca > valb;
                if (b is Vector vecb) return veca > vecb;
                if (b is Matrix matb) return veca > matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata > valb;
                if (b is Vector vecb) return mata > vecb;
                if (b is Matrix matb) return mata > matb;
            }
            Throw.InvalidOperand($"{a} > {b}");
            return null;
        }

        public static IValue operator <=(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala <= valb;
                if (b is Vector vecb) return vala <= vecb;
                if (b is Matrix matb) return vala <= matb;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca <= valb;
                if (b is Vector vecb) return veca <= vecb;
                if (b is Matrix matb) return veca <= matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata <= valb;
                if (b is Vector vecb) return mata <= vecb;
                if (b is Matrix matb) return mata <= matb;
            }
            Throw.InvalidOperand($"{a} ≤ {b}");
            return null;
        }

        public static IValue operator >=(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala >= valb;
                if (b is Vector vecb) return vala >= vecb;
                if (b is Matrix matb) return vala >= matb;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca >= valb;
                if (b is Vector vecb) return veca >= vecb;
                if (b is Matrix matb) return veca >= matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata >= valb;
                if (b is Vector vecb) return mata >= vecb;
                if (b is Matrix matb) return mata >= matb;
            }
            Throw.InvalidOperand($"{a} ≥ {b}");
            return null;
        }

        internal static IValue Equal(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala == valb;
                if (b is Vector vecb) return vecb == vala;
                if (b is Matrix matb) return matb == vala;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca == valb;
                if (b is Vector vecb) return veca == vecb;
                if (b is Matrix matb) return veca == matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata == valb;
                if (b is Vector vecb) return mata == vecb;
                if (b is Matrix matb) return mata == matb;
            }
            Throw.InvalidOperand($"{a} ≡ {b}");
            return null;
        }

        internal static IValue NotEqual(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala != valb;
                if (b is Vector vecb) return vecb != vala;
                if (b is Matrix matb) return matb != vala;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca != valb;
                if (b is Vector vecb) return veca != vecb;
                if (b is Matrix matb) return veca != matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata != valb;
                if (b is Vector vecb) return mata != vecb;
                if (b is Matrix matb) return mata != matb;
            }
            Throw.InvalidOperand($"{a} ≠ {b}");
            return null;
        }

        public static IValue operator &(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala & valb;
                if (b is Vector vecb) return vecb & vala;
                if (b is Matrix matb) return matb & vala;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca & valb;
                if (b is Vector vecb) return veca & vecb;
                if (b is Matrix matb) return veca & matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata & valb;
                if (b is Vector vecb) return mata & vecb;
                if (b is Matrix matb) return mata & matb;
            }
            Throw.InvalidOperand($"{a}∧{b}.");
            return null;
        }

        public static IValue operator |(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala | valb;
                if (b is Vector vecb) return vecb | vala;
                if (b is Matrix matb) return matb | vala;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca | valb;
                if (b is Vector vecb) return veca | vecb;
                if (b is Matrix matb) return veca | matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata | valb;
                if (b is Vector vecb) return mata | vecb;
                if (b is Matrix matb) return mata | matb;
            }
            Throw.InvalidOperand($"{a}∨{b}");
            return null;
        }

        public static IValue operator ^(IValue a, IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb) return vala ^ valb;
                if (b is Vector vecb) return vecb ^ vala;
                if (b is Matrix matb) return matb ^ vala;
            }
            else if (a is Vector veca)
            {
                if (b is Value valb) return veca ^ valb;
                if (b is Vector vecb) return veca ^ vecb;
                if (b is Matrix matb) return veca ^ matb;
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb) return mata ^ valb;
                if (b is Vector vecb) return mata ^ vecb;
                if (b is Matrix matb) return mata ^ matb;
            }
            Throw.InvalidOperand($"{a}⊕{b}");
            return null;
        }

        internal static IValue EvaluateFunction(MatrixCalculator calc, long index, in IValue a)
        {
            if (a is Value value)
                return calc.Calculator.EvaluateFunction(index, value);
            if (a is Vector vector)
                return calc.VectorCalculator.EvaluateFunction(index, vector);
            if (a is Matrix matrix)
                return calc.EvaluateFunction(index, matrix);

            Throw.InvalidArgument($"{a}");
            return null;
        }

        internal static IValue EvaluateOperator(MatrixCalculator calc, long index, in IValue a, in IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb)
                    return calc.Calculator.EvaluateOperator(index, vala, valb);
                if (b is Vector vecb)
                    return calc.VectorCalculator.EvaluateOperator(index, vala, vecb);
                if (b is Matrix matb)
                    return calc.EvaluateOperator(index, vala, matb);
            }
            else if (a is Vector veca)
            {
                if (b is Value valb)
                    return calc.VectorCalculator.EvaluateOperator(index, veca, valb);
                if (b is Vector vecb)
                    return calc.VectorCalculator.EvaluateOperator(index, veca, vecb);
                if (b is Matrix matb)
                    return calc.EvaluateOperator(index, veca, matb);

            }
            else if (a is Matrix mata)
            {
                if (b is Value valb)
                    return calc.EvaluateOperator(index, mata, valb);
                if (b is Vector vecb)
                {
                    var c = calc.EvaluateOperator(index, mata, vecb);
                    if (index == Calculator.MultiplyIndex)
                        return c.RowCount == 1 ? c[0, 0] : c.Col(1);

                    return c;
                }
                if (b is Matrix matb)
                    return calc.EvaluateOperator(index, mata, matb);
            }
            Throw.InvalidOperand($"{a}; {b}");
            return null;
        }

        internal static IValue EvaluateFunction2(MatrixCalculator calc, long index, in IValue a, in IValue b)
        {
            if (a is Value vala)
            {
                if (b is Value valb)
                    return calc.Calculator.EvaluateFunction2(index, vala, valb);
                if (b is Vector vecb)
                    return calc.VectorCalculator.EvaluateFunction2(index, vala, vecb);
                if (b is Matrix matb)
                    return calc.EvaluateFunction2(index, vala, matb);
            }
            else if (a is Vector veca)
            {
                if (b is Value valb)
                    return calc.VectorCalculator.EvaluateFunction2(index, veca, valb);
                if (b is Vector vecb)
                    return calc.VectorCalculator.EvaluateFunction2(index, veca, vecb);
                if (b is Matrix matb)
                    return calc.EvaluateFunction2(index, veca, matb);
            }
            else if (a is Matrix mata)
            {
                if (b is Value valb)
                    return calc.EvaluateFunction2(index, mata, valb);
                if (b is Vector vecb)
                    return calc.EvaluateFunction2(index, mata, vecb);
                if (b is Matrix matb)
                    return calc.EvaluateFunction2(index, mata, matb);
            }
            Throw.InvalidArgument($"{a}; {b}");
            return null;
        }

        internal static IValue EvaluateInterpolation(MatrixCalculator calc, long index, IValue[] values)
        {
            if (values[0] is Value xValue)
            {
                if (values.Length == 2 && values[1] is Vector vector)
                    return calc.VectorCalculator.EvaluateInterpolation(index, xValue, vector);
                if (values.Length == 3 && values[1] is Value yValue && values[2] is Matrix matrix)
                    return calc.EvaluateInterpolation(index, yValue, xValue, matrix);
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
                if (values[0] is Matrix matirx)
                    return calc.EvaluateMultiFunction(index, matirx);
            }
            return calc.Calculator.EvaluateMultiFunction(index, ExpandValues(values));
        }

        internal static Value[] ExpandValues(IValue[] values)
        {
            var len = values.Length;
            var valList = new List<Value>(len);
            for (var i = 0; i < len; ++i)
            {
                var ival = values[i];
                if (ival is Value value)
                    valList.Add(value);
                else if (ival is Vector vector)
                    valList.AddRange(vector.Values);
                else if (ival is Matrix matrix)
                    valList.AddRange(matrix.Values);
                else
                    Throw.InvalidArgument($"{ival}");
            }
            return [.. valList];
        }

        internal static Value AsValue(IValue iValue, Throw.Items item = Throw.Items.Argument)
        {
            if (iValue is Value val)
                return val;
            else
                Throw.MustBeScalarException(item);

            return Value.NaN;
        }

        internal static int AsInt(IValue iValue, Throw.Items item = Throw.Items.Argument)
        {
            var value = AsValue(iValue, item);
            if (value.IsReal && value.Units is null)
            {
                var d = value.Re;
                if (d > 0 && d <= int.MaxValue && d.AlmostEquals(Math.Truncate(d)))
                    return (int)d;
            }
            Throw.MustBePositiveIntegerException(item);
            return 1;
        }

        internal static Vector AsVector(IValue iValue, Throw.Items item = Throw.Items.Argument)
        {
            if (iValue is Vector vec)
                return vec;
            else
                Throw.MustBeVectorException(item);

            return null;
        }

        internal static Matrix AsMatrix(IValue iValue, Throw.Items item = Throw.Items.Argument)
        {
            if (iValue is Matrix mat)
                return mat;
            else if (iValue is Vector vec)
                return new ColumnMatrix(vec);
            else
                Throw.MustBeMatrixException(item);

            return null;
        }
    }
}