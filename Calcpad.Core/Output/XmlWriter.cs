using System;
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
                FormatUnitsStatic("gra"),
            ];
        }
        protected override OutputFormat FormatType => OutputFormat.Xml;
        internal const string NormalText = "<m:rPr><m:nor/></m:rPr>";
        internal static readonly string UnitDivision = Run("∕", 
        @$"{NormalText}<w:rPr>
            <w:rFonts w:ascii=""Cambria Math"" w:hAnsi=""Cambria Math"" />
            <w:sz w:val=""20"" />
        </w:rPr>");
        internal static readonly string UnitProduct = Run("·", NormalText);
        internal override string UnitString(Unit units) => units.Xml;
        internal override string FormatInput(string s, Unit units, int line, bool isCalculated)
        {
            const string format1 = 
            @$"<w:rPr>
                <w:color w:val=""FF0000"" />
                <w:shd w:fill=""FFFCCC"" />
            </w:rPr>";
            const string format2 = 
            @$"<w:rPr>
                <w:u />
                <w:shd w:val=""clear"" w:color=""auto"" w:fill=""FFFCCC"" />
            </w:rPr>";
            const string format3 = 
            @$"<w:rPr>
                <w:shd w:val=""clear"" w:color=""auto""  w:fill=""FFFCCC"" />
            </w:rPr>";
            string output;
            if (s == "?")
                output = Run(s, format1);
            else if (isCalculated)
                output = Run(s, format2);
            else
                output = Run(s, format3);

            return units is null ? output : output + units.Xml;
        }


        internal override string AppendSubscript(string sa, string sb)
        {
            var s = sb.StartsWith('<') ? Run(".") + sb : Run("." + sb);
            var i = sa.LastIndexOf("</m:sub>");
            return $"{sa[..i]}{s}</m:sub></m:sSub>";
        }

        internal override string FormatSubscript(string sa, string sb)
        {
            var s = sb.StartsWith('<') ? sb : Run(sb);
            return @$"<m:sSub>
                <m:e>{sa}</m:e>
                <m:sub>{s}</m:sub>
            </m:sSub>";
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
            const char vecCHar = '\u20D7';
            var isVector = s[0] == vecCHar;
            const string format = 
            @$"<w:rPr>
                <w:color w:val=""0066DD""/>
            </w:rPr>";
            const string formatBold = 
            @$"<w:rPr>
                <w:color w:val=""0044AA""/>
            </w:rPr>";
            return isVector ? 
                @$"<m:acc>
                    <m:accPr>
                        <m:chr m:val=""{vecCHar}""/>
                    </m:accPr>
                    <m:e>{Run(s[1..], format)}</m:e>
                </m:acc>" :
                Run(s, isBold ? formatBold : format);
        }


        internal override string FormatUnits(string s) => FormatUnitsStatic(s);

        private static string FormatUnitsStatic(string s)
        {
            const string format = 
            @$"{NormalText}<w:rPr>
                <w:rFonts w:ascii=""Cambria Math"" w:hAnsi=""Cambria Math"" />
                <w:sz w:val=""22"" />
            </w:rPr>";
            return Run(s, format);
        }

        internal override string FormatFunction(string s)
        {
            const string format =
            @$"{NormalText}<w:rPr>
                <w:rFonts w:ascii=""Cambria Math"" w:hAnsi=""Cambria Math"" />
                <w:b w:val=""true"" />
            </w:rPr>";
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
            "2" => @$"<m:rad>
                <m:radPr>
                    <m:degHide m:val=""1""/>
                </m:radPr>
                <m:deg/>
                <m:e>{s}</m:e>
            </m:rad>",

            "3" => @$"<m:rad>
                <m:deg>{Run(n)}</m:deg>
                <m:e>{s}</m:e>
            </m:rad>",

            _ => @$"<m:rad>
                <m:deg>{n}</m:deg>
                <m:e>{s}</m:e>
            </m:rad>"
        };

        private static readonly string[] opRuns = [
            Run("&lt;"),
            Run("&gt;"),
            Run("≤"),
            Run("≥"),
            Run("·"),
            Run("/"),
            Run("/"),
            Run(" mod ", NormalText),
            Run(" and ", NormalText),
            Run(" or ", NormalText),
            Run(" xor ", NormalText),
            Run(" | "),
            Run("-", NormalText),
            ];

        internal override string FormatOperator(char c) => c switch
        {
            '<' => opRuns[0],
            '>' => opRuns[1],
            '≤' => opRuns[2],
            '≥' => opRuns[3],
            '*' => opRuns[4],
            '/' => opRuns[5],
            '÷' => opRuns[6],
            '⦼' => opRuns[7],
            '∧' => opRuns[8],
            '∨' => opRuns[9],
            '⊕' => opRuns[10],
            '|' => opRuns[11],
            Calculator.NegChar => opRuns[12],
            _ => Run(c.ToString())
        };

        internal override string FormatPower(string sa, string sb, int level, int order)
        {
            const string se = "</m:sup></m:sSup>";
            if (order == PowerOrder)
                return string.Concat(sa[..^se.Length], FormatOperator('*'), sb, se);

            return @$"<m:sSup>
                <m:e>{sa}</m:e>
                <m:sup>{sb}</m:sup>
            </m:sSup>";
        }

        internal override string FormatDivision(string sa, string sb, int level) =>
        @$"<m:f>
            <m:num>{sa}</m:num>
            <m:den>{sb}</m:den>
        </m:f>";

        internal override string FormatNary(string symbol, string sub, string sup, string expr) =>
        @$"<m:nary>
            <m:naryPr>
                <m:chr m:val=""{symbol}""/>
                <m:limLoc m:val=""undOvr""/>
            </m:naryPr>
            <m:sub>{sub}</m:sub>
            <m:sup>{sup}</m:sup>
            <m:e>{expr}</m:e>
        </m:nary>";

        internal static readonly string RunThinSpace = $"<m:r><m:t>{ThinSpace}</m:t></m:r>";
        internal override string FormatValue(in IScalarValue value)
        {
            var u = value.Units;
            var s = FormatComplex(value.Re, value.Im, u?.FormatString);
            if (u is null)
                return s;

            if (!(value.IsReal || phasor && s.Contains('∠')))
                s = AddBrackets(s);

            return string.Concat(s, RunThinSpace, u.Xml);
        }

        internal override string FormatAbs(string s, int level = 0) => AddBrackets(s, level, '|', '|');
        private static readonly string Run0 = Run("0");
        private static readonly string Run10 = Run("10");
        private const string xFormat =
        @$"{NormalText}<w:rPr>
            <w:rFonts w:ascii=""Cambria Math"" w:hAnsi=""Cambria Math"" />
            <w:b/>
            <w:sz w:val=""20"" />
            <w:vertAlign w:val=""superscript"" />
        </w:rPr>";
        private static readonly string RunX = Run("×", xFormat);
        private static readonly string RunMinusNormal = Run("-", NormalText);

        internal override string FormatReal(double d, string format, bool zeroSmall)
        {
            var s = FormatNumberHelper(d, format);
            if (double.IsNaN(d))
                return Run(s, $"{NormalText}<w:rPr><w:color w:val=\"FF0000\" /></w:rPr>");

            if (double.IsInfinity(d))
                return Run(s, $"<w:rPr><w:color w:val=\"FF0000\" /></w:rPr>");

            var i = s.LastIndexOf('E');
            int i1;
            char sign;
            if (i <= 0)
            {
                i = s.LastIndexOf('e');
                if (i < 0)
                    return ValueRun(s);

                i1 = i + 1;
                sign = i1 < s.Length ? s[i1] : ' ' ;
                if (sign is '+' or '-')
                    ++i1;

                return string.Concat(ValueRun(s[..i]), Run(s[i..i1], NormalText), Run(s[i1..]));
            }

            i1 = i + 1;
            sign = s[i1];
            if (i1 < s.Length - 1)
                ++i1;

            if (sign is '+' or '-' or '0')
            {
                if (zeroSmall && sign == '-')
                    return Run0;

                format ??= formatString;
                if (s[i1] == '0' && (format is null || !format.Contains('E')))
                    i1++;
            }
            var ms = s[..i];
            if (string.Equals(ms, "1", StringComparison.Ordinal))
                ms = string.Empty;
            else
                ms = ValueRun(ms) + RunX;

            var es = s[i1..];
            if (sign == '+' && format is not null && (format.Contains("E+") || format.StartsWith('E')))
                es = Run("+" + es);
            else if (sign == '-')
                es = RunMinusNormal + Run(es);
            else
                es = Run(es);

            return @$"{ms}<m:sSup>
                <m:e>{Run10}</m:e>
                <m:sup>{es}</m:sup>
            </m:sSup>";

            static string ValueRun(string s) =>
                s.StartsWith('-') ?
                    RunMinusNormal + Run(s[1..]) :
                    Run(s);
        }
 
        private static readonly string RunI = Run("i");
        private static readonly string RunPlus = Run("+");
        private static readonly string RunMinus = Run("-");
        private static readonly string RunPh = Run("∠");
        private static readonly string RunUndefined = Run("Undefined", $"{NormalText}<w:rPr><w:color w:val=\"FF0000\" /></w:rPr>");
        private static readonly string RunInfinity = Run("∞", $"<w:rPr><w:color w:val=\"FF0000\" /></w:rPr>");

        internal override string FormatComplex(double re, double im, string format)
        {
            if (double.IsNaN(re) && double.IsNaN(im))
                return RunUndefined;

            if (double.IsInfinity(re) && double.IsInfinity(im))
                return RunInfinity;

            var t = Complex.GetType(re, im);
            if (t == Complex.Types.Real)
                return FormatReal(re, format, false);

            if (phasor)
            {
                var abs = Math.Sqrt(re * re + im * im);
                var phase = Math.Atan2(im, re) * AngleFactors[degrees];
                var absString = FormatReal(abs, format, false);
                var phaseString = FormatReal(phase, format, false) + AngleUnits[degrees];
                return string.Concat(absString, RunPh, phaseString);
            }

            var sImaginary = FormatReal(Math.Abs(im), format, false) + RunI;
            if (t == Complex.Types.Imaginary)
                return sImaginary;

            var sReal = FormatReal(re, format, false);
            return string.Concat(sReal, im < 0 ? RunMinus : RunPlus, sImaginary);
        }
        internal static string Run(string content) => $"<m:r><m:t>{content}</m:t></m:r>";
        internal static string Run(string content, string format) => $"<m:r>{format}<m:t>{content}</m:t></m:r>";
        internal static string Run(StringBuilder content) => content.Insert(0, "<m:r><m:t>").Append("</m:t></m:r>").ToString();
        internal override string AddBrackets(string s, int level = 0, char left = '(', char right = ')') =>
            Brackets(s, left, right);

        internal static string Brackets(string s, char left, char right) =>
        @$"<m:d>
            <m:dPr>
                <m:begChr m:val=""{left}""/>
                <m:endChr m:val=""{right}""/>
            </m:dPr>
            <m:e>{s}</m:e>
        </m:d>";

        private static readonly string RunSwitch = Run("switch", NormalText);
        private static readonly string RunSemicolon = Run("; ");
        internal override string FormatSwitch(string[] sa, int level = 0)
        {
            var len = sa.Length;
            if (len == 1)
                return RunSwitch + AddBrackets(sa[0], level);

            if (len == 2)
                return RunSwitch + AddBrackets(string.Concat(sa[0], RunSemicolon, sa[1]), level);

            var sb = new StringBuilder(
            @"<m:d>
                <m:dPr>
                    <m:begChr m:val=""{""/>
                    <m:endChr m:val="" ""/>
                </m:dPr>
                <m:e>
                    <m:m>"
                ).Append(mPr).Append(FormatIfRow(sa[0], sa[1]));
            for (int i = 2; i < len; i += 2)
            {
                if (len - i == 1)
                    sb.Append(FormatElseRow(sa[i]));
                else
                    sb.Append(FormatIfRow(sa[i], sa[i + 1]));
            }
            sb.Append("</m:m></m:e></m:d>");
            return sb.ToString();
        }

        internal override string CloseCurlyBrackets(string sa, int level)
        {
            const string rightBracketString = "<m:endChr m:val=\" \"/>";
            int index = sa.IndexOf(rightBracketString);
            if (index < 0)
                return sa;

            return string.Concat(sa[..index], "<m:endChr m:val=\"}\"/>", sa[(index + rightBracketString.Length)..]);
        }

        private const string mPr = 
        @"<m:mPr>
            <m:mcs>
                <m:mc>
                    <m:mcPr>
                        <m:count m:val=""2""/>
                        <m:mcJc m:val=""left""/>
                    </m:mcPr>
                </m:mc>
            </m:mcs>
        </m:mPr>";

        internal override string FormatIf(string sc, string sa, string sb, int level = 0) =>
        @$"<m:d>
            <m:dPr>
                <m:begChr m:val=""{{""/>
                <m:endChr m:val="" ""/>
            </m:dPr>
            <m:e>
                <m:m>
                    {mPr}
                    {FormatIfRow(sc, sa) + FormatElseRow(sb)}
                </m:m>
            </m:e>
        </m:d>";

        internal override string FormatMatrix(Matrix matrix)
        {
            const string smallerFontProperty = @"<w:rPr><w:sz w:val=""21"" /></w:rPr>";
            var nr = matrix.RowCount;
            var nc = matrix.ColCount;
            var zeroThreshold = GetMaxVisibleMatrixValue(matrix, out int _, out int _) * 1e-14;
            if (matrix is not HpMatrix hp_m)
                hp_m = null;

            var units = hp_m?.Units;
            var count = nc > maxCount ? maxCount + 2 : nc;
            var sb = new StringBuilder(
            @"<m:d>
                <m:dPr><m:begChr m:val=""[""/><m:endChr m:val=""]""/></m:dPr>
                <m:e>
                    <m:m>
                        <m:mPr>
                            <m:rSpRule m:val=""3""/>
                            <m:cGpRule m:val=""3""/>
                            <m:rSp m:val=""300""/>
                            <m:cGp m:val=""120""/>
                            <m:mcs>
                                <m:mc>
                                    <m:mcPr>"); //Appended constant part
                            sb.Append($"<m:count m:val=\"{count}\"/>"); //Appended variable part
                            sb.Append(@"<m:mcJc m:val=""center""/>
                                    </m:mcPr>
                                </m:mc>
                            </m:mcs>
                        </m:mPr>"
            ); //Appended second constant part
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
                    AppendMatrixElement(sb, s);
                }
                sb.Append("</m:mr>");
            }
            sb.Append("</m:m></m:e></m:d>");
            if (units is not null)
                sb.Append(units.Xml);

            return sb.ToString();

            static string td(string s) => $"<m:e><m:r>{smallerFontProperty}<m:t>{s}</m:t></m:r></m:e>";

            static void AppendMatrixElement(StringBuilder sb, string s)
            {
                var k1 = s.IndexOf("<m:r>") + 5;
                var k2 = s.IndexOf("</m:r>", k1);
                while (k1 >= 5 && k2 > k1 && s.AsSpan(k1, k2 - k1).Contains(":rPr>", StringComparison.Ordinal))
                {
                    k1 = s.IndexOf("<m:r>", k2) + 5;
                    k2 = s.IndexOf("</m:r>", k1);
                }
                sb.Append("<m:e>");
                if (k1 < 5)
                    sb.Append(s);
                else if (k1 == 5)
                    sb.Append("<m:r>").Append(smallerFontProperty).Append(s[5..]);
                else
                    sb.Append(s[..k1]).Append(smallerFontProperty).Append(s[k1..]);

                sb.Append("</m:e>");
            }
        }

        private static readonly string RunVectorSpacing = Run(VectorSpacing);
        private static readonly string RunDots = Run("...");
        internal override string FormatVector(Vector vector)
        {
            var div = RunVectorSpacing;
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
                    sb.Append(RunDots).Append(div);
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

        private static readonly string RunIf = Run(" if ", NormalText);
        private static readonly string RunColon = Run(":");
        private static string FormatIfRow(string sa, string sb) =>
            $"<m:mr><m:e>{RunIf}{sa}{RunColon}</m:e><m:e>{sb}</m:e></m:mr>";

        private static readonly string RunElse = Run(" else:", NormalText);
        private static string FormatElseRow(string sa) =>
            $"<m:mr><m:e>{RunElse}</m:e><m:e>{sa}</m:e></m:mr>";

        private const string mDPr = @"<m:dPr>
            <m:begChr m:val=""|""/>
            <m:endChr m:val="" ""/>
        </m:dPr>";

        internal override string FormatBlock(string[] sa)
        {
            var len = sa.Length;
            var sb = new StringBuilder("<m:d>")
                .Append(mDPr)
                .Append("<m:e><m:m>")
                .Append(mPr);

            for (int i = 0; i < len; ++i)
            {
                ref var s = ref sa[i];
                if (!string.IsNullOrEmpty(s))
                    sb.Append($"<m:mr><m:e>")
                      .Append(s)
                      .Append("</m:e></m:mr>");
            }

            sb.Append("</m:m></m:e></m:d>");
            return sb.ToString();
        }
    }
}
