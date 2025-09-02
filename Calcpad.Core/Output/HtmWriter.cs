using System;
using System.Text;

namespace Calcpad.Core
{
    internal class HtmlWriter : OutputWriter
    {
        internal HtmlWriter(MathSettings settings, bool phasor) : base(settings, phasor)
        {
            AngleUnits =
            [
                "°",
                "",
                ThinSpace + "<i>grad</i>",
            ];
        }

        private static readonly string[] SqrPad = [
            "&ensp;&hairsp;&hairsp;",
            "&ensp;&ensp;",
            "&emsp;&thinsp;",
            "&emsp;&ensp;"
        ];

        private static string RootPad(int level, string n) => level switch
        {
            0 => $"&hairsp;<sup class=\"nth\">{n}</sup>&hairsp;&hairsp;",
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
                -1 => $"{s[..n]} down{s[n..]}",
                1 => $"{s[..n]} up{s[n..]}",
                _ => s
            };
        }

        internal override string FormatInput(string s, Unit units, int line, bool isCalculated)
        {
            string output;
            if (s == "?")
                output = $"<input type=\"text\" size=\"2\" name=\"Var\" class=\"input-{line}\">\u2009";
            else if (isCalculated)
                output = $"<u class=\"input-{line}\">{s}</u>\u2009";
            else
                output = $"<input type=\"text\" size=\"2\" name=\"Var\" class=\"input-{line}\" value=\"{s}\">\u2009";

            if (units is not null)
                return $"{output} <i>{units.Html}</i>";

            return output;
        }

        internal override string FormatSubscript(string sa, string sb) => $"{sa}<sub>{sb}</sub>";
        internal override string FormatVariable(string name, string value, bool isBold)
        {
            var s = string.IsNullOrEmpty(value) ?
                string.Empty :
                $" class=\"value\" data-value='{value}'";
            if (name[0] == '\u20D7')
            {
                isBold = false;
                name = "<span class=\"vec\">\u20D7</span>" + name[1..];
            }

            var i = name.IndexOf('_');
            if (i > 0)
            {
                var i1 = i + 1;
                if (i1 < name.Length)
                    return FormatSubscript(FormatBold($"<var{s}>{name[..i]}</var>"), name[i1..]);
            }
            return FormatBold($"<var{s}>{name}</var>");

            string FormatBold(string s) => isBold ? $"<b>{s}</b>" : s;
        }

        internal override string FormatUnits(string s) => s == "10" ? "10" : $"<i>{s}</i>";
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

        internal override string FormatRoot(string s, int level = 0, string n = "2")
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
            '|' => ' ' + FormatBrackets('|', 0) + ' ',
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
            {
                if (level == 3)
                {
                    sa = sa.Replace("dvc up", "dvc").Replace("dvc down", "dvc");
                    sb = sb.Replace("dvc up", "dvc").Replace("dvc down", "dvc");
                }
                return $"<span class=\"dvc\">{sa}<span class=\"dvl\"></span>{sb}</span>";
            }
            return sa + " ÷ " + sb;
        }

        internal override string FormatNary(string symbol, string sub, string sup, string expr) =>
            $"<span class=\"dvr\"><small>{sup}</small><span class=\"nary\">{symbol}</span><small>{sub}</small></span>{expr}";

        internal override string FormatValue(in IScalarValue value)
        {
            var u = value.Units;
            var s = FormatComplex(value.Re, value.Im, u?.FormatString);
            var uHtml = u?.Html;
            if (string.IsNullOrEmpty(uHtml))
                return s;

            if (!(value.IsReal || phasor && s.Contains('∠')))
                s = AddBrackets(s);

            if (u.IsAngle && uHtml.StartsWith('°'))
                return s + uHtml;

            return s + ThinSpace + uHtml;
        }

        internal override string AddBrackets(string s, int level = 0, char left = '(', char right = ')') =>
            level == 0 && left == '(' ?
            $"\u200A{left}\u200A{s}\u200A{right}\u200A" :
            FormatBrackets(left, level) + s + FormatBrackets(right, level);

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

        internal override string FormatReal(double d, string format, bool zeroSmall)
        {
            var s = FormatNumberHelper(d, format);
            if (double.IsNaN(d) || double.IsInfinity(d))
            {
                return $"<span class=\"err\">{s}</span>";
            }
            var i = s.LastIndexOf('E');
            if (i <= 0)
                return s;

            var i1 = i + 1;
            var sign = s[i1];
            if (i1 < s.Length - 1)
                ++i1;

            if (sign is '+' or '-' or '0')
            {
                format ??= formatString;
                if (zeroSmall && sign == '-')
                    return "0";

                if (s[i1] == '0' && (format is null || !format.Contains('E')))
                    i1++;
            }
            var ms = s[..i];
            if (string.Equals(ms, "1", StringComparison.Ordinal))
                ms = string.Empty;
            else
                ms += '×';

            if (sign == '+' && format is not null && (format.Contains("E+") || format.StartsWith('E')))
                return $"{ms}10<sup>+{s[i1..]}</sup>";

            return sign == '-' ?
                $"{ms}10<sup>-{s[i1..]}</sup>" :
                $"{ms}10<sup>{s[i1..]}</sup>";
        }

        internal override string FormatComplex(double re, double im, string format)
        {
            if (double.IsNaN(re) && double.IsNaN(im))
                return "<span class=\"err\"> Undefined </span>";

            if (double.IsInfinity(re) && double.IsInfinity(im))
                return "<span class=\"err\"> ∞ </span>";

            return FormatComplexHelper(re, im, format);
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

        internal override string FormatMatrix(Matrix matrix)
        {
            var sb = new StringBuilder();
            var nc = matrix.ColCount;
            var zeroThreshold = GetMaxVisibleMatrixValue(matrix, out int _, out int _) * 1e-14;
            if (matrix is not HpMatrix hp_m)
                hp_m = null;

            var units = hp_m?.Units;
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
                    string s;
                    if (hp_m is null)
                        s = FormatMatrixValue(matrix[i, j], zeroThreshold);
                    else
                    {
                        var d = hp_m.GetValue(i, j);
                        s = FormatReal(d, units?.FormatString, zeroSmallElements && Math.Abs(d) < zeroThreshold);
                    }
                    sb.Append($"<span class=\"td\">{s}</span>");
                }
                sb.AppendLine("<span class=\"td\"></span></span>");
            }
            sb.AppendLine("</span>");
            if (units is not null)
                sb.Append(HairSpace).Append(units.Html);

            return sb.ToString();
        }

        internal override string FormatVector(Vector vector)
        {
            var div = VectorSpacing;
            var sb = new StringBuilder();
            const double tol = 1e-14;
            var zeroThreshold = GetMaxVisibleVectorValue(vector) * tol;
            if (zeroThreshold > tol)
                zeroThreshold = tol;

            if (vector is not HpVector hp_v)
                hp_v = null;

            var units = hp_v?.Units;
            var len = vector.Length;
            sb.Append("<b class=\"b0\">[</b>");
            for (int i = 0; i < len; ++i)
            {
                if (i > 0)
                    sb.Append(div);

                if (i == maxCount)
                {
                    var n = len - maxCount;
                    sb.Append($"<span title=\"{n - Math.Sign(n - 1)} elements skipped.\">...</span>")
                        .Append(div);
                    break;
                }
                AppendElement(i);
            }
            var last = len - 1;
            if (maxCount < last)
                AppendElement(last);
            sb.Append("<b class=\"b0\">]</b>");

            if (units is not null)
                sb.Append(HairSpace).Append(units.Html);

            return sb.ToString();

            void AppendElement(int index)
            {
                if (hp_v is null)
                    sb.Append(FormatMatrixValue(vector[index], zeroThreshold));
                else
                {
                    var d = hp_v.GetValue(index);
                    sb.Append(FormatReal(d, units?.FormatString, zeroSmallElements && Math.Abs(d) < zeroThreshold));
                }
            }
        }

        internal override string FormatMatrixValue(RealValue value, double zeroThreshold)
        {
            var d = value.D;
            var s = FormatReal(d, value.Units?.FormatString, zeroSmallElements && Math.Abs(d) < zeroThreshold);
            return value.Units is null ? s : s + ThinSpace + value.Units.Html;
        }
    }
}
