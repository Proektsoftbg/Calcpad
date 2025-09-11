using System;
using System.ComponentModel;
using System.Text;

namespace Calcpad.Core
{
    internal class XmlWriter : OutputWriter
    {
        internal XmlWriter(MathSettings settings, bool phasor) : base(settings, phasor) 
        {
            AngleUnits =
[
                Run("°"),
                string.Empty,
                FormatUnits("gra"),
            ];
        }
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

        internal override string FormatVariable(string name, string value, bool isBold)
        {

            var i = name.IndexOf('_');
            if (i <= 0)
                return FormatVector(name, isBold);

            var i1 = i + 1;
            return i1 < name.Length ?
                FormatSubscript(FormatVector(name[..i], isBold), name[i1..]) :
                FormatVector(name, isBold);
        }

        private static string FormatVector(string s, bool isBold)
        {
            var isVector = s[0] == '\u20D7';
            var format = $"<w:rPr {wXmlns}><w:color w:val=\"0066DD\"/></w:rPr>";
            var formatBold = isBold ?
                $"<w:rPr {wXmlns}><w:color w:val=\"0044AA\"/></w:rPr>" :
                format;
            return isVector ?
                $"<m:acc><m:accPr><m:chr m:val=\"\u20D7\"/></m:accPr><m:e>{Run(s[1..], format)}</m:e></m:acc>" :
                Run(s, formatBold);
        }

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
        internal override string FormatRoot(string s, int level = 0, string n = "2") => n switch
        {
            "2" => $"<m:rad><m:radPr><m:degHide m:val=\"1\"/></m:radPr><m:deg/><m:e>{s}</m:e></m:rad>",
            "3" => $"<m:rad><m:deg>{Run(n)}</m:deg><m:e>{s}</m:e></m:rad>",
            _ => $"<m:rad><m:deg>{n}</m:deg><m:e>{s}</m:e></m:rad>"
        };


        internal override string FormatOperator(char c) => c switch
        {
            '<' => Run("&lt;"),
            '>' => Run("&gt;"),
            '≤' => Run("≤"),
            '≥' => Run("≥"),
            '*' => Run("·"),
            '÷' => Run("/"),
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

        internal override string FormatValue(in IScalarValue value)
        {
            var u = value.Units;
            var s = FormatComplex(value.Re, value.Im, u?.FormatString);
            if (u is null)
                return s;

            if (!(value.IsReal || phasor && s.Contains('∠')))
                s = AddBrackets(s);

            return s + u.Xml;
        }

        internal override string FormatAbs(string s, int level = 0) => AddBrackets(s, level, '|', '|');

        internal override string FormatReal(double d, string format, bool zeroSmall)
        {
            var s = FormatNumberHelper(d, format);
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

            var ms = s[..i];
            if (string.Equals(ms, "1", StringComparison.Ordinal))
                ms = string.Empty;
            else
                ms = ValueRun(ms) +
                     Run("×", $"{NormalText}<w:rPr {wXmlns}><w:rFonts w:ascii=\"Cambria Math\" w:hAnsi=\"Cambria Math\" /><w:sz w:val=\"16\" /></w:rPr>");

            var es = ValueRun(s[i1..]);
            return $"{ms}<m:sSup><m:e>{Run("10")}</m:e><m:sup>{es}</m:sup></m:sSup>";

            static string ValueRun(string s) => s.StartsWith('-') ? Run("-", NormalText) + Run(s[1..]) : Run(s);
        }

        private static readonly string _runI = Run("i");
        private static readonly string _runPlus = Run("+");
        private static readonly string _runMinus = Run("-");
        private static readonly string _runPh = Run("∠");
        internal override string FormatComplex(double re, double im, string format)
        {
            if (double.IsNaN(re) && double.IsNaN(im))
                return Run("Undefined");//<w:rPr><w:color w:val=\"FF0000\" /></w:rPr>

            if (double.IsInfinity(re) && double.IsInfinity(im))
                return Run("∞");

            var t = Complex.GetType(re, im);
            if (t == Complex.Types.Real)
                return FormatReal(re, format, zeroSmallElements);

            if (phasor)
            {
                var abs = Math.Sqrt(re * re + im * im);
                var phase = Math.Atan2(im, re) * AngleFactors[degrees];
                var absString = FormatReal(abs, format, false);
                var phaseString = FormatReal(phase, format, false) + AngleUnits[degrees];
                return absString + _runPh + phaseString;
            }

            var sImaginary = FormatReal(Math.Abs(im), format, zeroSmallElements) + _runI;
            if (t == Complex.Types.Imaginary)
                return sImaginary;

            var sReal = FormatReal(re, format, zeroSmallElements);
            return im < 0 ?
                sReal + _runMinus + sImaginary :
                sReal + _runPlus + sImaginary;
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

        internal override string FormatMatrix(Matrix matrix)
        {
            var sb = new StringBuilder();
            var nr = matrix.RowCount;
            var nc = matrix.ColCount;
            var zeroThreshold = GetMaxVisibleMatrixValue(matrix, out int _, out int _) * 1e-14;
            if (matrix is not HpMatrix hp_m)
                hp_m = null;

            var units = hp_m?.Units;
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
                    string s;
                    if (hp_m is null)
                        s = FormatMatrixValue(matrix[i, j], zeroThreshold);
                    else
                    {
                        var d = hp_m.GetValue(i, j);
                        s = FormatReal(d, units?.FormatString, zeroSmallElements && Math.Abs(d) < zeroThreshold);
                    }
                    sb.Append($"<m:e>{s}</m:e>");
                }
                sb.Append("</m:mr>");
            }
            sb.Append("</m:m></m:e></m:d>");
            if (units is not null)
                sb.Append(units.Xml);

            return sb.ToString();

            static string td(string s) => $"<m:e><m:r><m:t>{s}</m:t></m:r></m:e>";
        }

        internal override string FormatVector(Vector vector)
        {
            var div = Run(VectorSpacing);
            var sb = new StringBuilder();
            const double tol = 1e-14;
            var zeroThreshold = GetMaxVisibleVectorValue(vector) * tol;
            if (zeroThreshold > tol)
                zeroThreshold = tol;

            if (vector is not HpVector hp_v)
                hp_v = null;

            var units = hp_v?.Units;
            var len = vector.Length;
            for (int i = 0; i < len; ++i)
            {
                if (i > 0)
                    sb.Append(div);

                if (i == maxCount)
                {
                    sb.Append(Run("...")).Append(div);
                    break;
                }
                AppendElement(i);
            }
            var last = len - 1;
            if (maxCount < last)
                AppendElement(last);

            string s = AddBrackets(sb.ToString(), 0, '[', ']');

            if (units is not null)
                s += units.Xml;

            return s;

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
            return value.Units is null ? s : s + value.Units.Xml;
        }

        private const string mPr = "<m:mPr><m:mcs><m:mc><m:mcPr><m:count m:val=\"2\"/><m:mcJc m:val=\"left\"/></m:mcPr></m:mc></m:mcs></m:mPr>";

        private static string FormatIfRow(string sa, string sb) =>
            $"<m:mr><m:e>{Run(" if ")}{sa}{Run(":")}</m:e><m:e>{sb}</m:e></m:mr>";
        private static string FormatElseRow(string sa) =>
            $"<m:mr><m:e>{Run(" else:")}</m:e><m:e>{sa}</m:e></m:mr>";
    }
}
