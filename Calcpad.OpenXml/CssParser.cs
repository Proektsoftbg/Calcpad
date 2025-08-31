using System;
using System.Globalization;

namespace Calcpad.OpenXml
{
    internal static class CssParser
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

        internal static string GetCSSParameter(string css, string parameter)
        {
            var i = css.IndexOf(parameter, 0, StringComparison.OrdinalIgnoreCase);
            if (i >= 0)
            {
                i = css.IndexOf(':', i) + 1;
                if (i > 0)
                {
                    var n = css.IndexOf(';', i);
                    if (n < 0)
                        n = css.Length;
                    return css[i..n].Trim();
                }
            }
            return null;
        }

        internal static double GetSizeCSSParameter(string css, string parameter, string outUnits = "PX")
        {
            var s = GetCSSParameter(css, parameter);
            if (string.IsNullOrEmpty(s))
                return 0;

            return ParseSize(s, outUnits);
        }

        internal static double ParseSize(string s, string outUnits = "PX")
        {
            var n = s.Length - 1;
            for (int i = n; i > 0; --i)
            {
                var c = s[i];
                if (c >= '0' && c <= '9' || c == '.')
                {
                    n = i + 1;
                    break;
                }
            }
            var value = s[..n];
            var inUnits = s[n..];
            CssUnits inCssUnits = GetCssUnits(inUnits);
            CssUnits outCssUnits = GetCssUnits(outUnits);
            if (inCssUnits == CssUnits.ERROR || outCssUnits == CssUnits.ERROR)
                return 0;
            var d = double.Parse(value, CultureInfo.InvariantCulture);
            var k = ScaleFactors[(int)inCssUnits, (int)outCssUnits];
            return d * k;
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
