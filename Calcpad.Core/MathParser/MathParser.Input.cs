using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class Input
        {
            internal readonly HashSet<string> DefinedVariables = new(StringComparer.Ordinal);
            private static readonly TokenTypes[] CharTypes = new TokenTypes[127];
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolveBlock> _solveBlocks;
            private readonly Dictionary<string, Variable> _variables;

            static Input()
            {
                // This array is needed to quickly check the token type of a character during parsing
                // Letters a-z are initially assumed to be variables unless the whole literal matches a function 

                for (int i = 0; i < 127; ++i)
                {
                    var c = (char)i;
                    if (Validator.IsDigit(c)) // 0-9
                        CharTypes[i] = TokenTypes.Constant;
                    else if (Validator.IsLetter(c))
                        CharTypes[i] = TokenTypes.Unit;
                    else if (Calculator.IsOperator(c))
                        CharTypes[i] = TokenTypes.Operator;
                    else
                        CharTypes[i] = TokenTypes.Error;
                }

                CharTypes[DecimalSymbol] = TokenTypes.Constant;
                CharTypes['\t'] = TokenTypes.None;
                CharTypes[' '] = TokenTypes.None;
                CharTypes['$'] = TokenTypes.Solver;
                CharTypes['?'] = TokenTypes.Input;
                CharTypes[';'] = TokenTypes.Divisor;
                CharTypes['('] = TokenTypes.BracketLeft;
                CharTypes[')'] = TokenTypes.BracketRight;
                CharTypes['_'] = TokenTypes.Unit;
                CharTypes[','] = TokenTypes.Variable;
                CharTypes['\''] = TokenTypes.Comment;
                CharTypes['\"'] = TokenTypes.Comment;
                CharTypes['!'] = TokenTypes.Function;
            }

            internal Input(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _variables = parser._variables;
                DefineVariables();
            }

            private void DefineVariables()
            {
                string[] keys = _variables.Keys.ToArray();
                for (int i = 0, len = keys.Length; i < len; ++i)
                    DefinedVariables.Add(keys[i]);
            }

            private static TokenTypes GetCharType(char c) => c switch
            {
                <= '~' => CharTypes[c],
                >= 'Α' and <= 'Ω' or >= 'α' and <= 'ω' or 'ϕ' or 'ϑ' or '℧' => TokenTypes.Unit,
                '⦼' or '≡' or '≠' or '≤' or '≥' or '÷' or '∧' or '∨' or '⊕' => TokenTypes.Operator,
                '%' or '‰' or '°' or '′' or '″' or '‴' or '⁗' or 'ø' or 'Ø' or '∡' => TokenTypes.Unit,
                _ => TokenTypes.Error,
            };

            internal Queue<Token> GetInput(ReadOnlySpan<char> expression, bool AllowAssignment)
            {
                var tokens = new Queue<Token>(expression.Length);
                var pt = TokenTypes.None;
                var st = SolveBlock.SolverTypes.None;
                var isSolver = false;
                var isDivision = false;
                var isUnitDivision = false;
                var isInput = false;
                var bracketCounter = 0;
                var n = expression.IndexOf('|');
                var tokenLiteral = new TextSpan(expression);
                var unitsLiteral = new TextSpan(expression);
                var textSpan = new TextSpan(expression);
                if (n >= 0)
                {
                    if (expression.Length - n > 0)
                    {
                        var unit = expression[(n + 1)..].ToString();
                        _parser._targetUnits = UnitsParser.Parse(unit);
                    }
                }
                else
                {
                    n = expression.Length;
                    _parser._targetUnits = null;
                }

                _parser._hasVariables = false;
                _parser._assignmentIndex = 0;
                Token t = null;
                int MultOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['*']];
                for (int i = 0; i <= n; ++i)
                {
                    var c = (i == n) ? ' ' : expression[i];
                    var tt = GetCharType(c); //Get the type from a predefined array
                    if (c == '%' && i > 0 && expression[i - 1] == '.')
                        tt = TokenTypes.Unit;

                    if (!isInput && InputSolver(c, tt, ref textSpan, tokenLiteral, i))
                        continue;

                    if (InputInput(c, ref tt, ref tokenLiteral, i))
                        continue;

                    if (tt == TokenTypes.Error)
#if BG
                        throw new MathParserException($"Невалиден символ '{c}'.");
#else
                        throw new MathParserException($"Invalid symbol '{c}'.");
#endif
                    if (tt == TokenTypes.Constant &&
                        unitsLiteral.IsEmpty ||
                        tt == TokenTypes.Unit ||
                        tt == TokenTypes.Variable)
                    {
                        if (pt == TokenTypes.Unit || pt == TokenTypes.Variable)
                        {
                            tokenLiteral.Expand();
                            if (tokenLiteral.StartsWith('°'))
                                tt = TokenTypes.Unit;
                            else
                                tt = TokenTypes.Variable;
                        }
                        else if (c == 'i' &&
                            _parser._settings.IsComplex &&
                            pt == TokenTypes.Constant
                            && unitsLiteral.IsEmpty)
                        {
                            var j = i + 1;
                            //If we have inches in complex mode
                            if (j < n && expression[j] == 'n')
                            {
                                unitsLiteral.Reset(i);
                                unitsLiteral.Expand();
                                tt = TokenTypes.Constant;
                            }
                            else
                            {
                                t = MakeValueToken(tokenLiteral.ToString() + 'i', string.Empty);
                                tokens.Enqueue(t);
                                tokenLiteral.Reset(i);
                            }
                        }
                        else if (pt == TokenTypes.Constant && tt == TokenTypes.Unit)
                        {
                            if (unitsLiteral.IsEmpty)
                                unitsLiteral.Reset(i);

                            unitsLiteral.Expand();
                            tt = TokenTypes.Constant;
                        }
                        else
                        {
                            if (tt == TokenTypes.Unit && !(char.IsLetter(c) || c == '°' || c == '∡' || c == '℧'))
#if BG
                                throw new MathParserException($"Невалиден символ: '{c}'. Имената на променливи, функции и мерни единици трябва да започват с буква, ∡ или '°' за градуси.");
#else
                                throw new MathParserException($"Invalid character: '{c}'. Variables, functions and units must begin with a letter, ∡ or '°' for degrees.");
#endif
                            if (tt != pt)
                                tokenLiteral.Reset(i);

                            tokenLiteral.Expand();
                        }
                    }
                    else
                    {
                        if (!(tokenLiteral.IsEmpty && unitsLiteral.IsEmpty))
                        {
                            if (pt == TokenTypes.Constant)
                            {
                                if (isDivision && !unitsLiteral.IsEmpty)
                                {
                                    isUnitDivision = true;
                                    tokens.Enqueue(new Token('(', TokenTypes.BracketLeft));
                                }
                                if (tokenLiteral.Equals(".") && !unitsLiteral.IsEmpty)
                                {
                                    t = MakeValueToken(null, unitsLiteral.ToString());
                                    tokens.Enqueue(t);
                                    tokenLiteral.Reset(i);
                                    unitsLiteral.Reset(i);
                                }
                                else
                                {
                                    t = MakeValueToken(tokenLiteral.ToString(), string.Empty);
                                    tokens.Enqueue(t);
                                    tokenLiteral.Reset(i);
                                    if (!unitsLiteral.IsEmpty)
                                    {
                                        tokens.Enqueue(new Token("*", TokenTypes.Operator, MultOrder - 1));
                                        t = MakeValueToken(null, unitsLiteral.ToString());
                                        tokens.Enqueue(t);
                                        unitsLiteral.Reset(i);
                                    }
                                }
                            }
                            else
                            {
                                var s = tokenLiteral.ToString();
                                if (tt == TokenTypes.BracketLeft)
                                {
                                    if (Calculator.IsFunction(s))
                                        t = new Token(s, TokenTypes.Function)
                                        {
                                            Index = Calculator.FunctionIndex[s]
                                        };
                                    else if (Calculator.IsFunction2(s))
                                        t = new Token(s, TokenTypes.Function2)
                                        {
                                            Index = Calculator.Function2Index[s]
                                        };
                                    else if (Calculator.IsMultiFunction(s))
                                        t = new FunctionToken(s)
                                        {
                                            Index = Calculator.MultiFunctionIndex[s]
                                        };
                                    else if (s.Equals("if", StringComparison.Ordinal))
                                        t = new Token(s, TokenTypes.If);
                                    else
                                    {
                                        var index = _functions.IndexOf(s);
                                        if (index < 0 && tokens.Any())
#if BG
                                            throw new MathParserException($"Невалидна функция: \"{s}\".");
#else
                                            throw new MathParserException($"Invalid function: \"{s}\".");
#endif
                                        t = new Token(s, TokenTypes.CustomFunction)
                                        {
                                            Index = index
                                        };
                                    }
                                }
                                else
                                {
                                    if (t is not null && (
                                        t.Type == TokenTypes.Input ||
                                        t.Type == TokenTypes.Constant
                                        ))
                                    {
                                        if (isDivision)
                                        {
                                            t = tokens.Last();
                                            tokens.Enqueue(new ValueToken(Value.Zero)
                                            {
                                                Content = t.Content,
                                                Type = t.Type
                                            });
                                            t.Content = "(";
                                            t.Type = TokenTypes.BracketLeft;
                                            isUnitDivision = true;
                                        }
                                        tokens.Enqueue(new Token("*", TokenTypes.Operator, MultOrder - 1));
                                        if (!tokenLiteral.IsEmpty)
                                            t = MakeValueToken(null, s);
                                    }
                                    else
                                    {
                                        if (!_variables.TryGetValue(s, out var v))
                                        {
                                            v = new Variable();
                                            _variables.Add(s, v);
                                        }
                                        t = new VariableToken(s, v);
                                    }

                                }
                                tokens.Enqueue(t);
                                tokenLiteral.Reset(i);
                            }
                        }
                        if (tt == TokenTypes.Comment)
                            break;

                        if (tt != TokenTypes.None)
                        {
                            if (t is not null)
                                pt = t.Type;

                            if (c == '-' && (pt == TokenTypes.BracketLeft ||
                                                pt == TokenTypes.Divisor ||
                                                pt == TokenTypes.Operator ||
                                                pt == TokenTypes.None))
                                t = new Token(NegateChar.ToString(), TokenTypes.Operator)
                                {
                                    Index = Calculator.FunctionIndex[NegateString.ToString()]
                                };
                            else if (c == '!')
                            {
                                if (pt == TokenTypes.Constant || pt == TokenTypes.BracketRight || pt == TokenTypes.Variable)
                                    t = new Token('!', TokenTypes.Function)
                                    {
                                        Index = Calculator.FunctionIndex["fact"]
                                    };
                                else
#if BG
                                    throw new MathParserException($"Липсва операнд 'n!'.");
#else
                                    throw new MathParserException($"Missing operand 'n!'.");
#endif
                            }
                            else if (tt == TokenTypes.Input)
                                t = new ValueToken(Value.Zero)
                                {
                                    Index = _parser.Line,
                                    Type = TokenTypes.Input,
                                    Content = c.ToString()
                                };
                            else
                            {
                                if (isUnitDivision &&
                                    (
                                        tt == TokenTypes.Operator &&
                                        c != '^' &&
                                        c != NegateChar
                                        || c == ')'
                                        || c == ';'))
                                {
                                    isUnitDivision = false;
                                    tokens.Enqueue(new Token(')', TokenTypes.BracketRight));
                                }
                                if (tt == TokenTypes.Operator)
                                {
                                    isDivision = c == '/' || c == '÷';
                                    if (c == '=')
                                    {
                                        if (!AllowAssignment || _parser._assignmentIndex > 0)
                                        {
#if BG
                                            throw new MathParserException($"Неправилно използване на оператора за присвояване '='.");
#else
                                            throw new MathParserException($"Improper use of the assignment operator '='.");
#endif
                                        }
                                        int count = tokens.Count;
                                        if (count == 1)
                                            DefinedVariables.Add(tokens.Peek().Content);

                                        _parser._assignmentIndex = count;
                                    }
                                }
                                else if (tt == TokenTypes.Divisor ||
                                            tt == TokenTypes.BracketLeft ||
                                            tt == TokenTypes.BracketRight)
                                    isDivision = false;

                                t = new Token(c.ToString(), tt);
                            }
                            tokens.Enqueue(t);
                        }
                    }

                    if (pt != TokenTypes.Input || tt != TokenTypes.None)
                        pt = tt;
                }

                if (isUnitDivision)
                    tokens.Enqueue(new Token(')', TokenTypes.BracketRight));

                if (!isSolver)
                    return tokens;

                if (st == SolveBlock.SolverTypes.None)
#if BG
                    throw new MathParserException("Липсва лява фигурна скоба '{' в команда за числени методи.");

                throw new MathParserException("Липсва дясна фигурна скоба '}' в команда за числени методи.");
#else
                    throw new MathParserException("Missing left bracket '{' in solver command.");

                throw new MathParserException("Missing right bracket '}' in solver command.");
#endif

                bool InputSolver(char c, TokenTypes tt, ref TextSpan ts, TextSpan tokenLiteral, int i)
                {
                    if (tt == TokenTypes.Solver && !isSolver)
                    {
                        if (!tokenLiteral.IsEmpty)
#if BG
                            throw new MathParserException($"Невалидeн идентификатор на макро '{tokenLiteral.ToString()}$'.");
#else
                            throw new MathParserException($"Invalid macro identifier: '{tokenLiteral.ToString()}$'.");
#endif
                        ts.Reset(i);
                        isSolver = true;
                    }
                    if (isSolver)
                    {
                        switch (c)
                        {
                            case '{':
                                if (bracketCounter == 0)
                                {
                                    var s = ts.Cut();
                                    st = SolveBlock.GetSolverType(s);
                                    if (st == SolveBlock.SolverTypes.Error)
#if BG
                                        throw new MathParserException($"Невалидна дефиниция на команда за числени методи \"{s}\".");
#else
                                        throw new MathParserException($"Invalid solver command definition \"{s}\".");
#endif
                                    ts.Reset(i + 1);
                                }
                                else
                                    ts.Expand();

                                ++bracketCounter;
                                break;
                            case '}':
                                --bracketCounter;
                                if (bracketCounter == 0)
                                {
                                    t = new Token(string.Empty, TokenTypes.Solver)
                                    {
                                        Index = AddSolver(ts.ToString(), st)
                                    };
                                    tokens.Enqueue(t);
                                    st = SolveBlock.SolverTypes.None;
                                    isSolver = false;
                                }
                                else
                                    ts.Expand();

                                break;
                            default:
                                ts.Expand();
                                break;
                        }
                        return true;
                    }
                    return false;
                }

                bool InputInput(char c, ref TokenTypes tt, ref TextSpan tokenLiteral, int i)
                {
                    if (c == '{' && pt == TokenTypes.Input)
                    {
                        tokenLiteral.Reset(i + 1);
                        isInput = true;
                        tt = TokenTypes.Constant;
                        return true;
                    }

                    if (c == '}' && isInput)
                    {
                        if (!tokenLiteral.IsEmpty)
                        {
                            t.Content = tokenLiteral.ToString();
                            ((ValueToken)t).Value = new Value(double.Parse(t.Content, CultureInfo.InvariantCulture));
                            tokenLiteral.Reset(i);
                            isInput = false;
                            tt = TokenTypes.Input;
                        }
                        return true;
                    }
                    if (isInput)
                    {
                        if (c == '-' && pt == TokenTypes.Input ||
                            (tt == TokenTypes.Constant || c == 'i' && _parser._settings.IsComplex) &&
                            (pt == TokenTypes.Input || pt == TokenTypes.Constant))
                        {
                            tokenLiteral.Expand();
                            if (c == 'i')
                                pt = TokenTypes.Unit;
                        }
                        else if (c != ' ' || !tokenLiteral.IsEmpty)
#if BG
                            throw new MathParserException($"Невалиден символ '{c}'.");
#else
                            throw new MathParserException($"Invalid symbol '{c}'.");
#endif
                        return true;
                    }
                    return false;
                }
            }

            private static ValueToken MakeValueToken(string value, string units)
            {
                Complex number;
                var tt = TokenTypes.Constant;
                var hasUnits = !string.IsNullOrWhiteSpace(units);
                var isUnit = hasUnits && string.IsNullOrWhiteSpace(value);
                if (isUnit)
                {
                    number = Complex.One;
                    tt = TokenTypes.Unit;
                }
                else
                {
                    try
                    {
                        number = NumberParser.Parse(value);
                    }
                    catch
                    {
#if BG
                        throw new MathParserException($"Грешка при опит за разпознаване на \"{value}\" като число.");
#else
                        throw new MathParserException($"Error parsing \"{value}\" as number.");
#endif
                    }
                }

                if (!hasUnits)
                    return new ValueToken(new Value(number))
                    {
                        Content = value,
                        Type = tt
                    };

                try
                {
                    var unit = Unit.Get(units);
                    value += "<i>" + units + "</i>";
                    var v = isUnit ? new Value(unit) : new Value(number, unit);
                    return new ValueToken(v)
                    {
                        Content = value,
                        Type = tt
                    };
                }
                catch
                {
#if BG
                    throw new MathParserException($"Грешка при опит за разпознаване на \"{units}\" като мерни единици.");
#else
                    throw new MathParserException($"Error parsing \"{units}\" as units.");
#endif
                }
            }
            internal void OrderOperators(Queue<Token> input, bool isDefinition, int assignmentIndex)
            {
                var isUnit = false;
                Token pt = null;
                var i = 0;
                foreach (var t in input)
                {
                    if (t.Type == TokenTypes.Operator)
                    {
                        if (t.Content == NegateString)
                            t.Order = 1;
                        else
                        {
                            t.Index = Calculator.OperatorIndex[t.Content[0]];
                            if (t.Order < 0)
                                t.Order = Calculator.OperatorOrder[t.Index];
                        }
                    }
                    if (!isDefinition && i > assignmentIndex && t.Type == TokenTypes.Variable)
                    {
                        if (!DefinedVariables.Contains(t.Content))
                        {
                            try
                            {
                                Variable v = ((VariableToken)t).Variable;
                                var u = Unit.Get(t.Content);
                                t.Type = TokenTypes.Unit;
                                v.SetValue(u);
                            }
                            catch
                            {
#if BG
                                //throw new MathParserException($"Недефинирана променлива или мерни единици: \"{t.Content}\".");
#else
                                //throw new MathParserException($"Undefined variable or units: \"{t.Content}\".");
#endif
                            }
                        }
                    }
                    if (t.Type == TokenTypes.Unit)
                    {
                        var c =  pt?.Content[0];
                        if (isUnit)
                        {
                            if (c == '*' || c == '/' || c == '÷')
                                pt.Order = 1;
                        }
                        else
                            isUnit = true;
                    }
                    else if (isUnit && (t.Type != TokenTypes.Constant || pt.Content[0] != '^'))
                    {
                        var c = t.Content[0];
                        if (c != '*' && c != '/' && c != '^')
                            isUnit = false;
                    }
                    pt = t;
                    ++i;
                }
            }

            internal static Token[] GetRpn(Queue<Token> input)
            {
                var output = new Queue<Token>(input.Count);
                var stackBuffer = new Stack<Token>(20);
                foreach (var t in input)
                    switch (t.Type)
                    {
                        case TokenTypes.Constant:
                        case TokenTypes.Unit:
                        case TokenTypes.Variable:
                        case TokenTypes.Solver:
                        case TokenTypes.Input:
                            output.Enqueue(t);
                            break;
                        case TokenTypes.Operator:
                            if (t.Content != NegateString)
                                while (stackBuffer.Any())
                                {
                                    var next = stackBuffer.Peek();
                                    if (next.Type == TokenTypes.Operator &&
                                        (next.Order > t.Order ||
                                        next.Order == t.Order &&
                                        t.Content == "^") ||
                                        next.Type == TokenTypes.BracketLeft)
                                        break;
                                    stackBuffer.Pop();
                                    output.Enqueue(next);
                                }
                            if (t.Order == 1 && t.Index == 4)
                                t.Order = Calculator.OperatorOrder[4];

                            stackBuffer.Push(t);
                            break;
                        case TokenTypes.Function:
                            if (t.Content == "!")
                            {
                                while (stackBuffer.Any())
                                {
                                    var next = stackBuffer.Peek();
                                    if (next.Type == TokenTypes.Operator || next.Type == TokenTypes.BracketLeft)
                                        break;
                                    stackBuffer.Pop();
                                    output.Enqueue(next);
                                }
                                output.Enqueue(t);
                            }
                            else
                                stackBuffer.Push(t);
                            break;
                        case TokenTypes.Function2:
                        case TokenTypes.MultiFunction:
                        case TokenTypes.If:
                        case TokenTypes.CustomFunction:
                            stackBuffer.Push(t);
                            break;
                        case TokenTypes.BracketLeft:
                            stackBuffer.Push(t);
                            break;
                        case TokenTypes.BracketRight:
                        case TokenTypes.Divisor:
                            while (stackBuffer.Any())
                            {
                                if (stackBuffer.Peek().Type == TokenTypes.BracketLeft)
                                {
                                    if (t.Type == TokenTypes.BracketRight)
                                        stackBuffer.Pop();

                                    break;
                                }
                                output.Enqueue(stackBuffer.Pop());
                            }
                            break;
                    }

                while (stackBuffer.Any())
                    output.Enqueue(stackBuffer.Pop());

                return output.ToArray();
            }

            private int AddSolver(string script, SolveBlock.SolverTypes st)
            {
                ++_parser._isSolver;
                _solveBlocks.Add(new SolveBlock(script, st, _parser));
                --_parser._isSolver;
                _parser._hasVariables = true;
                return _solveBlocks.Count - 1;
            }
        }
    }
}