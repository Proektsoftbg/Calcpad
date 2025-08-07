using System;
using System.Globalization;

namespace Calcpad.OpenXml
{
    internal static class HtmlStyleParser
    {
        private enum CssUnits
        {
            PX,
            PT,
            PC,
            MM,
            CM,
            IN,
            EM,
            REM,
            ERROR = -1
        };

        private static readonly double[,] ScaleFactors =
        {
            //PX,    PT,    PC,    MM,     CM,     IN,    EM
            {1.0, 72.0/96.0, 1.0/16.0, 127.0/480.0, 127.0/4800.0, 1.0/96.0},   //PX
            {96.0/72.0, 1.0, 1.0/12.0, 127.0/360.0, 127.0/3600.0, 1.0/72.0},   //PT
            {16.0, 12.0, 1.0, 127.0/30.0, 127.0/300.0, 1.0/6},                 //PC
            {480.0/127.0, 360.0/127.0, 30.0/127.0, 1.0, 1.0/10.0, 5.0/127.0},  //MM
            {4800.0/127.0, 3600.0/127.0, 300.0/127.0, 10.0, 1.0, 50.0/127.0},  //CM
            {96.0, 72.0, 6.0, 25.4, 2.54, 1.0},                                //IN
        };

        internal static double GetParameter(string text, string parameter, string outUnits = "PX")
        {
            var i = -1;
            Seek(text, parameter, ref i);
            if (i >= text.Length)
                return 0;

            Seek(text, ":", ref i);
            var n = i;
            var s = Seek(text, ";", ref i, true);
            if (string.IsNullOrEmpty(s))
            {
                i = n;
                s = Seek(text, "\"", ref i, true);
                if (string.IsNullOrEmpty(s))
                {
                    i = n;
                    s = Seek(text, "\'", ref i, true);
                }
            }
            string value = string.Empty, inUnits = string.Empty;
            var isValue = true;
            foreach (char c in s.Trim())
            {
                if (isValue && c >= '0' && c <= '9' || c == '.')
                    value += c;
                else
                {
                    isValue = false;
                    inUnits += c;
                }
            }
            CssUnits inCssUnits = GetCssUnits(inUnits);
            CssUnits outCssUnits = GetCssUnits(outUnits);
            if (inCssUnits == CssUnits.ERROR || outCssUnits == CssUnits.ERROR)
                return 0;
            var d = double.Parse(value, CultureInfo.InvariantCulture);
            var k = ScaleFactors[(int)inCssUnits, (int)outCssUnits];
            return d * k;
        }

        private static string Seek(string text, string term, ref int i, bool collect = false)
        {
            int j = 0, n = text.Length, m = term.Length;
            var result = string.Empty;
            while (++i < n)
            {
                var c = text[i];
                if (c == term[j])
                    ++j;
                else
                {
                    j = 0;
                    if (collect)
                        result += c;
                }
                if (j == m)
                    break;
            }
            return result;
        }

        private static CssUnits GetCssUnits(string units)
        {
            if (string.IsNullOrEmpty(units))
                return CssUnits.PX;

            try
            {
                object o = Enum.Parse<CssUnits>(units, true);
                if (o is CssUnits cssUnits)
                {
                    if (cssUnits == CssUnits.EM || cssUnits == CssUnits.REM)
                        return CssUnits.PC;
                    return cssUnits;
                }


                return CssUnits.ERROR;
            }
            catch
            {
                return CssUnits.ERROR;
            }
        }
    }
}
