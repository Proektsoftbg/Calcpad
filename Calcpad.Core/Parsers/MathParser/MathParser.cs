using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        //Tolerance for PCG Solver and Eigensolver
        internal double Tol
        {
            get
            {
                var tol = GetSettingsVariable("Tol", 1e-6);
                return tol switch
                {
                    < 1e-15 => 1e-15,
                    > 1e-2 => 1e-2,
                    _ => tol
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
        internal bool Phasor { get; set; } = false;
        public int Degrees { set => _calc.Degrees = value; }
        internal int PlotWidth => (int)GetSettingsVariable("PlotWidth", 500);
        internal int PlotHeight => (int)GetSettingsVariable("PlotHeight", 300);
        internal bool PlotSVG => GetSettingsVariable("PlotSVG", 0) != 0;
        internal int PlotStep => (int)GetSettingsVariable("PlotStep", 0);

        public const char DecimalSymbol = '.';
        internal Complex Result
        {
            get
            {
                if (_result is IScalarValue scalar)
                    return scalar.Complex;
                if (_result is Vector vector && vector.Length >= 1)
                    return vector[0].D;
                if (_result is Matrix matrix && matrix.RowCount >= 1 && matrix.ColCount >= 1)
                    return matrix[0, 0].D;
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
            _matrixCalc = new MatrixCalculator(_vectorCalc) { Parser = this };
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
            var v = new ComplexValue(Result, Units);
            SetVariable("ans", v);
            SetVariable("ANS", v);
        }

        public void ClearCustomUnits() => _units.Clear();

        internal IScalarValue GetVariable(string name)
        {
            try
            {
                return (IScalarValue)_variables[name].Value;
            }
            catch
            {
                throw Exceptions.VariableNotExist(name);
            }
        }

        internal Variable GetVariableRef(string name)
        {
            if (_variables.TryGetValue(name, out Variable v))
                return v;

            v = new Variable();
            _variables.Add(name, v);
            _input.DefinedVariables.Add(name);
            return v;
        }

        public void SetVariable(string name, double value) =>
            SetVariable(name, new RealValue(value));

        internal void SetVariable(string name, in IScalarValue value)
        {
            IScalarValue scalar = value is RealValue || _settings.IsComplex ?
                value :
                new RealValue(value.Re, value.Units);

            if (_variables.TryGetValue(name, out var variable))
                variable.Assign(scalar);
            else
            {
                _variables.Add(name, new Variable(scalar));
                _input.DefinedVariables.Add(name);
            }
        }

        internal string[][] GetMatrix(string name, char type)
        {
            if (!_variables.TryGetValue(name, out var variable))
                throw Exceptions.VariableNotExist(name);

            if (type != 'N' && type != 'Y')
                throw Exceptions.InvalidType(type);

            var value = variable.Value;
            Matrix matrix;
            if (value is Vector vec)
            {
                if (type == 'Y')
                    matrix = new Matrix(vec);
                else
                    matrix = new ColumnMatrix(vec);
            }
            else if (value is Matrix mat)
                matrix = mat;
            else
                throw Exceptions.VariableNotMatrix(name);

            var rows = matrix.RowCount;
            var cols = matrix.ColCount;
            if (type == 'Y')
            {
                switch (matrix)
                {
                    case ColumnMatrix:
                    case HpColumnMatrix:
                        var cmResult = new string[1][];
                        cmResult[0] = new string[rows];
                        for (int i = rows - 1; i >= 0; --i)
                            cmResult[0][i] = matrix[i, 0].ToString();
                        return cmResult;
                    case DiagonalMatrix:
                    case HpDiagonalMatrix:
                        var dmResult = new string[1][];
                        dmResult[0] = new string[rows];
                        for (int i = rows - 1; i >= 0; --i)
                            dmResult[0][i] = matrix[i, i].ToString();
                        return dmResult;
                    case LowerTriangularMatrix:
                    case HpLowerTriangularMatrix:
                        var ltmResult = new string[rows][];
                        for (int i = rows - 1; i >= 0; --i)
                        {
                            var size = 0;
                            for (int j = i; j >= 0; --j)
                                if (!Equals(matrix[i, j], RealValue.Zero))
                                {
                                    size = j;
                                    break;
                                }
                            ltmResult[i] = new string[size + 1];
                            for (int j = size; j >= 0; --j)
                                ltmResult[i][j] = matrix[i, j].ToString();
                        }
                        return ltmResult;
                    case UpperTriangularMatrix:
                    case SymmetricMatrix:
                    case HpUpperTriangularMatrix:
                    case HpSymmetricMatrix:
                        var utmResult = new string[rows][];
                        for (int i = rows - 1; i >= 0; --i)
                        {
                            var size = i;
                            for (int j = cols - 1; j >= i; --j)
                                if (!Equals(matrix[i, j], RealValue.Zero))
                                {
                                    size = j;
                                    break;
                                }
                            utmResult[i] = new string[size - i + 1];
                            for (int j = size; j >= i; --j)
                                utmResult[i][j - i] = matrix[i, j].ToString();
                        }
                        return utmResult;
                }
            }

            var result = new string[rows][];
            for (int i = rows - 1; i >= 0; --i)
            {
                var size = 0;
                for (int j = cols - 1; j >= 0; --j)
                    if (!Equals(matrix[i, j], RealValue.Zero))
                    {
                        size = j;
                        break;
                    }
                result[i] = new string[size + 1];
                for (int j = size; j >= 0; --j)
                    result[i][j] = matrix[i, j].ToString();
            }
            return result;
        }

        internal void SetMatrix(ReadOnlySpan<char> name, string[][] data, char type, bool isHp)
        {
            if (data == null || data.Length == 0 || (data[0]?.Length ?? 0) == 0)
                return;

            int rows = data.Length;
            var cols = data.Max(v => v.Length);
            var n = Math.Max(rows, cols);
            if ((type == 'C' || type == 'D') && cols != 1 && rows != 1)
                throw Exceptions.IndexOutOfRange($"{rows}, {cols}");

            Matrix matrix = isHp ?
             type switch
             {
                 'R' => new HpMatrix(rows, cols, null),
                 'C' => new HpColumnMatrix(n, null),
                 'D' => new HpDiagonalMatrix(n, null),
                 'L' => new HpLowerTriangularMatrix(rows, null),
                 'U' => new HpUpperTriangularMatrix(rows, null),
                 'S' => new HpSymmetricMatrix(rows, null),
                 _ => throw Exceptions.InvalidType(type)
             } :
             type switch
             {
                'R' => new Matrix(rows, cols),
                'C' => new ColumnMatrix(n),
                'D' => new DiagonalMatrix(n),
                'L' => new LowerTriangularMatrix(rows),
                'U' => new UpperTriangularMatrix(rows),
                'S' => new SymmetricMatrix(rows),
                _ => throw Exceptions.InvalidType(type)
            };
            switch (type)
            {
                case 'C':
                    if (rows == 1)
                        for (int j = 0; j < cols; ++j)
                            matrix[j, 0] = ParseValue(data[0][j]);
                    else
                        for (int i = 0; i < rows; ++i)
                            matrix[i, 0] = ParseValue(data[i][0]);
                    break;
                case 'D':
                    if (rows == 1)
                        for (int j = 0; j < cols; ++j)
                            matrix[j, j] = ParseValue(data[0][j]);
                    else
                        for (int i = 0; i < rows; ++i)
                            matrix[i, i] = ParseValue(data[i][0]);
                    break;
                case 'L':
                    for (int i = 0; i < rows; ++i)
                        for (int j = 0, m = Math.Min(data[i].Length, i + 1); j < m; ++j)
                            matrix[i, j] = ParseValue(data[i][j]);
                    break;
                case 'U':
                case 'S':
                    for (int i = 0; i < rows; ++i)
                        for (int j = 0, m = Math.Min(data[i].Length, rows - i); j < m; ++j)
                            matrix[i, i + j] = ParseValue(data[i][j]);

                    break;
                default:
                    for (int i = 0; i < rows; ++i)
                        for (int j = 0, m = data[i].Length; j < m; ++j)
                            matrix[i, j] = ParseValue(data[i][j]);
                    break;
            }
            var nameString = name.ToString();
            if (_variables.TryGetValue(nameString, out var variable))
                variable.Assign(matrix);
            else
            {
                _variables.Add(nameString, new Variable(matrix));
                _input.DefinedVariables.Add(nameString);
            }
        }

        internal void SetVector(ReadOnlySpan<char> name, string[][] data, bool isHp)
        {
            if (data == null || data.Length == 0 || data[0].Length == 0)
                return;

            var n = data.Length;
            var length = data.Sum(v => v.Length);
            RealValue[] values = new RealValue[length];
            var k = 0;
            for (int i = 0; i < n; ++i)
                for (int j = 0, m = data[i].Length; j < m; ++j)
                    values[k++] = ParseValue(data[i][j]);

            Vector vector = isHp ? new HpVector(values) : new LargeVector(values);
            var nameString = name.ToString();
            if (_variables.TryGetValue(nameString, out var variable))
                variable.Assign(vector);
            else
            {
                _variables.Add(nameString, new Variable(vector));
                _input.DefinedVariables.Add(nameString);
            }
        }

        private static RealValue ParseValue(ReadOnlySpan<char> s)
        {
            if (s.Length == 0)
                return RealValue.Zero;

            var isUnitChar = true;
            var n = s.IndexOf('^');
            if (n < 0)
                n = s.Length;

            var numFormat = CultureInfo.CurrentCulture.NumberFormat;
            var systemDecimalSymbol = numFormat.NumberDecimalSeparator[0];
            while (n > 0)
            {
                var c = s[n - 1];
                if (char.IsDigit(c) || c == DecimalSymbol || c == systemDecimalSymbol)
                {
                    if (isUnitChar)
                        break;

                    isUnitChar = false;
                }
                else
                    isUnitChar = char.IsLetter(c) || Validator.UnitChars.Contains(c);

                --n;
            }
            var d = 1d;
            var numberSpan = s[..n];
            if (n > 0 &&
                !double.TryParse(numberSpan, CultureInfo.InvariantCulture.NumberFormat, out d) &&
                !double.TryParse(numberSpan, numFormat, out d))
                throw Exceptions.InvalidNumber(numberSpan.ToString());

            Unit u = null;
            if (n < s.Length)
            {
                var unitSpan = s[n..];
                if (!Unit.TryGet(unitSpan.ToString(), out u))
                    u = UnitsParser.Parse(unitSpan, null);
            }
            return n == 0 ? new(u) : new(d, u);
        }


        internal void SetUnit(string name, ComplexValue value)
        {
            var d = value.A;
            if (Math.Abs(d) == 0d)
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
                throw Exceptions.UnitNotExist(name);
            }
        }

        internal void Cancel() => IsCanceled = true;

        private void InitVariables()
        {
            var pi = new RealValue(Math.PI);
            _variables.Add("e", new Variable(new RealValue(Math.E)));
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
            _result = RealValue.Zero;
            _isCalculated = false;
            _functionDefinitionIndex = -1;
            var input = _input.GetInput(expression, allowAssignment);
            new SyntaxAnalyser(_functions).Check(input, IsCalculation && IsEnabled, out var isFunctionDefinition);
            _input.OrderOperators(input, isFunctionDefinition || _isSolver > 0 || IsPlotting, _assignmentIndex);
            if (isFunctionDefinition)
            {
                if (_isSolver > 0)
                    throw Exceptions.FunctionDefinitionInSolver();

                _rpn = null;
                AddFunction(input);
            }
            else
                _rpn = Input.GetRpn(input);
        }

        public bool ReadEquationFromCache(int cacheId)
        {
            _result = RealValue.Zero;
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

            var equation = new Equation(_rpn, _targetUnits, f);
            _equationCache.Add(equation);
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
                throw Exceptions.InteruptedByUser();
        }

        public void Calculate(bool isVisible = true, int cacheId = -1)
        {
            if (!IsEnabled)
                throw Exceptions.CalculationsNotActive();

            BreakIfCanceled();
            if (_functionDefinitionIndex < 0)
            {
                bool isAssignment = _rpn[0].Type == TokenTypes.Variable && _rpn[^1].Content == "=";
                _calc.ReturnAngleUnits = GetSettingsVariable("ReturnAngleUnits", 0) != 0d;
                Func<IValue> f = null;
                if (!isVisible && cacheId >= 0 && cacheId < _equationCache.Count)
                    f = _equationCache[cacheId].Function;

                if (f is null)
                {
                    _result = _evaluator.Evaluate(_rpn, true);
                    if (isVisible && !isAssignment)
                        _evaluator.NormalizeUnits(ref _result);
                }
                else
                {
                    _result = f();
                    if (isAssignment && _rpn[0] is VariableToken vt)
                    {
                        ref var val = ref vt.Variable.ValueByRef();
                        Units = Evaluator.ApplyUnits(ref val, _targetUnits);
                    }
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

            if (_result is RealValue real)
            {
                _result = new RealValue(real.D, Units);
                return real.D;
            }

            if (_result is ComplexValue complex)
            {
                CheckReal(complex);
                _result = new RealValue(complex.A, Units);
                return complex.A;
            }
            throw Exceptions.MustBeScalar(Exceptions.Items.Result);
        }

        internal void ResetStack() => _evaluator.Reset();

        internal void CheckReal(in IScalarValue value)
        {
            if (_settings.IsComplex && !value.IsReal)
                throw Exceptions.ResultNotReal(Core.Complex.Format(value.Complex, _settings.Decimals, Phasor, OutputWriter.OutputFormat.Text));
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
                                throw Exceptions.MissingFunctionParameter();

                            break;
                        }
                        if (t.Type == TokenTypes.Variable)
                            parameters.Add(t.Content);
                        else if (t.Type != TokenTypes.Divisor)
                            throw Exceptions.InvalidFunctionToken(t.Content);

                        if ((pt.Type == t.Type ||
                            pt.Type == TokenTypes.BracketLeft &&
                            t.Type == TokenTypes.Divisor) && t.Type == TokenTypes.Divisor)
                            throw Exceptions.MissingFunctionParameter();
                        pt = t;
                    }
                    if (input.Count != 0 && input.Dequeue().Content == "=")
                    {
                        var n = parameters.Count;
                        var rpn = Input.GetRpn(input);
                        var index = _functions.IndexOf(name);
                        var cf = index >= 0 && _functions[index].ParameterCount == n ?
                            _functions[index] :
                            CreateFunction(n);

                        cf.AddParameters(parameters);
                        cf.Rpn = rpn;
                        try
                        {
                            cf.IsRecursion = cf.CheckRecursion(null, _functions);
                        }
                        catch
                        {
                            throw Exceptions.CircularReference(name);
                        }
                        if (cf.IsRecursion)
                            throw Exceptions.CircularReference(name);

                        if (IsEnabled)
                        {
                            BindParameters(cf.Parameters, cf.Rpn);
                            SubscribeOnChange(cf.Rpn, cf.Clear);
                        }
                        cf.Units = _targetUnits;
                        if (index >= 0)
                            cf.Clear();
                        _functionDefinitionIndex = _functions.Add(name, cf);
                        return;
                    }
                }
            }
            throw Exceptions.InvalidFunctionDefinition();
        }

        private static CustomFunction CreateFunction(int parameterCount)
        {
            return parameterCount switch
            {
                1 => new CustomFunction1(),
                2 => new CustomFunction2(),
                3 => new CustomFunction3(),
                _ => new CustomFunctionN()
            };
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
            return _compiler.Compile(rpn, false);
        }

        private Func<IValue> CompileRpn(Token[] rpn, bool allowAssignment = false) => _compiler.Compile(rpn, allowAssignment);
        public string ResultAsString
        {
            get
            {
                if (_result is IScalarValue scalar)
                    return FormatResultValue(scalar);

                var sb = new StringBuilder();
                sb.Append('[');
                if (_result is Vector vector)
                    for (int i = 0, len = vector.Length - 1; i <= len; ++i)
                    {
                        sb.Append(FormatResultValue(vector[i]));
                        if (i < len)
                            sb.Append(OutputWriter.VectorSpacing[0]);
                    }
                else if (_result is Matrix matrix)
                    for (int i = 0, m = matrix.RowCount - 1; i <= m; ++i)
                        for (int j = 0, n = matrix.ColCount - 1; j <= n; ++j)
                        {
                            sb.Append(FormatResultValue(matrix[i, j]));
                            if (j < n)
                                sb.Append(OutputWriter.VectorSpacing[0]);
                            else if (i < m)
                                sb.Append('|');
                        }

                sb.Append(']');
                return sb.ToString();

                string FormatResultValue(in IScalarValue value)
                {
                    var s = Core.Complex.Format(value.Complex, _settings.Decimals, Phasor, OutputWriter.OutputFormat.Text);
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
                if (_result is IScalarValue scalar)
                    return FormatResultValue(scalar);

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

                string FormatResultValue(in IScalarValue value)
                {
                    var decimals = _settings.Decimals;
                    if (value is RealValue real)
                        return Convert(real.D);

                    var c = value.Complex;
                    if (c.IsReal)
                        return Convert(c.Re);

                    if (c.IsImaginary)
                        return Convert(c.Im) + 'i';

                    var sa = Convert(c.Re);
                    var sb = Convert(Math.Abs(c.Im));
                    return c.Im < 0 ? $"{sa} – {sb}i" : $"{sa} + {sb}i";

                    string Convert(double d) =>
                        Math.Round(d, decimals).ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        public override string ToString() => _output.Render(OutputWriter.OutputFormat.Text);
        public string ToHtml() => _output.Render(OutputWriter.OutputFormat.Html);
        public string ToXml() => _output.Render(OutputWriter.OutputFormat.Xml);

        internal double GetSettingsVariable(string name, double defaultValue)
        {
            if (_variables.TryGetValue(name, out var v))
            {
                ref var ival = ref v.ValueByRef();
                if (ival is IScalarValue scalar)
                    return scalar.Re;

                if (ival is not null)
                    throw Exceptions.MustBeScalar(Exceptions.Items.Variable);
            }
            return defaultValue;
        }

        private void SubscribeOnChange(Token[] rpn, Action onChange)
        {
            var len = rpn.Length;
            if (len == 0)
                return;

            var last = rpn[^1];
            var i0 = 0;
            if (last.Content == "=")
            {
                var t = rpn[0].Type;
                if (t == TokenTypes.Variable || 
                    t == TokenTypes.Vector || 
                    t == TokenTypes.Matrix)
                        i0 = 1;
            }
            for (int i = i0; i < len; ++i)
            {
                var t = rpn[i];
                if (t is VariableToken vt && vt.Variable is not null)
                    vt.Variable.OnChange += onChange;
                else if (t.Type == TokenTypes.Solver)
                    _solveBlocks[(int)t.Index].OnChange += onChange;
                else if (t.Type == TokenTypes.CustomFunction && t.Index != -1)
                    _functions[t.Index].OnChange += onChange;
            }
        }
    }
}