using System;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    public static class Validator
    {
        public static bool IsVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            char c = name[0];
            if (!IsLetter(c) || "_,′″‴⁗".Contains(c))
                return false;

            for (int i = 1, len = name.Length; i < len; ++i)
            {
                c = name[i];
                if (!(IsLetter(c) || IsDigit(c)))
                    return false;
            }
            return true;
        }

        public static bool IsPlot(string text)
        {
            var s = text.TrimStart();
            if (!string.IsNullOrEmpty(s) && s[0] == '$')
                return s.StartsWith("$plot", StringComparison.OrdinalIgnoreCase);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsKeyword(ReadOnlySpan<char> s, ReadOnlySpan<char> keyword) =>
            s.TrimStart().StartsWith(keyword, StringComparison.OrdinalIgnoreCase);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMacroLetter(char c, int position) =>
            c >= 'a' && c <= 'z' ||
            c >= 'A' && c <= 'Z' ||
            c == '_' ||
            char.IsDigit(c) && position > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z' || // A - Z 
            c >= 'α' && c <= 'ω' ||   // alpha - omega
            c >= 'Α' && c <= 'Ω' ||  // Alpha - Omega
            "_,°′″‴⁗ϑϕøØ℧∡".Contains(c, StringComparison.Ordinal); // _ , ° ′ ″ ‴ ⁗ ϑ ϕ ø Ø ℧ ∡

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLatinLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z'; // A - Z 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDigit(char c) =>
            c >= '0' && c <= '9' || c == MathParser.DecimalSymbol;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnitStart(char c) =>
            c == '°' || c == '′' || c == '″';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(char c) =>
            c == ' ' || c == '\t';
    }
}
