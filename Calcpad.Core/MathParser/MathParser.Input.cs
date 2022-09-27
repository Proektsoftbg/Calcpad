using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

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
            private readonly StringBuilder _stringBuilder;

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
                _stringBuilder = parser._stringBuilder;
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
                '≡' or '≠' or '≤' or '≥' or '÷' => TokenTypes.Operator,
                '°' or '′' or '″' or '‴' or '⁗' or 'ø' or 'Ø' or '∡' => TokenTypes.Unit,
                _ => TokenTypes.Error,
            };

            internal Queue<Token> GetInput(string expression, bool AllowAssignment)
            {
                var tokens = new Queue<Token>(expression.Length);
                var pt = TokenTypes.None;
                var st = SolveBlock.SolverTypes.None;
                var tokenLiteral = string.Empty;
                var unitsLiteral = string.Empty;
                var isSolver = false;
                var isDivision = false;
                var isUnitDivision = false;
                var bracketCounter = 0;
                var s = expression.Split('|');
                Token t = null;
                _parser._targetUnits = s.Length == 2 ? UnitsParser.Parse(s[1]) : null;
                _parser._hasVariables = false;
                _parser._assignmentIndex = 0;
                int MultOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['*']];
                expression = s[0] + ' ';
                for (int i = 0, len = expression.Length; i < len; ++i)
                {
                    var c = expression[i];
                    var tt = GetCharType(c); //Get the type from predefined array
                    if (tt == TokenTypes.Solver && !isSolver)
                    {
                        if (tokenLiteral.Length > 0)
#if BG
                            throw new MathParserException($"Невалидeн идентификатор на макро '{tokenLiteral}$'.");
#else
                            throw new MathParserException($"Invalid macro identifier: '{tokenLiteral}$'.");
#endif
                        _stringBuilder.Clear();
                        isSolver = true;
                    }

                    if (isSolver)
                    {
                        switch (c)
                        {
                            case '{':
                                if (bracketCounter == 0)
                                {
                                    st = SolveBlock.GetSolverType(_stringBuilder.ToString());
                                    if (st == SolveBlock.SolverTypes.Error)
#if BG
                                        throw new MathParserException($"Невалидна дефиниция на команда за числени методи \"{_stringBuilder}\".");
#else
                                        throw new MathParserException($"Invalid solver command definition \"{_stringBuilder}\".");
#endif
                                    _stringBuilder.Clear();
                                }
                                else
                                    _stringBuilder.Append(c);

                                ++bracketCounter;
                                break;
                            case '}':
                                --bracketCounter;
                                if (bracketCounter == 0)
                                {
                                    t = new Token(string.Empty, TokenTypes.Solver)
                                    {
                                        Index = AddSolver(_stringBuilder.ToString(), st)
                                    };
                                    tokens.Enqueue(t);
                                    st = SolveBlock.SolverTypes.None;
                                    isSolver = false;
                                }
                                else
                                    _stringBuilder.Append(c);

                                break;
                            default:
                                _stringBuilder.Append(c);
                                break;
                        }
                    }
                    else
                    {
                        if (tt == TokenTypes.Error)
#if BG
                            throw new MathParserException($"Невалиден символ '{c}'.");
#else
                            throw new MathParserException($"Invalid symbol '{c}'.");
#endif
                        if (tt == TokenTypes.Constant &&
                            string.IsNullOrEmpty(unitsLiteral) ||
                            tt == TokenTypes.Unit ||
                            tt == TokenTypes.Variable)
                        {
                            if (pt == TokenTypes.Unit || pt == TokenTypes.Variable)
                            {
                                tokenLiteral += c;
                                if (tokenLiteral.StartsWith('°'))
                                    tt = TokenTypes.Unit;
                                else
                                    tt = TokenTypes.Variable;
                            }
                            else if (_parser._settings.IsComplex &&
                                c == 'i' &&
                                pt == TokenTypes.Constant
                                && string.IsNullOrEmpty(unitsLiteral))
                            {
                                var j = i + 1;
                                //If we have inches in complex mode
                                if (j < len && expression[j] == 'n')
                                {
                                    unitsLiteral += c;
                                    tt = TokenTypes.Constant;
                                }
                                else
                                {
                                    t = MakeValueToken(tokenLiteral + 'i', string.Empty);
                                    tokens.Enqueue(t);
                                    tokenLiteral = string.Empty;
                                }
                            }
                            else if (pt == TokenTypes.Constant && tt == TokenTypes.Unit)
                            {
                                unitsLiteral += c;
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
                                tokenLiteral += c;
                            }
                        }
                        else
                        {
                            if (tokenLiteral.Length != 0 || unitsLiteral.Length != 0)
                            {
                                if (pt == TokenTypes.Constant)
                                {
                                    if (isDivision && unitsLiteral.Length != 0)
                                    {
                                        isUnitDivision = true;
                                        tokens.Enqueue(new Token('(', TokenTypes.BracketLeft));
                                    }
                                    t = MakeValueToken(tokenLiteral, string.Empty);
                                    tokens.Enqueue(t);
                                    if (unitsLiteral.Length != 0)
                                    {
                                        tokens.Enqueue(new Token("*", TokenTypes.Operator, MultOrder - 1));
                                        t = MakeValueToken(null, unitsLiteral);
                                        tokens.Enqueue(t);
                                        unitsLiteral = string.Empty;
                                    }
                                }
                                else
                                {
                                    if (tt == TokenTypes.BracketLeft)
                                    {
                                        if (Calculator.IsFunction(tokenLiteral))
                                            t = new Token(tokenLiteral, TokenTypes.Function)
                                            {
                                                Index = Calculator.FunctionIndex[tokenLiteral]
                                            };
                                        else if (Calculator.IsFunction2(tokenLiteral))
                                            t = new Token(tokenLiteral, TokenTypes.Function2)
                                            {
                                                Index = Calculator.Function2Index[tokenLiteral]
                                            };
                                        else if (Calculator.IsMultiFunction(tokenLiteral))
                                            t = new FunctionToken(tokenLiteral)
                                            {
                                                Index = Calculator.MultiFunctionIndex[tokenLiteral]
                                            };
                                        else if (tokenLiteral.Equals("if", StringComparison.OrdinalIgnoreCase))
                                            t = new Token(tokenLiteral, TokenTypes.If);
                                        else
                                        {
                                            var index = _functions.IndexOf(tokenLiteral);
                                            if (index < 0 && tokens.Any())
#if BG
                                                throw new MathParserException($"Невалидна функция: \"{tokenLiteral}\".");
#else
                                                throw new MathParserException($"Invalid function: \"{tokenLiteral}\".");
#endif
                                            t = new Token(tokenLiteral, TokenTypes.CustomFunction)
                                            {
                                                Index = index
                                            };
                                        }
                                        tokens.Enqueue(t);
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
                                            if (!string.IsNullOrEmpty(tokenLiteral))
                                                t = MakeValueToken(null, tokenLiteral);
                                        }
                                        else
                                        {
                                            if (!_variables.TryGetValue(tokenLiteral, out var v))
                                            {
                                                v = new Variable();
                                                _variables.Add(tokenLiteral, v);
                                            }
                                            t = new VariableToken(tokenLiteral, v);
                                        }
                                        tokens.Enqueue(t);
                                    }
                                }
                                tokenLiteral = string.Empty;
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
                                    t = new Token(NegateChar, TokenTypes.Operator)
                                    {
                                        Index = Calculator.FunctionIndex[NegateString]
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

                                    t = new Token(c, tt);
                                }
                                tokens.Enqueue(t);
                            }
                        }
                    }
                    pt = tt;
                }

                if (isUnitDivision)
                    tokens.Enqueue(new Token(')', TokenTypes.BracketRight));

                if (!isSolver)
                    return tokens;

                if (st == SolveBlock.SolverTypes.None)
#if BG
                    throw new MathParserException("Липсва лява фигурна скоба '{' в команда за числени методи.");
#else
                    throw new MathParserException("Missing left bracket '{' in solver command.");
#endif
#if BG
                throw new MathParserException("Липсва дясна фигурна скоба '}' в команда за числени методи.");
#else
                throw new MathParserException("Missing right bracket '}' in solver command.");
#endif
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
                        var c = pt.Content[0];
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

            internal Token[] GetRpn(Queue<Token> input)
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
                            output.Enqueue(t);
                            break;
                        case TokenTypes.Input:
                            var s = _parser.GetInputField();
                            if (s == "?")
                            {
                                output.Enqueue(t);
                                break;
                            }
                            if (_parser.IsEnabled)
                            {
                                if (!double.TryParse(s.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
#if BG
                                    throw new MathParserException($"Не мога да разпозная \"{s}\" като число.");
#else
                                    throw new MathParserException($"Cannot parse \"{s}\" as number.");
#endif
                                var inputToken = new ValueToken(new Value(d, ((ValueToken)t).Value.Units))
                                {
                                    Type = TokenTypes.Input,
                                    Content = s
                                };
                                output.Enqueue(inputToken);
                            }
                            else
                            {
                                t.Content = s;
                                output.Enqueue(t);
                            }
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
                _parser._isSolver = true;
                _solveBlocks.Add(new SolveBlock(script, st, _parser));
                _parser._isSolver = false;
                _parser._hasVariables = true;
                return _solveBlocks.Count - 1;
            }
        }
    }
}