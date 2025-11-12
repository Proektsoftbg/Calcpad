using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private sealed class SolveBlock
        {
            internal enum SolverTypes
            {
                None,
                Find,
                Root,
                Sup,
                Inf,
                Area,
                Integral,
                Slope,
                Repeat,
                While,
                Sum,
                Product,
                Inline,
                Block,
                Error
            }

            private static readonly FrozenDictionary<string, SolverTypes> Definitions = new Dictionary<string, SolverTypes>()
            {
                { "$find", SolverTypes.Find },
                { "$root", SolverTypes.Root },
                { "$sup", SolverTypes.Sup },
                { "$inf", SolverTypes.Inf },
                { "$area", SolverTypes.Area },
                { "$integral", SolverTypes.Integral },
                { "$slope", SolverTypes.Slope },
                { "$repeat", SolverTypes.Repeat },
                { "$while", SolverTypes.While },
                { "$sum", SolverTypes.Sum },
                { "$product", SolverTypes.Product },
                { "$inline", SolverTypes.Inline },
                { "$block", SolverTypes.Block },
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

            private static readonly string[] TypeNames =
            [
                string.Empty,
                "$Find",
                "$Root",
                "$Sup",
                "$inf",
                "∫",
                "∫",
                "d/dt",
                "$Repeat",
                "$While",
                "∑",
                "∏",
                string.Empty,
                string.Empty,
                "$Error"
            ];
            private readonly Dictionary<string, Variable> _localVariables = [];
            private readonly MathParser _parser;
            private readonly SolverTypes _type;
            private Variable _var;
            private IScalarValue _va = RealValue.NaN, _vb = RealValue.NaN;
            private SolverItem[] _items;
            private Func<IValue> _a, _b, _f, _y;

            private string Script { get; }
            internal event Action OnChange;
            internal IValue Result { get; private set; }
            internal bool IsFigure { get; private set; }
            internal SolveBlock(string script, SolverTypes type, MathParser parser)
            {
                Script = script;
                _type = type;
                _parser = parser;
                Parse();
            }
            private bool IsBlock => _type == SolverTypes.Inline || _type == SolverTypes.Block || _type == SolverTypes.While;

            internal static SolverTypes GetSolverType(ReadOnlySpan<char> keyword)
            {
                var s = keyword.Trim().ToString();
                if (Definitions.TryGetValue(s, out SolverTypes value))
                    return value;

                return SolverTypes.Error;
            }

            private static string TypeName(SolverTypes st)
            {
                var i = (int)st;
                if (i >= 0 && i < (int)SolverTypes.Error)
                    return TypeNames[i];

                return TypeNames[(int)SolverTypes.Error];
            }

            private void Parse()
            {
                var targetUnits = _parser._targetUnits;
                if (IsBlock)
                {
                    _parser._input.AddLocalVariables(_localVariables);
                    _items = ParseBlockOrInline(Script);
                    _parser._input.RemoveLocalVariables();
                }
                else
                    _items = ParseSolver(Script);

                _parser._targetUnits = targetUnits;
                RenderOutput();
            }

            private SolverItem[] ParseSolver(string script)
            {
                var n = 3;
                if (_type == SolverTypes.Slope)
                    n = 2;

                const string delimiters = "@=:";
                var items = new SolverItem[n + 1];
                int current = 0, bracketCounter = 0, equalityIndex = -1;
                var ts = new TextSpan(script);  
                var len = script.Length;
                for (int i = 0; i < len; ++i)
                {
                    var c = script[i];
                    if (c == '{')
                        ++bracketCounter;
                    else if (c == '}')
                        --bracketCounter;

                    if (_type == SolverTypes.Root && bracketCounter == 0 && current == 0 && c == '=')
                    {
                        if (equalityIndex == -1)
                            equalityIndex = i;
                        else
                            throw Exceptions.MultipleAssignments($"{ts.Cut()} ...");
                    }
                    if (bracketCounter == 0 && current < n && c == delimiters[current])
                    {
                        ts.ExpandTo(i);
                        items[current].Input = ts.Cut().ToString();
                        ts.Reset(i + 1);
                        ++current;
                    }
                    else
                        ts.Expand();
                }
                ts.ExpandTo(len);
                items[current].Input = ts.Cut().ToString();
                for (int i = 0; i <= n; ++i)
                    if (string.IsNullOrWhiteSpace(items[i].Input))
                    {
                        var j = i == 0 ? 0 : i - 1;
                        throw Exceptions.MissingDelimiter(delimiters[j], $"{TypeName(_type)}{{{script}}}");
                    }

                if (_type == SolverTypes.Repeat)
                {
                    var additionalItems = ParseBlockOrInline(items[0].Input);
                    len = additionalItems.Length - 1;
                    if (len > 0)
                    {
                        items[0] = additionalItems[0];
                        n = 3 + len;
                        SolverItem[] result = new SolverItem[n + 1];
                        Array.Copy(items, 0, result, 0, 4);
                        Array.Copy(additionalItems, 1, result, 4, len);
                        items = result;
                    }
                }
                else if (_type == SolverTypes.Root)
                {
                    if (equalityIndex != -1)
                    {
                        ref var item = ref items[0];
                        var s = item.Input;
                        item.Input = s[..equalityIndex];
                        Array.Resize(ref items, 5);
                        items[4].Input = s[(equalityIndex + 1)..];
                        n = 4;
                    }
                }
                var allowAssignment = _type == SolverTypes.Repeat || _type == SolverTypes.Root;
                for (int i = 0; i <= n; ++i)
                {
                    ref var item = ref items[i];
                    item.Input = item.Input.Trim();
                    _parser.Parse(item.Input, (i == 0 || i > 3) && allowAssignment);
                    item.Rpn = _parser._rpn;
                }
                if (_type == SolverTypes.Inf || _type == SolverTypes.Sup)
                {
                    var s = items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    _parser.SetVariable(s, RealValue.NaN);
                }
                var rpn = items[1].Rpn;
                Parameter[] parameters;
                if (rpn.Length == 1 && rpn[0] is VariableToken vt)
                {
                    parameters = [new(vt.Content)];
                    vt.Variable = parameters[0].Variable;
                    _var = vt.Variable;
                }
                else
                    throw Exceptions.CounterMustBeASingleVariableName();

                if (_parser.IsEnabled)
                {
                    rpn = items[0].Rpn;
                    _parser.BindParameters(parameters, rpn);
                    _parser.SubscribeOnChange(rpn, Clear);
                    _parser.SubscribeOnChange(items[2].Rpn, Clear);
                    if (items.Length > 3)
                    {
                        rpn = items[3].Rpn;
                        if (rpn is not null)
                            _parser.SubscribeOnChange(rpn, Clear);
                    }
                }
                if (_type == SolverTypes.Repeat)
                    for(int i = 0; i < items.Length; i++)
                        if (i == 0 || i > 3)
                        {
                            rpn = items[i].Rpn;
                            if (rpn is not null && rpn.Length > 0)
                            {
                                FixRepeat(rpn);
                                if (i > 3)
                                {
                                    _parser.BindParameters(parameters, rpn);
                                    _parser.SubscribeOnChange(rpn, Clear);
                                }
                            }
                        }

                IsFigure = _type == SolverTypes.Sum ||
                   _type == SolverTypes.Product ||
                   _type == SolverTypes.Integral ||
                   _type == SolverTypes.Area;

                return items;
            }

            private SolverItem[] ParseBlockOrInline(string script)
            {
                int bracketCounter = 0, curlyBracketCounter = 0, squareBracketCounter = 0;
                var ts = new TextSpan(script);
                var len = script.Length;
                var itemList = new List<SolverItem>();
                for (int i = 0; i < len; ++i)
                {
                    var c = script[i];
                    if (c == '(')
                        ++bracketCounter;
                    else if (c == ')')
                        --bracketCounter;
                    else if(c == '[')
                        ++squareBracketCounter;
                    else if (c == ']')
                        --squareBracketCounter;
                    else if(c == '{')
                        ++curlyBracketCounter;
                    else if (c == '}')
                        --curlyBracketCounter;

                    if (bracketCounter == 0 && 
                        squareBracketCounter == 0 && 
                        curlyBracketCounter == 0 && 
                        c == ';')
                    {
                        ts.ExpandTo(i);
                        itemList.Add(new SolverItem() { Input = ts.Cut().ToString() });
                        ts.Reset(i + 1);
                    }
                    else
                        ts.Expand();
                }
                ts.ExpandTo(len);
                itemList.Add(new SolverItem() { Input = ts.Cut().ToString() });
                var n = itemList.Count - 1;
                var itemArray = itemList.ToArray();
                for (int i = 0; i <= n; ++i)
                {
                    ref var item = ref itemArray[i];
                    item.Input = item.Input.Trim();
                    _parser.Parse(item.Input,true);
                    item.Rpn = _parser._rpn;
                }
                return itemArray;
            }

            private void RenderOutput()
            {
                for (int i = 0, n = _items.Length; i < n; ++i)
                    _items[i].Render(_parser);

                if (IsFigure)
                {
                    var order = Calculator.OperatorOrder[Calculator.OperatorIndex['*']];
                    var rpn = _items[0].Rpn;
                    var t = rpn[^1];
                    if (t.Order > order)
                    {
                        ref var item = ref _items[0];
                        item.Html = new HtmlWriter(_parser._settings, _parser.Phasor).AddBrackets(item.Html, 1);
                        item.Xml = new XmlWriter(_parser._settings, _parser.Phasor).AddBrackets(item.Xml, 1);
                    }
                }
            }

            private static void FixRepeat(Token[] rpn)
            {
                ref var rpn0 = ref rpn[0];
                if (rpn[^1].Content == "=" && rpn0.Index == -1)
                {
                    rpn0.Index = 1;
                    for (int i = 0, len = rpn.Length; i < len; ++i)
                        if (rpn[i].Type == TokenTypes.Variable && rpn[i].Content == rpn0.Content)
                            rpn[i].Index = 1;
                }
            }

            private void Compile() 
            {
                var allowAssignment =
                    _type == SolverTypes.Repeat || IsBlock;
                _f = _parser.CompileRpn(_items[0].Rpn, allowAssignment);

                var len = _items.Length;
                if (allowAssignment)
                {
                    var i0 = _type == SolverTypes.Repeat ? 4 : 1;
                    if (len > i0)
                    {
                        var e = _parser.RpnToExpressionTree(_items[0].Rpn, allowAssignment);
                        var expressions = new List<Expression>(len - i0 + 1) { e };
                        for (int i = i0; i < len; ++i)
                        {
                            var rpn_i = _items[i].Rpn;
                            if (rpn_i.Length > 0)
                            {
                                e = _parser.RpnToExpressionTree(rpn_i, true);
                                expressions.Add(e);
                            }
                        }
                        if (_type == SolverTypes.While)
                            _f = Compiler.CompileWhileBLock(expressions);
                        else
                        {
                            var body = Expression.Block(expressions);
                            var lambda = Expression.Lambda<Func<IValue>>(body);
                            _f = lambda.Compile();
                        }

                    }
                    if (i0 == 1)
                        return;
                }
                var rpn = _items[2].Rpn;
                if (rpn.Length == 1 &&
                    rpn[0].Type == TokenTypes.Constant)
                    _va = ((ValueToken)rpn[0]).Value;
                else
                    _a = _parser.CompileRpn(rpn);

                
                if (len > 3)
                {
                    rpn = _items[3].Rpn;
                    if (rpn is not null)
                    {
                        if (rpn.Length == 1 &&
                            rpn[0].Type == TokenTypes.Constant)
                            _vb = ((ValueToken)rpn[0]).Value;
                        else
                            _b = _parser.CompileRpn(rpn);
                    }
                }

                if (len > 4)
                {
                    rpn = _items[4].Rpn;
                    if (rpn is not null && rpn.Length > 0)
                        _y = _parser.CompileRpn(_items[4].Rpn);
                }
            }

            internal void BindParameters(ReadOnlySpan<Parameter> parameters, MathParser parser)
            {
                if (parser.IsEnabled)
                    for (int i = 0, len = _items.Length; i < len; ++i)
                        if (i != 1 || IsBlock)
                            parser.BindParameters(parameters, _items[i].Rpn);
            }

            private void Clear() => OnChange?.Invoke();

            internal IValue Calculate()
            {
                if (_f is null)
                    Compile();

                if (IsBlock)
                {
                    Result = _f();
                    return Result;
                }
                ++_parser._isSolver;
                var x1 = IValue.AsReal((_a?.Invoke() ?? _va));
                var x2 = RealValue.Zero;
                var y = 0d;
                var ux1 = x1.Units;
                if (_type != SolverTypes.Slope)
                {
                    x2 = IValue.AsReal((_b?.Invoke() ?? _vb), Exceptions.Items.Limit);
                    var ux2 = x2.Units;
                    if (!Unit.IsConsistent(ux1, ux2))
                        throw Exceptions.InconsistentUnits2(_items[0].Input, Unit.GetText(ux1), Unit.GetText(ux2));

                    if (ux2 is not null)
                        x2 *= ux2.ConvertTo(ux1);
                }
                _var.SetValue(x1);
                if (_type == SolverTypes.Root && _y is not null)
                {
                    var y1 = IValue.AsReal(_f(), Exceptions.Items.Result);
                    var uy1 = y1.Units;
                    y1 = IValue.AsReal(_y(), Exceptions.Items.Result);
                    _var.SetNumber(x2.D);
                    var y2 = IValue.AsReal(_y(), Exceptions.Items.Result);
                    if (Math.Abs(y2.D - y1.D) > 1e-14)
                        throw Exceptions.NotConstantExpression(_items[4].Input);

                    y = y1.D;
                    var uy2 = y2.Units;
                    if (!Unit.IsConsistent(uy1, uy2))
                        throw Exceptions.InconsistentUnits1(_items[0].Input, _items[4].Input);

                    if (uy2 is not null)
                        y *= uy2.ConvertTo(uy1);
                }
                var solver = _parser._solver;
                var variable = solver.Variable;
                var function = solver.Function;
                var solverUnits = solver.Units;
                solver.Variable = _var;
                solver.Function = _f;
                solver.Precision = _parser.Precision;
                solver.Units = null;
                IValue result = RealValue.NaN;
                double d = 0d;
                try
                {
                    switch (_type)
                    {
                        case SolverTypes.Find:
                            d = solver.Find(x1.D, x2.D);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Root:
                            d = solver.Root(x1.D, x2.D, y);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Sup:
                            d = solver.Sup(x1.D, x2.D);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Inf:
                            d = solver.Inf(x1.D, x2.D);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Area:
                            solver.QuadratureMethod = QuadratureMethods.AdaptiveLobatto;
                            d = solver.Area(x1.D, x2.D);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Integral:
                            solver.QuadratureMethod = QuadratureMethods.TanhSinh;
                            d = solver.Area(x1.D, x2.D);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Repeat:
                            result = _parser._settings.IsComplex ?
                                new ComplexValue(solver.ComplexRepeat(x1.D, x2.D)) :
                                solver.Repeat(x1.D, x2.D);
                            break;
                        case SolverTypes.Sum:
                            result = _parser._settings.IsComplex ?
                                new ComplexValue(solver.ComplexSum(x1.D, x2.D)) :
                                new RealValue(solver.Sum(x1.D, x2.D));
                            break;
                        case SolverTypes.Product:
                            result = _parser._settings.IsComplex ?
                                new ComplexValue(solver.ComplexProduct(x1.D, x2.D)) :
                                new RealValue(solver.Product(x1.D, x2.D));
                            break;
                        case SolverTypes.Slope:
                            d = solver.Slope(x1.D);
                            result = new RealValue(d);
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
                    throw;
                }
                if (_type == SolverTypes.Sup || _type == SolverTypes.Inf)
                {
                    var s = _items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    _parser.SetVariable(s, (RealValue)_var.Value);
                }
                --_parser._isSolver;

                if (double.IsNaN(d) && !_parser.IsPlotting)
                    throw Exceptions.NoSolution(ToString());

                if (result is RealValue real)
                    Result = new RealValue(real.D, solver.Units);
                else if (result is ComplexValue complex)
                {
                    if (complex.B == 0d)
                        Result = new RealValue(complex.A, solver.Units);
                    else
                        Result = new ComplexValue(complex.A, complex.B, solver.Units);
                }
                else
                    Result = result;

                solver.Variable = variable;
                solver.Function = function;
                solver.Units = solverUnits;
                return Result;
            }

            internal string ToHtml(bool formatEquations)
            {
                var len = _items.Length;
                var writer = new HtmlWriter(_parser._settings, _parser.Phasor);
                if (IsBlock)
                {
                    var html = new string[len];
                    for (int i = 0; i < len; ++i)
                        html[i] = _items[i].Html;

                    if (_type == SolverTypes.Inline)
                        html = [string.Join("; ",  html)];
                    else if (_type == SolverTypes.While)
                        html[0] = "<span class=\"cond\">while</span> " + html[0];

                    return writer.FormatBlock(html);
                }
                if (formatEquations)
                {
                    if (_type == SolverTypes.Integral || _type == SolverTypes.Area)
                        return writer.FormatNary(
                            $"<em>{TypeName(_type)}</em>",
                            _items[2].Html + "&nbsp;",
                            "&ensp;" + _items[3].Html,
                            _items[0].Html + " d" + _items[1].Html
                            );

                    if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                        return writer.FormatNary(
                            TypeName(_type),
                            _items[1].Html + "=&hairsp;" + _items[2].Html,
                            _items[3].Html,
                            _items[0].Html
                            );

                    if (_type == SolverTypes.Slope)
                    {
                        return writer.FormatDivision("<em>d</em>", $"<em>d</em>\u200A{_items[1].Html}", 0) +
                            writer.AddBrackets(_items[0].Html, 1) +
                            $"<span class=\"low\"><em>{_items[1].Input}</em>\u200A=\u200A{_items[2].Html}</span>";
                    }
                }
                var sb = new StringBuilder($"<span class=\"cond\">{TypeName(_type)}</span>");
                if (_type == SolverTypes.Repeat && len > 4)
                {
                    var html = new string[len - 2];
                    html[0] = $"<span class=\"cond\">for</span> {_items[1].Html} = {_items[2].Html}...{_items[3].Html}";
                    html[1] = _items[0].Html;
                    for (int i = 4; i < len; ++i)
                        html[i - 2] = _items[i].Html;

                    sb.Append(' ').Append(writer.FormatBlock(html));
                    return sb.ToString();
                }
                sb.Append('{').Append(_items[0].Html);
                if (_type == SolverTypes.Root)
                {
                    if (_items.Length  > 4 &&  _items[4].Html is not null)
                        sb.Append(" = " + _items[4].Html);
                    else
                        sb.Append(" = 0");
                }

                if (_type == SolverTypes.Repeat)
                    sb.Append(" for ");
                else
                    sb.Append("; ");

                sb.Append(_items[1].Html);
                if (_type == SolverTypes.Repeat || _type == SolverTypes.Slope)
                {
                    sb.Append(" = ").Append(_items[2].Html);
                    if (_type == SolverTypes.Repeat)
                    {
                        sb.Append("...").Append(_items[3].Html);
                    }
                    sb.Append('}');
                }
                else
                    sb.Append(" ∈ [")
                        .Append(_items[2].Html)
                        .Append("; ")
                        .Append(_items[3].Html)
                        .Append("]}");

                return sb.ToString();
            }

            internal string ToXml()
            {
                var len = _items.Length;
                var writer = new XmlWriter(_parser._settings, _parser.Phasor);
                if (IsBlock)
                {
                    var xml = new string[len];
                    for (int i = 0; i < len; ++i)
                        xml[i] = _items[i].Xml;

                    if (_type == SolverTypes.Inline)
                        xml = [string.Join(XmlWriter.Run("; "), xml)];
                    else if (_type == SolverTypes.While)
                        xml[0] = XmlWriter.Run(TypeName(_type), XmlWriter.NormalText) + ' ' + xml[0];

                    return writer.FormatBlock(xml);
                }
                if (_type == SolverTypes.Integral || _type == SolverTypes.Area)
                    return writer.FormatNary(
                        TypeName(_type),
                        _items[2].Xml,
                        _items[3].Xml,
                        _items[0].Xml + XmlWriter.Run(" d") + _items[1].Xml
                        );

                if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                    return writer.FormatNary(
                        TypeName(_type),
                        _items[1].Xml + XmlWriter.Run("=") + _items[2].Xml,
                        _items[3].Xml,
                        _items[0].Xml
                        );

                if (_type == SolverTypes.Slope)
                {
                    return writer.FormatDivision(XmlWriter.Run("d"), $"{XmlWriter.Run("d")}{_items[1].Xml}", 0) +
                        writer.FormatSubscript(writer.AddBrackets(_items[0].Xml, 1),
                        $"{XmlWriter.Run(_items[1].Input)}{XmlWriter.Run("\u2009=\u2009")}{_items[2].Xml}");
                }
                var sb = new StringBuilder();
                if (_type == SolverTypes.Repeat && len > 4)
                {
                    sb.Append(XmlWriter.Run(TypeName(_type), XmlWriter.NormalText));
                    var xml = new string[len - 2];
                    xml[0] = $"{XmlWriter.Run("for ", XmlWriter.NormalText)}{_items[1].Xml}{XmlWriter.Run("=")}{_items[2].Xml}{XmlWriter.Run("...")}{_items[3].Xml}";
                    xml[1] = _items[0].Xml;
                    for (int i = 4; i < len; ++i)
                        xml[i - 2] = _items[i].Xml;

                    sb.Append(writer.FormatBlock(xml));
                    return sb.ToString();
                }
                else
                {
                    sb.Append(_items[0].Xml);
                    if (_type == SolverTypes.Root)
                    {
                        if (_items.Length > 4 && _items[4].Xml is not null)
                            sb.Append(XmlWriter.Run("=") + _items[4].Xml);
                        else
                            sb.Append(XmlWriter.Run("=0"));
                    }
                }
                if (_type == SolverTypes.Repeat)
                    sb.Append(XmlWriter.Run(" for ", XmlWriter.NormalText));
                else
                    sb.Append(XmlWriter.Run(";"));

                sb.Append(_items[1].Xml);
                if (_type == SolverTypes.Repeat || _type == SolverTypes.Slope)
                {
                    sb.Append(XmlWriter.Run("=")).Append(_items[2].Xml);
                    if (_type == SolverTypes.Repeat)
                        sb.Append(XmlWriter.Run("...")).Append(_items[3].Xml);
                }
                else
                {
                    sb.Append(XmlWriter.Run("∈"))
                        .Append(XmlWriter.Brackets(_items[2].Xml + XmlWriter.Run(";") + _items[3].Xml, '[', ']'));
                }
                var s = sb.ToString();
                return XmlWriter.Run(TypeName(_type), XmlWriter.NormalText) + XmlWriter.Brackets(s, '{', '}');
            }

            public override string ToString()
            {
                var len = _items.Length;
                var writer = new TextWriter(_parser._settings, _parser.Phasor);
                if (IsBlock)
                {
                    var text = new string[len];
                    for (int i = 0; i < len; ++i)
                        text[i] = _items[i].Input;

                    if (_type == SolverTypes.Inline)
                        text = [string.Join("; ", text)];
                    else if (_type == SolverTypes.While)
                        text[0] = "<span class=\"cond\">while</span> " + text[0];

                    return writer.FormatBlock(text);
                }
                if (_type == SolverTypes.Sum ||
                    _type == SolverTypes.Product ||
                    _type == SolverTypes.Integral ||
                    _type == SolverTypes.Area ||
                    _type == SolverTypes.Repeat)
                    return writer.FormatNary(
                        "$" + _type.ToString(),
                        _items[1].Input + " = " + _items[2].Input,
                        _items[3].Input,
                        _items[0].Input
                        );

                var sb = new StringBuilder();
                sb.Append(TypeName(_type)).Append('{');
                if (_type == SolverTypes.Repeat && len > 4)
                {
                    var text = new string[len - 3];
                    text[0] = _items[0].Input;
                    for (int i = 4; i < len; ++i)
                        text[i - 3] = _items[i].Input;

                    sb.Append(writer.FormatBlock(text));
                }
                sb.Append(_items[0].Input);
                if (_type == SolverTypes.Root)
                {
                    if (_items.Length > 4 && _items[4].Input is not null)
                        sb.Append(" = " + _items[4].Input);
                    else
                        sb.Append(" = 0");
                }
                sb.Append("; ").Append(_items[1].Input);
                if (_type == SolverTypes.Slope)
                    sb.Append(" = ")
                        .Append(_items[2].Input)
                        .Append('}');

                else
                    sb.Append(" ∈ [")
                        .Append(_items[2].Input)
                        .Append("; ")
                        .Append(_items[3].Input)
                        .Append("]}");

                return sb.ToString();
            }

            private struct SolverItem
            {
                internal string Input;
                internal string Html;
                internal string Xml;
                internal Token[] Rpn;

                internal void Render(MathParser parser)
                {
                    if (Rpn is not null)
                    {
                        parser._rpn = Rpn;
                        Html = parser.ToHtml();
                        Xml = parser.ToXml();
                    }
                }
            }
        }
    }
}