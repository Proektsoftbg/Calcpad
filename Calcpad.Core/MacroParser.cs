using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Calcpad.Core
{
    public class MacroParser
    {
        private readonly struct Macro
        {
            private readonly string Contents;
            private readonly string[] Parameters;
            private readonly int[] Order;

            internal Macro(string contents, List<string> parameters)
            {
                Contents = contents;
                if (parameters is null)
                {
                    Parameters = null;
                    Order = null;
                }
                else
                {
                    Parameters = parameters.ToArray();
                    Order = Sort(Parameters);
                }
            }

            private static int[] Sort(string[] parameters)
            {
                var n = parameters.Length;
                var sorted = new SortedList<string, int>();
                for (int i = 0; i < n; ++i)
                {
                    var s = new string(parameters[i].Reverse().ToArray());
                    try
                    {
                        sorted.Add(s, i);
                    }
                    catch (ArgumentException)
                    {
                        Throw.DuplicateMacroParameters(s);
                        return null;
                    }
                }
                return sorted.Values.Reverse().ToArray();
            }

            internal string Run(List<string> arguments)
            {
                if (arguments.Count != ParameterCount)
                    Throw.InvalidNumberOfArguments();

                if (ParameterCount == 0)
                    return Contents;

                var sb = new StringBuilder(Contents);
                for (int i = 0, _count = arguments.Count; i < _count; i++)
                {
                    var j = Order[i];
                    sb.Replace(Parameters[j], arguments[j]);
                }
                return sb.ToString();
            }
            internal bool IsEmpty => Contents is null;
            internal int ParameterCount => Parameters is null ? 0 : Parameters.Length;
        }

        private enum Keywords
        {
            None,
            Def,
            EndDef,
            Include,
        }
        private readonly List<int> LineNumbers = new();
        private static readonly Dictionary<string, Macro> Macros = new(StringComparer.Ordinal);
        public Func<string, Queue<string>, string> Include;

        private static Keywords GetKeyword(ReadOnlySpan<char> s)
        {
            if (s.Length < 4)
                return Keywords.None;
            if (s.StartsWith("#def", StringComparison.OrdinalIgnoreCase))
                return Keywords.Def;
            if (s.StartsWith("#end def", StringComparison.OrdinalIgnoreCase))
                return Keywords.EndDef;
            if (s.StartsWith("#include", StringComparison.OrdinalIgnoreCase))
                return Keywords.Include;

            return Keywords.None;
        }

        private int _parsedLineNumber;
        public bool Parse(string sourceCode, out string outCode, StringBuilder sb, int includeLine)
        {
            var sourceLines = sourceCode.EnumerateLines();
            if (includeLine == 0)
            {
                sb = new StringBuilder(sourceCode.Length);
                Macros.Clear();
                LineNumbers.Clear();
                _parsedLineNumber = 0;
            }
            var macroBuilder = new StringBuilder(1000);
            var macroName = string.Empty;
            var lineNumber = includeLine;
            var macroDefCount = 0;
            var hasErrors = false;
            ReadOnlySpan<char> lineContent = "code";
            List<string> macroParameters = null;
            try
            {
                foreach (ReadOnlySpan<char> sourceLine in sourceLines)
                {
                    if (includeLine == 0)
                    {
                        LineNumbers.Add(_parsedLineNumber);
                        ++lineNumber;
                    }

                    lineContent = sourceLine.Trim();
                    if (lineContent.IsEmpty)
                    {
                        AppendLine(sourceLine.ToString());
                        continue;
                    }
                    if (lineContent[0] == '#' && ParseKeyword(lineContent))
                        continue;

                    if (macroDefCount == 1)
                    {
                        macroBuilder.AppendLine(sourceLine.ToString());
                        continue;
                    }

                    if (Macros.Count != 0)
                    {
                        try
                        {
                            var insertCode = ApplyMacros(sourceLine);
                            var insertLines = insertCode.EnumerateLines();
                            foreach (var line in insertLines)
                                AppendLine(line.ToString());
                        }
                        catch (Exception ex)
                        {
                            AppendError(lineContent.ToString(), ex.Message);
                        }
                        continue;
                    }
                    AppendLine(sourceLine.ToString());
                }
                if (includeLine == 0)
                    LineNumbers.Add(_parsedLineNumber);

                if (macroDefCount > 0)
                {
#if BG
                    sb.Append($"#Грешка: Незатворена дефиниция на макрос. Липсва \"#end def\".");
#else
                    sb.Append($"#Error: Macro definition block not closed. Missing \"#end def\".");
#endif
                    hasErrors = true;
                }
            }
            catch (Exception ex)
            {
                AppendError(lineContent.ToString(), ex.Message);
            }
            finally
            {
                outCode = sb.ToString();
            }
            return hasErrors;

            bool ParseKeyword(ReadOnlySpan<char> lineContent)
            {
                var keyword = GetKeyword(lineContent);
                switch (keyword)
                {
                    case Keywords.Include:
                        ParseInclude(lineContent);
                        return true;
                    case Keywords.Def:
                        ParseDef(lineContent);
                        return true;
                    case Keywords.EndDef:
                        ParseEndDef(lineContent);
                        return true;
                    default:
                        return false;
                }
            }

            void ParseInclude(ReadOnlySpan<char> lineContent)
            {
                int n = lineContent.Length;
                if (n < 9)
#if BG
                    AppendError(lineContent.ToString(), $"Липсва изходен файл за вмъкване.");
#else
                    AppendError(lineContent.ToString(), $"Missing source file for include.");
#endif
                n = lineContent.IndexOfAny('\'', '"');
                var nf1 = lineContent.LastIndexOf('#');
                if (n < 9 || nf1 > 0 && nf1 < n)
                    n = nf1;

                if (n < 9)
                    n = lineContent.Length;

                var insertFileName = lineContent[8..n].Trim().ToString();
                var fileExist = File.Exists(insertFileName);
                if (!fileExist)
#if BG
                    AppendError(lineContent.ToString(), "Файлът не е намерен.");
#else
                    AppendError(lineContent.ToString(), "File not found.");
#endif
                Queue<string> fields = new();
                if (nf1 > 0)
                {
                    var nf2 = lineContent.LastIndexOf('}');
                    if (nf2 < 0)
                        AppendError(lineContent.ToString(), "Brackets not closed.");
                    else
                    {
                        SplitEnumerator split = lineContent[(nf1 + 1)..nf2].EnumerateSplits(';');
                        foreach (var item in split)
                            fields.Enqueue(item.Trim().ToString());
                    }
                }
                if (fileExist)
                    Parse(Include(insertFileName, fields), out _, sb, lineNumber);
            }

            void ParseDef(ReadOnlySpan<char> lineContent)
            {
                var textSpan = new TextSpan(lineContent);
                if (macroDefCount == 0)
                {
                    int i = 4, len = lineContent.Length;
                    var c = EatSpace(lineContent, ref i);
                    textSpan.Reset(i);
                    while (i < len)
                    {
                        c = lineContent[i];
                        if (c == '$')
                        {
                            textSpan.Expand();
                            macroName = textSpan.ToString();
                            break;
                        }
                        if (Validator.IsMacroLetter(c, textSpan.Length))
                            textSpan.Expand();
                        else
                        {
                            SymbolError(lineContent, c);
                            break;
                        }
                        ++i;
                    }
                    c = EatSpace(lineContent, ref i);
                    if (c == '(')
                    {
                        macroParameters = new();
                        c = EatSpace(lineContent, ref i);
                        textSpan.Reset(i);
                        while (i < len)
                        {
                            if (c == ' ')
                                c = EatSpace(lineContent, ref i);
                            if (c == ';' || c == ')')
                            {
                                macroParameters.Add(textSpan.ToString());
                                if (c == ')')
                                    break;

                                c = EatSpace(lineContent, ref i);
                                textSpan.Reset(i);
                            }
                            else
                            {
                                if (Validator.IsMacroLetter(c, textSpan.Length) || c == '$')
                                    textSpan.Expand();
                                else if (c != '\n')
                                    SymbolError(lineContent.ToString(), c);

                                c = lineContent[++i];
                            }
                        }
                        c = EatSpace(lineContent, ref i);
                    }
                    else
                        macroParameters = null;

                    if (c == '=')
                    {
                        c = EatSpace(lineContent, ref i);
                        AddMacro(lineContent.ToString(), macroName, new Macro(lineContent[i..].ToString(), macroParameters));
                        macroName = string.Empty;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(macroName))
                        {
                            macroName = textSpan.ToString();

#if BG
                            AppendError(lineContent.ToString(), $"Невалидно име на макрос: \"{macroName}\".");
#else
                            AppendError(lineContent.ToString(), $"Invalid macro name: \"{macroName}\".");
#endif
                            macroName = string.Empty;
                            textSpan.Reset(i);
                        }
                        ++macroDefCount;
                    }
                }
                else
                {
#if BG
                    AppendError(lineContent.ToString(), "Невалидно в дефиниция на макрос.");
#else
                    AppendError(lineContent.ToString(), "Invalid inside macro definition.");
#endif
                    ++macroDefCount;
                }
            }

            void ParseEndDef(ReadOnlySpan<char> lineContent)
            {
                if (macroDefCount < 1)
                {
#if BG
                    AppendError(lineContent.ToString(), "\"Няма съответен \"#def\".");
#else
                    AppendError(lineContent.ToString(), "\"There is no matching \"#def\".");
#endif
                }
                else
                {
                    macroBuilder.RemoveLastLineIfEmpty();
                    var macroContent = macroBuilder.ToString();
                    AddMacro(lineContent.ToString(), macroName, new Macro(macroContent, macroParameters));
                    macroName = string.Empty;
                    macroBuilder.Clear();
                }
                --macroDefCount;
            }

            void AppendLine(string line)
            {
                sb.AppendLine(line + '\v' + lineNumber.ToString());
                ++_parsedLineNumber;
            }

            void SymbolError(ReadOnlySpan<char> lineContent, char c)
            {
#if BG
                AppendError(lineContent.ToString(), $"Невалиден символ \"{c}\" в име на макрос.");
#else
                AppendError(lineContent.ToString(), $"Invalid symbol \"{c}\" in macro name.");
#endif
            }

            void AppendError(string lineContent, string errorMessage)
            {
#if BG
                sb.AppendLine($"#Грешка в \"{HttpUtility.HtmlEncode(lineContent)}\" на ред {LineHtml(lineNumber)}: {errorMessage}");
#else
                sb.AppendLine($"#Error in \"{HttpUtility.HtmlEncode(lineContent)}\" on line {LineHtml(lineNumber)}: {errorMessage}");
#endif
                hasErrors = true;
            }

            void AddMacro(string lineContent, string name, Macro macro)
            {
                if (!Macros.TryAdd(name, macro))
#if BG
                    AppendError(lineContent, $"Повтарящо се име на макрос: \"{name}\".");
#else
                    AppendError(lineContent, $"Duplicate macro name: \"{name}\".");
#endif
            }
            static string LineHtml(int line) => $"[<a href=\"#0\" data-text=\"{line}\">{line}</a>]";
        }

        public static Queue<string> GetFields(ReadOnlySpan<char> s, char delimiter)
        {
            var fields = new Queue<string>();
            var split = s.EnumerateSplits(delimiter);
            foreach (var item in split)
                fields.Enqueue(item.Trim().ToString());

            return fields;
        }

        private static string ApplyMacros(ReadOnlySpan<char> lineContent)
        {
            var index = lineContent.IndexOf("$");
            if (index < 0)
                return lineContent.ToString();

            index = lineContent.IndexOf("#{");
            var stringBuilder = new StringBuilder(200);
            var macroArguments = new List<string>();
            var bracketCount = 0;
            var emptyMacro = new Macro(null, null);
            var macro = emptyMacro;
            Queue<string> fields = null;
            if (index >= 0)
            {
                var s = lineContent[(index + 2)..];
                lineContent = lineContent[..index];
                var n = s.IndexOf('}');
                if (n < 0)
                    n = s.Length;
                fields = GetFields(s[..n], ';');
            }
            var textSpan = new TextSpan(lineContent);
            for (int i = 0, len = lineContent.Length; i < len; ++i)
            {
                var c = lineContent[i];
                if (macroArguments.Count < macro.ParameterCount)
                {
                    if (c == '(')
                    {
                        if (bracketCount == 0)
                            textSpan.Reset(i + 1);
                        ++bracketCount;
                    }
                    else if (c == ')')
                        --bracketCount;

                    if (c == ';' && bracketCount == 1 || c == ')' && bracketCount == 0)
                    {
                        var s = ApplyMacros(textSpan.Cut());
                        macroArguments.Add(s);
                        textSpan.Reset(i + 1);
                        if ((macroArguments.Count == macro.ParameterCount) != (c == ')'))
                            Throw.InvalidNumberOfArguments();
                    }
                    else if (bracketCount > 1 || c != '(')
                        textSpan.Expand();
                }
                else if (c == '$' && !textSpan.IsEmpty)
                {
                    textSpan.Expand();
                    var macroName = textSpan.ToString();
                    int j, mlen = macroName.Length - 1;
                    for (j = 0; j < mlen; ++j)
                        if (Macros.TryGetValue(macroName[j..], out macro))
                            break;

                    if (macro.IsEmpty)
                        Throw.UndefinedMacro(macroName);
                    else if (j > 0)
                        stringBuilder.Append(macroName[..j]);

                    bracketCount = 0;
                    macroArguments.Clear();
                    textSpan.Reset(i);
                }
                else
                {
                    if (!macro.IsEmpty)
                    {
                        var s = ApplyMacros(macro.Run(macroArguments));
                        var sbLength = stringBuilder.Length;
                        SetLineInputFields(s, stringBuilder, fields, false);
                        if (stringBuilder.Length == sbLength)
                            stringBuilder.Append(s);

                        textSpan.Reset(i);
                        macro = emptyMacro;
                    }
                    if (Validator.IsMacroLetter(c, textSpan.Length))
                    {
                        if (textSpan.IsEmpty)
                            textSpan.Reset(i);

                        textSpan.Expand();
                    }
                    else
                    {
                        if (!textSpan.IsEmpty)
                        {
                            stringBuilder.Append(textSpan.Cut());
                            textSpan.Reset(i);
                        }
                        stringBuilder.Append(c);
                    }
                }
            }
            if (macro.IsEmpty)
            {
                if (!textSpan.IsEmpty)
                    stringBuilder.Append(textSpan.Cut());
            }
            else if (macroArguments.Count == macro.ParameterCount)
            {
                var s = ApplyMacros(macro.Run(macroArguments));
                stringBuilder.Append(s);
            }
            return stringBuilder.ToString();
        }


        private static char EatSpace(ReadOnlySpan<char> s, ref int index)
        {
            var len = s.Length - 1;
            while (index < len)
            {
                ++index;
                if (s[index] != ' ')
                    return s[index];
            }
            return '\0';
        }

        public static int CountInputFields(ReadOnlySpan<char> s) =>
            CountOrHasInputFields(s, false);

        public static bool HasInputFields(ReadOnlySpan<char> s) =>
            CountOrHasInputFields(s, true) > 0;

        private static int CountOrHasInputFields(ReadOnlySpan<char> s, bool hasAny)
        {
            if (s.IsEmpty)
                return 0;

            var count = 0;
            var commentEnumerator = s.EnumerateComments();
            foreach (var item in commentEnumerator)
            {
                if (!item.IsEmpty && item[0] != '"' && item[0] != '\'')
                {
                    foreach (var c in item)
                    {
                        if (c == '?')
                        {
                            if (hasAny)
                                return 1;

                            ++count;
                        }
                    }
                }
            }
            return count;
        }

        public static bool SetLineInputFields(string s, StringBuilder sb, Queue<string> fields, bool forceLine)
        {
            if (string.IsNullOrEmpty(s) || fields is null || fields.Count == 0)
                return false;

            var inputChar = '\0';
            var count = fields.Count;
            var commentEnumerator = s.AsSpan().EnumerateComments();
            foreach (var item in commentEnumerator)
            {
                if (!item.IsEmpty)
                {
                    if (item[0] == '"' || item[0] == '\'')
                        sb.Append(item);
                    else
                    {
                        var j0 = 0;
                        var len = item.Length;
                        for (int j = 0; j < len; ++j)
                        {
                            var c = item[j];
                            if (c == '?')
                                inputChar = c;
                            else if (c == '{' && inputChar == '?')
                            {
                                inputChar = c;
                                sb.Append(item[j0..(j + 1)]);
                            }
                            else if (c == '}' && inputChar == '{')
                            {
                                inputChar = '\0';
                                if (!fields.TryDequeue(out string val))
                                    return false;

                                sb.Append(val);
                                j0 = j;
                            }
                            else if (inputChar == '{')
                                continue;
                            else if (c != ' ')
                            {
                                if (AddField(item[j0..j]))
                                    j0 = j;
                            }
                        }
                        if (!AddField($"{item[j0..]} "))
                            sb.Append($"{item[j0..]}");
                    }
                }
            }
            if (forceLine && fields.Count != 0)
            {
                RemoveLineFields(sb);
                AddLineFields(sb, fields);
            }
            return fields.Count < count;

            bool AddField(ReadOnlySpan<char> s)
            {
                if (inputChar == '?')
                {
                    inputChar = '\0';
                    if (!fields.TryDequeue(out string val))
                        return false;

                    sb.Append($"{s}{{{val}}}");
                    return true;
                }
                return false;
            }
        }

        private static void RemoveLineFields(StringBuilder sb)
        {
            var len = sb.Length;
            var i = len;
            while (--i > 0)
                if (sb[i] == '{')
                    break;

            if (i > 1 && sb[i - 1] == '#')
            {
                while (--i > 0)
                    if (sb[i] != ' ')
                        break;
            }
            else
                i = -1;

            if (i > -1)
            {
                len -= i - 1;
                if (len > 0)
                    sb.Remove(i - 1, len);
            }
        }

        private static void AddLineFields(StringBuilder sb, Queue<string> fields)
        {
            if (HasUnclosedComment(sb))
                sb.Append("' #{");
            else
                sb.Append(" #{");

            while (fields.TryDequeue(out string val))
            {
                sb.Append(val);
                sb.Append(';');
            }
            sb[^1] = '}';
        }

        private static bool HasUnclosedComment(StringBuilder sb)
        {
            var commentChar = '\0';
            var commentCount = 0;
            for (int i = 0, len = sb.Length; i < len; ++i)
            {
                var c = sb[i];
                if (commentChar == '\0')
                {
                    if (c == '\'' || c == '"')
                    {
                        commentChar = c;
                        ++commentCount;
                    }
                }
                else if (c == commentChar)
                    ++commentCount;
            }
            return commentCount % 2 == 1;
        }

        public int GetUnwarpedLineNumber(int sourceLineNumber)
        {
            if (sourceLineNumber < 1 || sourceLineNumber >= LineNumbers.Count)
                return sourceLineNumber;

            return LineNumbers[sourceLineNumber];
        }
    }
}
