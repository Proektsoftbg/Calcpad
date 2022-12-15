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
#if BG
                        throw new MathParser.MathParserException($"Дублиране на имената на параметрите на макрос: {s} и {s}.");
#else
                        throw new MathParser.MathParserException($"Duplicate macro parameter names: {s} and {s}.");
#endif
                    }
                }

                 return sorted.Values.Reverse().ToArray();
            }

            internal string Run(List<string> arguments)
            {
                if (arguments.Count != ParameterCount)

#if BG
                    throw new MathParser.MathParserException("Невалиден брой аргументи.");
#else
                    throw new MathParser.MathParserException("Invalid number of arguments.");
#endif
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
        private static readonly char[] Comments = { '\'', '"' };
        //private static readonly string[] NewLines = { "\r\n", "\r", "\n" };
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

        public bool Parse(string sourceCode, out string outCode, StringBuilder stringBuilder)
        {
            var sourceLines = sourceCode.AsSpan().EnumerateLines();
            if (stringBuilder is null)
                stringBuilder = new StringBuilder(sourceCode.Length);
            var macroName = string.Empty;
            var macroBuilder = new StringBuilder(50);
            var lineNumber = 0;
            var includeCount = 0;
            var macroDefCount = 0;
            var hasErrors = false;
            ReadOnlySpan<char> lineContent = "code";
            Macros.Clear();
            List<string> macroParameters = null;
            try
            {
                foreach (ReadOnlySpan<char> sourceLine in sourceLines)
                {
                    if (includeCount == 0)
                        ++lineNumber;
                    else
                        --includeCount;

                    lineContent = sourceLine.Trim();
                    var keyword = Keywords.None;
                    if (lineContent.IsEmpty)
                    {
                        stringBuilder.AppendLine(sourceLine.ToString());
                        continue;
                    }
                    if (lineContent[0] == '#')
                    {
                        var isKeyWord = true;
                        keyword = GetKeyword(lineContent);
                        if (keyword == Keywords.Include)
                        {
                            int n = lineContent.Length;
                            if (n < 9)
#if BG
                                AppendError($"Липсва изходен файл за вмъкване.");
#else
                                AppendError(lineContent.ToString(), $"Missing source file for include.");
#endif                      
                            n = lineContent.IndexOfAny(Comments);
                            if (n < 9)
                                n = lineContent.Length;

                            var insertFileName = lineContent[8..n].Trim().ToString();
                            if (!File.Exists(insertFileName))
#if BG
                                AppendError("Файлът не е намерен.");
#else
                                AppendError(lineContent.ToString(), "File not found.");
#endif  
                             
                            {
                                var includeCode = Include(insertFileName, null);
                                Parse(includeCode, out string outIncludeCode, stringBuilder);
                                stringBuilder.Append(outIncludeCode);
                            }
                        }
                        else if (keyword == Keywords.Def)
                        {
                            if (macroDefCount == 0)
                            {
                                macroBuilder.Clear();
                                int j = 4, len = lineContent.Length;
                                char c = EatSpace(lineContent, ref j);
                                while (j < len)
                                {
                                    c = lineContent[j];
                                    if (c == '$')
                                    {
                                        macroBuilder.Append(c);
                                        macroName = macroBuilder.ToString();
                                        macroBuilder.Clear();
                                        break;
                                    }
                                    if (IsMacroLetter(c, macroBuilder.Length))
                                        macroBuilder.Append(c);
                                    else
                                    {
                                        SymbolError(lineContent, c);
                                        break;
                                    }
                                    ++j;
                                }
                                c = EatSpace(lineContent, ref j);
                                if (c == '(')
                                {
                                    macroParameters = new();
                                    c = EatSpace(lineContent, ref j);
                                    while (j < len)
                                    {
                                        if (c == ' ')
                                            c = EatSpace(lineContent, ref j);
                                        if (c == ';' || c == ')')
                                        {
                                            macroParameters.Add(macroBuilder.ToString());
                                            macroBuilder.Clear();
                                            if (c == ')')
                                                break;

                                            c = EatSpace(lineContent, ref j);
                                        }
                                        else
                                        {
                                            if (IsMacroLetter(c, macroBuilder.Length) || c == '$')
                                                macroBuilder.Append(c);
                                            else if (c != '\n')
                                                SymbolError(lineContent.ToString(), c);

                                            c = lineContent[++j];
                                        }
                                    }
                                    c = EatSpace(lineContent, ref j);
                                }
                                else
                                    macroParameters = null;

                                if (c == '=')
                                {
                                    c = EatSpace(lineContent, ref j);
                                    AddMacro(lineContent.ToString(), macroName, new Macro(lineContent[j..].ToString(), macroParameters));
                                    macroName = string.Empty;
                                    macroBuilder.Clear();
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(macroName))
                                    {
                                        macroName = macroBuilder.ToString();

#if BG
                                        AppendError($"Невалидно име на макрос: \"{macroName}\".");
#else
                                        AppendError(lineContent.ToString(), $"Invalid macro name: \"{macroName}\".");
#endif
                                        macroName = string.Empty;
                                        macroBuilder.Clear();
                                    }
                                    ++macroDefCount;
                                }
                            }
                            else
                            {
#if BG
                                AppendError("Невалидно в дефиниция на макрос.");
#else
                                AppendError(lineContent.ToString(), "Invalid inside macro definition.");
#endif
                                ++macroDefCount;
                            }
                        }
                        else if (keyword == Keywords.EndDef)
                        {
                            if (macroDefCount < 1)
                            {
#if BG
                                AppendError("\"Няма съответен \"#def\".");
#else
                                AppendError(lineContent.ToString(), "\"There is no matching \"#def\".");
#endif
                            }
                            else
                            { 
                                var j = macroBuilder.Length - 2;
                                //if (j > 0) 
                                //    macroBuilder.Remove(j, 2);

                                string macroContent = macroBuilder.ToString();
                                AddMacro(lineContent.ToString(), macroName, new Macro(macroContent, macroParameters));
                                macroName = string.Empty;
                                macroBuilder.Clear();
                            }
                            --macroDefCount;
                        }
                        else
                            isKeyWord = false;

                        if (isKeyWord)
                            continue;
                    }
                    if (macroDefCount == 1)
                        macroBuilder.AppendLine(sourceLine.ToString());
                    else if (Macros.Any())
                    {
                        try
                        {
                            var insertCode = ApplyMacros(sourceLine);
                            var insertLines = insertCode.Split(Environment.NewLine, StringSplitOptions.None);
                            foreach (var line in insertLines)
                                stringBuilder.AppendLine(line);
                        }
                        catch (Exception ex)
                        {
                            AppendError(lineContent.ToString(), ex.Message);
                        }
                    }
                    else
                        stringBuilder.AppendLine(sourceLine.ToString());
                }
                if (macroDefCount > 0)
                {
#if BG
                    stringBuilder.Append($"#Грешка: Незатворена дефиниция на макрос. Липсва \"#end def\".");
#else
                    stringBuilder.Append($"#Error: Macro definition block not closed. Missing \"#end def\".");
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
                outCode = stringBuilder.ToString();
            }
            return hasErrors;

            void SymbolError(ReadOnlySpan<char> lineContent, char c)
            {
#if BG
                AppendError($"Невалиден символ \"{c}\" в име на макрос.");
#else
                AppendError(lineContent.ToString()  , $"Invalid symbol \"{c}\" in macro name.");
#endif
            }

            void AppendError(string lineContent, string errorMessage)
            {
#if BG
                stringBuilder.AppendLine($"#Грешка в \"{HttpUtility.HtmlEncode(lineContent)}\" на ред {LineHtml(lineNumber)}: {errorMessage}");
#else
                stringBuilder.AppendLine($"#Error in \"{HttpUtility.HtmlEncode(lineContent)}\" on line {LineHtml(lineNumber)}: {errorMessage}");
#endif
                hasErrors = true;
            }

            void AddMacro(string lineContent, string name, Macro macro)
            {
                if (!Macros.TryAdd(name, macro))
#if BG
                    AppendError($"Повтарящо се име на макрос: \"{name}\".");
#else
                    AppendError(lineContent, $"Duplicate macro name: \"{name}\".");
#endif
            }

            static string LineHtml(int line) => $"[<a href=\"#0\" data-text=\"{line}\">{line}</a>]";
        }

        private static string ApplyMacros(ReadOnlySpan<char> lineContent)
        {
            var stringBuilder = new StringBuilder(50);
            var macroBuilder = new StringBuilder(10);
            var macroArguments = new List<string>();
            var bracketCount = 0;
            var emptyMacro = new Macro(null, null);
            var macro = emptyMacro;
            for (int i = 0, len = lineContent.Length; i < len; ++i)
            {
                var c = lineContent[i];
                if (macroArguments.Count < macro.ParameterCount)
                {
                    if (c == '(')
                        ++bracketCount;
                    else if (c == ')')
                        --bracketCount;

                    if (c == ';' && bracketCount == 1 || c == ')' && bracketCount == 0)
                    {
                        var s = ApplyMacros(macroBuilder.ToString());
                        macroArguments.Add(s);
                        macroBuilder.Clear();
                        if ((macroArguments.Count == macro.ParameterCount) != (c == ')'))
#if BG
                            throw new ArgumentException("Невалиден брой аргументи.");
#else
                            throw new ArgumentException("Invalid number of arguments.");
#endif
                    }
                    else if (bracketCount > 1 || c != '(')
                        macroBuilder.Append(c);
                }
                else if (c == '$' && macroBuilder.Length > 0)
                {
                    macroBuilder.Append('$');
                    var macroName = macroBuilder.ToString();
                    int j, mlen = macroName.Length - 1;
                    for (j = 0; j < mlen; ++j)
                        if (Macros.TryGetValue(macroName[j..], out macro))
                            break;

                    if (macro.IsEmpty)
#if BG
                        throw new ArgumentException($"Недефиниран макрос: {macroName}.");
#else
                        throw new ArgumentException($"Macro not defined: {macroName}.");
#endif
                    else if (j > 0)
                        stringBuilder.Append(macroName[..j]);

                    bracketCount = 0;
                    macroArguments.Clear();
                    macroBuilder.Clear();
                }
                else
                {
                    if (!macro.IsEmpty)
                    {
                        var s = ApplyMacros(macro.Run(macroArguments));
                        stringBuilder.Append(s);
                        macro = emptyMacro;
                    }
                    if (IsMacroLetter(c, macroBuilder.Length))
                        macroBuilder.Append(c);
                    else
                    {
                        if (macroBuilder.Length > 0)
                        {
                            stringBuilder.Append(macroBuilder);
                            macroBuilder.Clear();
                        }
                        stringBuilder.Append(c);
                    }
                }
            }
            if (macro.IsEmpty)
            {
                if (macroBuilder.Length > 0)
                    stringBuilder.Append(macroBuilder);
            }
            else if (macroArguments.Count == macro.ParameterCount)
            {
                var s = ApplyMacros(macro.Run(macroArguments));
                stringBuilder.Append(s);
            }
            var l = stringBuilder.Length - 1;
            if (l > 1 && stringBuilder[l] == '\n')
            {
                if (stringBuilder[l - 1] == '\r')
                    stringBuilder.Remove(l - 1, 2);
                else
                    stringBuilder.Remove(l, 1);
            }
            return stringBuilder.ToString();
        }

        private static bool IsMacroLetter(char c, int position) =>
            c >= 'a' && c <= 'z' ||
            c >= 'A' && c <= 'Z' ||
            c == '_' ||
            char.IsDigit(c) && position > 0;

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

        private static int CountOrHasInputFields(ReadOnlySpan<char> s, bool any)
        {
            var count = 0;
            const char nullChar = (char)0;
            var commentChar = nullChar;
            foreach (var c in s)
            {
                if (c == '\n')
                    commentChar = nullChar;
                else if (c == '\'' || c == '"')
                {
                    if (commentChar == nullChar)
                        commentChar = c;
                    else if (commentChar == c)
                        commentChar = nullChar;
                }
                else if (c == '?' && commentChar == nullChar)
                {
                    if (any)
                        return 1;

                    ++count;
                }
            }
            return count;
        }
    }
}
