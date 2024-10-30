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
            private readonly string _contents;
            private readonly string[] _parameters;
            private readonly int[] _order;

            internal Macro(string contents, List<string> parameters)
            {
                _contents = contents;
                if (parameters is null)
                {
                    _parameters = null;
                    _order = null;
                }
                else
                {
                    _parameters = [.. parameters];
                    _order = Sort(_parameters);
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
                        Throw.DuplicateMacroParametersException(s);
                        return null;
                    }
                }
                return sorted.Values.Reverse().ToArray();
            }

            internal string Run(List<string> arguments)
            {
                if (arguments.Count != ParameterCount)
                    Throw.InvalidNumberOfArgumentsException();

                if (ParameterCount == 0)
                    return _contents;

                var sb = new StringBuilder(_contents);
                for (int i = 0, count = arguments.Count; i < count; i++)
                {
                    var j = _order[i];
                    var s = arguments[j];
                    if (s[0] == ' ' && s.Length > 1)
                        sb.Replace(_parameters[j], s[1..]);
                    else
                        sb.Replace(_parameters[j], s);
                }
                return sb.ToString();
            }
            internal bool IsEmpty => _contents is null;
            internal int ParameterCount => _parameters?.Length ?? 0;
        }

        private enum Keywords
        {
            None,
            Def,
            EndDef,
            Include,
        }
        private readonly List<int> _lineNumbers = [];
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
        public bool Parse(string sourceCode, out string outCode, StringBuilder sb, int includeLine, bool addLineNumbers)
        {
            var sourceLines = sourceCode.EnumerateLines();
            if (includeLine == 0)
            {
                sb = new StringBuilder(sourceCode.Length);
                Macros.Clear();
                _lineNumbers.Clear();
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
                        _lineNumbers.Add(_parsedLineNumber);
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
                    _lineNumbers.Add(_parsedLineNumber);

                if (macroDefCount > 0)
                {
                    sb.Append(Messages.Macro_definition_block_not_closed_Missing_end_def);
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
                    AppendError(lineContent.ToString(), Messages.Missing_source_file_for_include);
                n = lineContent.IndexOfAny('\'', '"');
                var nf1 = lineContent.LastIndexOf('#');
                if (n < 9 || nf1 > 0 && nf1 < n)
                    n = nf1;

                if (n < 9)
                    n = lineContent.Length;

                var insertFileName = Environment.ExpandEnvironmentVariables(lineContent[8..n].Trim().ToString());
                var fileExist = File.Exists(insertFileName);
                if (!fileExist)
                    AppendError(lineContent.ToString(), Messages.File_not_found);
                Queue<string> fields = new();
                if (nf1 > 0)
                {
                    var nf2 = lineContent.LastIndexOf('}');
                    if (nf2 < 0)
                        AppendError(lineContent.ToString(), "Brackets not closed.");
                    else
                    {
                        SplitEnumerator split = lineContent[(nf1 + 2)..nf2].EnumerateSplits(';');
                        foreach (var item in split)
                            fields.Enqueue(item.Trim().ToString());
                    }
                }
                if (fileExist)
                    Parse(Include(insertFileName, fields), out _, sb, lineNumber, addLineNumbers);
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
                        macroParameters = [];
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
                            AppendError(lineContent.ToString(), string.Format(Messages.Invalid_macro_name_0, macroName));

                            macroName = string.Empty;
                            textSpan.Reset(i);
                        }
                        ++macroDefCount;
                    }
                }
                else
                {
                    AppendError(lineContent.ToString(), Messages.Invalid_inside_macro_definition);
                    ++macroDefCount;
                }
            }

            void ParseEndDef(ReadOnlySpan<char> lineContent)
            {
                if (macroDefCount < 1)
                {
                    AppendError(lineContent.ToString(), Messages.There_is_no_matching_def);
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
                if (addLineNumbers)
                    sb.AppendLine(line + '\v' + lineNumber.ToString());
                else
                    sb.AppendLine(line);

                ++_parsedLineNumber;
            }

            void SymbolError(ReadOnlySpan<char> lineContent, char c)
            {
                AppendError(lineContent.ToString(), string.Format(Messages.Invalid_symbol_0_in_macro_name, c));
            }

            void AppendError(string lineContent, string errorMessage)
            {
                sb.AppendLine(string.Format(Messages.Error_in_0_on_line_1__2, HttpUtility.HtmlEncode(lineContent), LineHtml(lineNumber), errorMessage));
                hasErrors = true;
            }

            void AddMacro(string lineContent, string name, Macro macro)
            {
                if (!Macros.TryAdd(name, macro))
                    AppendError(lineContent, string.Format(Messages.Duplicate_macro_name_0_, name));
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
                            Throw.InvalidNumberOfArgumentsException();
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
                        Throw.UndefinedMacroException(macroName);
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
                if (inputChar != '?') return false;
                inputChar = '\0';
                if (!fields.TryDequeue(out var val))
                    return false;

                sb.Append($"{s}{{{val}}}");
                return true;
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
            sb.Append(HasUnclosedComment(sb) ? "' #{" : " #{");

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
            if (sourceLineNumber < 1 || sourceLineNumber >= _lineNumbers.Count)
                return sourceLineNumber;

            return _lineNumbers[sourceLineNumber];
        }
    }
}
