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
        private readonly VectorCalculator _vectorCalc;
        private readonly MatrixCalculator _matrixCalc;
        private readonly Solver _solver;
        private readonly Compiler _compiler;
        private readonly Output _output;
        private readonly List<Equation> _equationCache = [];
        private IValue _result;
        private int _assignmentIndex;
        private bool _isCalculated;
        private int _isSolver;
        private Unit _targetUnits;
        private int _functionDefinitionIndex;
        private KeyValuePair<string, IValue> _backupVariable;
        private double Precision
        {
            get
            {
                var precision = GetSettingsVariable("Precision", 1e-14);
                return precision switch
                {
                    < 1e-15 => 1e-15,
                    > 1e-2 => 1e-2,
                    _ => precision
                };
            }
        }
        internal bool HasInputFields;
        //If MathParser has input, the line is not cached in ExpressionParser
        internal int Line;
        internal VariableSubstitutionOptions VariableSubstitution { get; set; }
        internal Unit Units { get; private set; }
        internal bool IsCanceled { get; private set; }
        internal bool IsEnabled { get; set; } = true;
        internal bool IsPlotting { get; set; }
        internal bool IsCalculation { get; set; }
        internal bool Split { get; set; }
        internal bool ShowWarnings { get; set; } = true;
        public int Degrees { set => _calc.Degrees = value; }
        internal int PlotWidth => (int)GetSettingsVariable("PlotWidth", 500);
        internal int PlotHeight => (int)GetSettingsVariable("PlotHeight", 300);
        internal int PlotStep => (int)GetSettingsVariable("PlotStep", 0);
        internal bool PlotSVG => GetSettingsVariable("PlotSVG", 0) != 0d;

        public const char DecimalSymbol = '.'; //CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
        internal Complex Result
        {
            get
            {
                if (_result is Value value)
                    return value.Complex;

                if (_result is Vector vector && vector.Length >= 1)
                    return vector[0].Complex;
                if (_result is Matrix matrix && matrix.RowCount >= 1 && matrix.ColCount >= 1)
                    return matrix[0, 0].Complex;
                return 0;
            }
        }


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

            _vectorCalc = new VectorCalculator(_calc);
            _matrixCalc = new MatrixCalculator(_vectorCalc);
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
                return (Value)_variables[name].Value;
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
            _result = Value.Zero;
            _isCalculated = false;
            _functionDefinitionIndex = -1;
            var input = _input.GetInput(expression, allowAssignment);
            new SyntaxAnalyser(_functions).Check(input, IsCalculation && IsEnabled, out var isFunctionDefinition);
            _input.OrderOperators(input, isFunctionDefinition || _isSolver > 0 || IsPlotting, _assignmentIndex);
            if (isFunctionDefinition)
            {
                if (_isSolver > 0)
                    Throw.FunctionDefinitionInSolverException();

                _rpn = null;
                AddFunction(input);
            }
            else
                _rpn = Input.GetRpn(input);
        }

        public bool ReadEquationFromCache(int cacheId)
        {
            _result = Value.Zero;
            _isCalculated = false;
            _functionDefinitionIndex = -1;
            if (cacheId >= 0 && cacheId < _equationCache.Count)
            {
                _targetUnits = _equationCache[cacheId].TargetUnits;  
                _rpn = _equationCache[cacheId].Rpn;
                return true;
            }
            return false;
        }

        public int WriteEquationToCache(bool isVisible = true)
        {
            if (_rpn is null)
                return -1;

            Func<IValue> f = null;
            if (!isVisible)
                f = _compiler.Compile(_rpn, true);

            _equationCache.Add(new Equation(_rpn, _targetUnits, f));
            return _equationCache.Count - 1;
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

        public void Calculate(bool isVisible = true, int cacheId = -1)
        {
            if (!IsEnabled)
                Throw.CalculationsNotActiveException();

            BreakIfCanceled();
            if (_functionDefinitionIndex < 0)
            {
                bool isAssignment = _rpn[0].Type == TokenTypes.Variable && _rpn[^1].Content == "=";
                _calc.ReturnAngleUnits = GetSettingsVariable("ReturnAngleUnits", 0) != 0d;
                Func<IValue> f = null;
                if (!isVisible && cacheId >= 0 && cacheId < _equationCache.Count)
                    f = _equationCache[cacheId].Function;

                if (f is not null)
                {
                    _result = f();
                    if (isAssignment && _rpn[0] is VariableToken vt)
                    {
                        ref var v = ref vt.Variable.ValueByRef();
                        Units = Evaluator.ApplyUnits(ref v, _targetUnits);
                    }
                }
                else
                {
                    _result = _evaluator.Evaluate(_rpn, true);
                    if (isVisible && !isAssignment)
                        _evaluator.NormalizeUnits(ref _result);
                }
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
            _result = _evaluator.Evaluate(_rpn);

            if (_result is Value value)
            {
                CheckReal(value);
                _result = new Value(value.Re, Units);
                return value.Re;
            }
            else
                Throw.MustBeScalarException(Throw.Items.Result);

            return double.NaN;
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

                        if ((pt.Type == t.Type || 
                            pt.Type == TokenTypes.BracketLeft && 
                            t.Type == TokenTypes.Divisor) && t.Type == TokenTypes.Divisor)
                            Throw.MissingFunctionParameterException();
                        pt = t;
                    }
                    if (input.Count != 0 && input.Dequeue().Content == "=")
                    {
                        var rpn = Input.GetRpn(input);
                        var index = _functions.IndexOf(name);
                        var cf = index >= 0 && _functions[index].ParameterCount == parameters.Count ?
                            _functions[index] :
                            parameters.Count switch
                            {
                                1 => new CustomFunction1(),
                                2 => new CustomFunction2(),
                                3 => new CustomFunction3(),
                                _ => new CustomFunctionN()
                            };

                        cf.AddParameters(parameters);
                        cf.Rpn = rpn;
                        try
                        {
                            cf.IsRecursion = cf.CheckRecursion(null, _functions);
                        }
                        catch
                        {
                            Throw.CircularReferenceException(name);
                        }
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
                if (t.Type == TokenTypes.Variable || t.Type == TokenTypes.Vector)
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
                    _solveBlocks[(int)t.Index].BindParameters(parameters, this);
            }
        }

        internal Func<IValue> Compile(string expression, ReadOnlySpan<Parameter> parameters)
        {
            if (!IsEnabled)
                return null;

            var input = _input.GetInput(expression, false);
            new SyntaxAnalyser(_functions).Check(input, IsEnabled, out _);
            _input.OrderOperators(input, true, 0);
            var rpn = Input.GetRpn(input);
            BindParameters(parameters, rpn);
            return _compiler.Compile(rpn);
        }

        private Func<IValue> CompileRpn(Token[] rpn) => _compiler.Compile(rpn);
        public string ResultAsString
        {
            get
            {
                if (_result is Value value)
                    return FormatResultValue(value);

                var sb = new StringBuilder();
                sb.Append('[');
                if (_result is Vector vector)
                    for (int i = 0, len = vector.Length - 1; i <= len; ++i)
                    {
                        sb.Append(FormatResultValue(vector[i]));
                        if (i < len)
                            sb.Append(Output.VectorSpacing[0]);
                    }
                else if (_result is Matrix matrix)
                    for (int i = 0, m = matrix.RowCount - 1; i <= m; ++i)
                        for (int j = 0, n = matrix.ColCount - 1; j <= n; ++j)
                        {
                            sb.Append(FormatResultValue(matrix[i, j]));
                            if (j < n)
                                sb.Append(Output.VectorSpacing[0]);
                            else if (i < m)
                                sb.Append('|');
                        }

                sb.Append(']');
                return sb.ToString();   

                string FormatResultValue(in Value value)
                {
                    var s = Core.Complex.Format(value.Complex, _settings.Decimals, OutputWriter.OutputFormat.Text);
                    if (Units is null)
                        return s;

                    if (value.IsComplex)
                        return $"({s}){Units.Text}";

                    return s + Units.Text;
                }
            }
        }

        public string ResultAsVal
        {
            get
            {
                if (_result is Value value)
                    return FormatResultValue(value);

                var sb = new StringBuilder();
                sb.Append('[');
                if (_result is Vector vector)
                    for (int i = 0, len = vector.Length - 1; i <= len; ++i)
                    {
                        sb.Append(FormatResultValue(vector[i]));
                        if (i < len)
                            sb.Append(", ");
                    }
                else if (_result is Matrix matrix)
                    for (int i = 0, m = matrix.RowCount - 1; i <= m; ++i)
                    {
                        sb.Append('[');
                        for (int j = 0, n = matrix.ColCount - 1; j <= n; ++j)
                        {
                            sb.Append(FormatResultValue(matrix[i, j]));
                            if (j < n)
                                sb.Append(", ");
                        }
                        sb.Append(']');
                        if (i < m)
                            sb.Append(", ");
                    }

                sb.Append(']');
                return sb.ToString();

                string FormatResultValue(in Value value) =>
                    Core.Complex.Format(value.Complex, _settings.Decimals, OutputWriter.OutputFormat.Text);
            }
        }



        public override string ToString() => _output.Render(OutputWriter.OutputFormat.Text);
        public string ToHtml() => _output.Render(OutputWriter.OutputFormat.Html);
        public string ToXml() => _output.Render(OutputWriter.OutputFormat.Xml);

        public class MathParserException : Exception
        {
            internal MathParserException(string message) : base(message) { }
        }

        private double GetSettingsVariable(string name, double defaultValue)
        {
            if (_variables.TryGetValue(name, out var v))
            {
                ref var value = ref v.ValueByRef();
                if (value is Value val)
                    return val.Re;

                if (value is not null)
                    Throw.MustBeScalarException(Throw.Items.Variable);
            }
            return defaultValue;
        }
    }
}