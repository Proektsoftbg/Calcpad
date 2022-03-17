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
                if (format == OutputWriter.OutputFormat.Html || format == OutputWriter.OutputFormat.Text)
                    _parser._backupVariable = new(null, Value.Zero);
                return _stringBuilder.ToString(); 
            }

            

            private static readonly int PlusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['+']];
            private static readonly int MinusOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['-']];

            private string RenderRpn(Token[] rpn, bool substitute, OutputWriter writer, out bool hasOperators)
            {
                var textWriter = new TextWriter();
                var stackBuffer = new Stack<RenderToken>();
                hasOperators = _parser._targetUnits is not null;
                for (int i = 0, len = rpn.Length; i < len; ++i)
                {
                    var t = new RenderToken(rpn[i], 0);
                    var tt = t.Type;
                    if (tt == TokenTypes.Solver)
                    {
                        t.Content = RenderSolver(t.Index, substitute, writer);
                        t.Type = TokenTypes.Constant;
                    }
                    else if (tt == TokenTypes.Input)
                    {
                        var units = ((ValueToken)rpn[i]).Value.Units;
                        t.Content = writer.FormatInput(t.Content, units, _parser._isCalculated);
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
                                t.Content = writer.FormatValue(v, _decimals);

                            t.IsCompositeValue = v.IsComposite();
                        }
                    }
                    else if (tt == TokenTypes.Variable)
                    {
                        var vt = (VariableToken)rpn[i];
                        var v = i > 0 && vt.Content == _parser._backupVariable.Key?
                            _parser._backupVariable.Value:
                            vt.Variable.Value;
                        if (substitute)
                        {
                            t.Content = writer.FormatValue(v, _decimals);
                            t.IsCompositeValue = v.IsComposite() || t.Content.Contains('×');
                            t.Order = Token.DefaultOrder;
                            if (_parser._settings.IsComplex && v.Number.IsComplex && v.Units is null)
                                t.Order = v.Number.Im < 0 ? MinusOrder : PlusOrder;
                        }
                        else
                        {
                            var s = !_parser._settings.Substitute && _parser._functionDefinitionIndex < 0 && _parser._isCalculated ?
                                textWriter.FormatValue(v, _decimals):
                                string.Empty;
                            t.Content = writer.FormatVariable(t.Content, s);
                        }
                    }
                    else
                    {
                        var div = writer.FormatOperator(';');
                        var b = stackBuffer.Pop();
                        var sb = b.Content;
                        if (t.Content == NegateString)
                        {
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
                        else if (tt == TokenTypes.Operator)
                        {
                            if (!(substitute && t.Content == "="))
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
                                            t.Content = sa + char.ConvertFromUtf32(0x200A) + sb;
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
                                t.Content = writer.FormatRoot(a.Content, _formatEquations, a.Level, sb);
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
                            t.Content = writer.FormatVariable(t.Content, string.Empty) + writer.AddBrackets(s, t.Level);
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
                            t.Content = writer.FormatRoot(sb, _formatEquations, b.Level, t.Content == "cbrt" ? "3" : "2");
                            t.Level = b.Level;
                            hasOperators = true;
                        }
                        else
                        {
                            t.Content = (tt == TokenTypes.Function ? 
                                writer.FormatFunction(t.Content) : 
                                writer.FormatVariable(t.Content, string.Empty)) + writer.AddBrackets(sb, b.Level);
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

            private string RenderSolver(int index, bool substitute, OutputWriter writer)
            {
                if (substitute)
                    return writer.FormatValue(_solveBlocks[index].Result, _decimals);

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
                        if (Validator.IsDigit(c) || c == '(')
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