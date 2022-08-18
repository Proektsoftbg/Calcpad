using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        internal Func<string> GetInputField;
        private readonly StringBuilder _stringBuilder = new();
        private readonly MathSettings _settings;
        private Token[] _rpn;
        private readonly List<SolveBlock> _solveBlocks = new();
        private readonly Container<CustomFunction> _functions = new();
        private readonly Dictionary<string, Variable> _variables = new(StringComparer.Ordinal);
        private readonly Input _input;
        private readonly Evaluator _evaluator;
        private readonly Calculator _calc;
        private readonly Solver _solver;
        private readonly Compiler _compiler;
        private readonly Output _output;
        private bool _hasVariables;
        private int _assignmentIndex;
        private bool _isCalculated;
        private bool _isPlotting;
        private bool _isSolver;
        private Unit _targetUnits;
        private int _functionDefinitionIndex;
        private KeyValuePair<string, Value> _backupVariable;
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
        internal Unit Units { get; private set; }
        internal bool IsCanceled { get; private set; }
        internal bool IsEnabled { get; set; } = true;
        internal bool IsPlotting
        {
            set => _isPlotting = value;
            get => _isPlotting;

        }
        internal int Degrees { set => _calc.Degrees = value; }
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
            InitVariables();
            if (_settings.IsComplex)
                _calc = new ComplexCalculator();
            else
                _calc = new RealCalculator();

            Degrees = settings.Degrees;
            _solver = new Solver();
            _input = new Input(this);
            _evaluator = new Evaluator(this);
            _compiler = new Compiler(this);
            _output = new Output(this);
        }

        internal void SetVariable(string name, Value value)
        {
            if (_variables.ContainsKey(name))
                _variables[name].SetValue(value);
            else
            {
                _variables.Add(name, new Variable(value));
                _input.DefinedVariables.Add(name);
            }
        }

        internal Value GetVariable(string name)
        {
            try
            {
                return _variables[name].Value;
            }
            catch
            {
#if BG
                throw new MathParserException($"Променливата '{name}' не съществува.");
#else
                throw new MathParserException($"Variable '{name}' does not exist.");
#endif
            }
        }

        internal void Cancel() => IsCanceled = true;

        private void InitVariables()
        {
            var pi = new Value(Math.PI, null);
            _variables.Add("e", new Variable(new Value(Math.E, null)));
            _variables.Add("pi", new Variable(pi));
            _variables.Add("π", new Variable(pi));
            _variables.Add("g", new Variable(new Value(9.80665, null)));
            if (_settings.IsComplex)
            {
                _variables.Add("i", new Variable(Complex.ImaginaryOne));
                _variables.Add("ei", new Variable(Math.E * Complex.ImaginaryOne));
                _variables.Add("πi", new Variable(Math.PI * Complex.ImaginaryOne));
            }
        }

        public void Parse(string expression, bool AllowAssignment = true)
        {
            Result = Complex.Zero;
            _isCalculated = false;
            _functionDefinitionIndex = -1;
            var input = _input.GetInput(expression, AllowAssignment);
            new Validator(_functions).Check(input, out var isFucntionDefinition);
            _input.OrderOperators(input, isFucntionDefinition || _isSolver || _isPlotting, _assignmentIndex);
            if (isFucntionDefinition)
                AddFunction(input);
            else
            {
                _rpn = _input.GetRpn(input);
                PurgeCache();
            }
        }

        private void PurgeCache()
        {
            for (int i = 0, count = _functions.Count; i < count; ++i)
                _functions[i].PurgeCache();
        }

        internal void CompileBlocks()
        {
            for (int i = _solveBlocks.Count - 1; i >= 0; --i)
                _solveBlocks[i].Compile(this);
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
                CompileBlocks();
                Result = _evaluator.Evaluate(_rpn).Number;
                if (_variables.TryGetValue("ReturnAngleUnits", out var v))
                    Calculator.ReturnAngleUnits = v.Value.Number.Re != 0.0;
            }
            _isCalculated = true;
        }

        public double CalculateReal()
        {
            if (IsCanceled)
#if BG
                throw new MathParserException("Прекъсване от потребителя.");
#else
                throw new MathParserException("Interupted by user.");
#endif
            Value value = _evaluator.Evaluate(_rpn);
            CheckReal(value);
            Result = value.Number;
            return Result.Re;
        }

        internal void CheckReal(in Value value)
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
                        var rpn = _input.GetRpn(input);
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

        internal Func<Value> Compile(string expression, Parameter[] parameters)
        {
            var input = _input.GetInput(expression, false);
            _input.OrderOperators(input, true, 0);
            var rpn = _input.GetRpn(input);
            BindParameters(parameters, rpn);
            return _compiler.Compile(rpn);
        }

        private Func<Value> CompileRpn(Token[] rpn) => _compiler.Compile(rpn);
        public string ResultAsString => Complex.Format(Result, _settings.Decimals, OutputWriter.OutputFormat.Text) + Units?.Text;
        public override string ToString() => _output.Render(OutputWriter.OutputFormat.Text);
        public string ToHtml() => _output.Render(OutputWriter.OutputFormat.Html);
        public string ToXml() => _output.Render(OutputWriter.OutputFormat.Xml);

        [Serializable]
        internal class MathParserException : Exception
        {
            internal MathParserException(string message) : base(message) { }
        }
    }
}