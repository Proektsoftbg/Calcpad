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
            private readonly Dictionary<string, Unit> _units;
            private static readonly char[] UnitChars = Validator.UnitChars.ToCharArray();
            static Input()
            {
                // This array is needed to quickly check the token type of a character during parsing
                // Letters a-z are initially assumed to be variables unless the whole literal matches a function 

                for (int i = 0; i < 127; ++i)
                {
                    var c = (char)i;
                    if (Validator.IsDigit(c)) // 0-9 .
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
                CharTypes['['] = TokenTypes.SquareBracketLeft;
                CharTypes[']'] = TokenTypes.SquareBracketRight;
                CharTypes['|'] = TokenTypes.RowDivisor;
                CharTypes['_'] = TokenTypes.Unit;
                CharTypes[','] = TokenTypes.Unit;
                CharTypes['!'] = TokenTypes.Function;
            }

            internal Input(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _variables = parser._variables;
                _units = parser._units;
                DefineVariables();
            }

            private void DefineVariables()
            {
                string[] keys = [.. _variables.Keys];
                for (int i = 0, len = keys.Length; i < len; ++i)
                    DefinedVariables.Add(keys[i]);
            }

            private static TokenTypes GetCharType(char c) => c switch
            {
                <= '~' => CharTypes[c],
                '≡' or '≠' or
                '≤' or '≥' or
                '÷' or '⦼' or
                '∧' or '∨' or '⊕' => TokenTypes.Operator,
                >= 'Α' and <= 'Ω' or
                >= 'α' and <= 'ω' => TokenTypes.Unit,
                _ => Validator.IsVarAdditionalChar(c) ? TokenTypes.Unit :
                TokenTypes.Error,
            };

            internal Queue<Token> GetInput(ReadOnlySpan<char> expression, bool allowAssignment)
            {
                var tokens = new Queue<Token>(expression.Length);
                var pt = TokenTypes.None;
                var st = SolveBlock.SolverTypes.None;
                var isSolver = false;
                var isDivision = false;
                var isUnitDivision = false;
                var isInput = false;
                var isSubscript = false;
                var bracketCounter = 0;
                var tokenLiteral = new TextSpan(expression);
                var unitsLiteral = new TextSpan(expression);
                var textSpan = new TextSpan(expression);
                _parser._hasVariables = false;
                _parser._assignmentIndex = 0;
                Token t = null;
                var n = GetTargetUnits(expression);
                for (int i = 0; i <= n; ++i)
                {
                    var c = (i == n) ? ' ' : expression[i];
                    var tt = GetCharType(c); //Get the type from a predefined array
                    if (pt == TokenTypes.Unit || pt == TokenTypes.Variable)
                    {
                        if (c == '_')
                            isSubscript = true;
                        else if (tt == TokenTypes.Error && isSubscript && char.IsLetter(c))
                            tt = pt;
                    }
                    else
                        isSubscript = false;

                    if (!isInput && InputSolver(c, tt, ref textSpan, tokenLiteral, i))
                        continue;

                    if (InputInput(c, ref tt, ref tokenLiteral, i))
                        continue;

                    if (tt == TokenTypes.Error)
                        Throw.InvalidSymbolException(c);

                    if (tt == TokenTypes.Constant &&
                        unitsLiteral.IsEmpty ||
                        tt == TokenTypes.Unit ||
                        tt == TokenTypes.Variable)
                    {
                        if (pt == TokenTypes.Unit || pt == TokenTypes.Variable)
                        {
                            tt = tokenLiteral.StartsWithAny(UnitChars) ?
                                TokenTypes.Unit :
                                TokenTypes.Variable;

                            if (c == '.' && tt == TokenTypes.Variable)
                            {
                                var s = tokenLiteral.ToString();
                                t = MakeVectorOrMatrixToken(s);
                                //if (t is null && _parser._assignmentIndex > 0)
                                //{
                                //    t = MakeVariableToken(s);
                                //    t.Type = TokenTypes.Vector;
                                //}
                                if (t is not null)
                                {
                                    if (_parser._settings.IsComplex)
                                        Throw.ComplexVectorsAndMatricesNotSupportedException();

                                    tokens.Enqueue(t);
                                    tokenLiteral.Reset(i);
                                    tt = t.Type == TokenTypes.Vector ? TokenTypes.VectorIndex : TokenTypes.MatrixIndex;
                                    tokens.Enqueue(new Token(s, tt));
                                }
                                else
                                    tokenLiteral.Expand();
                            }
                            else
                                tokenLiteral.Expand();
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
                                t = MakeImaginaryValueToken(tokenLiteral.ToString());
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
                            if (tt == TokenTypes.Unit && !Validator.IsVarStartingChar(c))
                                Throw.InvalidCharacterException(c);

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
                                    var s = unitsLiteral.ToString();
                                    t = MakeUnitToken(s, Unit.Exists(s) || tokens.Count > 0);
                                    tokens.Enqueue(t);
                                    tokenLiteral.Reset(i);
                                    unitsLiteral.Reset(i);
                                }
                                else
                                {
                                    t = MakeRealValueToken(tokenLiteral.ToString());
                                    tokens.Enqueue(t);
                                    tokenLiteral.Reset(i);
                                    if (!unitsLiteral.IsEmpty)
                                    {
                                        var s = unitsLiteral.ToString();
                                        tokens.Enqueue(new Token("*", TokenTypes.Operator, Calculator.UnitMultOrder));
                                        t = MakeUnitToken(s, true);
                                        tokens.Enqueue(t);
                                        unitsLiteral.Reset(i);
                                    }
                                }
                            }
                            else
                            {
                                var s = tokenLiteral.ToString();
                                if (tt == TokenTypes.BracketLeft)
                                    t = MakeFunctionToken(s, tokens.Count != 0);
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
                                        tokens.Enqueue(new Token("*", TokenTypes.Operator, Calculator.UnitMultOrder));
                                        if (!tokenLiteral.IsEmpty)
                                            t = MakeUnitToken(s, true);
                                    }
                                    else if (UnitChars.Contains(s[0]))
                                        t = MakeUnitToken(s, true);
                                    else
                                        t = MakeVariableToken(s);
                                }
                                tokens.Enqueue(t);
                                tokenLiteral.Reset(i);
                            }
                        }
                        if (tt != TokenTypes.None)
                        {
                            if (t is not null)
                                pt = t.Type;

                            if (c == '-' &&
                                (
                                    pt == TokenTypes.BracketLeft ||
                                    pt == TokenTypes.SquareBracketLeft ||
                                    pt == TokenTypes.RowDivisor ||
                                    pt == TokenTypes.Divisor ||
                                    pt == TokenTypes.Operator ||
                                    pt == TokenTypes.None)
                                )
                                t = new Token(NegateChar.ToString(), TokenTypes.Operator)
                                {
                                    Index = Calculator.FunctionIndex[NegateString]
                                };
                            else if (c == '!')
                            {
                                if (pt == TokenTypes.Constant || 
                                    pt == TokenTypes.BracketRight || 
                                    pt == TokenTypes.SquareBracketRight || 
                                    pt == TokenTypes.Variable)
                                    t = new Token('!', TokenTypes.Function)
                                    {
                                        Index = Calculator.FunctionIndex["fact"]
                                    };
                                else
                                    Throw.MissingOperandException();

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
                                        if (!allowAssignment || _parser._assignmentIndex > 0)
                                            Throw.ImproperAssignmentException();

                                        int count = tokens.Count;
                                        if (count == 1)
                                            DefinedVariables.Add(tokens.Peek().Content);

                                        _parser._assignmentIndex = count;
                                    }
                                }
                                else if (tt == TokenTypes.Divisor ||
                                            tt == TokenTypes.BracketLeft ||
                                            tt == TokenTypes.BracketRight ||
                                            tt == TokenTypes.SquareBracketLeft ||
                                            tt == TokenTypes.SquareBracketRight ||
                                            tt == TokenTypes.RowDivisor)
                                    isDivision = false;

                                if (tt == TokenTypes.SquareBracketLeft && _parser._settings.IsComplex)
                                    Throw.ComplexVectorsAndMatricesNotSupportedException();

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
                    Throw.MissingLeftSolverBracketException();

                Throw.MissingRightSolverBracketException();
                return null;

                bool InputSolver(char c, TokenTypes tt, ref TextSpan ts, TextSpan tokenLiteral, int i)
                {
                    if (tt == TokenTypes.Solver && !isSolver)
                    {
                        if (!tokenLiteral.IsEmpty)
                            Throw.InvalidMacroException(tokenLiteral.ToString());

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
                                        Throw.InvalidSolverException(s.ToString());

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
                            if (_parser.IsEnabled)
                                ((ValueToken)t).Value = new Value(double.Parse(t.Content, CultureInfo.InvariantCulture));

                            tokenLiteral.Reset(i);
                            isInput = false;
                            tt = TokenTypes.Input;
                        }
                        return true;
                    }
                    if (isInput)
                    {
                        if (!_parser.IsEnabled)
                            tokenLiteral.Expand();
                        else if (c == '-' && pt == TokenTypes.Input ||
                            (tt == TokenTypes.Constant || c == 'i' && _parser._settings.IsComplex) &&
                            (pt == TokenTypes.Input || pt == TokenTypes.Constant))
                        {
                            tokenLiteral.Expand();
                            if (c == 'i')
                                pt = TokenTypes.Unit;
                        }
                        else if (c != ' ' || !tokenLiteral.IsEmpty)
                            Throw.InvalidSymbolException(c);

                        return true;
                    }
                    return false;
                }
            }

            internal int GetTargetUnits(ReadOnlySpan<char> expression)
            {
                var n = expression.LastIndexOf('|');
                if (n >= 0)
                {
                    if (expression.Length - n > 0)
                    {
                        var unit = expression[(n + 1)..];
                        if (unit.Contains(']'))
                        {
                            n = expression.Length;
                            _parser._targetUnits = null;
                        }
                        else
                            _parser._targetUnits = UnitsParser.Parse(unit.ToString(), _units);
                    }
                }
                else
                {
                    n = expression.Length;
                    _parser._targetUnits = null;
                }
                return n;
            }

            private Token MakeFunctionToken(string s, bool anyTokens)
            {
                if (Calculator.IsFunction(s))
                    return new Token(s, TokenTypes.Function)
                    {
                        Index = Calculator.FunctionIndex[s]
                    };
                if (Calculator.IsFunction2(s))
                    return new Token(s, TokenTypes.Function2)
                    {
                        Index = Calculator.Function2Index[s]
                    };
                if (Calculator.IsFunction3(s))
                    return new Token(s, TokenTypes.Function3)
                    {
                        Index = Calculator.Function3Index[s]
                    };
                if (Calculator.IsMultiFunction(s))
                    return new FunctionToken(s)
                    {
                        Index = Calculator.MultiFunctionIndex[s]
                    };
                if (Calculator.IsInterpolation(s))
                    return new FunctionToken(s)
                    {
                        Index = Calculator.InterpolationIndex[s],
                        Type = TokenTypes.Interpolation
                    };
                if (VectorCalculator.IsFunction(s))
                    return new Token(s, TokenTypes.VectorFunction)
                    {
                        Index = VectorCalculator.FunctionIndex[s]
                    };
                if (VectorCalculator.IsFunction2(s))
                    return new Token(s, TokenTypes.VectorFunction2)
                    {
                        Index = VectorCalculator.Function2Index[s]
                    };
                if (VectorCalculator.IsFunction3(s))
                    return new Token(s, TokenTypes.VectorFunction3)
                    {
                        Index = VectorCalculator.Function3Index[s]
                    };
                if (VectorCalculator.IsMultiFunction(s))
                    return new FunctionToken(s)
                    {
                        Index = VectorCalculator.MultiFunctionIndex[s],
                        Type = TokenTypes.VectorMultiFunction
                    };
                if (MatrixCalculator.IsFunction(s))
                    return new Token(s, TokenTypes.MatrixFunction)
                    {
                        Index = MatrixCalculator.FunctionIndex[s]
                    };
                if (MatrixCalculator.IsFunction2(s))
                    return new Token(s, TokenTypes.MatrixFunction2)
                    {
                        Index = MatrixCalculator.Function2Index[s]
                    };
                if (MatrixCalculator.IsFunction3(s))
                    return new Token(s, TokenTypes.MatrixFunction3)
                    {
                        Index = MatrixCalculator.Function3Index[s]
                    };
                if (MatrixCalculator.IsFunction4(s))
                    return new Token(s, TokenTypes.MatrixFunction4)
                    {
                        Index = MatrixCalculator.Function4Index[s]
                    };
                if (MatrixCalculator.IsFunction5(s))
                    return new Token(s, TokenTypes.MatrixFunction5)
                    {
                        Index = MatrixCalculator.Function5Index[s]
                    };
                if (MatrixCalculator.IsMultiFunction(s))
                    return new FunctionToken(s)
                    {
                        Index = MatrixCalculator.MultiFunctionIndex[s],
                        Type = TokenTypes.MatrixMultiFunction
                    };
                var index = _functions.IndexOf(s);
                if (index < 0 && anyTokens)
                    Throw.InvalidFunctionException(s);

                return new Token(s, TokenTypes.CustomFunction)
                {
                    Index = index
                };
            }

            private VariableToken MakeVariableToken(string s)
            {
                if (!_variables.TryGetValue(s, out var v))
                {
                    v = new Variable();
                    _variables.Add(s, v);
                }
                return new VariableToken(s, v);
            }

            private VariableToken MakeVectorOrMatrixToken(string s)
            {
                if (_variables.TryGetValue(s, out var v))
                {
                    ref var value = ref v.ValueByRef();
                    if (value is Vector)
                        return new VariableToken(s, v)
                        {
                            Type = TokenTypes.Vector
                        };
                    else if (value is Matrix)
                        return new VariableToken(s, v)
                        {
                            Type = TokenTypes.Matrix
                        };
                    else if (value is null && !_parser.IsEnabled)
                        return new VariableToken(s, null)
                        {
                            Type = TokenTypes.Vector
                        };
                }
                return null;
            }

            private ValueToken MakeUnitToken(string s, bool force)
            {
                if (_units.TryGetValue(s, out var u) && u is not null)
                {
                    return new ValueToken(new(u))
                    {
                        Content = s,
                        Type = TokenTypes.Unit
                    };
                }
                else if (force)
                    return MakeUnitValueToken(s);
                else
                {
                    _units.Add(s, null);
                    return new ValueToken(new())
                    {
                        Content = s,
                        Type = TokenTypes.Unit
                    };
                }
            }

            private static ValueToken MakeUnitValueToken(string units)
            {
                try
                {
                    var unit = Unit.Get(units);
                    var v = new Value(unit);
                    return new ValueToken(v)
                    {
                        Content = "<i>" + units + "</i>",
                        Type = TokenTypes.Unit
                    };
                }
                catch
                {
                    Throw.ErrorParsingUnitsException(units);
                    return null;
                }
            }

            private static ValueToken MakeRealValueToken(string value)
            {
                var number = NumberParser.Parse(value);
                return new ValueToken(new Value(number))
                {
                    Content = value,
                    Type = TokenTypes.Constant
                };
            }

            private static ValueToken MakeImaginaryValueToken(string value)
            {
                var number = NumberParser.Parse(value);
                return new ValueToken(new Value(0d, number, null))
                {
                    Content = value,
                    Type = TokenTypes.Constant
                };
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
                    if (!isDefinition &&
                        i > assignmentIndex &&
                        t.Type == TokenTypes.Variable &&
                        !DefinedVariables.Contains(t.Content) &&
                        Unit.TryGet(t.Content, out var u)
                    )
                    {
                        t.Type = TokenTypes.Unit;
                        var v = ((VariableToken)t).Variable;
                        v.SetValue(u);
                    }
                    if (t.Type == TokenTypes.Unit)
                    {
                        if (isUnit)
                        {
                            var c = pt?.Content[0];
                            if (c == '*' || c == '/' || c == '÷')
                                pt.Order = (sbyte)(Calculator.UnitMultOrder - 1);
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
                Span<int> vectorStack = stackalloc int[10];
                Span<int> matrixStack = stackalloc int[10];
                Span<int> functionStack = stackalloc int[20];
                int tovs = 0, tofs = 0;
                foreach (var t in input)
                    switch (t.Type)
                    {
                        case TokenTypes.Constant:
                        case TokenTypes.Unit:
                        case TokenTypes.Variable:
                        case TokenTypes.Vector:
                        case TokenTypes.Matrix:
                        case TokenTypes.Solver:
                        case TokenTypes.Input:
                            output.Enqueue(t);
                            break;
                        case TokenTypes.VectorIndex:
                        case TokenTypes.MatrixIndex:
                        case TokenTypes.Operator:
                            if (t.Content != NegateString)
                                while (stackBuffer.Count != 0)
                                {
                                    var next = stackBuffer.Peek();
                                    if (next.Type == TokenTypes.Operator &&
                                       (next.Order > t.Order ||
                                        next.Order == t.Order && t.Content == "^") ||
                                        (next.Type == TokenTypes.VectorIndex || next.Type == TokenTypes.MatrixIndex) &&
                                        (t.Type == TokenTypes.VectorIndex || t.Type == TokenTypes.MatrixIndex) ||
                                        next.Type == TokenTypes.BracketLeft ||
                                        next.Type == TokenTypes.SquareBracketLeft)
                                        break;
                                    stackBuffer.Pop();
                                    output.Enqueue(next);
                                }

                            stackBuffer.Push(t);
                            break;
                        case TokenTypes.Function:
                        case TokenTypes.VectorFunction:
                        case TokenTypes.MatrixFunction:
                            if (t.Content == "!")
                            {
                                while (stackBuffer.Count != 0)
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
                        case TokenTypes.Function3:
                        case TokenTypes.MultiFunction:
                        case TokenTypes.Interpolation:
                        case TokenTypes.VectorFunction2:
                        case TokenTypes.VectorFunction3:
                        case TokenTypes.VectorMultiFunction:
                        case TokenTypes.MatrixFunction2:
                        case TokenTypes.MatrixFunction3:
                        case TokenTypes.MatrixFunction4:
                        case TokenTypes.MatrixFunction5:
                        case TokenTypes.MatrixMultiFunction:
                        case TokenTypes.CustomFunction:
                            stackBuffer.Push(t);
                            break;
                        case TokenTypes.BracketLeft:
                            functionStack[++tofs] = vectorStack[tovs];
                            stackBuffer.Push(t);
                            break;
                        case TokenTypes.SquareBracketLeft:
                            vectorStack[++tovs] = 1;
                            matrixStack[tovs] = 1;
                            stackBuffer.Push(t);
                            break;
                        case TokenTypes.BracketRight:
                        case TokenTypes.SquareBracketRight:
                        case TokenTypes.Divisor:
                        case TokenTypes.RowDivisor:
                            while (stackBuffer.Count != 0)
                            {
                                var next = stackBuffer.Peek();
                                if (next.Type == TokenTypes.BracketLeft)
                                {
                                    if (t.Type == TokenTypes.BracketRight)
                                        stackBuffer.Pop();

                                    break;
                                }
                                else if (next.Type == TokenTypes.SquareBracketLeft)
                                {
                                    if (t.Type == TokenTypes.SquareBracketRight ||
                                        t.Type == TokenTypes.RowDivisor)
                                    {
                                        var vt = new VectorToken(null, new Vector(vectorStack[tovs]))
                                        {
                                            Index = vectorStack[tovs]
                                        };
                                        output.Enqueue(vt);
                                        if (t.Type == TokenTypes.SquareBracketRight)
                                        {
                                            stackBuffer.Pop();
                                            if (matrixStack[tovs] > 1)
                                            {
                                                var mt = new MatrixToken(null, null)
                                                {
                                                    Index = matrixStack[tovs] * Vector.MaxLength
                                                };
                                                output.Enqueue(mt);
                                                vt.Type = TokenTypes.RowDivisor;
                                            }
                                        }
                                        else
                                        {
                                            vt.Type = TokenTypes.RowDivisor;
                                            vectorStack[tovs] = 1;
                                            ++matrixStack[tovs];
                                        }
                                    }
                                    break;
                                }
                                output.Enqueue(stackBuffer.Pop());
                            }
                            if (t.Type == TokenTypes.BracketRight)
                                vectorStack[tovs] = functionStack[tofs--];
                            else if (t.Type == TokenTypes.Divisor)
                                ++vectorStack[tovs];
                            break;

                    }

                while (stackBuffer.Count != 0)
                    output.Enqueue(stackBuffer.Pop());

                return [.. output];
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