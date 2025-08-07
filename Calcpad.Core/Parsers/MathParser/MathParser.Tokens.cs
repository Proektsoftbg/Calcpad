namespace Calcpad.Core
{
    public partial class MathParser
    {
        private enum ValueTypes
        {
            None,
            Number,
            Unit,
            NumberWithUnit,
            Vector,
            Matrix
        }

        private enum TokenTypes
        {
            None,
            Constant,
            Variable,
            Unit,
            Input,
            Vector,
            Matrix,
            RowDivisor,
            VectorIndex,
            MatrixIndex,
            Operator,
            Function,
            Function2,
            Function3,
            MultiFunction,
            Interpolation,
            VectorFunction,
            VectorFunction2,
            VectorFunction3,
            VectorMultiFunction,
            MatrixFunction,
            MatrixFunction2,
            MatrixFunction3,
            MatrixFunction4,
            MatrixFunction5,
            MatrixMultiFunction,
            MatrixOptionalFunction,
            CustomFunction,
            BracketLeft,
            BracketRight,
            SquareBracketLeft,
            SquareBracketRight,
            Divisor,
            Solver,
            Error
        }

        private class Token
        {
            internal string Content;
            internal long Index = -1;
            internal TokenTypes Type;
            internal sbyte Order = DefaultOrder;
            internal const sbyte DefaultOrder = -1;
            internal Token(string content, TokenTypes type)
            {
                Content = content;
                Type = type;
            }

            internal Token(char content, TokenTypes type) : this(content.ToString(), type) { }
            internal Token(string content, TokenTypes type, sbyte order) : this(content, type)
            {
                Order = order;
            }
        }
        private sealed class FunctionToken : Token
        {
            internal int ParameterCount;
            internal FunctionToken(string content) : base(content, TokenTypes.MultiFunction) { }
        }

        private sealed class ValueToken : Token
        {
            internal IScalarValue Value;
            internal ValueToken(in IScalarValue value) : base(string.Empty, TokenTypes.Constant)
            {
                Value = value;
            }
        }

        private sealed class VariableToken : Token
        {
            internal Variable Variable;
            internal VariableToken(string content, Variable v) : this(content)
            {
                Variable = v;
            }

            private VariableToken(string content) : base(content, TokenTypes.Variable) { }
        }

        private sealed class VectorToken : Token
        {
            internal Vector Vector;
            internal VectorToken(string content, Vector v) : this(content)
            {
                Vector = v;
            }

            private VectorToken(string content) : base(content, TokenTypes.Vector) { }
        }

        private sealed class MatrixToken : Token
        {
            internal Matrix Matrix;
            internal MatrixToken(string content, Matrix M) : this(content)
            {
                Matrix = M;
            }

            private MatrixToken(string content) : base(content, TokenTypes.Matrix) { }
        }

        private sealed class RenderToken : Token
        {

            internal ValueTypes ValType;
            internal int Level;
            internal int Offset;          //-1 - down, 0 - none, 1 - up
            internal int MinOffset;
            internal int MaxOffset;
            internal readonly int ParameterCount;
            internal bool IsCompositeValue;

            internal RenderToken(string content, TokenTypes type, int level) : base(content, type)
            {
                Level = level;
            }

            internal RenderToken(Token t, int level) : base(t.Content, t.Type, t.Order)
            {
                Index = t.Index;
                Level = level;
                Order = t.Order;
                ParameterCount = -1;
                if (t is FunctionToken ft)
                    ParameterCount = ft.ParameterCount;
                else if (t is VectorToken)
                {
                    ParameterCount = (int)Index - 1;
                    Index = -1;
                }
                else if (t is MatrixToken)
                {
                    ParameterCount = (int)(Index / Vector.MaxLength) - 1;
                    Index = -1;
                }
                if (t.Type == TokenTypes.Unit ||
                    t.Type == TokenTypes.Constant ||
                    t.Type == TokenTypes.Variable ||
                    t.Type == TokenTypes.Input)
                {
                    IScalarValue v = RealValue.Zero;
                    if (t is ValueToken vt)
                        v = vt.Value;
                    else if (t is VariableToken vr)
                    {
                        var ival = vr.Variable.Value;
                        if (ival is Vector)
                        {
                            ValType = ValueTypes.Vector;
                            return;
                        }
                        if (ival is Matrix)
                        {
                            ValType = ValueTypes.Matrix;
                            return;
                        }
                        if (ival is IScalarValue scalar)
                            v = scalar;
                    }
                    if (v.IsUnit)
                        ValType = ValueTypes.Unit;
                    else if (v.Units is null && t.Type != TokenTypes.Variable)
                        ValType = ValueTypes.Number;
                    else
                        ValType = ValueTypes.NumberWithUnit;
                }
            }
        }
    }
}