namespace Calcpad.Core
{
    public partial class MathParser
    {
        private enum ValueTypes
        {
            None,
            Number,
            Unit,
            NumberWithUnit
        }

        private enum TokenTypes
        {
            None,
            Constant,
            Variable,
            Unit,
            Input,
            Operator,
            Function,
            Function2,
            MultiFunction,
            If,
            CustomFunction,
            BracketLeft,
            BracketRight,
            Divisor,
            Solver,
            Comment,
            Error
        }

        private class Token
        {
            internal string Content;
            internal int Index = -1;
            internal TokenTypes Type;
            internal int Order = DefaultOrder;
            internal const int DefaultOrder = -1;
            internal Token(string content, TokenTypes type)
            {
                Content = content;
                Type = type;
            }

            internal Token(char content, TokenTypes type) : this(content.ToString(), type) { }
            internal Token(string content, TokenTypes type, int order) : this(content, type)
            {
                Order = order;
            }
        }
        private class FunctionToken : Token
        {
            internal int ParameterCount;
            internal FunctionToken(string content) : base(content, TokenTypes.MultiFunction) { }
        }

        private class ValueToken : Token
        {
            internal Value Value;
            internal ValueToken(in Value value) : base(string.Empty, TokenTypes.Constant)
            {
                Value = value;
            }
        }

        private class VariableToken : Token
        {
            internal Variable Variable;
            internal VariableToken(string content, Variable v) : this(content)
            {
                Variable = v;
            }

            private VariableToken(string content) : base(content, TokenTypes.Variable) { }
        }

        private class RenderToken : Token
        {

            internal ValueTypes ValType;  //0 - none, 1 - number, 2 - unit, 3 - number + unit
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
                if (t is FunctionToken ft)
                    ParameterCount = ft.ParameterCount;
                if (t.Type == TokenTypes.Unit || t.Type == TokenTypes.Constant || t.Type == TokenTypes.Variable || t.Type == TokenTypes.Input)
                {
                    Value v = Value.Zero;
                    if (t is ValueToken vt)
                        v = vt.Value;
                    else if (t is VariableToken vr)
                        v = vr.Variable.Value;

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