using System;
using System.Text;

namespace Calcpad.Core
{
    public static class ExtensionMethods
    {
        public static bool EqualsBinary(this double d1, double d2)
        {
            var l1 = BitConverter.DoubleToInt64Bits(d1);
            var l2 = BitConverter.DoubleToInt64Bits(d2);

            if (l1 >> 63 != l2 >> 63)
                return d1.Equals(d2);

            return Math.Abs(l1 - l2) < 4;
        }

        public static SplitEnumerator EnumerateSplits(this ReadOnlySpan<char> s, char delimiter) =>
            new(s, delimiter);

        public static CommentEnumerator EnumerateComments(this ReadOnlySpan<char> s) =>
            new(s);

        public static void RemoveLastLineIfEmpty(this StringBuilder sb)
        {
            var len = sb.Length;
            if (len > 2 && sb[len - 1] == '\n')
                sb.Remove(len - 2, 2);
        }
    }
}
