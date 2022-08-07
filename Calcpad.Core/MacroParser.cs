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
        private struct Macro
        {
            private readonly string Contents;
            private readonly List<string> Parameters;

            internal Macro(string contents, List<string> parameters)
            {
                Contents = contents;
                Parameters = parameters;
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

                var s = Contents;
                for (int i = 0, _count = arguments.Count; i < _count; i++)
                    s = s.Replace(Parameters[i], arguments[i]);

                return s;
            }
            internal bool IsEmpty => Contents is null;
            internal int ParameterCount => Parameters is null ? 0 : Parameters.Count;
        }

        private enum Keywords
        {
            None,
            Def,
            EndDef,
            Include,
        }
        private static readonly char[] Comments = { '\'', '"' };
        private static readonly string[] NewLines = { "\r\n", "\r", "\n" };
        private static readonly Dictionary<string, Macro> Macros = new(StringComparer.Ordinal);
        public Func<string, Queue<string>, string> Include;

        private static Keywords GetKeyword(string s)
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

        public bool Parse(string sourceCode, out string outCode)
        {
            var sourceLines = sourceCode.Split(NewLines, StringSplitOptions.None).ToList();
            var stringBuilder = new StringBuilder(sourceCode.Length);
            var macroName = string.Empty;
            var macroBuilder = new StringBuilder(50);
            var lineNumber = 0;
            var includeCount = 0;
            var macroDefCount = 0;
            var hasErrors = false;
            string lineContent = "code";
            Macros.Clear();
            List<string> macroParameters = new();
            try
            {
                for (var i = 0; i < sourceLines.Count; ++i)
                {
                    if (includeCount == 0)
                        ++lineNumber;
                    else
                        --includeCount;

                    lineContent = sourceLines[i].Trim();
                    var keyword = Keywords.None;
                    if (string.IsNullOrEmpty(lineContent))
                    {
                        if (i < sourceLines.Count - 1)
                            stringBuilder.AppendLine(sourceLines[i]);

                        continue;
                    }
                    else if (lineContent[0] == '#')
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
                                AppendError($"Missing source file for include.");
#endif                      
                            n = lineContent.IndexOfAny(Comments);
                            if (n < 9)
                                n = lineContent.Length;

                            string insertFileName = lineContent[8..n].Trim();
                            if (!File.Exists(insertFileName))
#if BG
                                AppendError("Файлът не е намерен.");
#else
                                AppendError("File not found.");
#endif  
                            else
                            {
                                var includeCode = Include(insertFileName, null);
                                var includeLines = includeCode.Split(NewLines, StringSplitOptions.None);
                                includeCount += includeLines.Length - 1;
                                sourceLines.InsertRange(i + 1, includeLines);
                            }
                        }
                        else if (keyword == Keywords.Def)
                        {
                            if (macroDefCount == 0)
                            {
                                macroBuilder.Clear();
                                int j = 3, n = lineContent.Length;
                                char c = EatSpace(lineContent, ref j);
                                while (j < n)
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
                                        SymbolError(c);
                                        break;
                                    }

                                    ++j;
                                }
                                c = EatSpace(lineContent, ref j);
                                macroParameters = new();
                                if (c == '(')
                                {
                                    c = EatSpace(lineContent, ref j);
                                    while (j < n)
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
                                                SymbolError(c);

                                            c = lineContent[++j];
                                        }
                                    }
                                    c = EatSpace(lineContent, ref j);
                                }
                                if (c == '=')
                                {
                                    c = EatSpace(lineContent, ref j);
                                    var contents = lineContent[j..];
                                    if (!Macros.TryAdd(macroName, new Macro(contents, macroParameters)))
#if BG
                                        AppendError("Повтарящо се име на макрос.");
#else
                                        AppendError("Duplicate macro name.");
#endif
                                    macroName = string.Empty;
                                    macroBuilder.Clear();
                                }
                                else
                                    ++macroDefCount;
                            }
                            else
                            {
#if BG
                                    AppendError("Невалидно в дефиниция на макрос.");
#else
                                AppendError("Invalid inside macro definition.");
#endif
                                ++macroDefCount;
                            }

                        }
                        else if (keyword == Keywords.EndDef)
                        {
                            if (macroDefCount == 1)
                            {
                                if (string.IsNullOrWhiteSpace(macroName))
#if BG
                                    AppendError("Намерен е \"#end def\" без съответен \"#def\".");
#else
                                    AppendError("\"#end def\" without matching \"#def\".");
#endif                          
                                var j = macroBuilder.Length - 2;
                                //if (j > 0) 
                                //    macroBuilder.Remove(j, 2);

                                string macroContent = macroBuilder.ToString();
                                if (!Macros.TryAdd(macroName, new Macro(macroContent, macroParameters)))
#if BG
                                    AppendError("Повтарящо се име на макрос.");
#else
                                    AppendError("Duplicate macro name.");
#endif
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
                        macroBuilder.AppendLine(sourceLines[i]);
                    else if (Macros.Any())
                    {
                        try
                        {
                            var insertCode = ApplyMacros(sourceLines[i]);
                            var insertLines = insertCode.Split(NewLines, StringSplitOptions.None);
                            foreach (var line in insertLines)
                                stringBuilder.AppendLine(line);
                        }
                        catch (Exception ex)
                        {
                            AppendError(ex.Message);
                        }
                    }
                    else
                        stringBuilder.AppendLine(sourceLines[i]);
                }
            }
            catch (Exception ex)
            {
                AppendError(ex.Message);
            }
            finally
            {
                outCode = stringBuilder.ToString();
            }
            return hasErrors;

            void SymbolError(char c)
            {
#if BG
                AppendError($"Невалиден символ \"{c}\" в име на макрос.");
#else
                AppendError($"Invalid symbol \"{c}\" in macro name.</p>");
#endif
            }

            void AppendError(string errorMessage)
            {
#if BG
                stringBuilder.AppendLine($"#Грешка в \"{HttpUtility.HtmlEncode(lineContent)}\" на ред {LineHtml(lineNumber)}: {errorMessage}");
#else
                stringBuilder.AppendLine($"#Error in \"{HttpUtility.HtmlEncode(lineContent)}\" on line {LineHtml(lineNumber)}: {errorMessage}");
#endif
                hasErrors = true;
            }
            static string LineHtml(int line) => $"[<a href=\"#0\" data-text=\"{line}\">{line}</a>]";
        }

        private static string ApplyMacros(string lineContent)
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
                        throw new ArgumentException($Невалидно име на макрос: {macroName}.");
#else
                        throw new ArgumentException($"Invalid macro name: {macroName}.");
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

        private static char EatSpace(in string s, ref int index)
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
    }
}
