using System;
using System.Collections.Generic;

namespace Calcpad.Core
{
    internal static class ExtensionMethods
    {
        internal static bool EqualsBinary(this double d1, double d2)
        {
            var l1 = BitConverter.DoubleToInt64Bits(d1);
            var l2 = BitConverter.DoubleToInt64Bits(d2);

            if (l1 >> 63 != l2 >> 63)
                return d1.Equals(d2);

            return Math.Abs(l1 - l2) < 4;
        }
        
        public static IEnumerable<Tuple<T, T>> Bigrams<T>(this IEnumerable<T> items)
        {
            var firstRun = true;
            var prev = default(T);
            foreach (var item in items)
            {
                if (!firstRun)
                    yield return Tuple.Create(prev, item);
                firstRun = false;
                prev = item;
            }
        }
    }
}
