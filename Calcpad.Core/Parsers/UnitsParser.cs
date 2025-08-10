using System;
using System.Collections.Generic;
using System.Globalization;

namespace Calcpad.Core
{
    internal static class UnitsParser
    {
        private enum TokenTypes
        {
            None,
            Constant,
            Units,
            Operator,
            BracketLeft,
            BracketRight,
            Error
        }

        private class Token
        {
            internal int Order;
            internal const int DefaultOrder = -1;
            internal string Content;
            internal readonly TokenTypes Type;
            internal Token(string content, TokenTypes type)
            {
                Content = content;
                Type = type;
                Order = DefaultOrder;
            }
            internal Token(string content, TokenTypes type, int order) : this(content, type) { Order = order; }
            internal Token(Token T, int order) : this(T.Content, T.Type, order) { }
        }

        private sealed class UnitToken : Token
        {
            internal readonly Unit Value;
            internal UnitToken(Unit u) : base(u.Text, TokenTypes.Units)
            {
                Value = u;
                if (string.IsNullOrEmpty(Content))
                    Content = u.FormatString;
            }
        }

        private sealed class ValueToken : Token
        {
            internal readonly double Value;
            internal ValueToken(double d) : base(d.ToString(CultureInfo.InvariantCulture), TokenTypes.Constant)
            {
                Value = d;
            }
        }

        private static class UnitValidator
        {
            private static readonly bool[,] CorrectOrder =
            {
            //None  Const   Units   Oper    BrOp    BrCl   Error  
            {true,  true,   true,   true,   true,   true},  //None 
            {true,  false,  false,  true,   false,  true},  //Const
            {true,  false,  false,  true,   false,  true},  //Unit
            {true,  true,   true,   false,  true,   false}, //Oper
            {true,  true,   true,   false,  true,   false}, //BrOp
            {true,  false,  false,  true,   false,  true},  //BrCl  
            };

            internal static void Check(Queue<Token> input)
            {
                //Initializes the correct sequence of token types. 
                //Value if true if type in row can precede type in column
                //Text and heading are hot included. They are checked separately.
                var countOfBrackets = 0;
                var pT = new Token(string.Empty, TokenTypes.None);

                foreach (var T in input)
                {
                    if (T.Type == TokenTypes.BracketLeft)
                        ++countOfBrackets;
                    else if (T.Type == TokenTypes.BracketRight)
                    {
                        --countOfBrackets;
                        if (countOfBrackets < 0)
                            throw Exceptions.InvalidUnits(T.Content);
                    }

                    if (!CorrectOrder[(int)pT.Type, (int)T.Type])
                        throw Exceptions.InvalidSyntax($"{pT.Content} {T.Content}");

                    pT = T;
                }

                if (pT.Type == TokenTypes.Operator || pT.Type == TokenTypes.BracketLeft)
                    throw Exceptions.IncompleteExpression();

                if (countOfBrackets > 0)
                    throw Exceptions.MissingRightBracket();

                if (countOfBrackets < 0)
                    throw Exceptions.MissingLeftBracket();
            }
        }

        private static int OperatorOrder(string @operator) => (@operator[0]) switch
        {
            '^' => 0,
            '/' => 1,
            '*' => 2,
            _ => -1,
        };

        internal static Unit Parse(ReadOnlySpan<char> expression, Dictionary<string, Unit> units)
        {
            var input = GetInput(expression, units);
            UnitValidator.Check(input);
            var rpn = GetRpn(input);
            return Evaluate(rpn);
        }

        private static Queue<Token> GetInput(ReadOnlySpan<char> expression, Dictionary<string, Unit> units)
        {
            // Tokenize input string
            // It is converted to a queue of recognizable tokens
            // string is scanned char by char and token type is determined depending on char value

            var tokens = new Queue<Token>();
            var pt = TokenTypes.None;
            var ts = new TextSpan(expression);
            var len = expression.Length;
            for (int i = 0; i < len; ++i)
            {
                var c = expression[i];
                //Get the type of the token
                TokenTypes tt;
                if (Validator.IsLetter(c))
                    tt = TokenTypes.Units;
                else if (c == '-' || Validator.IsDigit(c))
                    tt = TokenTypes.Constant;
                else if (c == '*' || c == '/' || c == '^')
                    tt = TokenTypes.Operator;
                else if (c == '(')
                    tt = TokenTypes.BracketLeft;
                else if (c == ')')
                    tt = TokenTypes.BracketRight;
                else if (c == ' ' || c == ':')
                    tt = TokenTypes.None;
                else
                    tt = TokenTypes.Error;

                if (tt == TokenTypes.Error)
                    throw Exceptions.InvalidSymbol(c);

                //Collect characters in a string for text, constant, variable or function
                if (tt == TokenTypes.Constant || tt == TokenTypes.Units)
                {
                    ts.Expand();
                    if (ts.Length > 1 && (pt != tt || c == '-'))
                        throw Exceptions.InvalidSymbol(c);
                }
                else
                {
                    if (!ts.IsEmpty)
                    {
                        AddLiteral(ts);
                        ts.Reset(i + 1);
                        if (c == ':')
                        {
                            var s = expression[(i + 1)..].Trim().ToString();
                            if (Validator.IsValidFormatString(s))
                            {
                                if (tokens.Count > 0)
                                {
                                    var t = tokens.Peek();
                                    if (t is UnitToken tu)
                                    {
                                        t.Content += ':' + s;
                                        tu.Value.Text = t.Content;
                                        return tokens;
                                    }
                                }
                                var fu = Unit.GetFormattingUnit(':' + s);
                                tokens.Enqueue(new UnitToken(fu));
                                return tokens;
                            }
                            else
                                throw Exceptions.InvalidFormatString(s);
                        }
                    }
                    else
                        ts.Reset(i + 1);

                    if (tt != TokenTypes.None)
                        tokens.Enqueue(new Token(c.ToString(), tt));
                }
                //Saves the previous token type for the next loop
                pt = tt;
            }
            if (!ts.IsEmpty)
                AddLiteral(ts);

            return tokens;

            void AddLiteral(TextSpan ts)
            {
                var literal = ts.Cut().ToString();
                if (pt == TokenTypes.Units)
                {
                    if (units is null || !units.TryGetValue(literal, out var u))
                        Unit.TryGet(literal, out u);

                    if (u is null)
                        throw Exceptions.InvalidUnits(literal);

                    tokens.Enqueue(new UnitToken(u));
                }
                else
                {
                    if (double.TryParse(literal, out var d))
                        tokens.Enqueue(new ValueToken(d));
                    else
                        throw Exceptions.InvalidNumber(literal);
                }
            }
        }

        private static Queue<Token> GetRpn(Queue<Token> input)
        {
            //Converts the input queue to Reverse Polish Notation by using shunting yard algorithm
            var output = new Queue<Token>();
            var stackBuffer = new Stack<Token>();

            foreach (var T in input)
            {
                Token nextT;
                switch (T.Type)
                {
                    case TokenTypes.Constant:
                    case TokenTypes.Units:
                        output.Enqueue(T);
                        break;
                    case TokenTypes.Operator:
                        var tOrder = OperatorOrder(T.Content);
                        while (stackBuffer.Count != 0)
                        {
                            nextT = stackBuffer.Peek();
                            nextT.Order = OperatorOrder(nextT.Content);
                            if (nextT.Type == TokenTypes.Operator && nextT.Order > tOrder
                            || nextT.Type == TokenTypes.BracketLeft)
                                break;

                            output.Enqueue(new Token(stackBuffer.Pop(), nextT.Order));
                        }
                        stackBuffer.Push(new Token(T, tOrder));
                        break;

                    case TokenTypes.BracketLeft:
                        stackBuffer.Push(T);
                        break;
                    case TokenTypes.BracketRight:
                        while (stackBuffer.Count != 0)
                        {
                            nextT = stackBuffer.Peek();
                            if (nextT.Type == TokenTypes.BracketLeft)
                            {
                                if (T.Type == TokenTypes.BracketRight) stackBuffer.Pop();
                                break;
                            }
                            output.Enqueue(stackBuffer.Pop());
                        }
                        break;
                }
            }
            while (stackBuffer.Count != 0)
                output.Enqueue(stackBuffer.Pop());

            return output;
        }

        private static Unit Evaluate(Queue<Token> rpn)
        {
            //Evaluates the expression from the reverse polish notation queue

            var stackBuffer = new Stack<Token>();

            foreach (var T in rpn)
            {
                switch (T.Type)
                {
                    case TokenTypes.Constant:
                    case TokenTypes.Units:
                        stackBuffer.Push(T);
                        break;
                    case TokenTypes.Operator:
                        if (stackBuffer.Count == 0)
                            throw Exceptions.MissingOperand();

                        var b = stackBuffer.Pop();
                        if (stackBuffer.Count == 0)
                            throw Exceptions.MissingOperand();

                        var a = stackBuffer.Pop();
                        var c = EvaluateOperator(T, a, b);
                        stackBuffer.Push(c);
                        break;
                    default:
                        throw Exceptions.InvalidLiteral(T.Content, T.Type.GetType().GetEnumName(T.Type));
                }
            }
            if (stackBuffer.Count == 0)
                return null;

            var t = stackBuffer.Pop();
            if (t.Type == TokenTypes.Units)
            {
                var u = ((UnitToken)t).Value;
                var s = RenderExpression(rpn);
                if (s.Length > 0 && char.IsDigit(s[0]))
                    u.Text = "\u200A·\u200A" + s;
                else
                    u.Text = s;

                return u;
            }
            throw Exceptions.ResultIsNotUnits();
        }

        private static Token EvaluateOperator(Token T, Token a, Token b)
        {
            if (a.Type == TokenTypes.Units)
            {
                if (b.Type == TokenTypes.Units)
                    return EvaluateOperator(T, (UnitToken)a, (UnitToken)b);

                return EvaluateOperator(T, (UnitToken)a, (ValueToken)b);
            }
            if (b.Type == TokenTypes.Units)
                return EvaluateOperator(T, (ValueToken)a, (UnitToken)b);

            return EvaluateOperator(T, (ValueToken)a, (ValueToken)b);
        }

        private static Token EvaluateOperator(Token T, UnitToken a, UnitToken b)
        {
            char c = T.Content[0];
            double d;
            var u = c switch
            {
                '*' => Unit.Multiply(a.Value, b.Value, out d, false),
                '/' => Unit.Divide(a.Value, b.Value, out d, false),
                '^' => throw Exceptions.PowerNotUnitless(),
                _ => throw Exceptions.InvalidOperator(c)
            };
            return u is null ? new ValueToken(d) : new UnitToken(u * d);
        }

        private static UnitToken EvaluateOperator(Token T, UnitToken a, ValueToken b)
        {
            char c = T.Content[0];
            return c switch
            {
                '*' => new UnitToken(a.Value * b.Value),
                '/' => new UnitToken(a.Value / b.Value),
                '^' => new UnitToken(a.Value.Pow((float)b.Value)),
                _ => throw Exceptions.InvalidOperator(c)
            };
        }

        private static UnitToken EvaluateOperator(Token T, ValueToken a, UnitToken b)
        {
            char c = T.Content[0];
            return c switch
            {
                '*' => new UnitToken(a.Value * b.Value),
                '/' => new UnitToken(a.Value / b.Value),
                '^' => throw Exceptions.PowerNotUnitless(),
                _ => throw Exceptions.InvalidOperator(c)
            };
        }

        private static ValueToken EvaluateOperator(Token T, ValueToken a, ValueToken b)
        {
            char c = T.Content[0];
            return c switch
            {
                '*' => new ValueToken(a.Value * b.Value),
                '/' => new ValueToken(a.Value / b.Value),
                '^' => new ValueToken(Math.Pow(a.Value, b.Value)),
                _ => throw Exceptions.InvalidOperator(c)
            };
        }

        private static string RenderExpression(Queue<Token> rpn)
        {
            //Renders the expression from the reverse polish notation as text
            var stackBuffer = new Stack<Token>();
            foreach (var T in rpn)
            {
                if (T.Type == TokenTypes.Constant || T.Type == TokenTypes.Units)
                    stackBuffer.Push(T);
                else
                {
                    var b = stackBuffer.Pop();
                    var a = stackBuffer.Count == 0 ?
                        new Token(string.Empty, TokenTypes.None) :
                        stackBuffer.Pop();

                    if (a.Order > T.Order)
                        a.Content = AddBrackets(a.Content);

                    var tc = T.Content[0];
                    var sa = a.Content;
                    var sb = b.Content;
                    if (tc == '^')
                    {
                        if (IsNegative(a))
                            sa = AddBrackets(sa);

                        if (IsNegative(b) || b.Order != Token.DefaultOrder)
                            sb = AddBrackets(sb);
                    }
                    else if (b.Order > T.Order ||
                             b.Order == T.Order && tc == '/' ||
                             IsNegative(b))
                        sb = AddBrackets(sb);

                    var s = sb.StartsWith(':') ? sa + sb : sa + tc + sb;
                    var c = new Token(s, T.Type, T.Order);
                    stackBuffer.Push(c);
                }
                static string AddBrackets(string s) => $"({s})";
            }
            if (stackBuffer.TryPop(out var result))
                return result.Content;

            return string.Empty;
        }

        private static bool IsNegative(Token t) =>
            t.Order == Token.DefaultOrder && t.Content.Length > 0 && t.Content[0] == '-';
    }
}