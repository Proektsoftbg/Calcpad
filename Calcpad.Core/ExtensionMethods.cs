using System;
using System.Text;

namespace Calcpad.Core
{
    public static class ExtensionMethods
    {
        public static bool AlmostEquals(this double a, double b)
        {
            var l1 = BitConverter.DoubleToInt64Bits(a);
            var l2 = BitConverter.DoubleToInt64Bits(b);

            if (l1 >> 63 != l2 >> 63)
                return a.Equals(b);

            return Math.Abs(l1 - l2) < 4;
        }
        public static double IsNotEqual(this double a, double b) => a.AlmostEquals(b) ? 0d : 1d;
        public static double IsEqual(this double a, double b) => a.AlmostEquals(b) ? 1d : 0d;
        public static double IsLessThan(this double a, double b) => a < b && !a.AlmostEquals(b) ? 1d : 0d;
        public static double IsGreaterThan(this double a, double b) => a > b && !a.AlmostEquals(b) ? 1d : 0d;
        public static double IsLessThanOrEqual(this double a, double b) => a <= b || a.AlmostEquals(b) ? 1d : 0d;
        public static double IsGreaterThanOrEqual(this double a, double b) => a >= b || a.AlmostEquals(b) ? 1d : 0d;


        public static SplitEnumerator EnumerateSplits(this ReadOnlySpan<char> s, char delimiter) =>
            new(s, delimiter);

        public static CommentEnumerator EnumerateComments(this ReadOnlySpan<char> s) =>
            new(s);

        public static void RemoveLastLineIfEmpty(this StringBuilder sb)
        {
            var len = sb.Length - 1;
            if (len > 1 && sb[len] == '\n')
            {
                if (sb[len - 1] == '\r')
                    sb.Remove(len - 1, 2);
                else
                    sb.Remove(len, 1);
            }
        }

        public static SpanLineEnumerator EnumerateLines(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return new SpanLineEnumerator();

            if (s.Length > 1 && s[^2] == '\r')
                return s.AsSpan(..^2).EnumerateLines();

            return s.AsSpan().EnumerateLines();
        }
    }
}
