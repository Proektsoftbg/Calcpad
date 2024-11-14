using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Calcpad.Core
{
    internal abstract class OutputWriter
    {
        internal enum OutputFormat
        {
            Text,
            Html,
            Xml
        }
        private readonly StringBuilder _stringBuilder = new(200);
        protected static readonly int PowerOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['^']];

        internal abstract string UnitString(Unit units);
        internal abstract string FormatInput(string s, Unit units, int line, bool isCalculated);
        internal abstract string FormatSubscript(string sa, string sb);
        internal abstract string FormatVariable(string name, string value);
        internal abstract string FormatUnits(string s);
        internal abstract string FormatFunction(string s);
        internal abstract string FormatSwitch(string[] sa, int level = 0);
        internal abstract string FormatIf(string sc, string sa, string sb, int level = 0);
        internal abstract string FormatRoot(string s, bool formatEquations, int level = 0, string n = "2");
        internal abstract string FormatOperator(char c);
        internal abstract string FormatPower(string sa, string sb, int level, int order);
        internal abstract string FormatDivision(string sa, string sb, int level);
        internal abstract string FormatNary(string symbol, string sub, string sup, string expr); //Integral, sum, product
        internal abstract string FormatValue(in Value v, int decimals);
        internal abstract string AddBrackets(string s, int level = 0, char left = '(', char right = ')');
        internal abstract string FormatAbs(string s, int level = 0);
        internal abstract string FormatReal(double d, int decimals, bool zeroSmall = false);
        internal abstract string FormatComplex(double re, double im, int decimals);
        internal abstract string FormatMatrix(Matrix matrix, int decimals, int maxCount, bool zeroSmallElements);
        internal abstract string FormatMatrixValue(Value v, int decimals, bool zeroSmall);

        internal string FormatUnitsText(string text)
        {
            _stringBuilder.Clear();
            var literal = string.Empty;
            var power = string.Empty;
            var isPower = false;
            byte sub = 0;
            var brackets = new Stack<int>();

            for (int i = 0, len = text.Length; i < len; ++i)
            {
                var c = text[i];
                if (c == '·')
                    c = '*';

                switch (c)
                {
                    case '/':
                    case '*':
                        {
                            AppendPower();
                            _stringBuilder.Append(FormatOperator(c));
                            break;
                        }
                    case '^':
                        isPower = true;
                        break;
                    case '(':
                        brackets.Push(isPower ? power.Length : _stringBuilder.Length);
                        break;
                    case ')':
                        {
                            if (brackets.Count == 0)
                                Throw.MissingLeftBracketException();

                            var index = brackets.Pop();
                            if (isPower && index < power.Length)
                            {
                                if (index == 0)
                                    power = this is TextWriter ? $"({power})" : power;
                                else
                                {
                                    var s = power[index..];
                                    power = power.Remove(index);
                                    power += $"({s})";
                                }
                            }
                            else
                            {
                                AppendPower();
                                var length = _stringBuilder.Length - index;
                                var s = _stringBuilder.ToString(index, length);
                                _stringBuilder.Remove(index, length);
                                literal = AddBrackets(s);
                            }
                            break;
                        }
                    default:
                        {
                            if (isPower)
                                power += c;
                            else
                            {
                                var cl = string.IsNullOrEmpty(literal) ?
                                    '\0' :
                                    literal[^1];
                                var isSub = sub switch
                                {
                                    0 => c == '_',
                                    1 => c == 'U' ||
                                         c == 'd' ||
                                         c == 'm' ||
                                         c == 'f',
                                    2 => cl switch
                                    {
                                        'U' => c == 'K' ||
                                               c == 'S',
                                        'd' => c == 'r',
                                        _ => false
                                    },
                                    3 => cl == 'r' && c == 'y',
                                    _ => false
                                };
                                if (isSub)
                                    sub++;
                                else if (c == '_')
                                    sub = 1;
                                else
                                    sub = 0;

                                literal += c;
                            }
                            break;
                        }
                }
            }
            if (brackets.Count != 0)
                Throw.MissingRightBracketException();

            if (literal.Length > 0)
                AppendPower();

            return _stringBuilder.ToString();

            string FormatLocal(string s)
            {
                var n = sub;
                if (n >= 2 && n <= 4)
                {
                    sub = 0;
                    return FormatSubscript(FormatUnits(s[..^n]), "\u200A" + s[^(n - 1)..]);
                }
                if (s.Contains('/') || s.Contains('·'))
                    return s;

                return FormatUnits(s);
            }

            void AppendPower()
            {
                literal = FormatLocal(literal);
                if (isPower)
                {
                    if (this is XmlWriter)
                        power = XmlWriter.Run(power);

                    _stringBuilder.Append(FormatPower(literal, power, 0, -1));
                }
                else
                    _stringBuilder.Append(literal);

                literal = string.Empty;
                power = string.Empty;
                isPower = false;
            }
        }

        protected static string FormatOperatorHelper(char c) => c switch
        {
            Calculator.NegChar => "-",
            '-' => " − ",
            '<' => " < ",
            '>' => " > ",
            '+' => " + ",
            '=' => " = ",
            '≡' => " ≡ ",
            '≠' => " ≠ ",
            '≤' => " ≤ ",
            '≥' => " ≥ ",
            '⦼' => " mod ",
            '∧' => " and ",
            '∨' => " or ",
            '⊕' => " xor ",
            '*' => "·",
            ';' => "; ",
            ',' => ", ",
            '|' => " | ",
            _ => c.ToString()
        };

        protected string FormatComplexHelper(double re, double im, int decimals)
        {
            var t = Complex.GetType(re, im);
            if (t == Complex.Types.Real)
                return FormatReal(re, decimals);

            if (t == Complex.Types.Imaginary)
                return FormatReal(im, decimals) + 'i';

            var sRe = FormatReal(re, decimals);
            var sIm = FormatReal(Math.Abs(im), decimals);
            return im < 0 ?
                $"{sRe} – {sIm}i" :
                $"{sRe} + {sIm}i";
        }

        private const string Sharps = "################";

        internal static string FormatNumberHelper(double d, int decimals)
        {
            if (double.IsNaN(d))
                return " Undefined ";
            if (double.IsPositiveInfinity(d))
                return "+∞";
            if (double.IsNegativeInfinity(d))
                return "-∞";
            if (double.IsInfinity(d))
                return "∞";
            if (Math.Abs(d) < 1e16)
            {
                var a = Math.Abs(d);
                var i = GetDigits(a);
                if (i >= -2 * decimals - 1)
                {
                    if (i == 0)
                    {
                        if (a < 1)
                            i = decimals + 1;
                        else
                            i = decimals;
                    }
                    else if (i > 0)
                    {
                        i = decimals - i;
                        if (i < 0)
                            i = 0;
                    }
                    else
                        i = decimals - i + 1;

                    if (i <= 16)
                    {
                        var s = d.ToString("G17", CultureInfo.InvariantCulture);
                        if (!s.Contains('E'))
                        {
                            var dec = Math.Round(decimal.Parse(s, CultureInfo.InvariantCulture), i);
                            s = dec.ToString("G29", CultureInfo.InvariantCulture);
                            return s == "-0" ? "0" : s;
                        }
                    }
                }
            }
            var format = $"#.{Sharps[..decimals]}E+0";
            return d.ToString(format, CultureInfo.InvariantCulture);
        }

        private static int GetDigits(double d)
        {
            if (d >= 1)
                return d switch
                {
                    <= 1e4 => 0,
                    <= 1e5 => 1,
                    <= 1e6 => 2,
                    <= 1e7 => 3,
                    <= 1e8 => 4,
                    <= 1e9 => 5,
                    <= 1e10 => 6,
                    <= 1e11 => 7,
                    <= 1e12 => 8,
                    <= 1e13 => 9,
                    <= 1e14 => 10,
                    <= 1e15 => 11,
                    _ => 0
                };

            return d switch
            {
                0 => 0,
                < 1e-14 => -14,
                < 1e-13 => -13,
                < 1e-12 => -12,
                < 1e-11 => -11,
                < 1e-10 => -10,
                < 1e-9 => -9,
                < 1e-8 => -8,
                < 1e-7 => -7,
                < 1e-6 => -6,
                < 1e-5 => -5,
                < 1e-4 => -4,
                < 1e-3 => -3,
                < 1e-2 => -2,
                < 1e-1 => -1,
                _ => 0
            };
        }

        protected static double GetMaxVisibleMatrixValue(Matrix matrix, int maxCount, out int row, out int col)
        {
            var maxAbs = 0d;
            var m = Math.Min(maxCount, matrix.RowCount);
            var n = Math.Min(maxCount, matrix.ColCount);
            var lastRow = matrix.RowCount - 1;
            var lastCol = matrix.ColCount - 1;
            int ii = 0, jj = 0;
            for (int i = 0; i < m; ++i)
                GetMaxVisibleRowValue(i);
            
            if (maxCount < lastRow)
                GetMaxVisibleRowValue(lastRow);

            row = ii;
            col = jj;
            return maxAbs;

            void GetMaxVisibleRowValue(int i)
            {
                for (int j = 0; j < n; ++j)
                    CheckMatrixValue(i, j);

                if (maxCount < lastCol)
                    CheckMatrixValue(i, lastCol);
            }

            void CheckMatrixValue(int i, int j)
            {
                var d = Math.Abs(matrix[i, j].Re);
                if (d > maxAbs)
                {
                    maxAbs = d;
                    ii = i;
                    jj = j;
                }
            }
        }
    }

    internal class TextWriter : OutputWriter
    {
        internal override string UnitString(Unit units) => units.Text.Replace("‱", "‱ ");
        internal override string FormatInput(string s, Unit units, int line, bool isCalculated) =>
            units is null ? s : s + ' ' + units.Text;

        internal override string FormatSubscript(string sa, string sb) => sa + "_" + sb;
        internal override string FormatVariable(string name, string value) => 
            name[0] == '\u20D7' ? name[1..] : name;
        internal override string FormatUnits(string s) => s;
        internal override string FormatFunction(string s) => s;
        internal override string FormatRoot(string s, bool formatEquations, int level = 0, string n = "2") =>
            n switch
            {
                "2" => $"√({s})",
                "3" => $"³√({s})",
                "4" => $"⁴√({s})",
                _ => $"root({s}; {n})"
            };

        internal override string FormatOperator(char c) => FormatOperatorHelper(c);
        internal override string FormatPower(string sa, string sb, int level, int order) => sa + '^' + sb;
        internal override string FormatDivision(string sa, string sb, int level) => sa + '/' + sb;

        internal override string FormatValue(in Value v, int decimals)
        {
            var s = FormatComplex(v.Re, v.Im, decimals);
            if (v.Units is not null)
            {
                if (!v.IsReal)
                    s = AddBrackets(s);

                return s + v.Units.Text;
            }
            return s;
        }

        internal override string AddBrackets(string s, int level = 0, char left = '(', char right = ')') => left + s + right;
        internal override string FormatAbs(string s, int level = 0) => $"|{s}|";
        internal override string FormatReal(double d, int decimals, bool zeroSmall = false)
        {
            var s = FormatNumberHelper(d, decimals);
            if (double.IsNaN(d) || double.IsInfinity(d))
                return s;

            var i = s.LastIndexOf('E');
            if (i <= 0)
                return s;

            if (zeroSmall && s[i + 1] == '-')
                return "0";

            return s;
        }

        internal override string FormatComplex(double re, double im, int decimals)
        {
            if (double.IsNaN(re) && double.IsNaN(im))
                return " Undefined ";

            if (double.IsInfinity(re) && double.IsInfinity(im))
                return "∞";

            return FormatComplexHelper(re, im, decimals);
        }

        internal override string FormatNary(string symbol, string sub, string sup, string expr) =>
            $"{symbol}{{{expr}; {sub}...{sup}}}";

        internal override string FormatSwitch(string[] sa, int level = 0)
        {
            string s = "switch(" + sa[0];
            for (int i = 1, len = sa.Length; i < len; ++i)
                s += "; " + sa[i];

            return s + ")";
        }

        internal override string FormatIf(string sc, string sa, string sb, int level = 0) =>
            $"if({sc}; {sa}; {sb})";

        internal override string FormatMatrix(Matrix matrix, int decimals, int maxCount, bool zeroSmallElements)
        {
            var sb = new StringBuilder();
            var nc = matrix.ColCount - 1;
            var zeroThreshold = GetMaxVisibleMatrixValue(matrix, decimals, out int row, out int col) * 1e-14;
            var len = FormatMatrixValue(matrix[row, col], decimals, zeroSmallElements).Length;
            var format = $"{{0,{len}}}";
            var cont = string.Format(format, "...");
            sb.Append('[');
            for (int i = 0, nr = matrix.RowCount - 1; i <= nr; ++i)
            {
                if (i == maxCount)
                {
                    sb.Insert(sb.Length, cont, Math.Min(maxCount, nc));
                    i = nr;
                }
                for (int j = 0; j <= nc; ++j)
                {
                    if (j == maxCount)
                    {
                        sb.Append(cont);
                        j = nc;
                    }
                    var e = matrix[i, j];
                    var d = Math.Abs(e.Re);
                    var s = FormatMatrixValue(e, decimals, zeroSmallElements && d < zeroThreshold);
                    sb.Append(string.Format(format, s));
                    if (j < nc)
                        sb.Append("  ");
                }
                if (i < nr)
                    sb.Append('|'); 
            }
            sb.Append(']');
            return sb.ToString();
        }

        internal override string FormatMatrixValue(Value v, int decimals, bool zeroSmall)
        {
            var s = FormatReal(v.Re, decimals, zeroSmall);
            return v.Units is null ? s : s + v.Units.Text;
        }
    }

    internal class HtmlWriter : OutputWriter
    {
        private static readonly string[] SqrPad = [
            "&ensp;&hairsp;",
            "&ensp;&ensp;",
            "&emsp;&thinsp;",
            "&emsp;&ensp;"
        ];

        private static string RootPad(int level, string n) => level switch
        {
            0 => $"&hairsp;<sup class=\"nth\">{n}</sup>&hairsp;",
            1 => $"&nbsp;<small class=\"nth\">{n}</small>&nbsp;&hairsp;",
            2 => $"&hairsp;&nbsp;<small class=\"nth\">{n}</small>&nbsp;&thinsp;",
            _ => $"&ensp;<small class=\"nth\">{n}</small>&ensp;"
        };

        internal override string UnitString(Unit units) => units.Html;

        internal static string OffsetDivision(string s, int offset)
        {
            var n = s.IndexOf("\"dvc\"", StringComparison.Ordinal) + 4;
            if (n < 0)
                return s;

            return offset switch
            {
                -1 => s[..n] + " down" + s[n..],
                1 => s[..n] + " up" + s[n..],
                _ => s
            };
        }

        internal override string FormatInput(string s, Unit units, int line, bool isCalculated)
        {
            string output;
            if (s == "?")
                output = $"<input type=\"text\" size=\"2\" name=\"Var\" class=\"input-{line}\">&#8201;";
            else if (isCalculated)
                output = $"<u class=\"input-{line}\">{s}</u>&#8201;";
            else
                output = $"<input type=\"text\" size=\"2\" name=\"Var\" class=\"input-{line}\" value=\"{s}\">&#8201;";

            if (units is not null)
                return $"{output} <i>{units.Html}</i>";

            return output;
        }

        internal override string FormatSubscript(string sa, string sb) => $"{sa}<sub>{sb}</sub>";
        internal override string FormatVariable(string name, string value)
        {
            var s = string.IsNullOrEmpty(value) ?
                string.Empty :
                $" class=\"value\" data-value='{value}'";
            if (name[0] == '\u20D7')
                name = "<span class=\"vec\">\u20D7</span>" + name[1..];
            var i = name.IndexOf('_');
            if (i > 0)
            {
                var i1 = i + 1;
                if (i1 < name.Length)
                    return FormatSubscript($"<var{s}>{name[..i]}</var>", name[i1..]);
            }

            return $"<var{s}>{name}</var>";
        }

        internal override string FormatUnits(string s) => $"<i>{s}</i>";
        internal override string FormatFunction(string s)
        {
            var i = s.IndexOf('_');
            if (i > 0)
            {
                var i1 = i + 1;
                if (i1 < s.Length)
                    return FormatSubscript($"<b>{s[..i]}</b>", s[i1..]);
            }
            return $"<b>{s}</b>";
        }

        internal override string FormatRoot(string s, bool formatEquations, int level = 0, string n = "2")
        {
            if (n != "2")
            {
                if (level > 0)
                    return $"{RootPad(level, n)}<span class=\"o{level}\"><span class=\"r{level}\"></span>{s}</span>";

                return $"{RootPad(0, n)}<span class=\"o0\"><span class=\"r\">√</span>&hairsp;{s}</span>";
            }
            if (level > 0)
                return $"{SqrPad[level]}<span class=\"o{level}\"><span class=\"r{level}\"></span>{s}</span>";

            return $"{SqrPad[0]}<span class=\"o0\"><span class=\"r\">√</span>&hairsp;{s}</span>";
        }

        internal override string FormatOperator(char c) => c switch
        {
            '<' => " &lt; ",
            '>' => " &gt; ",
            '≤' => " &le; ",
            '≥' => " &ge; ",
            _ => FormatOperatorHelper(c),
        };

        internal override string FormatPower(string sa, string sb, int level, int order)
        {
            var s = level > 0 ? "<sup class=\"raised\">" : "<sup>";

            if (order == PowerOrder)
                s += '∙';
            return sa + s + sb + "</sup>";
        }

        internal override string FormatDivision(string sa, string sb, int level)
        {
            if (level < 4)
                return $"<span class=\"dvc\">{sa}<span class=\"dvl\"></span>{sb}</span>";

            return sa + " ÷ " + sb;
        }

        internal override string FormatNary(string symbol, string sub, string sup, string expr) =>
            $"<span class=\"dvr\"><small>{sup}</small><span class=\"nary\">{symbol}</span><small>{sub}</small></span>{expr}";

        internal override string FormatValue(in Value v, int decimals)
        {
            var s = FormatComplex(v.Re, v.Im, decimals);
            if (v.Units is null)
                return s;

            if (!v.IsReal)
                s = AddBrackets(s);

            return s + "&#8201;" + v.Units.Html;
        }

        internal override string AddBrackets(string s, int level = 0, char left = '(', char right = ')') =>
            level switch
            {
                0 => left + s + right,
                _ => FormatBrackets(left, level) + s + FormatBrackets(right, level)
            };

        private static string FormatLeftCurl(int level) =>
            $"<span class=\"c{level}\">{{</span>";

        private static string FormatBrackets(char symbol, int level) =>
            $"<span class=\"b{level}\">{symbol}</span>";

        internal override string FormatAbs(string s, int level = 0) =>
            level switch
            {
                0 => $"<b class=\"b0\">|</b>&hairsp;{s}&hairsp;<b class=\"b0\">|</b>",
                _ => $"<span class=\"b{level}\">|</span>{s}<span class=\"b{level}\">|</span>"
            };

        internal override string FormatReal(double d, int decimals, bool zeroSmall = false)
        {
            var s = FormatNumberHelper(d, decimals);
            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                return $"<span class=\"err\">{s}</span>";
            }
            var i = s.LastIndexOf('E');
            if (i <= 0)
                return s;

            var i1 = i + 1;
            var sign = s[i1++];
            if (sign is '+' or '-' or '0')
            {
                if (zeroSmall && sign == '-')
                    return "0";

                if (s[i1] is '0')
                    i1++;
            }
            return sign is '-' ?
                $"{s[..i]}×10<sup>-{s[i1..]}</sup>" :
                $"{s[..i]}×10<sup>{s[i1..]}</sup>";
        }

        internal override string FormatComplex(double re, double im, int decimals)
        {
            if (double.IsNaN(re) && double.IsNaN(im))
                return "<span class=\"err\"> Undefined </span>";

            if (double.IsInfinity(re) && double.IsInfinity(im))
                return "<span class=\"err\"> ∞ </span>";

            return FormatComplexHelper(re, im, decimals);
        }

        internal override string FormatSwitch(string[] sa, int level = 0)
        {
            var len = sa.Length;
            if (len == 1)
                return $"<b>switch</b>{AddBrackets(sa[0], level)}";

            if (len == 2)
                return $"<b>switch</b>{AddBrackets($"{sa[0]}; {sa[1]}", level)}";

            var s = $"if {sa[0]}: {sa[1]}";
            for (int i = 2; i < len; i += 2)
            {
                if (len - i == 1)
                    s += $"<br />else: {sa[i]}";
                else
                    s += $"<br />if {sa[i]}: {sa[i + 1]}";
            }
            if (level > 8)
                level = 8;

            return $"{FormatLeftCurl(level)}<span class=\"dvs\">{s}</span>";
        }

        internal override string FormatIf(string sc, string sa, string sb, int level = 0)
        {
            var s = $"if {sc}: {sa}<br />else: {sb}";
            return $"{FormatLeftCurl(level)}<span class=\"dvs\">{s}</span>";
        }

        internal override string FormatMatrix(Matrix matrix, int decimals, int maxCount, bool zeroSmallElements)
        {
            var sb = new StringBuilder();
            var nc = matrix.ColCount;
            var zeroThreshold = GetMaxVisibleMatrixValue(matrix, maxCount, out int _, out int _) * 1e-14;
            sb.AppendLine("<span class=\"matrix\">");
            for (int i = 0, nr = matrix.RowCount; i < nr; ++i)
            {
                sb.Append("<span class=\"tr\"><span class=\"td\"></span>");
                if (i == maxCount)
                {
                    sb.Insert(sb.Length, "<span class=\"td\">⋮</span>", Math.Min(maxCount, nc));
                    if (nc > maxCount)
                        sb.Append("<span class=\"td\">⋱</span><span class=\"td\">⋮</span>");

                    sb.Append("<span class=\"td\"></span></span><span class=\"tr\"><span class=\"td\"></span>");
                    i = nr - 1;
                }
                for (int j = 0; j < nc; ++j)
                {
                    if (j == maxCount)
                    {
                        sb.Append("<span class=\"td\">⋯</span>");
                        j = nc - 1;
                    }
                    var e = matrix[i, j];
                    var d = Math.Abs(e.Re);
                    var s = FormatMatrixValue(e, decimals, zeroSmallElements && d < zeroThreshold);
                    sb.Append($"<span class=\"td\">{s}</span>");
                }
                sb.AppendLine("<span class=\"td\"></span></span>");
            }
            sb.AppendLine("</span>");
            return sb.ToString();
        }

        internal override string FormatMatrixValue(Value v, int decimals, bool zeroSmall)
        {
            var s = FormatReal(v.Re, decimals, zeroSmall);
            return v.Units is null ? s : s + "&#8201;" + v.Units.Html;
        }   
    }

    internal class XmlWriter : OutputWriter
    {
        private const string wXmlns = "xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"";
        private const string NormalText = "<m:rPr><m:nor/></m:rPr>";
        internal override string UnitString(Unit units) => units.Xml;
        internal override string FormatInput(string s, Unit units, int line, bool isCalculated)
        {
            string output;
            if (s == "?")
                output = Run(s, $"<w:rPr {wXmlns}><w:color w:val=\"FF0000\" /><w:shd w:fill=\"FFFCCC\" /></w:rPr>");
            else if (isCalculated)
                output = Run(s, $"<w:rPr {wXmlns}><w:u /><w:shd w:val=\"clear\" w:color=\"auto\" w:fill=\"FFFCCC\" /></w:rPr>");
            else
                output = Run(s, $"<w:rPr {wXmlns}><w:shd w:val=\"clear\" w:color=\"auto\"  w:fill=\"FFFCCC\" /></w:rPr>");

            return units is null ? output : output + units.Xml;
        }
        internal override string FormatSubscript(string sa, string sb)
        {
            var s = sb.StartsWith('<') ? sb : Run(sb);
            return $"<m:sSub><m:e>{sa}</m:e><m:sub>{s}</m:sub></m:sSub>";
        }

        internal override string FormatVariable(string name, string value)
        {
            const string format = $"<w:rPr {wXmlns}><w:color w:val=\"0000FF\" /></w:rPr>";
            var i = name.IndexOf('_');
            if (i <= 0)
                return FormatVector(name, format);

            var i1 = i + 1;
            return i1 < name.Length ?
                FormatSubscript(FormatVector(name[..i], format), name[i1..]) :
                FormatVector(name, string.Empty);
        }

        private static string FormatVector(string s, string format) => s[0] == '\u20D7' ?
            $"<m:acc><m:accPr><m:chr m:val=\"\u20D7\"/></m:accPr><m:e>{Run(s[1..], format)}</m:e></m:acc>" :
            Run(s, format);

        internal override string FormatUnits(string s) =>
            Run(' ' + s, $"{NormalText}<w:rPr {wXmlns}><w:rFonts w:ascii=\"Cambria Math\" w:hAnsi=\"Cambria Math\" /><w:sz w:val=\"22\" /></w:rPr>");
        internal override string FormatFunction(string s)
        {
            var format = $"{NormalText}<w:rPr {wXmlns}><w:rFonts w:ascii=\"Cambria Math\" w:hAnsi=\"Cambria Math\" /><w:b w:val=\"true\" /></w:rPr>";
            var i = s.IndexOf('_');
            if (i <= 0) return
                Run(s, format);

            var i1 = i + 1;
            return i1 < s.Length ?
                FormatSubscript(Run(s[..i], format), s[(i + 1)..]) :
                Run(s, format);
        }
        internal override string FormatRoot(string s, bool formatEquations, int level = 0, string n = "2") => n switch
        {
            "2" => $"<m:rad><m:radPr><m:degHide m:val=\"1\"/></m:radPr><m:deg/><m:e>{s}</m:e></m:rad>",
            "3" => $"<m:rad><m:deg>{Run(n)}</m:deg><m:e>{s}</m:e></m:rad>",
            _ => $"<m:rad><m:deg>{n}</m:deg><m:e>{s}</m:e></m:rad>"
        };


        internal override string FormatOperator(char c) => c switch
        {
            '<' => Run("&lt;"),
            '>' => Run("&gt;"),
            '≤' => Run("&le;"),
            '≥' => Run("&ge;"),
            '*' => Run("·"),
            '⦼' => Run(" mod "),
            '∧' => Run(" and "),
            '∨' => Run(" or "),
            '⊕' => Run(" xor "),
            '|' => Run(" | "),
            Calculator.NegChar => Run("-", NormalText),
            _ => Run(c.ToString())
        };

        internal override string FormatPower(string sa, string sb, int level, int order)
        {
            const string se = "</m:sup></m:sSup>";
            if (order == PowerOrder)
                return sa[..^se.Length] + FormatOperator('*') + sb + se;

            return $"<m:sSup><m:e>{sa}</m:e><m:sup>{sb}{se}";
        }

        internal override string FormatDivision(string sa, string sb, int level) =>
            $"<m:f><m:num>{sa}</m:num><m:den>{sb}</m:den></m:f>";

        internal override string FormatNary(string symbol, string sub, string sup, string expr)
        {
            var sProp = $"<m:naryPr><m:chr m:val=\"{symbol}\"/><m:limLoc m:val=\"undOvr\"/></m:naryPr>";
            var sSubSup = $"<m:sub>{sub}</m:sub><m:sup>{sup}</m:sup>";
            var sExpr = $"<m:e>{expr}</m:e>";
            return $"<m:nary>{sProp}{sSubSup}{sExpr}</m:nary>";
        }

        internal override string FormatValue(in Value v, int decimals)
        {
            var s = FormatComplex(v.Re, v.Im, decimals);
            if (v.Units is null)
                return s;

            if (!v.IsReal)
                s = AddBrackets(s);

            return s + v.Units.Xml;
        }

        internal override string FormatAbs(string s, int level = 0) => AddBrackets(s, level, '|', '|');

        internal override string FormatReal(double d, int decimals, bool zeroSmall = false)
        {
            var s = FormatNumberHelper(d, decimals);
            if (double.IsNaN(d) || double.IsInfinity(d))
                return Run(s);//<w:rPr><w:color w:val=\"FF0000\" /></w:rPr>

            var i = s.LastIndexOf('E');
            if (i <= 0)
                return ValueRun(s);

            var i1 = i + 1;
            if (zeroSmall && s[i1] == '-')
                return Run("0");

            if (s[i1] == '+')
                i1++;

            var ms = ValueRun(s[..i]);
            var xs = Run("×", $"{NormalText}<w:rPr {wXmlns}><w:rFonts w:ascii=\"Cambria Math\" w:hAnsi=\"Cambria Math\" /><w:sz w:val=\"16\" /></w:rPr>");
            var es = ValueRun(s[i1..]);
            return $"{ms + xs}<m:sSup><m:e>{Run("10")}</m:e><m:sup>{es}</m:sup></m:sSup>";
            
            static string ValueRun(string s) => s[0] == '-' ? Run("-", NormalText) + Run(s[1..]) : Run(s);
        }


        internal override string FormatComplex(double re, double im, int decimals)
        {
            if (double.IsNaN(re) && double.IsNaN(im))
                return Run("Undefined");//<w:rPr><w:color w:val=\"FF0000\" /></w:rPr>

            if (double.IsInfinity(re) && double.IsInfinity(im))
                return Run("∞");

            var t = Complex.GetType(re, im);
            if (t == Complex.Types.Real)
                return FormatReal(re, decimals);

            var sImaginary = FormatReal(Math.Abs(im), decimals) + Run("i");
            if (t == Complex.Types.Imaginary)
                return sImaginary;

            var sReal = FormatReal(re, decimals);
            return im < 0 ?
                sReal + Run("–") + sImaginary :
                sReal + Run("+") + sImaginary;
        }

        internal static string Run(string content) => $"<m:r><m:t>{content}</m:t></m:r>";
        internal static string Run(string content, string format) => $"<m:r>{format}<m:t>{content}</m:t></m:r>";
        internal override string AddBrackets(string s, int level = 0, char left = '(', char right = ')') =>
            Brackets(s, left, right);

        internal static string Brackets(string s, char left, char right) =>
            $"<m:d><m:dPr><m:begChr m:val=\"{left}\"/><m:endChr m:val=\"{right}\"/></m:dPr><m:e>{s}</m:e></m:d>";

        internal override string FormatSwitch(string[] sa, int level = 0)
        {
            var len = sa.Length;
            if (len == 1)
                return Run("switch") + AddBrackets(sa[0], level);

            if (len == 2)
                return Run("switch") + AddBrackets(sa[0] + Run("; ") + sa[1], level);

            var s = FormatIfRow(sa[0], sa[1]);
            for (int i = 2; i < len; i += 2)
            {
                if (len - i == 1)
                    s += FormatElseRow(sa[i]);
                else
                    s += FormatIfRow(sa[i], sa[i + 1]);
            }
            return $"<m:d><m:dPr><m:begChr m:val=\"{{\"/><m:endChr m:val=\" \"/></m:dPr><m:e><m:m>{mPr}{s}</m:m></m:e></m:d>";
        }

        internal override string FormatIf(string sc, string sa, string sb, int level = 0)
        {
            var s = FormatIfRow(sc, sa) + FormatElseRow(sb);
            return $"<m:d><m:dPr><m:begChr m:val=\"{{\"/><m:endChr m:val=\" \"/></m:dPr><m:e><m:m>{mPr}{s}</m:m></m:e></m:d>";
        }

        internal override string FormatMatrix(Matrix matrix, int decimals, int maxCount, bool zeroSmallElements)
        {
            var sb = new StringBuilder();
            var nr = matrix.RowCount;
            var nc = matrix.ColCount;
            var zeroThreshold = GetMaxVisibleMatrixValue(matrix, maxCount, out int _, out int _) * 1e-14;
            sb.Append($"<m:d><m:dPr><m:begChr m:val=\"[\"/><m:endChr m:val=\"]\"/></m:dPr><m:e><m:m><m:mPr><m:mcs><m:mc><m:mcPr><m:count m:val=\"{nc}\"/><m:mcJc m:val=\"center\"/></m:mcPr></m:mc></m:mcs></m:mPr>");
            for (int i = 0; i < nr; ++i)
            {
                sb.Append("<m:mr>");
                if (i == maxCount)
                {
                    sb.Insert(sb.Length, td("⋮"), Math.Min(maxCount, nc));
                    if (nc > maxCount)
                        sb.Append(td("⋱")).Append(td("⋮"));

                    sb.Append("</m:mr><m:mr>");
                    i = nr - 1;
                }
                for (int j = 0; j < nc; ++j)
                {
                    if (j == maxCount)
                    {
                        sb.Append(td("⋯"));
                        j = nc - 1;
                    }
                    var e = matrix[i, j];
                    var d = Math.Abs(e.Re);
                    var s = FormatMatrixValue(e, decimals, zeroSmallElements && d < zeroThreshold);
                    sb.Append($"<m:e>{s}</m:e>");
                }
                sb.Append("</m:mr>");
            }
            sb.Append("</m:m></m:e></m:d>");
            return sb.ToString();

            static string td(string s) => $"<m:e><m:r><m:t>{s}</m:t></m:r></m:e>";
        }

        internal override string FormatMatrixValue(Value v, int decimals, bool zeroSmall)
        {
            var s = FormatReal(v.Re, decimals, zeroSmall);
            return v.Units is null ? s : s + v.Units.Xml;
        }

        private const string mPr = "<m:mPr><m:mcs><m:mc><m:mcPr><m:count m:val=\"2\"/><m:mcJc m:val=\"left\"/></m:mcPr></m:mc></m:mcs></m:mPr>";

        private static string FormatIfRow(string sa, string sb) =>
            $"<m:mr><m:e>{Run(" if ")}{sa}{Run(":")}</m:e><m:e>{sb}</m:e></m:mr>";
        private static string FormatElseRow(string sa) =>
            $"<m:mr><m:e>{Run(" else:")}</m:e><m:e>{sa}</m:e></m:mr>";
    }
}
