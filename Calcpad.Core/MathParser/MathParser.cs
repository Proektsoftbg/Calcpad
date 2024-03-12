using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        internal enum VariableSubstitutionOptions
        {
            VariablesAndSubstitutions,
            VariablesOnly,
            SubstitutionsOnly
        }

        private readonly StringBuilder _stringBuilder = new();
        private readonly MathSettings _settings;
        private Token[] _rpn;
        private readonly List<SolveBlock> _solveBlocks = [];
        private readonly Container<CustomFunction> _functions = new();
        private readonly Dictionary<string, Variable> _variables = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Unit> _units = new(StringComparer.Ordinal);
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
        private int _isSolver;
        private Unit _targetUnits;
        private int _functionDefinitionIndex;
        private KeyValuePair<string, Value> _backupVariable;
        private double Precision
        {
            get
            {
                if (!_variables.TryGetValue("Precision", out var v))
                    return 1e-14;

                var precision = v.ValueByRef().Re;
                return precision switch
                {
                    < 1e-15 => 1e-15,
                    > 1e-2 => 1e-2,
                    _ => precision
                };
            }
        }
        internal int Line;
        internal VariableSubstitutionOptions VariableSubstitution { get; set; }
        internal Unit Units { get; private set; }
        internal bool IsCanceled { get; private set; }
        internal bool IsEnabled { get; set; } = true;
        internal bool IsPlotting
        {
            set => _isPlotting = value;
            get => _isPlotting;

        }
        internal bool Split { get; set; }
        internal bool ShowWarnings { get; set; } = true;
        public int Degrees { set => _calc.Degrees = value; }
        internal int PlotWidth => _variables.TryGetValue("PlotWidth", out var v) ? (int)v.ValueByRef().Re : 500;
        internal int PlotHeight => _variables.TryGetValue("PlotHeight", out var v) ? (int)v.ValueByRef().Re : 300;
        internal int PlotStep => _variables.TryGetValue("PlotStep", out var v) ? (int)v.ValueByRef().Re : 0;

        public const char DecimalSymbol = '.'; //CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
        internal Complex Result { get; private set; }
        public double Real => Result.Re;
        public double Imaginary => Result.Im;
        public System.Numerics.Complex Complex => Result;

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
            _solver = new Solver
            {
                IsComplex = _settings.IsComplex
            };
            _input = new Input(this);
            _evaluator = new Evaluator(this);
            _compiler = new Compiler(this);
            _output = new Output(this);
        }

        public void SaveAnswer()
        {
            var v = new Value(Result, Units);
            SetVariable("ans", v);
            SetVariable("ANS", v);
        }

        public void ClearCustomUnits() => _units.Clear();

        public void SetVariable(string name, double value) => SetVariable(name, new Value(value));

        internal void SetVariable(string name, in Value value)
        {
            if (_variables.TryGetValue(name, out Variable v))
                v.Assign(value);
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
                Throw.VariableNotExistException(name);
                return default;
            }
        }

        internal void SetUnit(string name, Value value)
        {
            var d = value.Re;
            if (Math.Abs(d) == 0)
                d = 1d;

            var u = d * (value.Units ?? Unit.Get(string.Empty));
            u.Text = name;
            if (_units.TryAdd(name, u))
                _input.DefinedVariables.Add(name);
            else
                _units[name] = u;
        }

        internal Unit GetUnit(string name)
        {
            try
            {
                return _units[name];
            }
            catch
            {
                Throw.UnitNotExistException(name);
                return default;
            }
        }

        internal void Cancel() => IsCanceled = true;

        private void InitVariables()
        {
            var pi = new Value(Math.PI);
            _variables.Add("e", new Variable(new Value(Math.E)));
            _variables.Add("pi", new Variable(pi));
            _variables.Add("π", new Variable(pi));
            if (_settings.IsComplex)
            {
                _variables.Add("i", new Variable(Core.Complex.ImaginaryOne));
                _variables.Add("ei", new Variable(Math.E * Core.Complex.ImaginaryOne));
                _variables.Add("πi", new Variable(Math.PI * Core.Complex.ImaginaryOne));
            }
        }

        public void Parse(ReadOnlySpan<char> expression, bool allowAssignment = true)
        {
            Result = Core.Complex.Zero;
            _isCalculated = false;
            _functionDefinitionIndex = -1;
            var input = _input.GetInput(expression, allowAssignment);
            new SyntaxAnalyser(_functions).Check(input, out var isFucntionDefinition);
            _input.OrderOperators(input, isFucntionDefinition || _isSolver > 0 || _isPlotting, _assignmentIndex);
            if (isFucntionDefinition)
                AddFunction(input);
            else
                _rpn = Input.GetRpn(input);
        }

        private void PurgeCache()
        {
            for (int i = 0, count = _functions.Count; i < count; ++i)
                _functions[i].PurgeCache();
        }

        internal void ClearCache()
        {
            for (int i = 0, count = _functions.Count; i < count; ++i)
                _functions[i].ClearCache();
        }

        internal void BreakIfCanceled()
        {
            if (IsCanceled)
                Throw.InteruptedByUserException();
        }

        public void Calculate(bool isVisible = true)
        {
            if (!IsEnabled)
                Throw.CalculationsNotActiveException();

            BreakIfCanceled();
            if (_functionDefinitionIndex < 0)
            {
                //CompileBlocks();
                if (_variables.TryGetValue("ReturnAngleUnits", out var v))
                    _calc.ReturnAngleUnits = v.ValueByRef().Re != 0.0;
                else
                    _calc.ReturnAngleUnits = false;

                Result = _evaluator.Evaluate(_rpn, true).Complex;
                if (isVisible && Units is not null)
                    Result *= Units.Normalize();

                PurgeCache();
            }
            _isCalculated = true;
        }

        public void DefineCustomUnits()
        {
            if (_rpn?.Length > 2 &&
                _rpn[0].Type == TokenTypes.Unit &&
                _rpn[^1].Content == "=")
            {
                var s = _rpn[0].Content;
                ref var u = ref CollectionsMarshal.GetValueRefOrAddDefault(_units, s, out bool _);
                u = Unit.Get(string.Empty);
                u.Text = s;
            }
        }

        public double CalculateReal()
        {
            BreakIfCanceled();
            Value value = _evaluator.Evaluate(_rpn);
            CheckReal(value);
            Result = value.Re;
            return value.Re;
        }

        internal void ResetStack() => _evaluator.Reset();

        internal void CheckReal(in Value value)
        {
            if (_settings.IsComplex && !value.IsReal)
                Throw.ResultNotRealException(Core.Complex.Format(value.Complex, _settings.Decimals, OutputWriter.OutputFormat.Text));
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
                    while (input.Count != 0)
                    {
                        t = input.Dequeue();
                        if (t.Type == TokenTypes.BracketRight)
                        {
                            if (pt.Type != TokenTypes.Variable)
                                Throw.MissingFunctionParameterException();

                            break;
                        }

                        if (t.Type == TokenTypes.Variable)
                            parameters.Add(t.Content);
                        else if (t.Type != TokenTypes.Divisor)
                            Throw.IvalidFunctionTokenException(t.Content);

                        if (pt.Type == t.Type || pt.Type == TokenTypes.BracketLeft && t.Type == TokenTypes.Divisor)
                        {
                            if (t.Type == TokenTypes.Divisor)
                                Throw.MissingFunctionParameterException();
                        }
                        pt = t;
                    }
                    if (input.Count != 0 && input.Dequeue().Content == "=")
                    {
                        var rpn = Input.GetRpn(input);
                        var index = _functions.IndexOf(name);
                        var cf = index >= 0 && 
                            _functions[index].ParameterCount == parameters.Count ? 
                            _functions[index] : parameters.Count switch
                        {
                            1 => new CustomFunction1(),
                            2 => new CustomFunction2(),
                            3 => new CustomFunction3(),
                            _ => new CustomFunctionN()
                        };

                        cf.AddParameters(parameters);
                        cf.Rpn = rpn;
                        cf.IsRecursion = cf.CheckRecursion(null, _functions);
                        if (cf.IsRecursion)
                            Throw.CircularReferenceException(name);

                        if (IsEnabled)
                        {
                            BindParameters(cf.Parameters, cf.Rpn);
                            cf.SubscribeCache(this);
                        }
                        cf.Units = _targetUnits;
                        if (index >= 0)
                            cf.Change();
                        _functionDefinitionIndex = _functions.Add(name, cf);
                        return;
                    }
                }
            }
            Throw.InvalidFunctionDefinitionException();
        }

        private void BindParameters(ReadOnlySpan<Parameter> parameters, Token[] rpn)
        {
            for (int i = 0, len = rpn.Length; i < len; ++i)
            {
                var t = rpn[i];
                if (t.Type == TokenTypes.Variable)
                    foreach (var p in parameters)
                    {
                        if (t.Content == p.Name)
                        {
                            ((VariableToken)t).Variable = p.Variable;
                            t.Index = 1;
                            break;
                        }
                    }
                else if (t.Type == TokenTypes.Solver)
                    _solveBlocks[t.Index].BindParameters(parameters, this);
            }
        }

        internal Func<Value> Compile(string expression, ReadOnlySpan<Parameter> parameters)
        {
            var input = _input.GetInput(expression, false);
            new SyntaxAnalyser(_functions).Check(input, out _);
            _input.OrderOperators(input, true, 0);
            var rpn = Input.GetRpn(input);
            BindParameters(parameters, rpn);
            return _compiler.Compile(rpn);
        }

        private Func<Value> CompileRpn(Token[] rpn) => _compiler.Compile(rpn);
        public string ResultAsString
        {
            get
            {
                var s = Core.Complex.Format(Result, _settings.Decimals, OutputWriter.OutputFormat.Text);
                if (Units is null)
                    return s;

                if (Result.IsComplex)
                    return $"({s}){Units.Text}";

                return s + Units.Text;
            }
        }
        public override string ToString() => _output.Render(OutputWriter.OutputFormat.Text);
        public string ToHtml() => _output.Render(OutputWriter.OutputFormat.Html);
        public string ToXml() => _output.Render(OutputWriter.OutputFormat.Xml);

        [Serializable]
        public class MathParserException : Exception
        {
            internal MathParserException(string message) : base(message) { }
        }
    }
}