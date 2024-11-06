using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class Output
        {
            private readonly MathParser _parser;
            private readonly Container<CustomFunction> _functions;
            private readonly List<SolveBlock> _solveBlocks;
            private readonly StringBuilder _stringBuilder;
            private readonly int _decimals;
            private readonly bool _formatEquations;
            private readonly int _maxOutputCount = 20;
            private readonly bool _zeroSmallMatrixElements = true;  
            private int _assignmentIndex;

            internal const string VectorSpacing = " ";
            internal Output(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _stringBuilder = parser._stringBuilder;
                _formatEquations = _parser._settings.FormatEquations;
                _decimals = _parser._settings.Decimals;
                _maxOutputCount = parser._settings.MaxOutputCount;
                _zeroSmallMatrixElements = parser._settings.ZeroSmallMatrixElements;
            }

            internal string Render(OutputWriter.OutputFormat format)
            {
                _assignmentIndex = 0;
                Token[] rpn = _parser._rpn;
                OutputWriter writer = format switch
                {
                    OutputWriter.OutputFormat.Html => new HtmlWriter(),
                    OutputWriter.OutputFormat.Xml => new XmlWriter(),
                    _ => new TextWriter()
                };
                _stringBuilder.Clear();
                var delimiter = writer.FormatOperator(';');
                var assignment = writer.FormatOperator('=');
                if (_parser._functionDefinitionIndex >= 0)
                {
                    var cf = _functions[_parser._functionDefinitionIndex];
                    for (int i = 0, count = cf.ParameterCount; i < count; ++i)
                    {
                        _stringBuilder.Append(writer.FormatVariable(cf.ParameterName(i), string.Empty));
                        if (i < cf.ParameterCount - 1)
                            _stringBuilder.Append(delimiter);
                    }
                    var s = writer.AddBrackets(_stringBuilder.ToString());
                    _stringBuilder.Clear();
                    _stringBuilder.Append(writer.FormatVariable(_functions.LastName, string.Empty))
                        .Append(s)
                        .Append(assignment)
                        .Append(RenderRpn(cf.Rpn, false, writer, out _));
                  if (_parser.ShowWarnings && _parser._functionDefinitionIndex < _functions.Count - 1)
                    {
                        string warning = Messages.Function_redefined;
                        if (format == OutputWriter.OutputFormat.Text)
                            _stringBuilder.Append(warning);
                        else if (format == OutputWriter.OutputFormat.Html)
                            _stringBuilder.Append($"<b class=\"err\" title = \"{warning}\"> !!!</b>");
                    }
                }
                else
                {
                    var equation = RenderRpn(rpn, false, writer, out var hasOperators);
                    if (_parser.VariableSubstitution != VariableSubstitutionOptions.SubstitutionsOnly)
                        _stringBuilder.Append(equation);

                    if (_parser._isCalculated && _parser._functionDefinitionIndex < 0)
                    {
                        var subst = string.Empty;
                        var splitted = false;
                        if (
                            !(rpn.Length == 3 &&
                            rpn[2].Content == "=" &&
                            rpn[1].Type == TokenTypes.Solver
                            || rpn.Length == 1 &&
                            rpn[0].Type == TokenTypes.Solver)
                        )
                        {
                            if ((_parser._settings.Substitute &&
                                _parser.VariableSubstitution != VariableSubstitutionOptions.VariablesOnly)
                                && _parser._hasVariables)
                            {
                                subst = RenderRpn(rpn, true, writer, out hasOperators);
                                if (_parser.VariableSubstitution != VariableSubstitutionOptions.SubstitutionsOnly)
                                {
                                    _stringBuilder.Append(assignment);
                                    if (_parser.Split && format == OutputWriter.OutputFormat.Html)
                                    {
                                        splitted = true;
                                        _stringBuilder.Append($"<br/><span class=\"indent\">");
                                    }
                                }
                                _stringBuilder.Append(subst);
                            }
                            if (!_parser._hasVariables && _assignmentIndex > 0 && _assignmentIndex < _stringBuilder.Length)
                                subst = _stringBuilder.ToString()[_assignmentIndex..];
                        }
                        var dec = _parser._settings.Decimals;
                        var res = _parser._result switch
                        {
                            Value value => writer.FormatValue(value, dec),
                            Vector vector => RenderVector(vector, dec, writer, _maxOutputCount, _zeroSmallMatrixElements),
                            Matrix matrix => RenderMatrix(matrix, dec, writer, _maxOutputCount, _zeroSmallMatrixElements),
                            _ => null
                        };
                        if (hasOperators && res != subst || string.IsNullOrEmpty(subst))
                        {
                            if (_stringBuilder.Length > 0)
                                _stringBuilder.Append(assignment);
                                    
                            _stringBuilder.Append(res);
                        }
                        if (splitted)
                            _stringBuilder.Append("</span>");
                    }
                }
                return _stringBuilder.ToString();
            }

            private static readonly sbyte PlusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['+']];
            private static readonly sbyte MinusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['-']];

            private string RenderRpn(Token[] rpn, bool substitute, OutputWriter writer, out bool hasOperators)
            {
                var textWriter = new TextWriter();
                var stackBuffer = new Stack<RenderToken>();
                var div = writer.FormatOperator(';');
                var hairSpace = char.ConvertFromUtf32(0x200A)[0];
                hasOperators = _parser._targetUnits is not null;
                for (int i = 0, len = rpn.Length; i < len; ++i)
                {
                    var t = new RenderToken(rpn[i], 0);
                    var tt = t.Type;
                    var st = t.Content;
                    if (tt == TokenTypes.Solver)
                        RenderSolverToken(t, writer);
                    else if (tt == TokenTypes.Input)
                        RenderInputToken(t, i);
                    else if (tt == TokenTypes.Constant ||
                             tt == TokenTypes.Unit)
                        RenderConstantToken(t, i);
                    else if (tt == TokenTypes.Variable ||
                            (tt == TokenTypes.Vector ||
                             tt == TokenTypes.Matrix) &&
                             t.ParameterCount < 0)
                        RenderVariableToken(t, i);
                    else
                    {
                        var b = stackBuffer.Pop();
                        if (st == NegateString)
                            RenderNegationToken(t, b, ref hasOperators);
                        else if (tt == TokenTypes.Operator)
                            RenderOperatorToken(t, b, ref hasOperators);
                        else if (tt == TokenTypes.VectorIndex)
                            RenderVectorIndexToken(t, b);
                        else if (tt == TokenTypes.MatrixIndex)
                        {
                            var a = stackBuffer.Pop();
                            RenderMatrixIndexToken(t, a, b);
                        }
                        else
                        {
                            if (tt == TokenTypes.Function2 ||
                                tt == TokenTypes.VectorFunction2 ||
                                tt == TokenTypes.MatrixFunction2)
                                RenderFunction2Token(t, b);
                            else if (tt == TokenTypes.Function3 ||
                                     tt == TokenTypes.VectorFunction3 ||
                                     tt == TokenTypes.MatrixFunction3)
                                RenderFunction3Token(t, b);
                            else if (tt == TokenTypes.MatrixFunction4)
                                RenderFunction4Token(t, b);
                            else if (tt == TokenTypes.MatrixFunction5)
                                RenderFunction5Token(t, b);
                            else if (tt == TokenTypes.Interpolation ||
                                     tt == TokenTypes.MultiFunction ||
                                     tt == TokenTypes.VectorMultiFunction ||
                                     tt == TokenTypes.MatrixMultiFunction)
                                RenderMultiFunctionToken(t, b);
                            else if (tt == TokenTypes.CustomFunction)
                                RenderCustomFunctionToken(t, b);
                            else if (tt == TokenTypes.Vector)
                                RenderVectorToken(t, b);
                            else if (tt == TokenTypes.RowDivisor)
                                t.Content = RenderParameters(t, b, t.ParameterCount);
                            else if (tt == TokenTypes.Matrix)
                                RenderMatrixToken(t, b);
                            else if (st == "!")
                                RenderFactorialToken(t, b);
                            else if (string.Equals(st, "sqr", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(st, "sqrt", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(st, "cbrt", StringComparison.OrdinalIgnoreCase))
                                RenderRootToken(t, b, string.Equals(st, "cbrt", StringComparison.OrdinalIgnoreCase) ? "3" : "2");
                            else if (string.Equals(st, "abs", StringComparison.OrdinalIgnoreCase))
                                RenderAbsToken(t, b);
                            else
                                RenderFunctionToken(t, b);

                            hasOperators = true;
                        }
                    }
                    stackBuffer.Push(t);
                }
                return stackBuffer.TryPop(out var result) ? result.Content : string.Empty;

                void RenderSolverToken(RenderToken t, OutputWriter writer)
                {
                    t.Content = RenderSolver((int)t.Index, substitute, _formatEquations, writer);
                    if (_solveBlocks[(int)t.Index].IsFigure && !substitute)
                    {
                        t.Type = TokenTypes.Solver;
                        t.Order = MinusOrder;
                        t.Level = 1;
                    }
                    else
                        t.Type = TokenTypes.Constant;
                }

                void RenderInputToken(RenderToken t, int i)
                {
                    var units = ((ValueToken)rpn[i]).Value.Units;
                    t.Content = writer.FormatInput(t.Content, units, _parser.Line, _parser._isCalculated);
                }

                void RenderConstantToken(RenderToken t, int i)
                {
                    var v = Value.Zero;
                    var hasValue = true;
                    if (rpn[i] is ValueToken valToken)
                        v = valToken.Value;
                    else if (rpn[i] is VariableToken varToken)
                    {
                        IValue ival = varToken.Variable.Value;
                        if (ival is Value value)
                            v = value;
                        else
                            hasValue = false;
                    }
                    else
                        hasValue = false;

                    if (hasValue)
                    {
                        if (t.Type == TokenTypes.Unit)
                        {
                            if (v.Units is null)
                            {
                                if (_parser._isCalculated)
                                    Throw.InvalidUnitsException(t.Content);
                            }
                            else
                                t.Content = writer.UnitString(v.Units);
                        }
                        else
                            t.Content = writer.FormatValue(v, _decimals);

                        t.IsCompositeValue = v.IsComposite();
                    }
                }

                void RenderVariableToken(RenderToken t, int i)
                {
                    var vt = (VariableToken)rpn[i];
                    var ival = i > 0 && vt.Content == _parser._backupVariable.Key ?
                        _parser._backupVariable.Value :
                        vt.Variable?.Value;
                    if (ival is null)
                        t.Content = writer.FormatVariable(t.Content, string.Empty);
                    else if (ival is Value value)
                    {
                        if (substitute)
                        {
                            t.Content = writer.FormatValue(value, _decimals);
                            t.IsCompositeValue = value.IsComposite() || t.Content.Contains('×');
                            t.Order = Token.DefaultOrder;
                            if (_parser._settings.IsComplex && value.IsComplex && value.Units is null)
                                t.Order = value.Im < 0 ? MinusOrder : PlusOrder;
                        }
                        else
                        {
                            var s = !_parser._settings.Substitute &&
                                     _parser._functionDefinitionIndex < 0 &&
                                     _parser._isCalculated ?
                                textWriter.FormatValue(value, _decimals) :
                                string.Empty;
                            t.Content = writer.FormatVariable(t.Content, s);
                        }
                    }
                    else if (ival is Vector vector)
                    {
                        var s = !_parser._settings.Substitute &&
                                 _parser._functionDefinitionIndex < 0 &&
                                 _parser._isCalculated ?
                            RenderVector(vector, _decimals, new TextWriter(), _maxOutputCount, _zeroSmallMatrixElements) :
                            string.Empty;
                        t.Content = writer.FormatVariable('\u20D7' + t.Content, s);
                    }
                    else
                    {
                        var matrix = (Matrix)ival;
                        var s = !_parser._settings.Substitute &&
                                 _parser._functionDefinitionIndex < 0 &&
                                 _parser._isCalculated ?
                            RenderMatrix(matrix, _decimals, new TextWriter(), _maxOutputCount, _zeroSmallMatrixElements) :
                            string.Empty;
                        t.Content = writer.FormatVariable(t.Content, s);
                    }
                }

                void RenderNegationToken(RenderToken t, RenderToken b, ref bool hasOperators)
                {
                    var sb = b.Content;
                    bool isNegative = IsNegative(b);
                    if (isNegative || b.Order > Token.DefaultOrder)
                    {
                        if (b.Index == 1 && b.Level > 0)
                            sb = " " + sb;
                        else
                            sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset, '(', ')');

                        hasOperators = true;
                    }
                    else if (b.Type == TokenTypes.Variable)
                        hasOperators = true;

                    t.Content = writer.FormatOperator(NegateChar) + sb;
                    t.Level = b.Level;
                    if (b.Type == TokenTypes.Constant)
                    {
                        t.Type = b.Type;
                        t.Order = b.Order;
                        t.ValType = b.ValType;
                    }
                    if (b.ValType == ValueTypes.Vector)
                        t.ValType = ValueTypes.Vector;
                }

                void RenderOperatorToken(RenderToken t, RenderToken b, ref bool hasOperators)
                {
                    var sb = b.Content;
                    if (substitute && t.Content == "=")
                    {
                        t.Content = sb;
                        t.Level = b.Level;
                    }
                    else
                    {
                        var content = t.Content;
                        var formatEquation = writer is not TextWriter &&
                            (_formatEquations && content == "/" || content == "÷");

                        if (!stackBuffer.TryPop(out var a))
                            a = new RenderToken(string.Empty, content == "-" ? TokenTypes.Constant : TokenTypes.None, 0);

                        var sa = a.Content;
                        if (a.Order > t.Order && !formatEquation)
                            sa = AddBrackets(sa, a.Level, a.MinOffset, a.MaxOffset, '(', ')');

                        if (content == "^" &&
                            (a.ValType != ValueTypes.Vector &&
                            b.ValType != ValueTypes.Vector))
                        {
                            if (a.IsCompositeValue || IsNegative(a))
                                sa = AddBrackets(sa, a.Level, a.MinOffset, a.MaxOffset, '(', ')');

                            if (writer is TextWriter && (IsNegative(b) || b.Order != Token.DefaultOrder))
                                sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset, '(', ')');
                            t.Content = writer.FormatPower(sa, sb, a.Level, a.Order);

                            t.Level = a.Level;
                            t.ValType = a.ValType;
                            if (a.ValType != ValueTypes.Unit)
                                hasOperators = true;
                        }
                        else
                        {
                            if (!formatEquation && (
                                b.Order > t.Order ||
                                b.Order == t.Order && (content == "-" || content == "/") ||
                                IsNegative(b) && content != "="))
                                sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset, '(', ')');

                            var level = 0;
                            if (formatEquation)
                            {
                                level = (a.Level + b.Level + 1);
                                if (level >= 4)
                                {
                                    if (a.Order > t.Order)
                                        sa = AddBrackets(sa, a.Level, a.MinOffset, a.MaxOffset, '(', ')');

                                    if (b.Order > t.Order)
                                        sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset, '(', ')');
                                }
                                t.Content = writer.FormatDivision(sa, sb, level);
                                if (level < 4)
                                {
                                    if (level == 2 && writer is HtmlWriter)
                                    {
                                        t.Offset = a.Level - b.Level;
                                        if (t.Offset != 0)
                                            t.Content = HtmlWriter.OffsetDivision(t.Content, t.Offset);
                                    }
                                }
                                else
                                    level = Math.Max(a.Level, b.Level);
                            }
                            else
                            {
                                if (writer is not TextWriter)
                                    level = Math.Max(a.Level, b.Level);

                                if (content == "*" && a.ValType == ValueTypes.Number && b.ValType == ValueTypes.Unit)
                                {
                                    if (writer is TextWriter)
                                        t.Content = sa + sb;
                                    else
                                        t.Content = sa + hairSpace + sb;
                                }
                                else
                                    t.Content = sa + writer.FormatOperator(content[0]) + sb;

                            }
                            t.ValType = a.ValType == b.ValType ? a.ValType : ValueTypes.NumberWithUnit;
                            t.Level = level;
                            t.MinOffset = Math.Min(Math.Min(a.MinOffset, b.MinOffset), t.Offset);
                            t.MaxOffset = Math.Max(Math.Max(a.MaxOffset, b.MaxOffset), t.Offset);
                            if (content == "=")
                                _assignmentIndex = t.Content.Length - sb.Length;
                            else
                            {
                                if (t.Order != 1 && !(a.ValType == ValueTypes.Number && b.ValType == ValueTypes.Unit))
                                    hasOperators = true;
                            }
                        }
                        if (a.ValType == ValueTypes.Vector ||
                            b.ValType == ValueTypes.Vector)
                            t.ValType = ValueTypes.Vector;
                    }
                }

                void RenderFunctionToken(RenderToken t, RenderToken b)
                {
                    var st = t.Content;
                    t.Content = (t.Type == TokenTypes.Function ||
                                 t.Type == TokenTypes.VectorFunction ||
                                 t.Type == TokenTypes.MatrixFunction ?
                        writer.FormatFunction(st) :
                        writer.FormatVariable(st, string.Empty)) +
                        AddBrackets(b.Content, b.Level, b.MinOffset, b.MaxOffset, '(', ')');
                    t.Level = b.Level;

                    if (t.Type == TokenTypes.Function &&
                        b.ValType == ValueTypes.Vector ||
                        t.Type == TokenTypes.VectorFunction &&
                        VectorCalculator.IsVectorResultFunction(st))
                        t.ValType = ValueTypes.Vector;
                }

                void RenderFunction2Token(RenderToken t, RenderToken b)
                {
                    var st = t.Content;
                    var a = stackBuffer.Pop();
                    if (string.Equals(st, "root", StringComparison.OrdinalIgnoreCase))
                        RenderRootToken(t, a, b.Content);
                    else
                    {
                        t.Level = Math.Max(a.Level, b.Level);
                        t.MinOffset = Math.Min(a.MinOffset, b.MinOffset);
                        t.MaxOffset = Math.Max(a.MaxOffset, b.MaxOffset);
                        t.Content = writer.FormatFunction(st) +
                            AddBrackets(
                                a.Content + div + b.Content,
                                t.Level, t.MinOffset, t.MaxOffset, '(', ')');
                    }
                    if (t.Type == TokenTypes.Function2 &&
                        (a.ValType == ValueTypes.Vector ||
                        b.ValType == ValueTypes.Vector) ||
                        t.Type == TokenTypes.VectorFunction2 &&
                        VectorCalculator.IsVectorResultFunction(st))
                        t.ValType = ValueTypes.Vector;
                }

                void RenderFunction3Token(RenderToken t, RenderToken c)
                {
                    var st = t.Content;
                    var b = stackBuffer.Pop();
                    var a = stackBuffer.Pop();
                    if (_formatEquations && t.Type == TokenTypes.Function3 && t.Index == 0)
                    {
                        t.Level = (Math.Max(a.Level, b.Level) + c.Level + 1);
                        t.Content = writer.FormatIf(a.Content, b.Content, c.Content, t.Level);
                    }
                    else
                    {
                        t.Level = Math.Max(a.Level, Math.Max(b.Level, c.Level));
                        t.Content = writer.FormatFunction(st) +
                            AddBrackets(
                                a.Content + div +
                                b.Content + div +
                                c.Content,
                                t.Level, t.MinOffset, t.MaxOffset, '(', ')');
                    }
                    if (t.Type == TokenTypes.Function3 &&
                        (b.ValType == ValueTypes.Vector ||
                        c.ValType == ValueTypes.Vector) ||
                        t.Type == TokenTypes.VectorFunction3 &&
                        VectorCalculator.IsVectorResultFunction(st))
                        t.ValType = ValueTypes.Vector;
                }

                void RenderFunction4Token(RenderToken t, RenderToken d)
                {
                    var st = t.Content;
                    var c = stackBuffer.Pop();
                    var b = stackBuffer.Pop();
                    var a = stackBuffer.Pop();
                    t.Level = Math.Max(
                        Math.Max(a.Level, b.Level),
                        Math.Max(c.Level, d.Level));
                    t.Content = writer.FormatFunction(st) +
                        AddBrackets(
                            a.Content + div +
                            b.Content + div +
                            c.Content + div +
                            d.Content,
                            t.Level, t.MinOffset, t.MaxOffset, '(', ')');
                    t.ValType = ValueTypes.Vector;
                }

                void RenderFunction5Token(RenderToken t, RenderToken e)
                {
                    var st = t.Content;
                    var d = stackBuffer.Pop();
                    var c = stackBuffer.Pop();
                    var b = stackBuffer.Pop();
                    var a = stackBuffer.Pop();
                    t.Level = Math.Max(
                        Math.Max(a.Level, b.Level),
                        Math.Max(c.Level, Math.Max(e.Level, d.Level)));
                    t.Content = writer.FormatFunction(st) +
                        AddBrackets(
                            a.Content + div +
                            b.Content + div +
                            c.Content + div +
                            d.Content + div +
                            e.Content,
                            t.Level, t.MinOffset, t.MaxOffset, '(', ')');
                    t.ValType = ValueTypes.Matrix;
                }

                void RenderMultiFunctionToken(RenderToken t, RenderToken b)
                {
                    var st = t.Content;
                    var mfParamCount = t.ParameterCount - 1;
                    if (string.Equals(st, "switch", StringComparison.OrdinalIgnoreCase) && _formatEquations)
                    {
                        var args = new string[mfParamCount + 1];
                        args[mfParamCount] = b.Content;
                        for (int j = mfParamCount - 1; j >= 0; --j)
                        {
                            var a = stackBuffer.Pop();
                            args[j] = a.Content;
                            if ((mfParamCount - j) % 2 == 1 || j == 0)
                                t.Level += (Math.Max(a.Level, b.Level) + 1);

                            b = a;
                        }
                        if (mfParamCount == 0)
                            t.Level = b.Level;
                        else
                            t.Level -= 1;

                        t.Content = writer.FormatSwitch(args, t.Level);
                    }
                    else
                    {
                        var s = RenderParameters(t, b, mfParamCount);
                        t.Content = writer.FormatFunction(st) +
                            AddBrackets(s, t.Level, t.MinOffset, t.MaxOffset, '(', ')');
                        t.MinOffset = 0;
                        t.MaxOffset = 0;
                    }
                    if (t.Type == TokenTypes.VectorMultiFunction)
                        t.ValType = ValueTypes.Vector;
                }

                void RenderCustomFunctionToken(RenderToken t, RenderToken b)
                {
                    var cf = _functions[t.Index];
                    var cfParamCount = cf.ParameterCount - 1;
                    var s = RenderParameters(t, b, cfParamCount);
                    t.Content = writer.FormatVariable(t.Content, string.Empty) +
                        AddBrackets(s, t.Level, t.MinOffset, t.MaxOffset, '(', ')');
                    t.MinOffset = 0;
                    t.MaxOffset = 0;
                }

                void RenderMatrixToken(RenderToken t, RenderToken b)
                {
                    var s = RenderParameters(t, b, t.ParameterCount, true);
                    t.Content = AddBrackets(s, t.Level, t.MinOffset, t.MaxOffset, '[', ']');
                    t.MinOffset = 0;
                    t.MaxOffset = 0;
                }

                void RenderVectorToken(RenderToken t, RenderToken b)
                {
                    var s = RenderParameters(t, b, t.ParameterCount);
                    t.Content = AddBrackets(s, t.Level, t.MinOffset, t.MaxOffset, '[', ']');
                    t.MinOffset = 0;
                    t.MaxOffset = 0;
                }

                string RenderParameters(RenderToken t, RenderToken b, int count, bool matrix = false)
                {
                    var s = b.Content;
                    t.Level = b.Level;
                    t.MinOffset = b.MinOffset;
                    t.MaxOffset = b.MaxOffset;
                    var d = matrix ? writer.FormatOperator('|') : div;
                    for (int j = 0; j < count; ++j)
                    {
                        var a = stackBuffer.Pop();
                        s = a.Content + d + s;
                        if (a.Level > t.Level)
                            t.Level = a.Level;

                        t.MinOffset = Math.Min(a.MinOffset, t.MinOffset);
                        t.MaxOffset = Math.Max(a.MaxOffset, t.MaxOffset);
                    }
                    return s;
                }

                void RenderFactorialToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    if (IsNegative(b) || b.Order > Token.DefaultOrder)
                        sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset, '(', ')');
                    t.Content = sb + writer.FormatOperator('!');
                    t.Level = b.Level;
                }

                void RenderRootToken(RenderToken t, RenderToken b, string s)
                {
                    var offset = b.MaxOffset + b.MinOffset;
                    b.Level += (b.MaxOffset - b.MinOffset) / 2;
                    var sb = offset == 0 ? b.Content : FixOffset(b.Content, offset);
                    t.Content = writer.FormatRoot(sb, _formatEquations, b.Level, s);
                    t.Level = b.Level;
                }

                void RenderAbsToken(RenderToken t, RenderToken b)
                {
                    t.Content = writer.FormatAbs(b.Content, b.Level);
                    t.Level = b.Level;
                }

                void RenderVectorIndexToken(RenderToken t, RenderToken b)
                {
                    var a = stackBuffer.Pop();
                    if (substitute)
                    {
                        Value value;
                        var variableName = t.Content + '.' + t.Index;
                        if (variableName == _parser._backupVariable.Key)
                            value = (Value)_parser._backupVariable.Value;
                        else
                        {
                            var vector = (Vector)_parser._variables[t.Content].Value;
                            value = vector[(int)t.Index - 1];
                        }
                        t.Content = writer.FormatValue(value, _decimals);
                    }
                    else
                    {
                        var s = t.Content.Contains('_') ?
                            writer.FormatOperator('.') :
                            string.Empty;
                        s += _parser._isCalculated && _parser._functionDefinitionIndex < 0 ?
                            t.Index.ToString() :
                            b.Content;
                        t.Content = writer.FormatSubscript(a.Content, s);
                    }
                }

                void RenderMatrixIndexToken(RenderToken t, RenderToken b, RenderToken c)
                {
                    var a = stackBuffer.Pop();
                    var i = (int)(t.Index / Vector.MaxLength);
                    var j = (int)(t.Index % Vector.MaxLength);
                    if (substitute)
                    {
                        Value value;
                        var variableName = t.Content + '.' + i + '.' + j;
                        if (variableName == _parser._backupVariable.Key)
                            value = (Value)_parser._backupVariable.Value;
                        else
                        {
                            var matrix = (Matrix)_parser._variables[t.Content].Value;
                            value = matrix[i - 1, j - 1];
                        }
                        t.Content = writer.FormatValue(value, _decimals);
                    }
                    else
                    {
                        var s = t.Content.Contains('_') ?
                            writer.FormatOperator('.') :
                            string.Empty;
                        var comma = writer.FormatOperator(',');
                        s += _parser._isCalculated && _parser._functionDefinitionIndex < 0 ?
                            $"{i}{comma}{j}" :
                            $"{b.Content}{comma}{c.Content}";
                        t.Content = writer.FormatSubscript(a.Content, s);
                    }
                }

                string AddBrackets(string s, int level, int minOffset, int maxOffset, char left, char right)
                {
                    var offset = minOffset + maxOffset;
                    level += (maxOffset - minOffset) / 2;
                    if (offset == 0)
                        return writer.AddBrackets(s, level, left, right);

                    return writer.AddBrackets(FixOffset(s, offset), level, left, right);
                }

                string FixOffset(string s, int offset) => offset < 0 ?
                    $"<span class=\"dvc up\">{s}</span>" : offset > 0 ?
                    $"<span class=\"dvc down\">{s}</span>" :
                    s;
            }

            private static string RenderMatrix(Matrix matrix, int decimals, OutputWriter writer, int maxCount, bool zeroSmallElements) =>
                writer.FormatMatrix(matrix, decimals, maxCount, zeroSmallElements);

            private static string RenderVector(Vector vector, int decimals, OutputWriter writer, int maxCount, bool zeroSmallElements)
            {
                var div = writer is XmlWriter ? 
                    XmlWriter.Run(VectorSpacing) :
                    writer is TextWriter?
                        "  " :
                        VectorSpacing;
                var sb = new StringBuilder();
                const double tol = 1e-14;
                var zeroThreshold = GetMaxVisibleVectorValue(vector, maxCount) * tol;
                if (zeroThreshold > tol)
                    zeroThreshold = tol;

                var len = vector.Length;
                for (int i = 0; i < len; ++i)
                {
                    if (i > 0)
                        sb.Append(div);

                    if (i == maxCount)
                    {
                        if (writer is HtmlWriter)
                        {
                            var n = len - maxCount;
                            sb.Append($"<span title=\"{n - Math.Sign(n - 1)} elements skipped.\">...</span>");

                        }
                        else if (writer is XmlWriter)
                            sb.Append(XmlWriter.Run("..."));
                        else
                            sb.Append("...");

                        sb.Append(div);
                        break;
                    }
                    var e = vector[i];
                    var d = Math.Abs(e.Re);
                    sb.Append(writer.FormatMatrixValue(e, decimals, zeroSmallElements && d < zeroThreshold));
                }
                var last = len - 1;
                if (maxCount < last)
                {
                    var e = vector[last];
                    var d = Math.Abs(e.Re);
                    sb.Append(writer.FormatMatrixValue(e, decimals, zeroSmallElements && d < zeroThreshold));
                }
                return writer.AddBrackets(sb.ToString(), 0, '[', ']');
            }

            private static double GetMaxVisibleVectorValue(Vector vector, int maxCount)
            {
                var maxAbs = 0d;
                var len = Math.Min(maxCount, vector.Size);
                for (int i = 0; i < len; ++i)
                {
                    var d = Math.Abs(vector[i].Re);
                    if (d > maxAbs)
                        maxAbs = d;
                }
                var last = vector.Length - 1;
                if (maxCount < last)
                {
                    var d = Math.Abs(vector[last].Re);
                    if (d > maxAbs)
                        maxAbs = d;
                }
                return maxAbs;
            }

            private string RenderSolver(int index, bool substitute, bool formatEquations, OutputWriter writer)
            {
                if (substitute)
                {
                    var result = _solveBlocks[index].Result;
                    if (result is Value value)
                        return writer.FormatValue(value, _decimals);
                    
                    if (result is Vector vector)
                        return RenderVector(vector, _decimals, writer, _maxOutputCount, _zeroSmallMatrixElements);

                    if (result is Matrix matirx)
                        return RenderMatrix(matirx, _decimals, writer, _maxOutputCount, _zeroSmallMatrixElements);

                    return writer.FormatValue(Value.NaN, _decimals);
                }
                if (writer is HtmlWriter)
                    return _solveBlocks[index].ToHtml(formatEquations);

                if (writer is XmlWriter)
                    return _solveBlocks[index].ToXml();

                return _solveBlocks[index].ToString();
            }

            private static bool IsNegative(RenderToken t)
            {
                ref var s = ref t.Content;
                var n = s.Length;
                if (t.Order != Token.DefaultOrder || n == 0)
                    return false;

                var isTag = false;
                for (int i = 0; i < n; ++i)
                {
                    var c = s[i];
                    if (c == '<')
                        isTag = true;
                    else if (c == '>')
                        isTag = false;
                    else if (isTag)
                    {
                        if (c == 'r')
                        {
                            if (i > 8 && string.Equals(s[(i - 7)..i], "class=\"", StringComparison.OrdinalIgnoreCase))
                                break;
                        }
                    }
                    else
                    {
                        if (Validator.IsDigit(c) || c == '(' || c == '|' || c == '√')
                            break;
                        else if (c == '-')
                            return true;
                    }
                }
                return false;
            }
        }
    }
}