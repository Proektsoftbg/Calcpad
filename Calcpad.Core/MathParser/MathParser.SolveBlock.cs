﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private class SolveBlock
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

            private static readonly Dictionary<string, SolverTypes> Definitions = new(StringComparer.OrdinalIgnoreCase)
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
            };

            private static readonly string[] TypeNames =
            {
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
            };
            private readonly MathParser _parser;
            private readonly SolverTypes _type;
            private Variable _var;
            private Value _va = Value.NaN, _vb = Value.NaN;
            private SolverItem[] _items;
            private Func<Value> _a, _b, _f, _y;
            private string Script { get; }
            internal Value Result { get; private set; }
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
#if BG
                        throw new MathParserException($"Липсва разделител \"{delimiters[i]}\" в команда за числени методи {{{Script}}}.");
#else
                        throw new MathParserException($"Missing delimiter \"{delimiters[i]}\" in solver command {{{Script}}}.");
#endif
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
                        {
#if BG
                            throw new MathParserException($"Повече от един оператор '=' в '{string.Join('=', s)}'.");
#else
                            throw new MathParserException($"More than one operators '=' in '{string.Join('=', s)}'.");
#endif
                        }
                    }
                    _parser.Parse(_items[i].Input, i == 0 && allowAssignment);
                    _items[i].Rpn = _parser._rpn;
                    _items[i].Html = _parser.ToHtml();
                    _items[i].Xml = _parser.ToXml();
                }
                IsFigure = _type == SolverTypes.Sum ||
                           _type == SolverTypes.Product ||
                           _type == SolverTypes.Integral ||
                           _type == SolverTypes.Area;
                if (IsFigure)
                {
                    var order = Calculator.OperatorOrder[Calculator.OperatorIndex['*']];
                    var rpn = _items[0].Rpn;
                    var t = rpn[^1];
                    if (t.Order > order)
                    {
                        _items[0].Html = new HtmlWriter().AddBrackets(_items[0].Html, 1);
                        _items[0].Xml = new XmlWriter().AddBrackets(_items[0].Xml, 1);
                        IsFigure = false;
                    }
                }
                if (_type == SolverTypes.Inf || _type == SolverTypes.Sup)
                {
                    var s = _items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    _parser.SetVariable(s, new Value(double.NaN));
                }
                var vt = (VariableToken)_items[1].Rpn[0];
                Parameter[] parameters = { new(vt.Content) };
                vt.Variable = parameters[0].Variable;
                _var = vt.Variable;
                _parser.BindParameters(parameters, _items[0].Rpn);
                _parser._targetUnits = targetUnits;
            }

            internal void Compile(MathParser parser)
            {
                if (_f is not null)
                    return;

                _f = parser.CompileRpn(_items[0].Rpn);
                if (_items[2].Rpn.Length == 1 &&
                    _items[2].Rpn[0].Type == TokenTypes.Constant)
                    _va = ((ValueToken)_items[2].Rpn[0]).Value;
                else
                    _a = parser.CompileRpn(_items[2].Rpn);

                if (_items[3].Rpn is not null)
                {
                    if (_items[3].Rpn.Length == 1 &&
                        _items[3].Rpn[0].Type == TokenTypes.Constant)
                        _vb = ((ValueToken)_items[3].Rpn[0]).Value;
                    else
                        _b = parser.CompileRpn(_items[3].Rpn);
                }
                if (_items[4].Rpn is not null)
                    _y = parser.CompileRpn(_items[4].Rpn);
            }

            internal void BindParameters(Parameter[] parameters, MathParser parser)
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

            internal Value Calculate()
            {
                ++_parser._isSolver;
                var x1 = _a is null ? _va : _a();
                _parser.CheckReal(x1);
                var x2 = Value.Zero;
                var y = 0.0;
                var ux1 = x1.Units;
                if (_type != SolverTypes.Slope)
                {
                    x2 = _b is null ? _vb : _b();
                    _parser.CheckReal(x2);
                    var ux2 = x2.Units;
                    if (!Unit.IsConsistent(ux1, ux2))
#if BG
                        throw new MathParserException($"Несъвместими мерни единици за {_items[0].Input} = \"{Unit.GetText(ux1)}' : \"{Unit.GetText(ux2)}'.");
#else
                        throw new MathParserException($"Inconsistent units for {_items[0].Input} = \"{Unit.GetText(ux1)}' : \"{Unit.GetText(ux2)}'.");
#endif
                    if (ux2 is not null)
                        x2 *= ux2.ConvertTo(ux1);
                }
                _var.SetValue(x1);
                if (_type == SolverTypes.Root && _y is not null)
                {
                    var y1 = _f();
                    var uy1 = y1.Units;
                    y1 = _y();
                    _parser.CheckReal(y1);
                    _var.SetNumber(x2.Re);
                    var y2 = _y();
                    _parser.CheckReal(y2);
                    if (Math.Abs(y2.Re - y1.Re) > 1e-14)
#if BG
                        throw new MathParserException($"Изразът от дясната страна трябва да е константа: \"{_items[4].Input}\".");
#else
                        throw new MathParserException($"The expression on the right side must be constant: \"{_items[4].Input}\".");
#endif
                    y = y1.Re;
                    var uy2 = y2.Units;
                    if (!Unit.IsConsistent(uy1, uy2))
#if BG
                        throw new MathParserException($"Несъвместими мерни единици за \"{ _items[0].Input} = {_items[4].Input}\".");
#else
                        throw new MathParserException($"Inconsistent units for \"{_items[0].Input} = {_items[4].Input}\".");
#endif
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
                var result = new Complex(double.NaN);
                try
                {
                    switch (_type)
                    {
                        case SolverTypes.Find:
                            result = solver.Find(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Root:
                            result = solver.Root(x1.Re, x2.Re, y);
                            break;
                        case SolverTypes.Sup:
                            result = solver.Sup(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Inf:
                            result = solver.Inf(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Area:
                            solver.QuadratureMethod = QuadratureMethods.AdaptiveLobatto;
                            result = solver.Area(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Integral:
                            solver.QuadratureMethod = QuadratureMethods.TanhSinh;
                            result = solver.Area(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Repeat:
                            result = _parser._settings.IsComplex ?
                                solver.ComplexRepeat(x1.Re, x2.Re) :
                            solver.Repeat(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Sum:
                            result = _parser._settings.IsComplex ?
                                solver.ComplexSum(x1.Re, x2.Re) :
                            solver.Sum(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Product:
                            result = _parser._settings.IsComplex ?
                                solver.ComplexProduct(x1.Re, x2.Re) :
                                solver.Product(x1.Re, x2.Re);
                            break;
                        case SolverTypes.Slope:
                            result = solver.Slope(x1.Re);
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
                    throw e;
                }
                if (_type == SolverTypes.Sup || _type == SolverTypes.Inf)
                {
                    var s = _items[1].Input + (_type == SolverTypes.Sup ? "_sup" : "_inf");
                    _parser.SetVariable(s, _var.Value);
                }
                --_parser._isSolver;

                if (double.IsNaN(result.Re) && !_parser.IsPlotting)
#if BG
                    throw new MathParserException($"Няма решение за: {ToString()}.");
#else
                    throw new MathParserException($"No solution for: {ToString()}.");
#endif

                Result = new Value(result, solver.Units);
                solver.Variable = variable;
                solver.Function = function;
                solver.Units = solverUnits;
                return Result;
            }

            internal string ToHtml()
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
                    sb.Append(XmlWriter.Brackets('[', ']', _items[2].Xml + XmlWriter.Run(";") + _items[3].Xml));
                }
                string s = sb.ToString();
                return XmlWriter.Run(TypeName(_type)) + XmlWriter.Brackets('{', '}', s);
            }

            public override string ToString()
            {
                if (_type == SolverTypes.Sum ||
                    _type == SolverTypes.Product ||
                    _type == SolverTypes.Integral ||
                    _type == SolverTypes.Area ||
                    _type == SolverTypes.Repeat)
                    return new TextWriter().FormatNary(
                        TypeName(_type),
                        _items[1].Html + " = " + _items[2].Html,
                        _items[3].Html,
                        _items[0].Html
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
            }
        }
    }
}