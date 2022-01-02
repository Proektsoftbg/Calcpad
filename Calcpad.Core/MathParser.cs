using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Calcpad.Core
{
    public class MathParser
    {
        internal delegate string GetInputDelegate();
        private static readonly TokenTypes[] CharTypes = new TokenTypes[127];
        private readonly StringBuilder _stringBuilder = new();
        private readonly List<SolveBlock> _solveBlocks = new();
        private readonly Container<CustomFunction> _functions = new();
        private readonly Dictionary<string, Variable> _variables = new();
        private readonly Value[] _stackBuffer = new Value[100];
        private readonly Calculator _calc;
        private readonly MathSettings _settings;
        private readonly HashSet<string> DefinedVariables = new();
        private Token[] _rpn;
        private bool _hasVariables;
        private int _assignmentIndex;
        private bool _isCalculated;
        private bool _isPlotting;
        private bool _isSolver;
        private Unit _targetUnits;
        private int _tos;
        private int _functionDefinitionIndex;
        private double Precision
        {
            get
            {
                if (!_variables.TryGetValue("Precision", out var v)) 
                    return 1e-14;

                var precision = v.Value.Number.Re;
                return precision switch
                {
                    < 1e-16 => 1e-16,
                    > 1e-2 => 1e-2,
                    _ => precision
                };
            }
        }

        internal Solver Solver;
        internal GetInputDelegate GetInputField;
        internal Unit Units { get; private set; }
        internal bool IsCanceled { get; private set; }
        internal bool IsEnabled { get; set; } = true;

        internal bool IsPlotting
        {
            set => _isPlotting = value;
            get => _isPlotting;

        }
        internal bool Degrees { set => _calc.Degrees = value; }
        internal int PlotWidth => _variables.TryGetValue("PlotWidth", out var v) ? (int)v.Value.Number.Re : 500;
        internal int PlotHeight => _variables.TryGetValue("PlotHeight", out var v) ? (int)v.Value.Number.Re : 300;
        internal int PlotStep => _variables.TryGetValue("PlotStep", out var v) ? (int)v.Value.Number.Re : 0;

        public const char DecimalSymbol = '.'; //CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
        internal Complex Result { get; private set; }
        public double Real => Result.Re;
        public static readonly char NegateChar = Calculator.NegChar;
        public static readonly string NegateString = Calculator.NegChar.ToString();
        public static bool IsUnit(string name) => Unit.Exists(name);

        public MathParser(MathSettings settings)
        {
            _settings = settings;
            Solver = new Solver(_settings.IsComplex);
            if (_settings.IsComplex)
                _calc = new ComplexCalculator();
            else
                _calc = new RealCalculator();

            Degrees = settings.Degrees;
            InitVariables();
        }

        internal void SetVariable(string name, Value value)
        {
            if (_variables.ContainsKey(name))
                _variables[name].SetValue(value);
            else
            {
                _variables.Add(name, new Variable(value));
                DefinedVariables.Add(name);
            }
        }

        internal void Cancel() => IsCanceled = true;

        private void InitVariables()
        {
            var pi = MakeValue(Math.PI, null);
            _variables.Add("e", new Variable(MakeValue(Math.E, null)));
            _variables.Add("pi", new Variable(pi));
            _variables.Add("π", new Variable(pi));
            _variables.Add("g", new Variable(MakeValue(9.80665, null)));
            DefinedVariables.Add("e");
            DefinedVariables.Add("pi");
            DefinedVariables.Add("π");
            DefinedVariables.Add("g");
            if (_settings.IsComplex)
            {
                _variables.Add("i", new Variable(Complex.ImaginaryOne));
                _variables.Add("ei", new Variable(Math.E * Complex.ImaginaryOne));
                _variables.Add("πi", new Variable(Math.PI * Complex.ImaginaryOne));
                DefinedVariables.Add("i");
                DefinedVariables.Add("ei");
                DefinedVariables.Add("πi");
            }
        }

        static MathParser()
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

        private static TokenTypes GetCharType(char c) => c switch
            {
                <= '~' => CharTypes[c],
                >= 'Α' and <= 'Ω' or >= 'α' and <= 'ω' => TokenTypes.Unit,
                '≡' or '≠' or '≤' or '≥' or '÷' => TokenTypes.Operator,
                '°' => TokenTypes.Unit,
                _ => TokenTypes.Error,
            };

        public void Parse(string expression)
        {
            Result = Complex.Zero;
            _isCalculated = false;
            _functionDefinitionIndex = -1;
            var input = GetInput(expression);
            new Validator(_functions).Check(input, out var isFucntionDefinition);
            OrderOperators(input, isFucntionDefinition || _isSolver || _isPlotting, _assignmentIndex);
            if (isFucntionDefinition)
                AddFunction(input);
            else
            {
                _rpn = GetRpn(input);
                PurgeCache();
            }
        }

        private void PurgeCache()
        {
            for (int i = 0, count = _functions.Count; i < count; ++i)
                _functions[i].PurgeCache();
        }

        public void Calculate()
        {
            if (!IsEnabled)
#if BG
                throw new MathParserException("Изчислителното ядро не е активно.");
#else            
                throw new MathParserException("Calculations are not active.");
#endif
            if (IsCanceled)
#if BG
                throw new MathParserException("Прекъсване от потребителя.");
#else
                throw new MathParserException("Interupted by user.");
#endif
            if (_functionDefinitionIndex < 0)
            {
                Compile();
                Result = EvaluateRpn(_rpn).Number;
            }
            _isCalculated = true;
        }

        internal void Compile()
        {
            for (int i = _solveBlocks.Count - 1; i >= 0; --i)
                _solveBlocks[i].Compile(this);
        }

        public double CalculateReal()
        {
            if (IsCanceled)
#if BG
                throw new MathParserException("Прекъсване от потребителя.");
#else
                throw new MathParserException("Interupted by user."); 
#endif
            Value value = EvaluateRpn(_rpn);
            CheckReal(value);
            Result = value.Number;
            return Result.Re;
        }

        internal void CheckReal(Value value)
        {
            if (_settings.IsComplex && !value.Number.IsReal)
#if BG
                throw new MathParserException($"Резултатът не е реално число: \"{Complex.Format(value.Number, _settings.Decimals, OutputWriter.OutputFormat.Text)}\".");
#else
                throw new MathParserException($"The result is not a real number: \"{Complex.Format(value.Number, _settings.Decimals, OutputWriter.OutputFormat.Text)}\".");
#endif
        }

        private void AddFunction(Queue<Token> input)
        {
            var t = input.Dequeue();
            var parameters = new List<string>(2);
            if (t.Type == TokenTypes.CustomFunction)
            {
                var name = t.Content;
                t = input.Dequeue();
                if (t.Type == TokenTypes.BracketLeft)
                {
                    var pt = t;
                    while (input.Any())
                    {
                        t = input.Dequeue();
                        if (t.Type == TokenTypes.BracketRight)
                        {
                            if (pt.Type != TokenTypes.Variable)
#if BG
                                throw new MathParserException("Липсва параметър в дефиниция на функция.");
#else
                                throw new MathParserException("Missing parameter in function definition.");
#endif

                            break;
                        }

                        if (t.Type == TokenTypes.Variable)
                            parameters.Add(t.Content);
                        else if (t.Type != TokenTypes.Divisor)
#if BG
                            throw new MathParserException($"Невалиден обект в дефиниция на функция: \"{t.Content}\".");
#else
                            throw new MathParserException($"Invalid token in function definition: \"{t.Content}\".");
#endif
                        if (pt.Type == t.Type || pt.Type == TokenTypes.BracketLeft && t.Type == TokenTypes.Divisor)
                        {
                            if (t.Type == TokenTypes.Divisor)
#if BG
                                throw new MathParserException("Липсва параметър в дефиниция на функция.");
#else
                                throw new MathParserException("Missing parameter in function definition.");
#endif

#if BG
                            throw new MathParserException("Липсва разделител в дефиниция на функция.");
#else
                            throw new MathParserException("Missing delimiter in function definition.");
#endif
                        }
                        pt = t;
                    }
                    if (input.Any() && input.Dequeue().Content == "=")
                    {
                        var rpn = GetRpn(input);
                        var i = _functions.IndexOf(name);
                        CustomFunction cf;
                        if (i >= 0)
                            cf = _functions[i];
                        else
                            cf = new CustomFunction();

                        cf.AddParameters(parameters);
                        cf.Rpn = rpn;
                        if (IsEnabled)
                        {
                            cf.SubscribeCache(_functions);
                            BindParameters(cf.Parameters, cf.Rpn);
                        }
                        cf.Units = _targetUnits;
                        if (i >= 0)
                            cf.BeforeChange();
                        cf.Function = null;
                        _functionDefinitionIndex = _functions.Add(name, cf);
                        if (cf.CheckReqursion(null, _functions))
#if BG
                            throw new MathParserException($"Открита е циклична референция за функция \"{name}\".");
#else
                            throw new MathParserException($"Circular reference detected for function \"{name}\".");
#endif
                        return;
                    }
                }
            }
#if BG
            throw new MathParserException("Невалидна дефиниция на функция. Трябва да съответства на шаблона: \"f(x; y; z...) =\".");
#else
            throw new MathParserException("Invalid function definition. It have to match the pattern: \"f(x; y; z...) =\".");
#endif
        }

        private void BindParameters(Parameter[] parameters, Token[] rpn)
        {
            for (int i = 0, len = rpn.Length; i < len; ++i)
            {
                var t = rpn[i];
                if (t.Type == TokenTypes.Variable)
                    foreach (var p in parameters)
                    {
                        if (p is null)
                            break;

                        if (t.Content == p.Name)
                        {
                            ((VariableToken)t).Variable = p;
                            break;
                        }
                    }
                else if (t.Type == TokenTypes.Solver)
                    _solveBlocks[t.Index].BindParameters(parameters, this);
            }
        }

        private Queue<Token> GetInput(string expression)
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
            _targetUnits = s.Length == 2 ? UnitsParser.Parse(s[1]) : null;
            _hasVariables = false;
            _assignmentIndex = 0;
            int MultOrder =  Calculator.OperatorOrder[Calculator.OperatorIndex['*']];
            expression = s[0] + ' ';
            for (int i = 0, len = expression.Length; i < len; ++i)
            {
                var c = expression[i];
                var tt = GetCharType(c); //Get the type from predefined array
                if (tt == TokenTypes.Solver && !isSolver)
                {
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
                                if (st == SolveBlock.SolverTypes.Error)
#if BG
                                    throw new MathParserException($"Невалидна дефиниция на команда за числени методи \"{_stringBuilder}\".");
#else
                                    throw new MathParserException($"Invalid solver command definition \"{_stringBuilder}\".");
#endif
                                st = SolveBlock.GetSolverType(_stringBuilder.ToString());
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
                    if (tt == TokenTypes.Constant && string.IsNullOrEmpty(unitsLiteral) || tt == TokenTypes.Unit || tt == TokenTypes.Variable)
                    {
                        if (pt == TokenTypes.Unit || pt == TokenTypes.Variable)
                        {
                            tokenLiteral += c;
                            tt = TokenTypes.Variable;
                        }
                        else if (_settings.IsComplex && pt == TokenTypes.Constant && c == 'i')
                        {
                            t = MakeValueToken(tokenLiteral + 'i', string.Empty);
                            tokens.Enqueue(t);
                            tokenLiteral = string.Empty;
                        }
                        else if (pt == TokenTypes.Constant && tt == TokenTypes.Unit)
                        {
                            unitsLiteral += c;
                            tt = TokenTypes.Constant;
                        }
                        else
                        {
                            if (tt == TokenTypes.Variable)
#if BG
                                throw new MathParserException($"Невалиден символ: '{c}'. Имената на променливи, функции и мерни единици трябва да започват с буква или '°'.");
#else
                                throw new MathParserException($"Invalid character: '{c}'. Variables, functions and units must begin with letter or '°'.");
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
                                    tokens.Enqueue(new Token("*", TokenTypes.Operator, MultOrder));
                                    t = MakeValueToken(null, unitsLiteral);
                                    tokens.Enqueue(t);
                                    unitsLiteral = string.Empty;
                                }
                            }
                            else
                            {
                                if (tt == TokenTypes.BracketLeft)
                                {
                                    var functionLiteral = tokenLiteral.ToLowerInvariant();
                                    if (Calculator.IsFunction(functionLiteral))
                                        t = new Token(functionLiteral, TokenTypes.Function)
                                        {
                                            Index = Calculator.FunctionIndex[functionLiteral]
                                        };
                                    else if (Calculator.IsFunction2(functionLiteral))
                                        t = new Token(functionLiteral, TokenTypes.Function2)
                                        {
                                            Index = Calculator.Function2Index[functionLiteral]
                                        };
                                    else if (Calculator.IsMultiFunction(functionLiteral))
                                        t = new FunctionToken(functionLiteral)
                                        {
                                            Index = Calculator.MultiFunctionIndex[functionLiteral]
                                        };
                                    else if (functionLiteral == "if")
                                        t = new Token(functionLiteral, TokenTypes.If);
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
                                    if (t is not null && (t.Type == TokenTypes.Input || t.Type == TokenTypes.Constant))
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
                                        tokens.Enqueue(new Token("*", TokenTypes.Operator, MultOrder));
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
                            else if (c == '!' && (pt == TokenTypes.Constant ||
                                pt == TokenTypes.BracketRight ||
                                pt == TokenTypes.Variable))
                                t = new Token('!', TokenTypes.Function)
                                {
                                    Index = Calculator.FunctionIndex["fact"]
                                };
                            else if (tt == TokenTypes.Input)
                                t = new ValueToken(Value.Zero)
                                {
                                    Type = TokenTypes.Input,
                                    Content = c.ToString()
                                };
                            else
                            {
                                if (isUnitDivision && (tt == TokenTypes.Operator && 
                                    c != '^' && c != NegateChar || c == ')'))
                                {
                                    isUnitDivision = false;
                                    tokens.Enqueue(new Token(')', TokenTypes.BracketRight));
                                }
                                if (tt == TokenTypes.Operator)
                                {

                                    isDivision = c == '/' || c == '÷';
                                    if (c == '=')
                                    {
                                        int count = tokens.Count;
                                        if (count == 1)
                                            DefinedVariables.Add(tokens.Peek().Content);
                                        _assignmentIndex = count;
                                    }
                                }
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

        internal Complex MakeNumber(double d)
        {
            if (_settings.IsComplex)
            {
                var c = new Complex(d, 0.0);
                return c;
            }
            var a = new Complex(d);
            return a;
        }

        private Value MakeValue(double d, Unit unit)
        {
            var number = MakeNumber(d);
            return new Value(number, unit);
        }

        private ValueToken MakeValueToken(string value, string units)
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
                    number = NumberParser.Parse(value, _settings.IsComplex);
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
                return new ValueToken(new Value(number, null))
                {
                    Content = value,
                    Type = tt
                };

            try
            {
                var unit = new Unit(units);
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
        private void OrderOperators(Queue<Token> input, bool isDefinition, int assignmentIndex)
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
                            var u = new Unit(t.Content);
                            t.Type = TokenTypes.Unit;
                            v.SetValue(u);
                        }
                        catch
                        {
#if BG
                            throw new MathParserException($"Недефинирана променлива или мерни единици: \"{t.Content}\".");
#else
                            throw new MathParserException($"Undefined variable or units: \"{t.Content}\".");
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
                    {
                        if (c == '*')
                            pt.Order = 2;

                        isUnit = true;
                    }
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

        private Token[] GetRpn(Queue<Token> input)
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
                        var s = GetInputField();
                        if (s == "?")
                        {
                            output.Enqueue(t);
                            break;
                        }
                        if (IsEnabled)
                        {
                            if (!double.TryParse(s.Trim(), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var d))
#if BG
                                throw new MathParserException($"Не мога да разпозная \"{s}\" като число.");
#else
                                throw new MathParserException($"Cannot parse \"{s}\" as number.");
#endif
                            var inputToken = new ValueToken(MakeValue(d, ((ValueToken)t).Value.Units))
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

        private Value EvaluateRpn(Token[] rpn)
        {
            var tos = _tos;
            var rpnLength = rpn.Length;
            if (rpnLength < 1)
#if BG
                throw new MathParserException("Празен израз.");
#else
                throw new MathParserException("Expression is empty.");
#endif

            var i0 = 0;
            if (rpn[0].Type == TokenTypes.Variable && rpn[rpnLength - 1].Content == "=")
                i0 = 1;

            for (int i = i0; i < rpnLength; ++i)
            {
                if (_tos < tos)
#if BG
                    throw new MathParserException("Стекът е празен. Невалиден израз.");
#else
                    throw new MathParserException("Stack empty. Invalid expression.");
#endif

                var t = rpn[i];
                switch (t.Type)
                {
                    case TokenTypes.Constant:
                        _stackBuffer[++_tos] = ((ValueToken)t).Value;
                        continue;
                    case TokenTypes.Unit:
                    case TokenTypes.Variable:
                    case TokenTypes.Input:
                        _stackBuffer[++_tos] = EvaluateToken(t);
                        continue;
                    case TokenTypes.Operator:
                    case TokenTypes.Function:
                        
                        Value c;
                        if (t.Type == TokenTypes.Function || t.Content == NegateString)
                        {
                            var a = _stackBuffer[_tos--];
                            c = EvaluateToken(t, a);
                        }
                        else
                        {
                            if (_tos == tos)
#if BG
                                throw new MathParserException("Липсва операнд.");
#else
                                throw new MathParserException("Missing operand.");
#endif

                            var b = _stackBuffer[_tos--];
                            if (t.Content == "=")
                            {
                                var ta = (VariableToken)rpn[0];
                                Units = ApplyUnits(ref b, _targetUnits);
                                ta.Variable.SetValue(b);
                                return b;
                            }

                            if (_tos == tos)
                            {
                                if (t.Content != "-")
#if BG
                                    throw new MathParserException("Липсва операнд.");
#else
                                    throw new MathParserException("Missing operand.");
#endif

                                c = new Value(-b.Number, b.Units);
                            }
                            else
                            {
                                var a = _stackBuffer[_tos--];
                                c = EvaluateToken(t, a, b);
                            }
                        }
                        _stackBuffer[++_tos] = c;
                        continue;
                    case TokenTypes.If:
                        Value vFalse = _stackBuffer[_tos--],
                            vTrue = _stackBuffer[_tos--],
                            vCond = _stackBuffer[_tos--];
                        _stackBuffer[++_tos] = EvaluateIf(vCond, vTrue, vFalse);
                        continue;
                    case TokenTypes.MultiFunction:
                        var mfParamCount = ((FunctionToken)t).ParameterCount;
                        var mfParams = new Value[mfParamCount];
                        for (int j = mfParamCount - 1; j >= 0; --j)
                            mfParams[j] = _stackBuffer[_tos--];

                        _stackBuffer[++_tos] = _calc.EvaluateMultiFunction(t.Index, mfParams);
                        continue;
                    case TokenTypes.CustomFunction:
                        var cf = _functions[t.Index];
                        var cfParamCount = cf.ParameterCount;
                        var cfParams = new Value[cfParamCount];
                        for (int j = cfParamCount - 1; j >= 0; --j)
                            cfParams[j] = _stackBuffer[_tos--];

                        _stackBuffer[++_tos] = EvaluateFunction(cf, cfParams);
                        continue;
                    case TokenTypes.Solver:
                        _stackBuffer[++_tos] = EvaluateSolver(t);
                        continue;
                    default:
#if BG
                        throw new MathParserException($"Не мога да изчисля \"{t.Content}\" като \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#else
                        throw new MathParserException($"Cannot evaluate \"{t.Content}\" as \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#endif
                }
            }
            if (_tos > tos)
            {
                var v = _stackBuffer[_tos--];
                Units = ApplyUnits(ref v, _targetUnits);
                return v;
            }
            if (_tos > tos)
#if BG
                throw new MathParserException("Неосвободена памет в стека. Невалиден израз.");
#else
                throw new MathParserException("Stack memory leak. Invalid expression.");
#endif
            Units = null;
            return MakeValue(0, null);
        }

        private static Unit ApplyUnits(ref Value v, Unit u)
        {
            var vu = v.Units;
            if (u is null)
            {
                if (vu is null || !vu.IsForce) 
                    return v.Units;

                u = Unit.GetForceUnit(vu);
                if (u is null)
                    return v.Units;

                //v.Number *= vu.ConvertTo(u);
                //v.Units = u;
                v = new Value(v.Number * vu.ConvertTo(u), u);
                return v.Units;
            }
            if (!Unit.IsConsistent(vu, u))
#if BG
                throw new MathParserException($"Получените мерни единици \"{Unit.GetText(vu)}\" не съответстват на отправните \"{Unit.GetText(u)}\".");
#else
                throw new MathParserException($"The calculated units \"{Unit.GetText(vu)}\" are inconsistent with the target units \"{Unit.GetText(u)}\".");
#endif
            var delta = u.IsTemp ? GetTempUnitsDelta(vu.Text, u.Text) : 0;
            var number = v.Number * vu.ConvertTo(u) + delta;
            //v.Number = number;
            //v.Units = u;
            v = new Value(number, u);
            return v.Units;
        }

        private static double GetTempUnitsDelta(string src, string tgt) =>
            src switch
            {
                "°C" => tgt switch
                {
                    "K" => 273.15,
                    "°F" => 32.0,
                    "R" => 491.67,
                    _ => 0
                },
                "K" => tgt switch
                {
                    "°C" => -273.15,
                    "°F" => -459.67,
                    "R" => 0,
                    _ => 0
                },
                "°F" => tgt switch
                {
                    "°C" => -17.0,
                    "K" => 255.372222222222,
                    "R" => 459.67,
                    _ => 0
                },
                "R" => tgt switch
                {
                    "°C" => -273.15,
                    "°F" => -459.67,
                    "K" => 0,
                    _ => 0
                },
                _ => 0,
            };

        private int AddSolver(string script, SolveBlock.SolverTypes st)
        {
            _isSolver = true;
            _solveBlocks.Add(new SolveBlock(script, st, this));
            _isSolver = false;
            _hasVariables = true;
            return _solveBlocks.Count - 1;
        }

        private Value EvaluateSolver(Token t)
        {
            var solveBlock = _solveBlocks[t.Index];
            solveBlock.Calculate(this);
            return solveBlock.Result;
        }

        private Value EvaluateFunction(CustomFunction cf, Value[] parameters)
        {
            if (cf.IsRecursion)
                return MakeValue(double.NaN, null);

            if (cf.Function is null)
                cf.Function = CompileRpn(cf.Rpn);

            if (IsCanceled)
#if BG
                throw new MathParserException("Прекъсване от потребителя.");
#else
                throw new MathParserException("Interrupted by user.");
#endif
            var result = cf.Calculate(parameters);
            Units = ApplyUnits(ref result, cf.Units);
            return result;
        }

        private Value EvaluateToken(Token t)
        {
            if (t.Type == TokenTypes.Unit)
            {
                if (t is ValueToken vt)
                    return vt.Value;

                return ((VariableToken)t).Variable.Value;
            }

            else if (t.Type == TokenTypes.Variable)
                return EvaluateVariableToken((VariableToken)t);

            if (t.Type == TokenTypes.Input && t.Content == "?")
#if BG
                throw new MathParserException("Недефинирано поле за въвеждане.");
#else
                throw new MathParserException("Undefined input field.");
#endif
            return ((ValueToken)t).Value;
        }

        private Value EvaluateVariableToken(VariableToken t)
        {
            var v = t.Variable;
            if (v.IsInitialized)
            {
                _hasVariables = true;
                return v.Value;
            }
            try
            {
                var u = new Unit(t.Content);
                t.Type = TokenTypes.Unit;
                v.SetValue(u);
                return v.Value;
            }
            catch
            {
#if BG
                throw new MathParserException($"Недефинирана променлива или мерни единици: \"{t.Content}\".");
#else
                throw new MathParserException($"Undefined variable or units: \"{t.Content}\".");
#endif
            }
        }

        private Value EvaluateToken(Token t, Value a)
        {
            if (t.Type != TokenTypes.Function && t.Content != NegateString )
#if BG
                throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция.");
#else
                throw new MathParserException($"Error evaluating \"{t.Content}\" as function.");
#endif

            return _calc.EvaluateFunction(t.Index, a);
        }

        private Value EvaluateToken(Token t, Value a, Value b)
        {
            if (t.Type == TokenTypes.Operator)
                return _calc.EvaluateOperator(t.Index, a, b);

            if (t.Type != TokenTypes.Function2)
#if BG
                throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция или оператор.");
#else
                throw new MathParserException($"Error evaluating \"{t.Content}\" as function or operator.");
#endif

            return _calc.EvaluateFunction2(t.Index, a, b);
        }

        private static Value EvaluateIf(Value condition, Value valueIfTrue, Value valueIfFalse)
        {
            if (Math.Abs(condition.Number.Re) < 1e-8)
                return valueIfFalse;

            return valueIfTrue;
        }

        internal Func<Value> CompileRpn(string expression, Parameter[] p)
        {
            var input = GetInput(expression);
            OrderOperators(input, true, 0);
            var rpn = GetRpn(input);
            BindParameters(p, rpn);
            return CompileRpn(rpn);
        }

        private Func<Value> CompileRpn(Token[] rpn)
        {
            var rpnLength = rpn.Length;
            if (rpnLength < 1)
#if BG
                throw new MathParserException("Изразът е празен.");
#else
                throw new MathParserException("Expression is empty.");
#endif

            var stackBuffer = new Stack<Expression>();
            for (int i = 0; i < rpnLength; ++i)
            {
                var t = rpn[i];
                switch (t.Type)
                {
                    case TokenTypes.Constant:
                        stackBuffer.Push(Expression.Constant(((ValueToken)t).Value));
                        continue;
                    case TokenTypes.Unit:
                    case TokenTypes.Input:
                    case TokenTypes.Variable:
                        stackBuffer.Push(ParseToken(t));
                        continue;
                    case TokenTypes.Operator:
                    case TokenTypes.Function:
                    case TokenTypes.Function2:
                        Expression c;
                        if (t.Type == TokenTypes.Function || t.Content == NegateString)
                        {
                            var a = stackBuffer.Pop();
                            c = ParseToken(t, a);
                        }
                        else
                        {
                            if (!stackBuffer.Any())
#if BG
                                throw new MathParserException("Липсва операнд.");
#else
                                throw new MathParserException("Missing operand.");
#endif

                            var b = stackBuffer.Pop();
                            if (!stackBuffer.TryPop(out var a))
                            {
                                if (t.Content != "-")
#if BG
                                    throw new MathParserException("Липсва операнд.");
#else
                                    throw new MathParserException("Missing operand.");
#endif

                                c = Expression.Negate(b);
                            }
                            else
                                c = ParseToken(t, a, b);
                        }
                        stackBuffer.Push(c);
                        continue;
                    case TokenTypes.If:
                        Expression vFalse = stackBuffer.Pop(),
                            vTrue = stackBuffer.Pop(),
                            vCond = stackBuffer.Pop();
                        stackBuffer.Push(ParseIf(vCond, vTrue, vFalse));
                        continue;
                    case TokenTypes.MultiFunction:
                        var mfValueCount = ((FunctionToken)t).ParameterCount;
                        var mfValues = new Expression[mfValueCount];
                        for (int j = mfValueCount - 1; j >= 0; --j)
                            mfValues[j] = stackBuffer.Pop();

                        stackBuffer.Push(ParseMultiFunction(t.Index, mfValues));
                        continue;
                    case TokenTypes.CustomFunction:
                        if (t.Index < 0)
#if BG
                            throw new MathParserException($"Невалидна функция: \"{t.Content}\".");
#else
                            throw new MathParserException($"Invalid function: \"{t.Content}\".");
#endif
                        var cf = _functions[t.Index];
                        var cfValueCount = cf.ParameterCount;
                        var cfValues = new Expression[cfValueCount];
                        for (int j = cfValueCount - 1; j >= 0; --j)
                            cfValues[j] = stackBuffer.Pop();

                        stackBuffer.Push(ParseFunction(cf, cfValues));
                        continue;
                    case TokenTypes.Solver:
                        stackBuffer.Push(ParseSolver(t));
                        continue;
                    default:
#if BG
                        throw new MathParserException($"Не мога да изчисля \"{t.Content}\" като \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#else
                        throw new MathParserException($"Cannot evaluate \"{t.Content}\" as \"{t.Type.GetType().GetEnumName(t.Type)}\".");
#endif
                }
            }
            if (!stackBuffer.Any())
#if BG
                throw new MathParserException("Неосвободена памет в стека. Невалиден израз.");
#else
                throw new MathParserException("Stack memory leak. Invalid expression.");
#endif
            var result = stackBuffer.Pop();
            var lambda = Expression.Lambda<Func<Value>>(result);
            return lambda.Compile();
        }

        private static Expression ParseToken(Token t)
        {
            if (t.Type == TokenTypes.Unit)
            {
                if (t is ValueToken vt)
                    return Expression.Constant(vt.Value);

                return Expression.Constant(((VariableToken)t).Variable.Value);
            }

            if (t.Type == TokenTypes.Variable)
                return ParseVariableToken((VariableToken)t);

            if (t.Type == TokenTypes.Input && t.Content == "?")
#if BG
                throw new MathParserException("Недефинирано поле за въвеждане.");
#else
                throw new MathParserException("Undefined input field.");
#endif
            return Expression.Constant(((ValueToken)t).Value);
        }

        private static Expression ParseVariableToken(VariableToken t)
        {
            var v = t.Variable;
            if (v.IsInitialized)
                return Expression.Field(Expression.Constant(v), "Value");

            try
            {
                var u = new Unit(t.Content);
                t.Type = TokenTypes.Unit;
                v.SetValue(u);
                return Expression.Constant(v.Value);
            }
            catch
            {
#if BG
                throw new MathParserException($"Недефинирана променлива или мерни единици: \"{t.Content}\".");
#else
                throw new MathParserException($"Undefined variable or units: \"{t.Content}\".");
#endif
            }
        }

        private Expression ParseToken(Token t, Expression a)
        {
            if (t.Type != TokenTypes.Function && t.Content != NegateString)
#if BG
                throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция.");
#else
                throw new MathParserException($"Error evaluating \"{t.Content}\" as function.");
#endif
            return Expression.Invoke(Expression.Constant(_calc.GetFunction(t.Index)), a);
        }

        private Expression ParseToken(Token t, Expression a, Expression b)
        {
            if (t.Type == TokenTypes.Operator)
            {
                if (t.Content == "=")
                {
                    Expression e = ((MemberExpression)a).Expression;
                    ConstantExpression c = (ConstantExpression)e;
                    Variable v = (Variable)c.Value;
                    Type type = typeof(Variable);
                    MethodInfo method = type.GetMethod(
                        "SetValue",
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        Type.DefaultBinder,
                        new[] {typeof(Value).MakeByRefType()},
                        null
                        );
                    return Expression.Block(
                        Expression.Call(Expression.Constant(v), method, b),
                        Expression.Field(Expression.Constant(v), "Value")
                        );
                }

                if (_settings.IsComplex)
                    return Expression.Invoke(Expression.Constant(_calc.GetOperator(t.Index)), a, b);

                return t.Content[0] switch
                {
                    '+' => Expression.Add(a, b),
                    '-' => Expression.Subtract(a, b),
                    '/' or '÷' => Expression.Divide(a, b),
                    '*' => Expression.Multiply(a, b),
                    '<' => Expression.LessThan(a, b),
                    '>' => Expression.GreaterThan(a, b),
                    '≤' => Expression.LessThanOrEqual(a, b),
                    '≥' => Expression.GreaterThanOrEqual(a, b),
                    '≡' => Expression.Equal(a, b),
                    '≠' => Expression.NotEqual(a, b),
                    _ => Expression.Invoke(Expression.Constant(_calc.GetOperator(t.Index)), a, b),
                };
            }

            if (t.Type != TokenTypes.Function2)
#if BG
                throw new MathParserException($"Грешка при изчисляване на \"{t.Content}\" като функция или оператор.");
#else
                throw new MathParserException($"Error evaluating \"{t.Content}\" as function or operator.");
#endif

            return Expression.Invoke(Expression.Constant(_calc.GetFunction2(t.Index)), a, b);
        }

        private static Expression ParseIf(Expression condition, Expression valueIfTrue, Expression valueIfFalse)
        {
            var method = typeof(MathParser).GetMethod("EvaluateIf", BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Call(method, condition, valueIfTrue, valueIfFalse);
        }

        private Expression ParseSolver(Token t)
        {
            var solveBlock = _solveBlocks[t.Index];
            Expression instance = Expression.Constant(solveBlock);
            var method = typeof(SolveBlock).GetMethod("Calculate", BindingFlags.Instance | BindingFlags.NonPublic);
            Expression arg = Expression.Constant(this);
            return Expression.Call(instance, method, arg);
        }

        private Expression ParseFunction(CustomFunction cf, Expression[] parameters)
        {
            Expression instance = Expression.Constant(this);
            var method = typeof(MathParser).GetMethod("EvaluateFunction", BindingFlags.NonPublic | BindingFlags.Instance);
            Expression args = Expression.NewArrayInit(typeof(Value), parameters);

            return Expression.Call(instance, method, Expression.Constant(cf), args);
        }

        private Expression ParseMultiFunction(int Index, Expression[] parameters)
        {
            var method = Expression.Constant(_calc.GetMultiFunction(Index));
            Expression args = Expression.NewArrayInit(typeof(Value), parameters);
            return Expression.Invoke(method, args);
        }

        public string ResultAsString => Complex.Format(Result, _settings.Decimals, OutputWriter.OutputFormat.Text) + Units?.Text;
        public override string ToString() => RenderOutput(OutputWriter.OutputFormat.Text);
        public string ToHtml() => RenderOutput(OutputWriter.OutputFormat.Html);
        public string ToXml() => RenderOutput(OutputWriter.OutputFormat.Xml);

        private string RenderOutput(OutputWriter.OutputFormat format)
        {
            OutputWriter writer = format switch
            {
                OutputWriter.OutputFormat.Html => new HtmlWriter(),
                OutputWriter.OutputFormat.Xml => new XmlWriter(),
                _ => new TextWriter()
            };
            _stringBuilder.Clear();
            var delimiter = writer.FormatOperator(';');
            var assignment = writer.FormatOperator('=');
            if (_functionDefinitionIndex >= 0)
            {
                var cf = _functions[_functionDefinitionIndex];
                for (int i = 0, count = cf.ParameterCount; i < count; ++i)
                {
                    _stringBuilder.Append(writer.FormatVariable(cf.ParameterName(i)));
                    if (i < cf.ParameterCount - 1)
                        _stringBuilder.Append(delimiter);
                }
                var s = writer.AddBrackets(_stringBuilder.ToString());
                _stringBuilder.Clear();
                _stringBuilder.Append(writer.FormatVariable(_functions.LastName));
                _stringBuilder.Append(s);
                _stringBuilder.Append(assignment);
                _stringBuilder.Append(RenderExpression(cf.Rpn, false, writer, out _));
                if (_functionDefinitionIndex < _functions.Count - 1)
                {
#if BG
                    const string warning = " Внимание! Функцията е предефинирана.";
#else
                    const string warning = " Warning! Function redefined.";
#endif
                    if (format == OutputWriter.OutputFormat.Text)
                        _stringBuilder.Append(warning);
                    else if (format == OutputWriter.OutputFormat.Html)
                        _stringBuilder.Append($"<b class=\"err\" title = \"{warning}\"> !!!</b>");
                }
            }
            else
            {
                _stringBuilder.Append(RenderExpression(_rpn, false, writer, out var hasOperators));
                if (_isCalculated && _functionDefinitionIndex < 0)
                {
                    var subst = string.Empty;
                    if (!(_rpn.Length == 3  && 
                        _rpn[2].Content == "=" && 
                        _rpn[1].Type == TokenTypes.Solver
                        || _rpn.Length == 1 &&
                        _rpn[0].Type == TokenTypes.Solver))
                    {
                        if (_hasVariables)
                        {
                            subst = RenderExpression(_rpn, true, writer, out _);
                            _stringBuilder.Append(assignment);
                            _stringBuilder.Append(subst);
                        }
                        if (_assignmentIndex > 0)
                        {
                            subst = _stringBuilder.ToString()[_assignmentIndex..];
                        }
                    }
                    var res = writer.FormatValue(new Value(Result, Units), _settings.Decimals);
                    if (hasOperators && res != subst || subst.Length == 0)
                    {
                        _stringBuilder.Append(assignment);
                        _stringBuilder.Append(res);
                    }
                }
            }
            return _stringBuilder.ToString();
        }

        private static readonly int PlusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['+']];
        private static readonly int MinusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['-']];

        private string RenderExpression(Token[] rpn, bool substitute, OutputWriter writer, out bool hasOperators)
        {
            var stackBuffer = new Stack<RenderToken>();
            hasOperators = _targetUnits is not null;
            for (int i = 0, len = rpn.Length; i < len; ++i)
            {
                var t = new RenderToken(rpn[i], 0);
                var tt = t.Type;
                if (tt == TokenTypes.Solver)
                {
                    t.Content = FormatSolver(t.Index, substitute, writer);
                    t.Type = TokenTypes.Constant;
                }
                else if (tt == TokenTypes.Input)
                {
                    var units = ((ValueToken)rpn[i]).Value.Units;
                    t.Content = writer.FormatInput(t.Content, units, _isCalculated);
                }
                else if (tt == TokenTypes.Constant || tt == TokenTypes.Unit)
                {
                    var v = Value.Zero;
                    var hasValue = true;
                    if (rpn[i] is ValueToken valToken)
                        v = valToken.Value;
                    else if (rpn[i] is VariableToken varToken)
                        v = varToken.Variable.Value;
                    else
                        hasValue = false;

                    if (hasValue)
                    {
                        if (tt == TokenTypes.Unit)
                            t.Content = writer.UnitString(v.Units);
                        else
                            t.Content = writer.FormatValue(v, _settings.Decimals);

                        t.IsCompositeValue = v.IsComposite();
                    }
                }
                else if (tt == TokenTypes.Variable)
                {
                    if (substitute)
                    {
                        var v = ((VariableToken)rpn[i]).Variable.Value;
                        t.Content = writer.FormatValue(v, _settings.Decimals);
                        t.IsCompositeValue = v.IsComposite() || t.Content.Contains('×');
                        t.Order = Token.DefaultOrder;
                        if (_settings.IsComplex && v.Number.IsComplex && v.Units is null)
                            t.Order = v.Number.Im < 0 ? MinusOrder : PlusOrder;
                    }
                    else
                        t.Content = writer.FormatVariable(t.Content);
                }
                else
                {
                    var div = writer.FormatOperator(';');
                    var b = stackBuffer.Pop();
                    var sb = b.Content;
                    if (t.Content == NegateString)
                    {
                        if (IsNegative(b) || b.Order > Token.DefaultOrder)
                            sb = writer.AddBrackets(sb, b.Level);
                        t.Content = writer.FormatOperator(NegateChar) + sb;
                        t.Level = b.Level;
                        if (b.Type == TokenTypes.Constant)
                        {
                            t.Type = b.Type;
                            t.Order = b.Order;
                            t.ValType = b.ValType;
                        }
                    }
                    else if(tt == TokenTypes.Operator)
                    {
                        if (!(substitute && t.Content == "="))
                        {
                            var content = t.Content;
                            if (!stackBuffer.TryPop(out var a))
                                a = new RenderToken(string.Empty, t.Content == "-" ? TokenTypes.Constant : TokenTypes.None, 0);

                            var level = a.Level + b.Level + 1;
                            var isInline = writer is HtmlWriter && t.Content != "^" && !(level < 4 && (t.Content == "/" || t.Content == "÷"));
                            if (b.Offset != 0 && isInline)
                                sb = HtmlWriter.OffsetDivision(sb, b.Offset);

                            var sa = a.Content;
                            if (a.Order > t.Order && !(_settings.FormatEquations && t.Content == "/"))
                                sa = writer.AddBrackets(sa, a.Level);

                            if (a.Offset != 0 && isInline)
                                sa = HtmlWriter.OffsetDivision(sa, a.Offset);

                            if (t.Content == "^")
                            {
                                if (a.IsCompositeValue || IsNegative(a))
                                    sa = writer.AddBrackets(sa, a.Level);

                                if (writer is TextWriter && (IsNegative(b) || b.Order != Token.DefaultOrder))
                                    sb = writer.AddBrackets(sb, b.Level);

                                t.Content = writer.FormatPower(sa, sb, a.Level, a.Order);
                                t.Level = a.Level;
                                t.ValType = a.ValType;
                                if (a.ValType != 2)
                                    hasOperators = true;
                            }
                            else
                            {
                                var formatEquation = writer is not TextWriter && (_settings.FormatEquations && t.Content == "/" || t.Content == "÷");
                                if (
                                        !formatEquation &&
                                        (
                                            b.Order > t.Order ||
                                            b.Order == t.Order && (t.Content == "-" || t.Content == "/") ||
                                            IsNegative(b) && t.Content[0] != '='
                                        )
                                    )
                                    sb = writer.AddBrackets(sb, b.Level);

                                if (formatEquation)
                                {
                                    level = a.Level + b.Level + 1;
                                    if (level >= 4)
                                    {
                                        if (a.Order > t.Order)
                                            sa = writer.AddBrackets(sa, a.Level);

                                        if (b.Order > t.Order)
                                            sb = writer.AddBrackets(sb, b.Level);
                                    }
                                    t.Content = writer.FormatDivision(sa, sb, level);
                                    if (level < 4)
                                    {
                                        if (level == 2)
                                            t.Offset = a.Level - b.Level;
                                    }
                                    else
                                    {
                                        level = Math.Max(a.Level, b.Level);
                                    }
                                }
                                else
                                {
                                    if (writer is TextWriter)
                                        level = 0;
                                    else
                                        level = Math.Max(a.Level, b.Level);

                                    if (t.Content == "*" && a.ValType == 1 && b.ValType == 2)
                                        t.Content = sa + char.ConvertFromUtf32(0x200A) + sb;
                                    else
                                        t.Content = sa + writer.FormatOperator(t.Content[0]) + sb;
                                }
                                t.ValType = a.ValType == b.ValType ? a.ValType : (byte)3;
                                t.Level = level;
                                if (content == "=")
                                    _assignmentIndex = t.Content.Length - sb.Length;
                                else
                                {
                                    if (t.Order != 1 && !(a.ValType == 1 && b.ValType == 2))
                                        hasOperators = true;
                                }
                            }
                        }
                        else
                        {
                            if (b.Offset != 0 && writer is HtmlWriter)
                                sb = HtmlWriter.OffsetDivision(sb, b.Offset);

                            t.Content = sb;
                            t.Level = b.Level;
                        }
                    }
                    else if (tt == TokenTypes.Function2)
                    {
                        var a = stackBuffer.Pop();
                        if (t.Content == "root")
                        {
                            t.Content = writer.FormatRoot(a.Content, _settings.FormatEquations, a.Level, sb);
                            t.Level = b.Level;
                        }
                        else
                        {
                            t.Level = Math.Max(a.Level, b.Level);
                            t.Content = writer.FormatFunction(t.Content) + writer.AddBrackets(a.Content + div + sb, t.Level);
                        }
                        hasOperators = true;
                    }
                    else if (tt == TokenTypes.If)
                    {
                        var a = stackBuffer.Pop();
                        var c = stackBuffer.Pop();
                        t.Level = Math.Max(Math.Max(a.Level, b.Level), c.Level);
                        t.Content = writer.FormatFunction(t.Content) + writer.AddBrackets(c.Content + div + a.Content + div + sb, t.Level);
                        hasOperators = true;
                    }
                    else if (tt == TokenTypes.MultiFunction)
                    {
                        var mfParamCount = t.ParameterCount - 1;
                        var s = sb;
                        t.Level = b.Level;
                        for (int j = 0; j < mfParamCount; ++j)
                        {
                            var a = stackBuffer.Pop();
                            s = a.Content + div + s;
                            if (a.Level > t.Level)
                                t.Level = a.Level;
                        }
                        t.Content = writer.FormatFunction(t.Content) + writer.AddBrackets(s, t.Level);
                        hasOperators = true;
                    }
                    else if (tt == TokenTypes.CustomFunction)
                    {
                        var cf = _functions[t.Index];
                        var cfParamCount = cf.ParameterCount - 1;
                        var s = sb;
                        t.Level = b.Level;
                        for (int j = 0; j < cfParamCount; ++j)
                        {
                            var a = stackBuffer.Pop();
                            s = a.Content + div + s;
                            if (a.Level > t.Level)
                                t.Level = a.Level;
                        }
                        t.Content = writer.FormatVariable(t.Content) + writer.AddBrackets(s, t.Level);
                        hasOperators = true;
                    }
                    else if (t.Content == "!")
                    {
                        if (IsNegative(b) || b.Order > Token.DefaultOrder)
                            sb = writer.AddBrackets(sb, b.Level);
                        t.Content = sb + writer.FormatOperator('!');
                        t.Level = b.Level;
                        hasOperators = true;
                    }
                    else if (t.Content == "sqr" || t.Content == "sqrt" || t.Content == "cbrt")
                    {
                        t.Content = writer.FormatRoot(sb, _settings.FormatEquations, b.Level, t.Content == "cbrt" ? "3" : "2");
                        t.Level = b.Level;
                        hasOperators = true;
                    }
                    else
                    {
                        t.Content = (tt == TokenTypes.Function ? writer.FormatFunction(t.Content) : writer.FormatVariable(t.Content)) + writer.AddBrackets(sb, b.Level);
                        t.Level = b.Level;
                        hasOperators = true;
                    }
                }
                stackBuffer.Push(t);
            }

            if (stackBuffer.TryPop(out var result))
                return result.Content;

            return string.Empty;
        }

        private string FormatSolver(int index, bool substitute, OutputWriter writer)
        {
            if (substitute)
                return writer.FormatValue(_solveBlocks[index].Result, _settings.Decimals);

            if (writer is HtmlWriter)
                return _solveBlocks[index].ToHtml();

            if (writer is XmlWriter)
                return _solveBlocks[index].ToXml();

            return _solveBlocks[index].ToString();
        }

        private static bool IsNegative(Token t)
        {
            var n = t.Content.Length;
            if (t.Order != Token.DefaultOrder || n == 0)
                return false;

            for (int i = 0; i < n; ++i)
            {
                var c = t.Content[i];
                if (Validator.IsDigit(c) || c == '(')
                    break;
                if (c == '-')
                    return true;
            }
            return false;
        }

        private static class NumberParser
        {
            private static readonly double[] Pow10 =
{
                0.0,   1.0,   1e-01, 1e-02, 1e-03, 1e-04, 1e-05, 1e-06, 1e-07,
                1e-08, 1e-09, 1e-10, 1e-11, 1e-12, 1e-13, 1e-14, 1e-15, 1e-16,
                1e-17, 1e-18, 1e-19, 1e-20, 1e-21, 1e-22, 1e-23, 1e-24, 1e-25,
                1e-26, 1e-27, 1e-28, 1e-29, 1e-30, 1e-31, 1e-32, 1e-33, 1e-34
            };
            private static readonly long[,] Mult;

            static NumberParser()
            {
                Mult = new long[10, 16];
                long k = 1;
                for (int j = 0; j < 16; ++j)
                {
                    for (int i = 1; i < 10; ++i)
                        Mult[i, j] = i * k;

                    k *= 10;
                }
            }

            internal static Complex Parse(string s, bool isComplex)
            {
                var maxDigits = 16;
                var isImaginary = false;
                var digits = s.Length;
                if (s[digits - 1] == 'i')
                {
                    isImaginary = true;
                    --digits;
                }
                var decimalPosition = digits;
                var isLeadingZeros = true;
                var leadingZeros = 0;
                for (int i = 0; i < digits; ++i)
                {
                    if (s[i] == DecimalSymbol)
                    {
                        if (decimalPosition < digits)
                            throw new FormatException();
                        decimalPosition = i;
                    }
                    else if (s[i] != '0')
                        isLeadingZeros = false;

                    if (isLeadingZeros)
                        ++leadingZeros;
                }
                maxDigits += leadingZeros;
                var n = digits;
                if (n > maxDigits)
                    n = maxDigits;
                var n1 = n - 1;
                long k = 0;
                if (decimalPosition > leadingZeros && decimalPosition < n)
                {
                    for (int i = decimalPosition + 1; i < n; ++i)
                        k += Mult[(s[i] - '0'), n1 - i];

                    --n1;
                    for (int i = leadingZeros; i < decimalPosition; ++i)
                        k += Mult[(s[i] - '0'), n1 - i];
                }
                else
                {
                    for (int i = leadingZeros; i < n; ++i)
                        k += Mult[(s[i] - '0'), n1 - i];
                }

                double d;
                if (decimalPosition >= n)
                {
                    d = k;
                    for (int i = n; i < decimalPosition; ++i)
                        d = d * 10.0 + (s[i] - '0');
                }
                else
                {
                    n -= decimalPosition;
                    if (n < Pow10.Length)
                        d = k * Pow10[n];
                    else
                        d = k * Math.Pow(10, -n);

                }

                if (isImaginary)
                    return new Complex(0.0, d);

                if (isComplex)
                    return new Complex(d, 0.0);

                return new Complex(d);
            }
        }

        [Serializable]
        internal class MathParserException : Exception
        {
            internal MathParserException(string message) : base(message) { }
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
            internal ValueToken(Value value) : base(string.Empty, TokenTypes.Constant)
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

            internal byte ValType;  //0 - none, 1 - number, 2 - unit, 3 - number + unit
            internal int Level;
            internal int Offset; //-1 - down, 0 - none, 1 - up
            internal int ParameterCount;
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
                        ValType = 2;
                    else if (v.Units is null)
                        ValType = 1;
                    else
                        ValType = 3;
                }
            }
        }

        private class SolveBlock
        {
            internal enum SolverTypes
            {
                None,
                Find,
                Root,
                Sup,
                Inf,
                Area,
                Slope,
                Repeat,
                Sum,
                Product,
                Error
            }

            private static readonly Dictionary<string, SolverTypes> Definitions = new()
            {
                { "$find", SolverTypes.Find },
                { "$root", SolverTypes.Root },
                { "$sup", SolverTypes.Sup },
                { "$inf", SolverTypes.Inf },
                { "$area", SolverTypes.Area },
                { "$slope", SolverTypes.Slope },
                { "$repeat", SolverTypes.Repeat },
                { "$sum", SolverTypes.Sum },
                { "$product", SolverTypes.Product }
            };

            private static readonly string[] TypeNames =
            {
                string.Empty,
                "$Find",
                "$Root",
                "$Sup",
                "$Inf",
                "$Area",
                "$Slope",
                "$Repeat",
                "∑",
                "∏",
                "$Error"
            };

            private readonly StringBuilder _stringBuilder = new();
            private readonly SolverTypes _type;
            private SolverItem[] _items;
            private string Script { get; }
            private Func<Value> _function;
            internal Value Result { get; private set; }

            internal SolveBlock(string script, SolverTypes type, MathParser parser)
            {
                Script = script;
                _type = type;
                Parse(parser);
            }

            internal static SolverTypes GetSolverType(string keyword)
            {
                var s = keyword.Trim().ToLowerInvariant();
                if (Definitions.ContainsKey(s))
                    return Definitions[s];

                return SolverTypes.Error;
            }

            private static string TypeName(SolverTypes st)
            {
                var i = (int)st;
                if (i >= 0 && i < (int)SolverTypes.Error)
                    return TypeNames[i];

                return TypeNames[(int)SolverTypes.Error];
            }

            private void Parse(MathParser parser)
            {
                var n = 3;
                if (_type == SolverTypes.Slope)
                    n = 2;

                const string delimiters = "@=:";
                _items = new SolverItem[5];
                int current = 0, bracketCounter = 0;
                _stringBuilder.Clear();
                for (int i = 0, len = Script.Length; i < len; ++i)
                {
                    var c = Script[i];
                    if (c == '{')
                        ++bracketCounter;
                    else if (c == '}')
                        --bracketCounter;

                    if (bracketCounter == 0 && current < n && c == delimiters[current])
                    {
                        _items[current].Input = _stringBuilder.ToString();
                        _stringBuilder.Clear();
                        ++current;
                    }
                    else
                        _stringBuilder.Append(c);
                }

                _items[current].Input = _stringBuilder.ToString();
                var targetUnits = parser._targetUnits;
                for (int i = 0; i <= n; ++i)
                {
                    if (string.IsNullOrWhiteSpace(_items[i].Input))
#if BG
                        throw new MathParserException($"Липсва разделител \"{delimiters[i]}\" в команда за числени методи {{{Script}}}.");
#else
                        throw new MathParserException($"Missing delimiter \"{delimiters[i]}\" in solver command {{{Script}}}.");
#endif
                    _items[i].Input = _items[i].Input.Trim();
                    if (i == 0 && _type == SolverTypes.Root)
                    {
                        var s = _items[0].Input.Split('=');
                        _items[0].Input = s[0];
                        if (s.Length == 2)
                        {
                            _items[4].Input = s[1];
                            n = 4;
                        }
                    }
                    parser.Parse(_items[i].Input);
                    _items[i].Rpn = parser._rpn;
                    _items[i].Html = parser.ToHtml();
                    _items[i].Xml = parser.ToXml();
                }
                if (_type == SolverTypes.Inf || _type == SolverTypes.Sup)
                {
                    var s = _items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    parser.SetVariable(s, new Value(double.NaN));
                }
                parser._targetUnits = targetUnits;
            }

            internal void Compile(MathParser parser)
            {
                if (_function is not null)
                    return;

                var vt = (VariableToken)_items[1].Rpn[0];
                Parameter[] p = { new(vt.Content) };
                p[0].SetValue(Value.Zero);
                vt.Variable = p[0];
                parser.BindParameters(p, _items[0].Rpn);
                _function = parser.CompileRpn(_items[0].Rpn);
            }

            internal void BindParameters(Parameter[] parameters, MathParser parser)
            {
                if (parser.IsEnabled)
                {
                    parser.BindParameters(parameters, _items[0].Rpn);
                    parser.BindParameters(parameters, _items[2].Rpn);
                    if (_items[3].Rpn is not null)
                        parser.BindParameters(parameters, _items[3].Rpn);

                    if (_items[4].Rpn is not null)
                        parser.BindParameters(parameters, _items[4].Rpn);
                }
            }

            internal Value Calculate(MathParser parser)
            {
                var parserRpn = parser._rpn;
                var parserUnits = parser.Units;
                var targetUnits = parser._targetUnits;
                var isPlotting = parser.IsPlotting;
                var t = (VariableToken)_items[1].Rpn[0];
                var x = t.Variable;
                parser._rpn = _items[2].Rpn;
                parser._targetUnits = null;
                var x1 = parser.CalculateReal();
                double x2 = 0.0, y = 0.0;
                var ux1 = parser.Units;
                if (_type != SolverTypes.Slope)
                {
                    parser._rpn = _items[3].Rpn;
                    x2 = parser.CalculateReal();
                    var ux2 = parser.Units;
                    if (!Unit.IsConsistent(ux1, ux2))
#if BG
                        throw new MathParserException($"Несъвместими мерни единици за {_items[0].Input} = \"{Unit.GetText(ux1)}' : \"{Unit.GetText(ux2)}'.");
#else
                        throw new MathParserException($"Inconsistent units for {_items[0].Input} = \"{Unit.GetText(ux1)}' : \"{Unit.GetText(ux2)}'.");
#endif
                    if (ux2 is not null)
                        x2 *= ux2.ConvertTo(ux1);
                }
                var number = parser.MakeNumber(x1);
                x.SetValue(number, ux1);
                if (_type == SolverTypes.Root && _items[4].Rpn is not null)
                {
                    _function();
                    var uy1 = parser.Units;
                    parser._rpn = _items[4].Rpn;
                    var y1 = parser.CalculateReal();
                    number = parser.MakeNumber(x2);
                    x.SetNumber(number);
                    var y2 = parser.CalculateReal();
                    if (Math.Abs(y2 - y1) > 1e-14)
#if BG
                        throw new MathParserException($"Изразът от дясната страна трябва да е константа: \"{_items[4].Input}\".");
#else
                        throw new MathParserException($"The expression on the right side must be constant: \"{_items[4].Input}\".");
#endif
                    y = y1;
                    var uy2 = parser.Units;
                    if (!Unit.IsConsistent(uy1, uy2))
#if BG
                        throw new MathParserException($"Несъвместими мерни единици за \"{ _items[0].Input} = {_items[4].Input}\".");
#else
                        throw new MathParserException($"Inconsistent units for \"{_items[0].Input} = {_items[4].Input}\".");
#endif
                    if (uy2 is not null)
                        y *= uy2.ConvertTo(uy1);
                }
                var solver = parser.Solver;
                var variable = solver.Variable;
                var function = solver.Function;
                var solverUnits = solver.Units;
                solver.Variable = x;
                solver.Function = _function;
                solver.Precision = parser.Precision;
                solver.Units = null;
                var result = parser.MakeNumber(double.NaN);
                try
                {
                    switch (_type)
                    {
                        case SolverTypes.Find:
                            result = solver.Find(x1, x2);
                            break;
                        case SolverTypes.Root:
                            result = solver.Root(x1, x2, y);
                            break;
                        case SolverTypes.Sup:
                            result = solver.Sup(x1, x2);
                            break;
                        case SolverTypes.Inf:
                            result = solver.Inf(x1, x2);
                            break;
                        case SolverTypes.Area:
                            result = solver.Area(x1, x2);
                            break;
                        case SolverTypes.Repeat:
                            result = solver.Repeat(x1, x2);
                            break;
                        case SolverTypes.Sum:
                            result = solver.Sum(x1, x2);
                            break;
                        case SolverTypes.Product:
                            result = solver.Product(x1, x2);
                            break;
                        case SolverTypes.Slope:
                            result = solver.Slope(x1);
                            break;
                    }
                }
                catch (MathParserException e)
                {
                    if (e.Message.Contains("%F"))
                    {
                        var s = e.Message.Replace("%F", _items[0].Input).Replace("%V", _items[1].Input);
                        throw new MathParserException(s);
                    }
                    throw e;
                }
                parser._rpn = parserRpn;
                if (_type == SolverTypes.Sup || _type == SolverTypes.Inf)
                {
                    var s = _items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    parser.SetVariable(s, x.Value);
                }

                parser.IsPlotting = isPlotting;
                parser.Units = parserUnits;
                parser._targetUnits = targetUnits;

                if (double.IsNaN(result.Re) && !isPlotting)
#if BG
                    throw new MathParserException($"Няма решение за: {ToString()}.");
#else
                    throw new MathParserException($"No solution for: {ToString()}.");
#endif

                Result = new Value(result, solver.Units);
                solver.Variable = variable;
                solver.Function = function;
                solver.Units = solverUnits;
                return Result;
            }

            internal string ToHtml()
            {
                _stringBuilder.Clear();
                if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                    _stringBuilder.Append("<span class=\"cond b1\">" + TypeName(_type) + "</span><span class=\"b1\">{</span> ");
                else
                    _stringBuilder.Append("<span class=\"cond\">" + TypeName(_type) + "</span>{");

                _stringBuilder.Append(_items[0].Html);
                if (_type == SolverTypes.Root)
                {
                    if (_items[4].Html is not null)
                        _stringBuilder.Append(" = " + _items[4].Html);
                    else
                        _stringBuilder.Append(" = 0");
                }

                _stringBuilder.Append("; ");
                _stringBuilder.Append(_items[1].Html);
                if (_type == SolverTypes.Repeat || _type == SolverTypes.Sum || _type == SolverTypes.Product)
                {
                    _stringBuilder.Append(" = ");
                    _stringBuilder.Append(_items[2].Html);
                    _stringBuilder.Append("...");
                    _stringBuilder.Append(_items[3].Html);
                    if (_type == SolverTypes.Repeat)
                        _stringBuilder.Append('}');
                    else
                        _stringBuilder.Append(" <span class=\"b1\">}</span>");
                }
                else if (_type == SolverTypes.Slope)
                {
                    _stringBuilder.Append(" = ");
                    _stringBuilder.Append(_items[2].Html);
                    _stringBuilder.Append('}');
                }
                else
                {
                    _stringBuilder.Append(" ∈ [");
                    _stringBuilder.Append(_items[2].Html);
                    _stringBuilder.Append("; ");
                    _stringBuilder.Append(_items[3].Html);
                    _stringBuilder.Append("]}");
                }
                return _stringBuilder.ToString();
            }

            internal string ToXml()
            {
                _stringBuilder.Clear();
                if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                    _stringBuilder.Append($"<m:nary><m:naryPr><m:chr m:val=\"{ TypeName(_type)}\"/><m:limLoc m:val=\"undOvr\"/><m:subHide m:val=\"1\"/><m:supHide m:val=\"1\" /></m:naryPr><m:sub/><m:sup/><m:e>");
                else
                    _stringBuilder.Append($"<m:r><m:t>{TypeName(_type)}</m:t></m:r>");//<w:rPr><w:color w:val=\"FF00FF\" /></w:rPr>

                _stringBuilder.Append("<m:d><m:dPr><m:begChr m:val = \"{\" /><m:endChr m:val = \"}\" /></m:dPr><m:e>");

                _stringBuilder.Append(_items[0].Xml);
                if (_type == SolverTypes.Root)
                {
                    if (_items[4].Xml is not null)
                        _stringBuilder.Append($"<m:r><m:t>=</m:t></m:r>{_items[4].Xml}");
                    else
                        _stringBuilder.Append("<m:r><m:t>=0</m:t></m:r>");
                }

                _stringBuilder.Append("<m:r><m:t>;</m:t></m:r>");
                _stringBuilder.Append(_items[1].Xml);
                if (_type == SolverTypes.Repeat || _type == SolverTypes.Sum || _type == SolverTypes.Product)
                {
                    _stringBuilder.Append("<m:r><m:t>=</m:t></m:r>");
                    _stringBuilder.Append(_items[2].Xml);
                    _stringBuilder.Append("<m:r><m:t>...</m:t></m:r>");
                    _stringBuilder.Append(_items[3].Xml);
                }
                else if (_type == SolverTypes.Slope)
                {
                    _stringBuilder.Append("<m:r><m:t>=</m:t></m:r>");
                    _stringBuilder.Append(_items[2].Xml);
                }
                else
                {
                    _stringBuilder.Append("<m:r><m:t>∈</m:t></m:r><m:d><m:dPr><m:begChr m:val = \"[\" /><m:endChr m:val = \"]\" /></m:dPr><m:e>");
                    _stringBuilder.Append(_items[2].Xml);
                    _stringBuilder.Append("<m:r><m:t>;</m:t></m:r>");
                    _stringBuilder.Append(_items[3].Xml);
                    _stringBuilder.Append("</m:e></m:d>");
                }
                _stringBuilder.Append("</m:e></m:d>");
                if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                    _stringBuilder.Append("</m:e></m:nary>");

                return _stringBuilder.ToString();
            }

            public override string ToString()
            {
                _stringBuilder.Clear();
                _stringBuilder.Append(TypeName(_type));
                _stringBuilder.Append('{');
                _stringBuilder.Append(_items[0].Input);
                if (_type == SolverTypes.Root)
                {
                    if (_items[4].Input is not null)
                        _stringBuilder.Append(" = " + _items[4].Input);
                    else
                        _stringBuilder.Append(" = 0");
                }
                _stringBuilder.Append("; ");
                _stringBuilder.Append(_items[1].Input);
                if (_type == SolverTypes.Repeat || _type == SolverTypes.Sum || _type == SolverTypes.Product)
                {
                    _stringBuilder.Append(" = ");
                    _stringBuilder.Append(_items[2].Input);
                    _stringBuilder.Append("...");
                    _stringBuilder.Append(_items[3].Input);
                    _stringBuilder.Append('}');
                }
                else if (_type == SolverTypes.Slope)
                {
                    _stringBuilder.Append(" = ");
                    _stringBuilder.Append(_items[2].Input);
                    _stringBuilder.Append('}');
                }
                else
                {
                    _stringBuilder.Append(" ∈ [");
                    _stringBuilder.Append(_items[2].Input);
                    _stringBuilder.Append("; ");
                    _stringBuilder.Append(_items[3].Input);
                    _stringBuilder.Append("]}");
                }
                return _stringBuilder.ToString();
            }

            private struct SolverItem
            {
                internal string Input;
                internal string Html;
                internal string Xml;
                internal Token[] Rpn;
            }
        }

        private class CustomFunction
        {
            private readonly struct Tuple : IEquatable<Tuple>
            {
                private readonly Value _x, _y;

                internal Tuple(Value x, Value y)
                {
                    _x = x;
                    _y = y;
                }

                public override int GetHashCode() => HashCode.Combine(_x, _y);

                public override bool Equals(object obj)
                {
                    if (obj is Tuple t)
                        return _x.Equals(t._x) && _y.Equals(t._y);

                    return false;
                }
                public bool Equals(Tuple other) => _x.Equals(other._x) && _y.Equals(other._y);
            }

            private const int MaxCacheSize = 100;
            internal delegate void ChangeEvent();
            internal event ChangeEvent OnChange;
            internal Token[] Rpn;
            private Dictionary<Value, Value> _cache;
            private Dictionary<Tuple, Value> _cache2;
            private Parameter[] _parameters;
            internal Unit Units;
            internal int ParameterCount { get; private set; }
            internal bool IsRecursion { get; private set; }

            internal Func<Value> Function;

            internal void AddParameters(List<string> parameters)
            {
                ParameterCount = parameters.Count;
                _parameters = new Parameter[ParameterCount];
                for (int i = 0; i < ParameterCount; ++i)
                    _parameters[i] = new Parameter(parameters[i]);

                switch (ParameterCount)
                {
                    case 1:
                        _cache = new Dictionary<Value, Value>();
                        break;
                    case 2:
                        _cache2 = new Dictionary<Tuple, Value>();
                        break;
                }
            }

            internal Parameter[] Parameters => _parameters;

            internal string ParameterName(int index) => _parameters[index].Name;

            private void ClearCache()
            {
                _cache?.Clear();
                _cache2?.Clear();
            }

            internal void PurgeCache()
            {
                if (_cache?.Count >= MaxCacheSize)
                    _cache.Clear();
                else if (_cache2?.Count >= MaxCacheSize)
                    _cache2.Clear();
            }

            internal void BeforeChange()
            {
                ClearCache();
                OnChange?.Invoke();
            }

            internal void SubscribeCache(Container<CustomFunction> functions)
            {
                for (int i = 0, len = Rpn.Length; i < len; ++i)
                {
                    var t = Rpn[i];
                    if (t is VariableToken vt)
                        vt.Variable.OnChange += ClearCache;
                    else if (t.Type == TokenTypes.CustomFunction)
                    {
                        var index = functions.IndexOf(t.Content);
                        if (index >= 0)
                            functions[index].OnChange += ClearCache;
                    }
                }
            }

            internal bool CheckReqursion(CustomFunction f, Container<CustomFunction> functions)
            {
                if (ReferenceEquals(f, this))
                {
                    IsRecursion = true;
                    return true;
                }
                IsRecursion = false;
                f ??= this;
                for (int i = 0, len = Rpn.Length; i < len; ++i)
                {
                    var t = Rpn[i];
                    if (t.Type == TokenTypes.CustomFunction)
                    {
                        var cf = functions[functions.IndexOf(t.Content)];
                        if (cf.CheckReqursion(f, functions))
                        {
                            IsRecursion = true;
                            break;
                        }
                    }
                }
                return IsRecursion;
            }

            internal Value Calculate(Value[] parameters)
            {
                if (parameters.Length == 1)
                {
                    ref var v = ref parameters[0];
                    if (!_cache.TryGetValue(v, out var z))
                    {
                        _parameters[0].SetValue(parameters[0]);
                        z = Function();
                        _cache.Add(v, z);
                    }
                    return z;
                }
                if (parameters.Length == 2)
                {
                    Tuple args = new(parameters[0], parameters[1]);
                    if (!_cache2.TryGetValue(args, out var z))
                    {
                        _parameters[0].SetValue(parameters[0]);
                        _parameters[1].SetValue(parameters[1]);
                        z = Function();
                        _cache2.Add(args, z);
                    }
                    return z;
                }
                for (int i = 0, len = parameters.Length; i < len; ++i)
                    _parameters[i].SetValue(parameters[i]);

                return Function();
            }
        }

        private class Validator
        {
            private struct MultiFunctionStackItem
            {
                internal Token Token;
                internal int CountOfBrackets;
                internal int CountOfDivisors;
                internal MultiFunctionStackItem(Token token, int countOfBrackets, int countOfDivisors)
                {
                    Token = token;
                    CountOfBrackets = countOfBrackets;
                    CountOfDivisors = countOfDivisors;
                }
            }
            private static readonly int[] OrderIndex = 
                //N  C  V  U  I  O  F  2  M  I  C  L  R  D  S  E
                { 0, 1, 1, 1, 1, 2, 3, 3, 3, 3, 3, 4, 5, 6, 1, 0 };
                //[N]one
                //[C]onstant
                //[V]ariable
                //[U]nit
                //[I]nput
                //[O]perator
                //[F]unction
                //Function[2]
                //[M]ultiFunction
                //[I]f
                //[C]ustomFunction
                //Bracket[L]eft
                //Bracket[R]ight
                //[D]ivisor
                //[S]olver
                //[E]rror
            private static readonly bool[,] CorrectOrder =
            {
                {true, true, true, true, true, true, true},
                {true, false, true, false, false, true, true},
                {true, true, false, true, true, false, false},
                {true, true, false, false, true, false, false},
                {true, true, false, true, true, false, false},
                {true, false, true, false, false, true, true},
                {true, true, false, true, true, false, false}
            };

            private readonly Container<CustomFunction> _functions;

            internal Validator(Container<CustomFunction> functions)
            {
                _functions = functions;
            }

            internal void Check(Queue<Token> input, out bool isFunctionDefinition)
            {
                isFunctionDefinition = false;
                if (!input.Any())
                    return;

                var countOfBrackets = 0;
                var countOfOperators = 0;
                var countOfDivisors = 0;
                var multiFunctionStack = new Stack<MultiFunctionStackItem>();
                var pt = new Token(string.Empty, TokenTypes.None);
                var firstToken = input.Peek();
                foreach (var t in input)
                {
                    if (t.Type == TokenTypes.Function2)
                        --countOfDivisors;
                    else if (t.Type == TokenTypes.If)
                        countOfDivisors -= 2;
                    else if (t.Type == TokenTypes.MultiFunction)
                        multiFunctionStack.Push(new MultiFunctionStackItem(t, countOfBrackets, countOfDivisors));
                    else if (t.Type == TokenTypes.CustomFunction)
                    {
                        if (t.Index < 0)
                        {
                            if (isFunctionDefinition && t.Content == firstToken.Content)
#if BG
                                throw new MathParserException($"Не e разрешена рекурсия в дефиницията на функция:\"{t.Content}\".");
#else
                                throw new MathParserException($"Recursion is not allowed in function definition:\"{t.Content}\".");
#endif
                        }
                        else
                            countOfDivisors = countOfDivisors - _functions[t.Index].ParameterCount + 1;
                    }
                    else if (t.Type == TokenTypes.BracketLeft)
                        ++countOfBrackets;
                    else if (t.Type == TokenTypes.BracketRight)
                    {
                        --countOfBrackets;
                        if (countOfBrackets < 0)
#if BG
                            throw new MathParserException("Липсва лява скоба '('.");
#else
                            throw new MathParserException("Missing left bracket '('.");
#endif
                        if (multiFunctionStack.TryPeek(out var mfsItem))
                        {
                            if (countOfBrackets == mfsItem.CountOfBrackets)
                            {
                                multiFunctionStack.Pop();
                                FunctionToken ft = (FunctionToken)mfsItem.Token;
                                ft.ParameterCount = countOfDivisors - mfsItem.CountOfDivisors + 1;
                                countOfDivisors = mfsItem.CountOfDivisors;
                            }
                        }
                    }
                    else if (t.Type == TokenTypes.Divisor)
                        ++countOfDivisors;
                    else if (t.Type == TokenTypes.Operator)
                    {
                        ++countOfOperators;
                        if (t.Content == "=")
                        {
                            if (firstToken.Type == TokenTypes.CustomFunction)
                            {
                                countOfDivisors = 0;
                                isFunctionDefinition = true;
                            }
                            else if (pt.Type != TokenTypes.Variable)
                            {
#if BG
                                throw new MathParserException("Преди оператора за присвояване '=' може да има само дефиниция на функция или променлива.");
#else
                                throw new MathParserException("Only custom function or variable definitions are allowed before the assignment operator '='.");
#endif
                            }
                            else if (countOfOperators != 1)
                            {
#if BG
                                throw new MathParserException("Преди оператора за присвояване '=' не може да има други оператори.");
#else
                                throw new MathParserException("Assignment '=' must be the first operator in the expression.");
#endif
                            }
                        }
                    }
                    bool correctOrder;
                    if (pt.Content == NegateString)
                        correctOrder = CorrectOrder[OrderIndex[(int)TokenTypes.Operator], OrderIndex[(int)t.Type]];
                    else if (pt.Content == "!")
                        correctOrder = CorrectOrder[OrderIndex[(int)TokenTypes.Constant], OrderIndex[(int)t.Type]];
                    else if (t.Content == "!")
                        correctOrder = CorrectOrder[OrderIndex[(int)pt.Type], OrderIndex[(int)TokenTypes.Operator]];
                    else if (t.Content == NegateString)
                        correctOrder = CorrectOrder[OrderIndex[(int)pt.Type], OrderIndex[(int)TokenTypes.Function]];
                    else
                        correctOrder = CorrectOrder[OrderIndex[(int)pt.Type], OrderIndex[(int)t.Type]];

                    if (!correctOrder)
#if BG
                        throw new MathParserException($"Невалиден синтаксис: \"{pt.Content} {t.Content}\".");
#else
                        throw new MathParserException($"Invalid syntax: \"{pt.Content} {t.Content}\".");
#endif
                    pt = t;
                }

                if (pt.Type == TokenTypes.Operator ||
                    pt.Type == TokenTypes.Function && pt.Content != "!" ||
                    pt.Type == TokenTypes.Function2 ||
                    pt.Type == TokenTypes.MultiFunction ||
                    pt.Type == TokenTypes.If ||
                    pt.Type == TokenTypes.CustomFunction ||
                    pt.Type == TokenTypes.BracketLeft)
#if BG
                    throw new MathParserException("Непълен израз.");
#else
                    throw new MathParserException("Incomplete expression.");
#endif
                if (firstToken.Type == TokenTypes.CustomFunction && firstToken.Index < 0 && !isFunctionDefinition)
#if BG
                    throw new MathParserException($"Невалидна функция: \"{firstToken.Content}\".");
#else
                    throw new MathParserException($"Invalid function: \"{firstToken.Content}\".");
#endif
                if (countOfBrackets > 0)
#if BG
                    throw new MathParserException("Липсва дясна скоба ')'.");
#else
                    throw new MathParserException("Missing right bracket ')'.");
#endif

                if (countOfDivisors > 0)
#if BG
                    throw new MathParserException("Неочакван символ за разделител ';'.");
#else
                    throw new MathParserException("Unexpected delimiter ';'.");
#endif
                if (countOfDivisors < 0)
#if BG
                    throw new MathParserException("Невалиден брой аргументи на функция.");
#else
                    throw new MathParserException("Invalid number of function arguments.");
#endif
            }

            internal static bool IsLetter(char c) => c >= 'a' && c <= 'z' ||
                       c >= 'A' && c <= 'Z' ||
                       c >= 'α' && c <= 'ω' ||
                       c >= 'Α' && c <= 'Ω';

            internal static bool IsDigit(char c) => c >= '0' && c <= '9';
        }
    }
}