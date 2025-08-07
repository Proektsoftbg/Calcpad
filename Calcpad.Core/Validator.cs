using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Calcpad.Core
{
    public static class Validator
    {
        private const string CurrencyChars = "€£₤¥¢₽₹₩₪"; //For custom currency units
        private const string UnitSymbolChars = "°′″%‰‱";
        private const string VarSymbolChars = ",_‾‴⁗";
        private const string VarNonLetterChars = "℧∡";
        private const string VarLetterChars = "ϑϕøØ";
        private const string SuperscriptChars = "⁰¹²³⁴⁵⁶⁷⁸⁹⁺⁻⁼⁽⁾";
        private const string SubscriptChars = "₀₁₂₃₄₅₆₇₈₉₊₋₌₍₎";
        internal const string UnitChars = UnitSymbolChars + CurrencyChars;
        private const string VarStartingChars = UnitChars + VarNonLetterChars;
        private const string VarChars =
            VarStartingChars +
            VarSymbolChars +
            SuperscriptChars +
            SubscriptChars +
            VarLetterChars;

        private static readonly Regex MyFormatRegex = new(@"^[FCEGN]\d{0,2}$|^[0#]+(,[0#]+)?(\.[0#]+)?([eE][+-]?0+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var c = name[0];
            if (!IsVarStartingChar(c))
                return false;

            for (int i = 1, len = name.Length; i < len; ++i)
            {
                c = name[i];
                if (!(IsVarChar(c)))
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
            char.IsLetter(c) || VarChars.Contains(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLatinLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z';   // A - Z 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDigit(char c) =>
            c >= '0' && c <= '9' || c == MathParser.DecimalSymbol;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsUnitStart(char c) => UnitChars.Contains(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhiteSpace(char c) => c == ' ' || c == '\t';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsVarAdditionalChar(char c) =>
            VarChars.Contains(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsVarStartingChar(char c) =>
            char.IsLetter(c) ||
            VarStartingChars.Contains(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsVarChar(char c) => IsLetter(c) || IsDigit(c);

        public static bool IsValidFormatString(string format)
        {
            try
            {
                1d.ToString(format, CultureInfo.CurrentCulture);
                return MyFormatRegex.Match(format).Success;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
