using Markdig.Helpers;
using System;
using System.Text;

namespace Calcpad.Core
{
    internal class TextWriter : OutputWriter
    {
        internal TextWriter(MathSettings settings, bool phasor) : base(settings, phasor) 
        {
            AngleUnits =
            [           
                "°",
                "",
                " grad"
            ];
        }
        internal override string UnitString(Unit units) => units.Text.Replace("‱", "‱ ");
        internal override string FormatInput(string s, Unit units, int line, bool isCalculated) =>
            units is null ? s : s + ' ' + units.Text;

        internal override string FormatSubscript(string sa, string sb) => sa + "_" + sb;
        internal override string FormatVariable(string name, string value, bool isBold) =>
            name[0] == '\u20D7' ? name[1..] : name;
        internal override string FormatUnits(string s) => s;
        internal override string FormatFunction(string s) => s;
        internal override string FormatRoot(string s, int level = 0, string n = "2") =>
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

        internal override string FormatValue(in IScalarValue value)
        {
            var u = value.Units;
            var s = FormatComplex(value.Re, value.Im, u?.FormatString);
            var uText = u?.Text;
            if (string.IsNullOrEmpty(uText))
                return s;

            if (!(value.IsReal || phasor && s.Contains('∠')))
                s = AddBrackets(s);

            if (uText == "°")
                return s + uText;

            return s + ' ' + uText;
        }

        internal override string AddBrackets(string s, int level = 0, char left = '(', char right = ')') => left + s + right;
        internal override string FormatAbs(string s, int level = 0) => $"|{s}|";
        internal override string FormatReal(double d, string format, bool zeroSmall)
        {
            var s = FormatNumberHelper(d, format);
            if (double.IsNaN(d) || double.IsInfinity(d))
                return s;

            var i = s.LastIndexOf('E');
            if (i <= 0)
                return s;

            if (zeroSmall && s[i + 1] == '-')
                return "0";

            return s;
        }

        internal override string FormatComplex(double re, double im, string format)
        {
            if (double.IsNaN(re) && double.IsNaN(im))
                return " Undefined ";

            if (double.IsInfinity(re) && double.IsInfinity(im))
                return "∞";

            return FormatComplexHelper(re, im, format);
        }

        internal override string FormatNary(string symbol, string sub, string sup, string expr) =>
            $"{symbol}{{{expr}; {sub}...{sup}}}";

        internal override string FormatSwitch(string[] sa, int level = 0) =>
            "switch(" + string.Join("; ", sa) + ")";

        internal override string FormatIf(string sc, string sa, string sb, int level = 0) =>
            $"if({sc}; {sa}; {sb})";

        internal override string FormatMatrix(Matrix matrix)
        {
            var sb = new StringBuilder("[");
            var nc = matrix.ColCount - 1;
            var zeroThreshold = GetMaxVisibleMatrixValue(matrix, out int row, out int col) * 1e-14;
            var len = FormatMatrixValue(matrix[row, col], zeroThreshold).Length;
            var format = $"{{0,{len}}}";
            var cont = string.Format(format, "...");
            if (matrix is not HpMatrix hp_m)
                hp_m = null;

            var units = hp_m?.Units;
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
                    string s;
                    if (hp_m is null)
                        s = FormatMatrixValue(matrix[i, j], zeroThreshold);
                    else
                    {
                        var d = hp_m.GetValue(i, j);
                        s = FormatReal(d, units?.FormatString, zeroSmallElements && Math.Abs(d) < zeroThreshold);
                    }
                    sb.Append(string.Format(format, s));
                    if (j < nc)
                        sb.Append("  ");
                }
                if (i < nr)
                    sb.Append(" |");
            }
            sb.Append(']');
            if (units is not null)
                sb.Append(units.Text);

            return sb.ToString();
        }

        internal override string FormatVector(Vector vector)
        {
            var div = "  ";
            var sb = new StringBuilder("[");
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
                    sb.Append("...").Append(div);
                    break;
                }
                AppendElement(i);
            }
            var last = len - 1;
            if (maxCount < last)
                AppendElement(last);

            sb.Append(']');
            if (units is not null)
                sb.Append(units.Text);

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
            return value.Units is null ? s : s + value.Units.Text;
        }
    }
}
