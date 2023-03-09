using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

        private class UnitToken : Token
        {
            internal readonly Unit Value;
            internal UnitToken(Unit u) : base(u.Text, TokenTypes.Units)
            {
                Value = u;
            }
        }

        private class ValueToken : Token
        {
            internal readonly double Value;
            internal ValueToken(double d) : base(d.ToString(CultureInfo.InvariantCulture), TokenTypes.Constant)
            {
                Value = d;
            }
        }

        private static class Validator
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
#if BG
                            throw new MathParser.MathParserException("Липсва лява скоба '('.");
#else
                            throw new MathParser.MathParserException("Missing left bracket '('.");
#endif
                    }

                    if (!CorrectOrder[(int)pT.Type, (int)T.Type])
#if BG
                        throw new MathParser.MathParserException($"Невалиден синтаксис: \"{pT.Content} {T.Content}\".");
#else
                        throw new MathParser.MathParserException($"Invalid syntax: \"{pT.Content} {T.Content}\".");
#endif

                    pT = T;
                }

                if (pT.Type == TokenTypes.Operator || pT.Type == TokenTypes.BracketLeft)
#if BG
                    throw new MathParser.MathParserException("Непълен израз.");
#else
                    throw new MathParser.MathParserException("Incomplete expression.");
#endif
                if (countOfBrackets > 0)
#if BG
                    throw new MathParser.MathParserException("Липсва дясна скоба ')'.");
#else
                    throw new MathParser.MathParserException("Missing right bracket ')'.");
#endif
                if (countOfBrackets < 0)
#if BG
                    throw new MathParser.MathParserException("Липсва лява скоба '('.");
#else
                    throw new MathParser.MathParserException("Missing left bracket '('.");
#endif
            }
            internal static bool IsLetter(char c)
            {
                return c >= 'A' && c <= 'Z' || // A - Z 
                       c >= 'a' && c <= 'z' || // a - z
                       c >= 'Α' && c <= 'Ω' || // Alpha - Omega
                       c >= 'α' && c <= 'ω' ||   // alpha - omega
                       "_°′″‴⁗℧%‰".Contains(c, StringComparison.Ordinal); // _ ° ′ ″ ‴ ⁗ ℧ % ‰
            }

            internal static bool IsDigit(char c)
            {
                int i = c;
                return i >= 48 && i <= 57; // 0-9 
            }
        }

        private static int OperatorOrder(string @operator) => (@operator[0]) switch
        {
            '^' => 0,
            '/' => 1,
            '*' => 2,
            _ => -1,
        };

        internal static Unit Parse(string expression)
        {
            var input = GetInput(expression);
            Validator.Check(input);
            var rpn = GetRpn(input);
            return Evaluate(rpn);
        }

        private static Queue<Token> GetInput(string expression)
        {
            // Tokenize input string
            // It is converted to a queue of recognizable tokens
            // string is scanned char by char and token type is determined depending on char value

            var tokens = new Queue<Token>();
            var pt = TokenTypes.None;
            var terminatedExpression = expression + " ";
            var literal = string.Empty;
            //HasSolver = false;
            for (int i = 0, len = terminatedExpression.Length; i < len; ++i)
            {
                var c = terminatedExpression[i];
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
                    tt = TokenTypes.BracketLeft;
                else if (c == ' ')
                    tt = TokenTypes.None;
                else
                    tt = TokenTypes.Error;

                if (tt == TokenTypes.Error)
#if BG
                    throw new MathParser.MathParserException($"Невалиден символ '{c}'.");
#else
                    throw new MathParser.MathParserException($"Invalid symbol '{c}'.");
#endif
                //Collect characters in a string for text, constant, variable or function
                if (tt == TokenTypes.Constant || tt == TokenTypes.Units)
                {
                    literal += c;
                    if (literal.Length > 1 && (pt != tt || c == '-'))
#if BG
                        throw new MathParser.MathParserException($"Невалиден символ '{c}'.");
#else
                        throw new MathParser.MathParserException($"Invalid symbol '{c}'.");
#endif
                }
                else
                {
                    if (!string.IsNullOrEmpty(literal))
                    {
                        if (pt == TokenTypes.Units)
                        {
                            try
                            {
                                tokens.Enqueue(new UnitToken(Unit.Get(literal)));
                            }
                            catch
                            {
#if BG
                                throw new MathParser.MathParserException($"Невалидни мерни единици: \"{literal}\".");
#else
                                throw new MathParser.MathParserException($"Invalid units: \"{literal}\".");
#endif
                            }
                        }
                        else
                        {
                            try
                            {
                                tokens.Enqueue(new ValueToken(double.Parse(literal)));
                            }
                            catch
                            {
#if BG
                                throw new MathParser.MathParserException($"Не мога да изчисля \"{literal}\" като число.");
#else
                                throw new MathParser.MathParserException($"Cannot evaluate \"{literal}\" as number.");
#endif
                            }
                        }
                        literal = string.Empty;
                    }

                    if (tt != TokenTypes.None)
                        tokens.Enqueue(new Token(c.ToString(), tt));
                }
                //Saves the previous token type for the next loop
                pt = tt;
            }
            return tokens;
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
                        while (stackBuffer.Any())
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
                        while (stackBuffer.Any())
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
            while (stackBuffer.Any())
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
                        if (!stackBuffer.Any())
#if BG
                            throw new MathParser.MathParserException("Липсва операнд.");
#else
                            throw new MathParser.MathParserException("Missing operand.");
#endif

                        var b = stackBuffer.Pop();
                        if (!stackBuffer.Any())
#if BG
                            throw new MathParser.MathParserException("Липсва операнд.");
#else
                            throw new MathParser.MathParserException("Missing operand.");
#endif

                        var a = stackBuffer.Pop();
                        var c = EvaluateOperator(T, a, b);
                        stackBuffer.Push(c);
                        break;
                    default:
#if BG
                        throw new MathParser.MathParserException($"Не мога да изчисля \"{T.Content}\" като \"{T.Type.GetType().GetEnumName(T.Type)}\".");
#else
                        throw new MathParser.MathParserException($"Cannot evaluate \"{T.Content}\" as \"{T.Type.GetType().GetEnumName(T.Type)}\".");
#endif
                }
            }
            if (!stackBuffer.Any())
                return null;

            var t = stackBuffer.Pop();
            if (t.Type == TokenTypes.Units)
            {
                var u = ((UnitToken)t).Value;
                u.Text = RenderExpression(rpn, OutputWriter.OutputFormat.Text);
                return u;
            }
#if BG
            throw new MathParser.MathParserException("Изразът не се изчислява до мерни единици.");
#else
            throw new MathParser.MathParserException("This expression does not evaluate to units.");
#endif
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
            double d = c == '^' ? 1.0 : Unit.GetProductOrDivisionFactor(a.Value, b.Value);
            return c switch
            {
                '*' => new UnitToken(a.Value * b.Value * d),
                '/' => new UnitToken(a.Value / b.Value / d),
#if BG
                '^' => throw new MathParser.MathParserException("Степенният показател трябва да е бездименсионен."),
                _ => throw new MathParser.MathParserException($"Невалиден оператор: \"{T.Content}\".")
#else
                '^' => throw new MathParser.MathParserException("Power must be unitless."),
                _ => throw new MathParser.MathParserException($"Invalid operator: \"{T.Content}\".")
#endif
            };
        }

        private static Token EvaluateOperator(Token T, UnitToken a, ValueToken b)
        {
            return T.Content switch
            {
                "*" => new UnitToken(a.Value * b.Value),
                "/" => new UnitToken(a.Value / b.Value),
                "^" => new UnitToken(a.Value.Pow(b.Value)),
#if BG
                _ => throw new MathParser.MathParserException($"Невалиден оператор: \"{T.Content}\".")
#else
                _ => throw new MathParser.MathParserException($"Invalid operator: \"{T.Content}\".")
#endif
            };
        }

        private static Token EvaluateOperator(Token T, ValueToken a, UnitToken b)
        {
            return T.Content switch
            {
                "*" => new UnitToken(a.Value * b.Value),
                "/" => new UnitToken(a.Value / b.Value),
#if BG
                "^" => throw new MathParser.MathParserException("Степенният показател трябва да е бездименсионен."),
                 _ => throw new MathParser.MathParserException($"Невалиден оператор: \"{T.Content}\".")
#else
                "^" => throw new MathParser.MathParserException("Power must be unitless."),
                _ => throw new MathParser.MathParserException($"Invalid operator: \"{T.Content}\".")
#endif
            };
        }

        private static Token EvaluateOperator(Token T, ValueToken a, ValueToken b)
        {
            return T.Content switch
            {
                "*" => new ValueToken(a.Value * b.Value),
                "/" => new ValueToken(a.Value / b.Value),
                "^" => new ValueToken(Math.Pow(a.Value, b.Value)),
#if BG
                _ => throw new MathParser.MathParserException($"Невалиден оператор: \"{T.Content}\".")
#else
                _ => throw new MathParser.MathParserException($"Invalid operator: \"{T.Content}\".")
#endif
            };
        }

        private static string RenderExpression(Queue<Token> rpn, OutputWriter.OutputFormat format)
        {

            OutputWriter writer = format switch
            {
                OutputWriter.OutputFormat.Html => new HtmlWriter(),
                OutputWriter.OutputFormat.Xml => new XmlWriter(),
                _ => new TextWriter()
            };
            //Renders the expression from the reverse polish notation as text
            var stackBuffer = new Stack<Token>();
            foreach (var T in rpn)
            {
                switch (T.Type)
                {
                    case TokenTypes.Constant:
                        stackBuffer.Push(new Token(writer.FormatReal(((ValueToken)T).Value, 2), TokenTypes.Constant));
                        break;
                    case TokenTypes.Units:
                        stackBuffer.Push(new Token(FormatLocal(T.Content), TokenTypes.Units));
                        break;
                    default:
                        {
                            Token c;
                            var b = stackBuffer.Pop();
                            var a = !stackBuffer.Any() ?
                                new Token(string.Empty, TokenTypes.None) :
                                stackBuffer.Pop();

                            if (a.Order > T.Order)
                                a.Content = writer.AddBrackets(a.Content);

                            if (T.Content == "^")
                            {
                                if (IsNegative(a))
                                    a.Content = writer.AddBrackets(a.Content);

                                if (writer is TextWriter && IsNegative(b) || b.Order != Token.DefaultOrder)
                                    b.Content = writer.AddBrackets(b.Content);

                                c = new Token(writer.FormatPower(a.Content, b.Content, 0, a.Order), T.Type, T.Order);
                            }
                            else
                            {
                                if (b.Order > T.Order || b.Order == T.Order && T.Content == "/" || IsNegative(b))
                                    b.Content = writer.AddBrackets(b.Content);

                                c = new Token(a.Content + writer.FormatOperator(T.Content[0]) + b.Content, T.Type, T.Order);
                            }
                            stackBuffer.Push(c);
                            break;
                        }
                }
            }
            if (stackBuffer.TryPop(out var result))
                return result.Content;

            return string.Empty;

            string FormatLocal(string s)
            {
                if (s.Length > 3 && s[^3] == '_' && s[^2] == 'U' && (s[^1] == 'K' || s[^1] == 'S'))
                    return writer.FormatSubscript(writer.FormatUnits(s[..^3]), " " + s[^2..]);
                else
                    return writer.FormatUnits(s);
            }
        }

        private static bool IsNegative(Token t) =>
            t.Order == Token.DefaultOrder && t.Content.Length > 0 && t.Content[0] == '-';
    }
}