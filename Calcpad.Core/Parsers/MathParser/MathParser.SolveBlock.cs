using System;
using System.Collections.Frozen;
using System.Collections.Generic;
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
                Sum,
                Product,
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
                { "$sum", SolverTypes.Sum },
                { "$product", SolverTypes.Product }
            }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

            private static readonly string[] TypeNames =
            [
                string.Empty,
                "$Find",
                "$Root",
                "$Sup",
                "$Inf",
                "∫",
                "∫",
                "d/dt",
                "$Repeat",
                "∑",
                "∏",
                "$Error"
            ];
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
                var n = 3;
                if (_type == SolverTypes.Slope)
                    n = 2;

                const string delimiters = "@=:";
                _items = new SolverItem[5];
                int current = 0, bracketCounter = 0;
                var sb = new StringBuilder();
                for (int i = 0, len = Script.Length; i < len; ++i)
                {
                    var c = Script[i];
                    if (c == '{')
                        ++bracketCounter;
                    else if (c == '}')
                        --bracketCounter;

                    if (bracketCounter == 0 && current < n && c == delimiters[current])
                    {
                        _items[current].Input = sb.ToString();
                        sb.Clear();
                        ++current;
                    }
                    else
                        sb.Append(c);
                }

                _items[current].Input = sb.ToString();
                var targetUnits = _parser._targetUnits;
                var allowAssignment = _type == SolverTypes.Repeat || _type == SolverTypes.Root;
                for (int i = 0; i <= n; ++i)
                    if (string.IsNullOrWhiteSpace(_items[i].Input))
                    {
                        var j = i == 0 ? 0 : i - 1;
                        throw Exceptions.MissingDelimiter(delimiters[j], $"{TypeName(_type)}{{{Script}}}" );
                    }

                for (int i = 0; i <= n; ++i)
                {
                    ref var item = ref _items[i];
                    item.Input = item.Input.Trim();
                    if (i == 0 && _type == SolverTypes.Root)
                    {
                        var s = item.Input.Split('=');
                        item.Input = s[0];
                        if (s.Length == 2)
                        {
                            _items[4].Input = s[1];
                            n = 4;
                        }
                        else if (s.Length > 2)
                            throw Exceptions.MultipleAssignments(string.Join('=', s));
                    }
                    _parser.Parse(item.Input, i == 0 && allowAssignment);
                    item.Rpn = _parser._rpn;
                }
                if (_type == SolverTypes.Inf || _type == SolverTypes.Sup)
                {
                    var s = _items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    _parser.SetVariable(s, RealValue.NaN);
                }
                var rpn = _items[1].Rpn;
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
                    rpn = _items[0].Rpn;
                    _parser.BindParameters(parameters, rpn);
                    _parser.SubscribeOnChange(rpn, Clear);
                    _parser.SubscribeOnChange(_items[2].Rpn, Clear);
                    if (n == 3)
                        _parser.SubscribeOnChange(_items[3].Rpn, Clear);
                }

                if (_type == SolverTypes.Repeat)
                    FixRepeat(_items[0].Rpn);

                _parser._targetUnits = targetUnits;
                IsFigure = _type == SolverTypes.Sum ||
                   _type == SolverTypes.Product ||
                   _type == SolverTypes.Integral ||
                   _type == SolverTypes.Area;

                RenderOutput();
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
                _f = _parser.CompileRpn(_items[0].Rpn, _type == SolverTypes.Repeat);
                if (_items[2].Rpn.Length == 1 &&
                    _items[2].Rpn[0].Type == TokenTypes.Constant)
                    _va = ((ValueToken)_items[2].Rpn[0]).Value;
                else
                    _a = _parser.CompileRpn(_items[2].Rpn);

                if (_items[3].Rpn is not null)
                {
                    if (_items[3].Rpn.Length == 1 &&
                        _items[3].Rpn[0].Type == TokenTypes.Constant)
                        _vb = ((ValueToken)_items[3].Rpn[0]).Value;
                    else
                        _b = _parser.CompileRpn(_items[3].Rpn);
                }
                if (_items[4].Rpn is not null)
                    _y = _parser.CompileRpn(_items[4].Rpn);
            }

            internal void BindParameters(ReadOnlySpan<Parameter> parameters, MathParser parser)
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

            private void Clear() => OnChange?.Invoke();

            internal IValue Calculate()
            {
                if (_f is null)
                    Compile();

                ++_parser._isSolver;
                IScalarValue x1 = IValue.AsValue((_a?.Invoke() ?? _va));
                _parser.CheckReal(x1);
                IScalarValue x2 = RealValue.Zero;
                var y = 0d;
                var ux1 = x1.Units;
                if (_type != SolverTypes.Slope)
                {
                    x2 = IValue.AsValue((_b?.Invoke() ?? _vb), Exceptions.Items.Limit);
                    _parser.CheckReal(x2);
                    var ux2 = x2.Units;
                    if (!Unit.IsConsistent(ux1, ux2))
                        throw Exceptions.InconsistentUnits2(_items[0].Input, Unit.GetText(ux1), Unit.GetText(ux2));

                    if (ux2 is not null)
                        x2 *= ux2.ConvertTo(ux1);
                }
                _var.SetValue(x1);
                if (_type == SolverTypes.Root && _y is not null)
                {
                    var y1 = IValue.AsValue(_f(), Exceptions.Items.Result);
                    var uy1 = y1.Units;
                    y1 = IValue.AsValue(_y(), Exceptions.Items.Result);
                    _parser.CheckReal(y1);
                    _var.SetNumber(x2.Re);
                    var y2 = IValue.AsValue(_y(), Exceptions.Items.Result);
                    _parser.CheckReal(y2);
                    if (Math.Abs(y2.Re - y1.Re) > 1e-14)
                        throw Exceptions.NotConstantExpression(_items[4].Input);

                    y = y1.Re;
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
                            d = solver.Find(x1.Re, x2.Re);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Root:
                            d = solver.Root(x1.Re, x2.Re, y);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Sup:
                            d = solver.Sup(x1.Re, x2.Re);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Inf:
                            d = solver.Inf(x1.Re, x2.Re);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Area:
                            solver.QuadratureMethod = QuadratureMethods.AdaptiveLobatto;
                            d = solver.Area(x1.Re, x2.Re);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Integral:
                            solver.QuadratureMethod = QuadratureMethods.TanhSinh;
                            d = solver.Area(x1.Re, x2.Re);
                            result = new RealValue(d);
                            break;
                        case SolverTypes.Repeat:
                            result = _parser._settings.IsComplex ?
                                new ComplexValue(solver.ComplexRepeat(x1.Re, x2.Re)) :
                                solver.Repeat(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Sum:
                            result = _parser._settings.IsComplex ?
                                new ComplexValue(solver.ComplexSum(x1.Re, x2.Re)) :
                                new RealValue(solver.Sum(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Product:
                            result = _parser._settings.IsComplex ?
                                new ComplexValue(solver.ComplexProduct(x1.Re, x2.Re)) :
                                new RealValue(solver.Product(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Slope:
                            d = solver.Slope(x1.Re);
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
                if (formatEquations)
                {
                    if (_type == SolverTypes.Integral || _type == SolverTypes.Area)
                        return new HtmlWriter(_parser._settings, _parser.Phasor).FormatNary(
                            $"<em>{TypeName(_type)}</em>",
                            _items[2].Html + "&nbsp;",
                            "&ensp;" + _items[3].Html,
                            _items[0].Html + " d" + _items[1].Html
                            );

                    if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                        return new HtmlWriter(_parser._settings, _parser.Phasor).FormatNary(
                            TypeName(_type),
                            _items[1].Html + "=&hairsp;" + _items[2].Html,
                            _items[3].Html,
                            _items[0].Html
                            );

                    if (_type == SolverTypes.Slope)
                    {
                        var writer = new HtmlWriter(_parser._settings, _parser.Phasor);
                        return writer.FormatDivision("<em>d</em>", $"<em>d</em>\u200A{_items[1].Html}", 0) +
                            writer.AddBrackets(_items[0].Html, 1) +
                            $"<span class=\"low\"><em>{_items[1].Input}</em>\u200A=\u200A{_items[2].Html}</span>";
                    }
                }
                var sb = new StringBuilder();
                sb.Append("<span class=\"cond\">" + TypeName(_type) + "</span>{");
                sb.Append(_items[0].Html);
                if (_type == SolverTypes.Root)
                {
                    if (_items[4].Html is not null)
                        sb.Append(" = " + _items[4].Html);
                    else
                        sb.Append(" = 0");
                }
                sb.Append("; ");
                sb.Append(_items[1].Html);
                if (_type == SolverTypes.Repeat || _type == SolverTypes.Slope)
                {
                    sb.Append(" = ");
                    sb.Append(_items[2].Html);
                    if (_type == SolverTypes.Repeat)
                    {
                        sb.Append("...");
                        sb.Append(_items[3].Html);
                    }
                    sb.Append('}');
                }
                else
                {
                    sb.Append(" ∈ [");
                    sb.Append(_items[2].Html);
                    sb.Append("; ");
                    sb.Append(_items[3].Html);
                    sb.Append("]}");
                }
                return sb.ToString();
            }

            internal string ToXml()
            {
                if (_type == SolverTypes.Integral || _type == SolverTypes.Area)
                    return new XmlWriter(_parser._settings, _parser.Phasor).FormatNary(
                        TypeName(_type),
                        _items[2].Xml,
                        _items[3].Xml,
                        _items[0].Xml + XmlWriter.Run(" d") + _items[1].Xml
                        );

                if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                    return new XmlWriter(_parser._settings, _parser.Phasor).FormatNary(
                        TypeName(_type),
                        _items[1].Xml + XmlWriter.Run("=") + _items[2].Xml,
                        _items[3].Xml,
                        _items[0].Xml
                        );

                if (_type == SolverTypes.Slope)
                {
                    var writer = new XmlWriter(_parser._settings, _parser.Phasor);
                    return writer.FormatDivision(XmlWriter.Run("d"), $"{XmlWriter.Run("d")}{_items[1].Xml}", 0) +
                        writer.FormatSubscript(writer.AddBrackets(_items[0].Xml, 1),
                        $"{XmlWriter.Run(_items[1].Input)}{XmlWriter.Run("\u2009=\u2009")}{_items[2].Xml}");
                }

                var sb = new StringBuilder();
                sb.Append(_items[0].Xml);
                if (_type == SolverTypes.Root)
                {
                    if (_items[4].Xml is not null)
                        sb.Append(XmlWriter.Run("=") + _items[4].Xml);
                    else
                        sb.Append(XmlWriter.Run("=0"));
                }
                sb.Append(XmlWriter.Run(";"));
                sb.Append(_items[1].Xml);
                if (_type == SolverTypes.Repeat || _type == SolverTypes.Slope)
                {
                    sb.Append(XmlWriter.Run("="));
                    sb.Append(_items[2].Xml);
                    if (_type == SolverTypes.Repeat)
                    {
                        sb.Append(XmlWriter.Run("..."));
                        sb.Append(_items[3].Xml);
                    }
                }
                else
                {
                    sb.Append(XmlWriter.Run("∈"));
                    sb.Append(XmlWriter.Brackets(_items[2].Xml + XmlWriter.Run(";") + _items[3].Xml, '[', ']'));
                }
                string s = sb.ToString();
                return XmlWriter.Run(TypeName(_type)) + XmlWriter.Brackets(s, '{', '}');
            }

            public override string ToString()
            {
                if (_type == SolverTypes.Sum ||
                    _type == SolverTypes.Product ||
                    _type == SolverTypes.Integral ||
                    _type == SolverTypes.Area ||
                    _type == SolverTypes.Repeat)
                    return new TextWriter(_parser._settings, _parser.Phasor).FormatNary(
                        "$" + _type.ToString(),
                        _items[1].Input + " = " + _items[2].Input,
                        _items[3].Input,
                        _items[0].Input
                        );

                var sb = new StringBuilder();
                sb.Append(TypeName(_type));
                sb.Append('{');
                sb.Append(_items[0].Input);
                if (_type == SolverTypes.Root)
                {
                    if (_items[4].Input is not null)
                        sb.Append(" = " + _items[4].Input);
                    else
                        sb.Append(" = 0");
                }
                sb.Append("; ");
                sb.Append(_items[1].Input);
                if (_type == SolverTypes.Slope)
                {
                    sb.Append(" = ");
                    sb.Append(_items[2].Input);
                    sb.Append('}');
                }
                else
                {
                    sb.Append(" ∈ [");
                    sb.Append(_items[2].Input);
                    sb.Append("; ");
                    sb.Append(_items[3].Input);
                    sb.Append("]}");
                }
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