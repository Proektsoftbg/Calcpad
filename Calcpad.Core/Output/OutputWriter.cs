using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Calcpad.Core
{
    internal abstract class OutputWriter
    {
        private readonly StringBuilder _stringBuilder = new(200);
        protected static readonly double[] AngleFactors =
        [
            180.0 / Math.PI,
            1.0,
            200.0 / Math.PI
        ];
        protected string[] AngleUnits;
        protected const char ThinSpace = '\u2009';
        protected const char HairSpace = '\u200A';
        protected readonly int decimals = 2;
        protected readonly string formatString = null;
        protected readonly bool formatEquations;
        protected readonly bool zeroSmallElements;
        protected readonly int maxCount = 20;
        protected readonly bool phasor = false;
        protected readonly int degrees = 0;
        protected static readonly int PowerOrder = Calculator.OperatorOrder[Calculator.OperatorIndex['^']];

        protected OutputWriter(MathSettings settings, bool phasor)
        {
            if (settings is null)
                return;

            decimals = settings.Decimals;
            formatString = settings.FormatString;
            formatEquations = settings.FormatEquations;
            zeroSmallElements = settings.ZeroSmallMatrixElements;
            maxCount = settings.MaxOutputCount;
            degrees = settings.Degrees;
            this.phasor = phasor;
        }
        internal enum OutputFormat
        {
            Text,
            Html,
            Xml
        }
        internal const string VectorSpacing = "\u2002";
        internal abstract string UnitString(Unit units);
        internal abstract string FormatInput(string s, Unit units, int line, bool isCalculated);
        internal abstract string FormatSubscript(string sa, string sb);
        internal abstract string FormatVariable(string name, string value, bool isBold);
        internal abstract string FormatUnits(string s);
        internal abstract string FormatFunction(string s);
        internal abstract string FormatSwitch(string[] sa, int level = 0);
        internal abstract string FormatIf(string sc, string sa, string sb, int level = 0);
        internal abstract string FormatRoot(string s, int level = 0, string n = "2");
        internal abstract string FormatOperator(char c);
        internal abstract string FormatPower(string sa, string sb, int level, int order);
        internal abstract string FormatDivision(string sa, string sb, int level);
        internal abstract string FormatNary(string symbol, string sub, string sup, string expr); //Integral, sum, product
        internal abstract string FormatValue(in IScalarValue value);
        internal abstract string AddBrackets(string s, int level = 0, char left = '(', char right = ')');
        internal abstract string FormatAbs(string s, int level = 0);
        internal abstract string FormatReal(double d, string format, bool zeroSmall);
        internal abstract string FormatComplex(double re, double im, string format);
        internal abstract string FormatMatrix(Matrix matrix);
        internal abstract string FormatVector(Vector vector);
        internal abstract string FormatMatrixValue(RealValue value, double zeroThreshold);

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
                        if (isPower && brackets.TryPeek(out int ind) && ind < power.Length)
                            power += c == '*' ? "\u200A·\u200A" : '/';
                        else
                        {
                            AppendPower();
                            _stringBuilder.Append(FormatOperator(c));
                        }
                        break;
                    case '^':
                        isPower = true;
                        break;
                    case '(':
                        brackets.Push(isPower ? power.Length : _stringBuilder.Length);
                        break;
                    case ')':
                        {
                            if (brackets.Count == 0)
                                throw Exceptions.MissingLeftBracket();

                            var index = brackets.Pop();
                            if (isPower && index < power.Length)
                            {
                                if (index == 0)
                                {
                                    if (this is TextWriter)
                                        power = $"({power})";
                                }
                                else
                                    power = $"{power[..index]}({power[index..]})";
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
                                var cl = string.IsNullOrEmpty(literal) ? '\0' :     literal[^1];
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
                throw Exceptions.MissingRightBracket();

            if (literal.Length > 0)
                AppendPower();

            return _stringBuilder.ToString();

            string FormatLocal(string s)
            {
                var n = sub;
                if (n >= 2 && n <= 4)
                {
                    sub = 0;
                    return FormatSubscript(FormatUnits(s[..^n]), HairSpace + s[^(n - 1)..]);
                }
                if (s.Contains('/') || s.Contains('·'))
                    return s;

                return FormatUnits(s);
            }

            void AppendPower()
            {
                var isXmlWriter = this is XmlWriter;
                if (!literal.Equals("°", StringComparison.OrdinalIgnoreCase))
                    literal = FormatLocal(literal);
                else if (isXmlWriter)
                    literal = XmlWriter.Run(literal);

                if (isPower)
                {
                    if (isXmlWriter)
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
            '*' => "\u200A·\u200A",
            '÷' => "/",
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
            ';' => "; ",
            ',' => ", ",
            '|' => " | ",
            _ => c.ToString()
        };

        protected string FormatComplexHelper(double re, double im, string format)
        {
            var t = Complex.GetType(re, im);
            if (t == Complex.Types.Real)
                return FormatReal(re, format, false);

            if (t == Complex.Types.Imaginary)
                return FormatReal(im, format, false) + 'i';

            if (phasor)
            {
                var abs = Math.Sqrt(re * re + im * im);
                var phase = Math.Atan2(im, re) * AngleFactors[degrees];
                var absString = FormatReal(abs, format, false);
                var phaseString = FormatReal(phase, format, false) + AngleUnits[degrees];
                return $"{absString}∠{phaseString}";
            }
            var sRe = FormatReal(re, format, false);
            var sIm = FormatReal(Math.Abs(im), format, false);
            return im < 0 ? $"{sRe} – {sIm}i" : $"{sRe} + {sIm}i";
        }

        private const string Sharps = "################";

        internal string FormatNumberHelper(double d, string format, CultureInfo culture = null)
        {
            if (double.IsNaN(d))
                return " Undefined ";
            if (double.IsPositiveInfinity(d))
                return "+∞";
            if (double.IsNegativeInfinity(d))
                return "-∞";
            if (double.IsInfinity(d))
                return "∞";

            format ??= formatString;
            culture ??= CultureInfo.CurrentCulture;
            if (!string.IsNullOrEmpty(format))
            {
                var s = d.ToString(format, culture);
                return s == "-0" ? "0" : s;
            }

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
                        var s = d.ToString("G17", culture);
                        if (!s.Contains('E'))
                        {
                            var dec = Math.Round(decimal.Parse(s, culture), i);
                            s = dec.ToString("G29", culture);
                            return s == "-0" ? "0" : s;
                        }
                    }
                }
            }
            format = $"#.{Sharps[..decimals]}E+0";
            return d.ToString(format, culture);
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

        protected double GetMaxVisibleMatrixValue(Matrix matrix, out int row, out int col)
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
                var d = Math.Abs(matrix[i, j].D);
                if (d > maxAbs)
                {
                    maxAbs = d;
                    ii = i;
                    jj = j;
                }
            }
        }

        protected double GetMaxVisibleVectorValue(Vector vector)
        {
            var maxAbs = 0d;
            var len = Math.Min(maxCount, vector.Size);
            for (int i = 0; i < len; ++i)
            {
                var d = Math.Abs(vector[i].D);
                if (d > maxAbs)
                    maxAbs = d;
            }
            var last = vector.Length - 1;
            if (maxCount < last)
            {
                var d = Math.Abs(vector[last].D);
                if (d > maxAbs)
                    maxAbs = d;
            }
            return maxAbs;
        }
    }
}
