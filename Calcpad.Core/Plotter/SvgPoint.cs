using System;
using System.Globalization;

namespace Calcpad.Core
{
    internal struct SvgPoint
    {
        private const int Decimals = 2;
        internal double X;
        internal double Y;

        internal SvgPoint(double x, double y)
        {
            X = Math.Round(x, Decimals);
            Y = Math.Round(y, Decimals);
        }

        public override string ToString() =>
            X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture);
    }
}
