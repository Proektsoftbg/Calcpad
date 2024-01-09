using System;
using System.Collections.Generic;
using System.Text;

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
            private int _decimals;
            private bool _formatEquations;
            private int _assignmentIndex;


            internal Output(MathParser parser)
            {
                _parser = parser;
                _functions = parser._functions;
                _solveBlocks = parser._solveBlocks;
                _stringBuilder = parser._stringBuilder;
            }

            internal string Render(OutputWriter.OutputFormat format)
            {
                _formatEquations = _parser._settings.FormatEquations;
                _decimals = _parser._settings.Decimals;
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
                                _parser.VariableSubstitution == VariableSubstitutionOptions.VariablesAndSubstitutions ||
                                _parser.VariableSubstitution == VariableSubstitutionOptions.SubstitutionsOnly)
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
                            if (!_parser._hasVariables && _assignmentIndex > 0)
                                subst = _stringBuilder.ToString()[_assignmentIndex..];
                        }
                        var res = writer.FormatValue(new Value(_parser.Result, _parser.Units), _parser._settings.Decimals);
                        if (hasOperators && res != subst || string.IsNullOrEmpty(subst))
                        {
                            _stringBuilder.Append(assignment)
                                .Append(res);
                        }
                        if (splitted)
                            _stringBuilder.Append("</span>");
                    }
                }
                return _stringBuilder.ToString();
            }

            private static readonly int PlusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['+']];
            private static readonly int MinusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['-']];

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
                    if (tt == TokenTypes.Solver)
                        RenderSolverToken(t, writer);
                    else if (tt == TokenTypes.Input)
                        RenderInputToken(t, i);
                    else if (tt == TokenTypes.Constant || tt == TokenTypes.Unit)
                        RenderConstantToken(t, i);
                    else if (tt == TokenTypes.Variable)
                        RenderVariableToken(t, i);
                    else
                    {
                        var b = stackBuffer.Pop();
                        if (t.Content == NegateString)
                            RenderNegationToken(t, b, ref hasOperators);
                        else if (tt == TokenTypes.Operator)
                            RenderOperatorToken(t, b, ref hasOperators);
                        else
                        {
                            if (tt == TokenTypes.Function2)
                                RenderFunction2Token(t, b);
                            else if (tt == TokenTypes.If)
                                RenderIfToken(t, b);
                            else if (tt == TokenTypes.MultiFunction)
                                RenderMultiFunctionToken(t, b);
                            else if (tt == TokenTypes.CustomFunction)
                                RenderCustomFunctionToken(t, b);
                            else if (t.Content == "!")
                                RenderFactorialToken(t, b);
                            else if (t.Content == "sqr" || t.Content == "sqrt" || t.Content == "cbrt")
                                RenderRootToken(t, b, t.Content == "cbrt" ? "3" : "2");
                            else if (t.Content == "abs")
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
                    t.Content = RenderSolver(t.Index, substitute, _formatEquations, writer);
                    if (_solveBlocks[t.Index].IsFigure && !substitute)
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
                        v = varToken.Variable.Value;
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
                    var v = i > 0 && vt.Content == _parser._backupVariable.Key ?
                        _parser._backupVariable.Value :
                        vt.Variable.Value;
                    if (substitute)
                    {
                        t.Content = writer.FormatValue(v, _decimals);
                        t.IsCompositeValue = v.IsComposite() || t.Content.Contains('×');
                        t.Order = Token.DefaultOrder;
                        if (_parser._settings.IsComplex && v.IsComplex && v.Units is null)
                            t.Order = v.Im < 0 ? MinusOrder : PlusOrder;
                    }
                    else
                    {
                        var s = !_parser._settings.Substitute && _parser._functionDefinitionIndex < 0 && _parser._isCalculated ?
                            textWriter.FormatValue(v, _decimals) :
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
                            sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset);

                        hasOperators = true;
                    }
                    t.Content = writer.FormatOperator(NegateChar) + sb;
                    t.Level = b.Level;
                    if (b.Type == TokenTypes.Constant)
                    {
                        t.Type = b.Type;
                        t.Order = b.Order;
                        t.ValType = b.ValType;
                    }
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
                            sa = AddBrackets(sa, a.Level, a.MinOffset, a.MaxOffset);

                        if (content == "^")
                        {
                            if (a.IsCompositeValue || IsNegative(a))
                                sa = AddBrackets(sa, a.Level, a.MinOffset, a.MaxOffset);

                            if (writer is TextWriter && (IsNegative(b) || b.Order != Token.DefaultOrder))
                                sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset);

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
                                sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset);

                            var level = 0;
                            if (formatEquation)
                            {
                                level = (a.Level + b.Level + 1);
                                if (level >= 4)
                                {
                                    if (a.Order > t.Order)
                                        sa = AddBrackets(sa, a.Level, a.MinOffset, a.MaxOffset);

                                    if (b.Order > t.Order)
                                        sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset);
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
                    }
                }

                void RenderFunction2Token(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    var a = stackBuffer.Pop();
                    if (t.Content == "root")
                        RenderRootToken(t, a, sb);
                    else
                    {
                        t.Level = Math.Max(a.Level, b.Level);
                        t.MinOffset = Math.Min(a.MinOffset, b.MinOffset);
                        t.MaxOffset = Math.Max(a.MaxOffset, b.MaxOffset);
                        t.Content = writer.FormatFunction(t.Content) + AddBrackets(a.Content + div + sb, t.Level, t.MinOffset, t.MaxOffset);
                    }
                }

                void RenderIfToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    var a = stackBuffer.Pop();
                    var c = stackBuffer.Pop();
                    if (_formatEquations)
                    {
                        t.Level = (Math.Max(a.Level, c.Level) + b.Level + 1);
                        t.Content = writer.FormatIf(c.Content, a.Content, sb, t.Level);
                    }
                    else
                    {
                        t.Level = Math.Max(a.Level, Math.Max(b.Level, c.Level));
                        t.Content = writer.FormatFunction(t.Content) + AddBrackets(c.Content + div + a.Content + div + sb, t.Level, t.MinOffset, t.MaxOffset);
                    }
                }

                void RenderMultiFunctionToken(RenderToken t, RenderToken b)
                {
                    var mfParamCount = t.ParameterCount - 1;
                    if (string.Equals(t.Content, "switch", StringComparison.OrdinalIgnoreCase) && _formatEquations)
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
                        t.Content = writer.FormatFunction(t.Content) +
                            AddBrackets(s, t.Level, t.MinOffset, t.MaxOffset);
                        t.MinOffset = 0;
                        t.MaxOffset = 0;
                    }
                }

                void RenderCustomFunctionToken(RenderToken t, RenderToken b)
                {
                    var cf = _functions[t.Index];
                    var cfParamCount = cf.ParameterCount - 1;
                    var s = RenderParameters(t, b, cfParamCount);
                    t.Content = writer.FormatVariable(t.Content, string.Empty) +
                        AddBrackets(s, t.Level, t.MinOffset, t.MaxOffset);
                    t.MinOffset = 0;
                    t.MaxOffset = 0;
                }

                string RenderParameters(RenderToken t, RenderToken b, int count)
                {
                    var s = b.Content;
                    t.Level = b.Level;
                    t.MinOffset = b.MinOffset;
                    t.MaxOffset = b.MaxOffset;
                    for (int j = 0; j < count; ++j)
                    {
                        var a = stackBuffer.Pop();
                        s = a.Content + div + s;
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
                        sb = AddBrackets(sb, b.Level, b.MinOffset, b.MaxOffset);
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

                void RenderFunctionToken(RenderToken t, RenderToken b)
                {
                    t.Content = (t.Type == TokenTypes.Function ?
                        writer.FormatFunction(t.Content) :
                        writer.FormatVariable(t.Content, string.Empty)) +
                        AddBrackets(b.Content, b.Level, b.MinOffset, b.MaxOffset);
                    t.Level = b.Level;
                }

                string AddBrackets(string s, int level, int minOffset, int maxOffset)
                {
                    var offset = minOffset + maxOffset;
                    level += (maxOffset - minOffset) / 2;
                    if (offset == 0)
                        return writer.AddBrackets(s, level);

                    return writer.AddBrackets(FixOffset(s, offset), level);
                }

                string FixOffset(string s, int offset) => offset < 0 ?
                    $"<span class=\"dvc up\">{s}</span>" : offset > 0 ?
                    $"<span class=\"dvc down\">{s}</span>" :
                    s;
            }

            private string RenderSolver(int index, bool substitute, bool formatEquations, OutputWriter writer)
            {
                if (substitute)
                    return writer.FormatValue(_solveBlocks[index].Result, _decimals);

                if (writer is HtmlWriter)
                    return _solveBlocks[index].ToHtml(formatEquations);

                if (writer is XmlWriter)
                    return _solveBlocks[index].ToXml();

                return _solveBlocks[index].ToString();
            }

            private static bool IsNegative(Token t)
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
                            if (i > 8 && s[(i - 7)..i] == "class=\"")
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