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
                    _stringBuilder.Append(writer.FormatVariable(_functions.LastName, string.Empty));
                    _stringBuilder.Append(s);
                    _stringBuilder.Append(assignment);
                    _stringBuilder.Append(RenderRpn(cf.Rpn, false, writer, out _));
                    if (_parser._functionDefinitionIndex < _functions.Count - 1)
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
                    _stringBuilder.Append(RenderRpn(rpn, false, writer, out var hasOperators));
                    if (_parser._isCalculated && _parser._functionDefinitionIndex < 0)
                    {
                        var subst = string.Empty;
                        if (
                            !(rpn.Length == 3 &&
                            rpn[2].Content == "=" &&
                            rpn[1].Type == TokenTypes.Solver
                            || rpn.Length == 1 &&
                            rpn[0].Type == TokenTypes.Solver)
                        )
                        {
                            string s = RenderRpn(rpn, true, writer, out hasOperators);
                            if (_parser._settings.Substitute && _parser._hasVariables)
                            {
                                subst = s;
                                _stringBuilder.Append(assignment);
                                _stringBuilder.Append(subst);
                            }
                            if (!_parser._hasVariables && _assignmentIndex > 0)
                                subst = _stringBuilder.ToString()[_assignmentIndex..];
                        }
                        var res = writer.FormatValue(new Value(_parser.Result, _parser.Units), _parser._settings.Decimals);
                        if (hasOperators && res != subst || string.IsNullOrEmpty(subst))
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
                                RenderRootToken(t, b);
                            else if (t.Content == "abs")
                                RenderAbsToken(t, b);
                            else
                                RenderFunctionToken(t, b);

                            hasOperators = true;
                        }
                    }
                    stackBuffer.Push(t);
                }

                if (stackBuffer.TryPop(out var result))
                    return result.Content;

                return string.Empty;

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
                            t.Content = writer.UnitString(v.Units);
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
                        sb = writer.AddBrackets(sb, b.Level);
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
                        if (b.Offset != 0 && writer is HtmlWriter)
                            sb = HtmlWriter.OffsetDivision(sb, b.Offset);

                        t.Content = sb;
                        t.Level = b.Level;
                    }
                    else
                    {
                        var content = t.Content;
                        if (!stackBuffer.TryPop(out var a))
                            a = new RenderToken(string.Empty, content == "-" ? TokenTypes.Constant : TokenTypes.None, 0);

                        var level = a.Level + b.Level + 1;
                        var isInline = writer is HtmlWriter && content != "^" && !(level < 4 && (content == "/" || content == "÷"));
                        if (b.Offset != 0 && isInline)
                            sb = HtmlWriter.OffsetDivision(sb, b.Offset);

                        var sa = a.Content;
                        if (a.Order > t.Order && !(_formatEquations && content == "/"))
                            sa = writer.AddBrackets(sa, a.Level);

                        if (a.Offset != 0 && isInline)
                            sa = HtmlWriter.OffsetDivision(sa, a.Offset);

                        if (content == "^")
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
                            var formatEquation = writer is not TextWriter &&
                                (_formatEquations && content == "/" || content == "÷");
                            if (
                                    !formatEquation &&
                                    (
                                        b.Order > t.Order ||
                                        b.Order == t.Order && (content == "-" || content == "/") ||
                                        IsNegative(b) && content != "="
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
                                    level = Math.Max(a.Level, b.Level);
                            }
                            else
                            {
                                if (writer is TextWriter)
                                    level = 0;
                                else
                                    level = Math.Max(a.Level, b.Level);

                                if (content == "*" && a.ValType == 1 && b.ValType == 2)
                                {
                                    if (writer is TextWriter)
                                        t.Content = sa + sb;
                                    else
                                        t.Content = sa + hairSpace + sb;
                                }
                                else
                                    t.Content = sa + writer.FormatOperator(content[0]) + sb;
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
                }

                void RenderFunction2Token(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    var a = stackBuffer.Pop();
                    if (t.Content == "root")
                    {
                        t.Content = writer.FormatRoot(a.Content, _formatEquations, a.Level, sb);
                        t.Level = b.Level;
                    }
                    else
                    {
                        t.Level = Math.Max(a.Level, b.Level);
                        t.Content = writer.FormatFunction(t.Content) + writer.AddBrackets(a.Content + div + sb, t.Level);
                    }
                }

                void RenderIfToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    var a = stackBuffer.Pop();
                    var c = stackBuffer.Pop();
                    if (_formatEquations)
                    {
                        t.Level = Math.Max(a.Level, c.Level);
                        if (t.Level == 0)
                            t.Level = b.Level + 1;
                        else
                            t.Level += b.Level == 0 ? 1 : b.Level;

                        t.Content = writer.FormatIf(c.Content, a.Content, sb, t.Level);
                    }
                    else
                    {
                        t.Level = Math.Max(a.Level, Math.Max(b.Level, c.Level));
                        t.Content = writer.FormatFunction(t.Content) + writer.AddBrackets(c.Content + div + a.Content + div + sb, t.Level);
                    }
                }

                void RenderMultiFunctionToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    var mfParamCount = t.ParameterCount - 1;

                    if (string.Equals(t.Content, "switch", StringComparison.OrdinalIgnoreCase) && _formatEquations)
                    {
                        var args = new string[mfParamCount + 1];
                        args[mfParamCount] = sb;
                        for (int j = mfParamCount - 1; j >= 0; --j)
                        {
                            var a = stackBuffer.Pop();
                            args[j] = a.Content;
                            if ((mfParamCount - j) % 2 == 1 || j == 0)
                                t.Level += Math.Max(a.Level, b.Level) + 1;

                            b = a;
                        }
                        t.Level -= 1;
                        t.Content = writer.FormatSwitch(args, t.Level);
                    }
                    else
                    {
                        t.Level = b.Level;
                        var s = sb;
                        for (int j = 0; j < mfParamCount; ++j)
                        {
                            var a = stackBuffer.Pop();
                            s = a.Content + div + s;
                            if (a.Level > t.Level)
                                t.Level = a.Level;
                        }
                        t.Content = writer.FormatFunction(t.Content) + writer.AddBrackets(s, t.Level);
                    }
                }

                void RenderCustomFunctionToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
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
                    t.Content = writer.FormatVariable(t.Content, string.Empty) + writer.AddBrackets(s, t.Level);
                }

                void RenderFactorialToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    if (IsNegative(b) || b.Order > Token.DefaultOrder)
                        sb = writer.AddBrackets(sb, b.Level);
                    t.Content = sb + writer.FormatOperator('!');
                    t.Level = b.Level;
                }

                void RenderRootToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    t.Content = writer.FormatRoot(sb, _formatEquations, b.Level, t.Content == "cbrt" ? "3" : "2");
                    t.Level = b.Level;
                }

                void RenderAbsToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    t.Content = writer.FormatAbs(sb, b.Level);
                    t.Level = b.Level;
   
                }

                void RenderFunctionToken(RenderToken t, RenderToken b)
                {
                    var sb = b.Content;
                    t.Content = (t.Type == TokenTypes.Function ?
                        writer.FormatFunction(t.Content) :
                        writer.FormatVariable(t.Content, string.Empty)) + writer.AddBrackets(sb, b.Level);
                    t.Level = b.Level;
                }
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
                var n = t.Content.Length;
                if (t.Order != Token.DefaultOrder || n == 0)
                    return false;
                var isTag = false;
                for (int i = 0; i < n; ++i)
                {
                    var c = t.Content[i];
                    if (c == '<')
                        isTag = true;
                    else if (c == '>')
                        isTag = false;
                    else if (!isTag)
                    {
                        if (Validator.IsDigit(c) || c == '(' || c == '|')
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