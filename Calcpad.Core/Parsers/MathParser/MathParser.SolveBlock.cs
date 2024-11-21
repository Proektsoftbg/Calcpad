using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;
using static Calcpad.Core.Throw;

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
                "$Slope",
                "$Repeat",
                "∑",
                "∏",
                "$Error"
            ];
            private readonly MathParser _parser;
            private readonly SolverTypes _type;
            private Variable _var;
            private Value _va = Value.NaN, _vb = Value.NaN;
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
                {
                    if (string.IsNullOrWhiteSpace(_items[i].Input))
                        Throw.MissingDelimiterException(delimiters[i], Script);

                    _items[i].Input = _items[i].Input.Trim();
                    if (i == 0 && _type == SolverTypes.Root)
                    {
                        var s = _items[0].Input.Split('=');
                        _items[0].Input = s[0];
                        if (s.Length == 2)
                        {
                            _items[4].Input = s[1];
                            n = 4;
                        }
                        else if (s.Length > 2)
                            Throw.MultipleAssignmentsException(string.Join('=', s));
                    }
                    _parser.Parse(_items[i].Input, i == 0 && allowAssignment);
                    _items[i].Rpn = _parser._rpn;
                }
                if (_type == SolverTypes.Inf || _type == SolverTypes.Sup)
                {
                    var s = _items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    _parser.SetVariable(s, Value.NaN);
                }
                var vt = (VariableToken)_items[1].Rpn[0];
                Parameter[] parameters = [new(vt.Content)];
                vt.Variable = parameters[0].Variable;
                _var = vt.Variable;
                if (_parser.IsEnabled)
                {
                    _parser.BindParameters(parameters, _items[0].Rpn);
                    SubscribeCompile(_items[0].Rpn);
                    SubscribeCompile(_items[2].Rpn);
                    if (n == 3)
                        SubscribeCompile(_items[3].Rpn);
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
                        _items[0].Html = new HtmlWriter().AddBrackets(_items[0].Html, 1);
                        _items[0].Xml = new XmlWriter().AddBrackets(_items[0].Xml, 1);
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
                _f = _parser.CompileRpn(_items[0].Rpn);
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

            private void SubscribeCompile(Token[] rpn)
            {
                for (int i = 0, len = rpn.Length; i < len; ++i)
                {
                    var t = rpn[i];
                    if (t is VariableToken vt && vt.Variable is not null)
                        vt.Variable.OnChange += Clear;
                    else if (t.Type == TokenTypes.CustomFunction)
                        _parser._functions[t.Index].OnChange += Clear;
                }
            }

            private void Clear()
            {
                _f = null;
                OnChange?.Invoke();
            }

            internal IValue Calculate()
            {
                if (_f is null)
                    Compile();

                ++_parser._isSolver;
                var x1 = IValue.AsValue((_a?.Invoke() ?? _va));
                _parser.CheckReal(x1);
                var x2 = Value.Zero;
                var y = 0d;
                var ux1 = x1.Units;
                if (_type != SolverTypes.Slope)
                {
                    x2 = IValue.AsValue((_b?.Invoke() ?? _vb),Throw.Items.Limit);
                    _parser.CheckReal(x2);
                    var ux2 = x2.Units;
                    if (!Unit.IsConsistent(ux1, ux2))
                        Throw.InconsistentUnits2Exception(_items[0].Input, Unit.GetText(ux1), Unit.GetText(ux2));

                    if (ux2 is not null)
                        x2 *= ux2.ConvertTo(ux1);
                }
                _var.Value = x1;
                if (_type == SolverTypes.Root && _y is not null)
                {
                    var y1 = IValue.AsValue(_f(), Throw.Items.Result);
                    var uy1 = y1.Units;
                    y1 = IValue.AsValue(_y(), Throw.Items.Result);
                    _parser.CheckReal(y1);
                    _var.SetNumber(x2.Re);
                    var y2 = IValue.AsValue(_y(), Throw.Items.Result);
                    _parser.CheckReal(y2);
                    if (Math.Abs(y2.Re - y1.Re) > 1e-14)
                        Throw.NotConstantExpressionException(_items[4].Input);

                    y = y1.Re;
                    var uy2 = y2.Units;
                    if (!Unit.IsConsistent(uy1, uy2))
                        Throw.InconsistentUnits1Exception(_items[0].Input, _items[4].Input);

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
                IValue result = Value.NaN;
                try
                {
                    switch (_type)
                    {
                        case SolverTypes.Find:
                            result = new Value(solver.Find(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Root:
                            result = new Value(solver.Root(x1.Re, x2.Re, y));
                            break;
                        case SolverTypes.Sup:
                            result = new Value(solver.Sup(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Inf:
                            result = new Value(solver.Inf(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Area:
                            solver.QuadratureMethod = QuadratureMethods.AdaptiveLobatto;
                            result = new Value(solver.Area(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Integral:
                            solver.QuadratureMethod = QuadratureMethods.TanhSinh;
                            result = new Value(solver.Area(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Repeat:
                            result = _parser._settings.IsComplex ?
                                new Value(solver.ComplexRepeat(x1.Re, x2.Re)) :
                                solver.Repeat(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Sum:
                            result = _parser._settings.IsComplex ?
                                new Value(solver.ComplexSum(x1.Re, x2.Re)) :
                                new Value(solver.Sum(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Product:
                            result = _parser._settings.IsComplex ?
                                new Value(solver.ComplexProduct(x1.Re, x2.Re)):
                                new Value(solver.Product(x1.Re, x2.Re));
                            break;
                        case SolverTypes.Slope:
                            result = new Value(solver.Slope(x1.Re));
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
                    _parser.SetVariable(s, (Value)_var.Value);
                }
                --_parser._isSolver;

                if (result is Value value)
                {
                    if (double.IsNaN(value.Re) && !_parser.IsPlotting)
                        Throw.NoSolutionException(ToString());

                    Result = new Value(value.Re, value.Im, solver.Units);
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
                        return new HtmlWriter().FormatNary(
                            $"<em>{TypeName(_type)}</em>",
                            _items[2].Html,
                            _items[3].Html,
                            _items[0].Html + " d" + _items[1].Html
                            );

                    if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                        return new HtmlWriter().FormatNary(
                            TypeName(_type),
                            _items[1].Html + "=&hairsp;" + _items[2].Html,
                            _items[3].Html,
                            _items[0].Html
                            );

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
                    return new XmlWriter().FormatNary(
                        TypeName(_type),
                        _items[2].Xml,
                        _items[3].Xml,
                        _items[0].Xml + XmlWriter.Run(" d") + _items[1].Xml
                        );

                if (_type == SolverTypes.Sum || _type == SolverTypes.Product)
                    return new XmlWriter().FormatNary(
                        TypeName(_type),
                        _items[1].Xml + XmlWriter.Run("=") + _items[2].Xml,
                        _items[3].Xml,
                        _items[0].Xml
                        );

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
                    return new TextWriter().FormatNary(
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