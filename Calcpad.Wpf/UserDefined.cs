using Calcpad.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Calcpad.Wpf
{
    internal class UserDefined
    {
        internal static Func<string, Queue<string>, string> Include;
        internal readonly Dictionary<string, int> Variables = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> Units = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> Functions = new(StringComparer.Ordinal);
        internal readonly List<KeyValuePair<string, int>> FunctionDefs = [];
        internal readonly Dictionary<string, string> MacroProcedures = [];
        internal readonly Dictionary<string, int> Macros = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, List<int>> MacroParameters = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, List<int>> MacroVariables = new(StringComparer.Ordinal);
        internal readonly Dictionary<string, List<int>> MacroFunctions = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _macroContents = new(StringComparer.Ordinal);
        internal bool HasMacros => _hasIncludes || Macros.Count > 0;
        private readonly StringBuilder _macroBuilder = new();
        private bool _hasIncludes;
        private string _macroName;

        internal void Clear(bool isComplex)
        {
            Variables.Clear();
            Units.Clear();
            Functions.Clear();
            FunctionDefs.Clear();
            Macros.Clear();
            MacroParameters.Clear();
            _macroContents.Clear();
            MacroVariables.Clear();
            MacroFunctions.Clear();
            MacroProcedures.Clear();
            Variables.Add("e", -1);
            Variables.Add("pi", -1);
            Variables.Add("π", -1);
            if (isComplex)
            {
                Variables.Add("i", -1);
                Variables.Add("ei", -1);
                Variables.Add("πi", -1);
            }
            _hasIncludes = false;
            _macroName = null;
            _macroBuilder.Clear();
        }

        private readonly StringBuilder sb = new();
        internal void Get(SpanLineEnumerator lines, bool isComplex)
        {
            Clear(isComplex);
            var lineNumber = 0;
            var firstLine = 0;
            sb.Clear();
            foreach (var line in lines)
            {
                ++lineNumber;
                if (!(line.IsEmpty && string.IsNullOrEmpty(_macroName)))
                {
                    if (line.EndsWith(" _"))
                    {
                        if (sb.Length == 0)
                            firstLine = lineNumber;

                        sb.Append(line[..^2]);
                    }
                    else if (sb.Length > 0)
                    {
                        sb.Append(line);
                        Get(sb.ToString().AsSpan(), firstLine);
                        sb.Clear();
                    }
                    else
                        Get(line, lineNumber);
                }
            }
        }

        internal void Get(ReadOnlySpan<char> lineContent, int lineNumber)
        {
            if (Validator.IsKeyword(lineContent, "#include"))
            {
                var s = GetFileName(lineContent);
                if (File.Exists(s))
                {
                    s = Include(s, null);
                    var lines = s.EnumerateLines();
                    foreach (var line in lines)
                        Get(line, lineNumber);
                }
                _hasIncludes = true;
            }
            else if (Validator.IsKeyword(lineContent, "#def"))
                GetMacros(lineContent[4..], lineNumber);
            else if (Validator.IsKeyword(lineContent, "#end def"))
            {
                if (!string.IsNullOrEmpty(_macroName))
                {
                    CompleteMacroParameters(lineNumber);
                    _macroContents.TryAdd(_macroName, _macroBuilder.ToString());
                    GetMacroVariablesAndFunctions(_macroBuilder.ToString().AsSpan(), lineNumber);
                    _macroName = null;
                    _macroBuilder.Clear();
                }
            }
            else if (Validator.IsKeyword(lineContent, "#read"))
            {
                var i1 = lineContent.IndexOf(' ');
                if (i1 > 0)
                {
                    var s = lineContent.Slice(i1 + 1);
                    i1 = s.IndexOf(' ');
                    if (i1 > 0)
                        Variables.TryAdd(s.Slice(0, i1).ToString(), lineNumber);
                }
            }
            else if (string.IsNullOrEmpty(_macroName))
                GetVariablesUnitsAndFunctions(lineContent, lineNumber);
            else
                _macroBuilder.AppendLine(lineContent.ToString());
        }

        internal static string GetFileName(ReadOnlySpan<char> s)
        {
            var n = s.Length;
            if (n < 9)
                return null;

            n = s.IndexOfAny('\'', '"');
            var n1 = s.LastIndexOf('#');
            if (n < 9 || n1 > 0 && n1 < n)
                n = n1;

            if (n < 9)
                n = s.Length;

            return s[8..n].Trim().ToString();
        }

        private void GetVariablesUnitsAndFunctions(ReadOnlySpan<char> lineContent, int lineNumber)
        {
            lineContent = lineContent.Trim();
            if (lineContent.StartsWith("#for ", StringComparison.OrdinalIgnoreCase))
                lineContent = lineContent[5..];

            var commentEnumerator = lineContent.EnumerateComments();
            foreach (var item in commentEnumerator)
            {
                if (!item.IsEmpty && item[0] != '"' && item[0] != '\'')
                {
                    var ts = new TextSpan(item);
                    var isDone = false;
                    var isFunction = false;
                    var isSubscript = false;
                    ts.Reset(0);
                    for (int j = 0, len = item.Length; j < len; ++j)
                    {
                        var c = item[j];
                        if (Validator.IsWhiteSpace(c) && ts.IsEmpty)
                            continue;
                        else if (Validator.IsVarChar(c) || isSubscript && char.IsLetter(c))
                        {
                            if (isDone)
                            {
                                isSubscript = false;
                                if (isFunction)
                                    continue;

                                break;
                            }
                            if (c == '_')
                                isSubscript = true;

                            if (ts.IsEmpty)
                                ts.Reset(j);

                            ts.Expand();
                        }
                        else if (c == '=')
                        {
                            if (!ts.IsEmpty)
                            {
                                var s = ts.Cut();
                                if (isFunction)
                                {
                                    if (s[^1] == '.')
                                    {
                                        if (Variables.ContainsKey(s[..^1].ToString()))
                                            break;
                                    }
                                    Functions.TryAdd(s.ToString(), lineNumber);
                                    FunctionDefs.Add(new(item[0..j].Trim().ToString(), lineNumber));
                                }
                                else if (ts.StartsWith('.'))
                                    Units.TryAdd(s[1..].ToString(), lineNumber);
                                else if (!Validator.IsUnitStart(s[0]))
                                    Variables.TryAdd(s.ToString(), lineNumber);
                            }
                            break;
                        }
                        else if (c == '$' && !ts.IsEmpty)
                        {
                            ts.Expand();
                            var s = ts.Cut();
                            ts.Reset(j);
                            for (int i = 0, blen = s.Length; i < blen; ++i)
                            {
                                if (Validator.IsMacroLetter(s[i], i))
                                {
                                    var s1 = s[i..];
                                    ref var contents = ref CollectionsMarshal.GetValueRefOrNullRef(_macroContents, s1.ToString());
                                    if (!System.Runtime.CompilerServices.Unsafe.IsNullRef(ref contents))
                                    {
                                        if (!string.IsNullOrEmpty(contents))
                                        {
                                            GetVariablesUnitsAndFunctions(contents, lineNumber);
                                            contents = null;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            isDone = true;
                            if (c == '(')
                                isFunction = true;
                        }
                    }
                    var index = item.IndexOf("$sup", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                        AddVariable(item[index..], "_sup");

                    index = item.IndexOf("$inf", StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                        AddVariable(item[index..], "_inf");
                }
            }

            void AddVariable(ReadOnlySpan<char> span, string suffix)
            {
                var i1 = span.IndexOf('@') + 1;
                if (i1 > 0)
                {
                    var i2 = span[i1..].IndexOf('=');
                    if (i2 >= 0)
                    {
                        var name = span.Slice(i1, i2).Trim().ToString() + suffix;
                        Variables.TryAdd(name, lineNumber);
                    }
                }
            }
        }

        private void GetMacroVariablesAndFunctions(ReadOnlySpan<char> content, int lineNumber)
        {
            var lines = content.Trim().EnumerateLines();
            var i = lineNumber - content.Count(Environment.NewLine) - 1;
            foreach (var line in lines)
            {
                ++i;
                var lineSpan = line.Trim();
                if (lineSpan.StartsWith("#for ", StringComparison.OrdinalIgnoreCase))
                    lineSpan = lineSpan[5..];
                var commentEnumerator = lineSpan.EnumerateComments();
                foreach (var item in commentEnumerator)
                {
                    if (!item.IsEmpty && item[0] != '"' && item[0] != '\'')
                    {
                        var ts = new TextSpan(item);
                        var isDone = false;
                        var isFunction = false;
                        ts.Reset(0);
                        for (int j = 0, len = item.Length; j < len; ++j)
                        {
                            var c = item[j];
                            if (Validator.IsWhiteSpace(c) && ts.IsEmpty)
                                continue;

                            if (Validator.IsVarChar(c))
                            {
                                if (isDone)
                                {
                                    if (isFunction)
                                        continue;

                                    break;
                                }
                                if (ts.IsEmpty)
                                    ts.Reset(j);

                                ts.Expand();
                            }
                            else if (c == '=')
                            {
                                if (!ts.IsEmpty)
                                {
                                    TryAdd(isFunction ? MacroFunctions : MacroVariables,
                                        ts.Cut().ToString(), i, lineNumber);
                                }
                                break;
                            }
                            else
                            {
                                isDone = true;
                                if (c == '(')
                                    isFunction = true;
                            }
                        }
                    }
                }
            }
        }

        private void GetMacros(ReadOnlySpan<char> lineContent, int lineNumber)
        {
            var isFunction = false;
            var isComplete = false;
            var ts = new TextSpan(lineContent);
            for (int i = 0, len = lineContent.Length; i < len; ++i)
            {
                var c = lineContent[i];
                if (Validator.IsWhiteSpace(c) && ts.IsEmpty)
                    continue;
                else if (Validator.IsMacroLetter(c, ts.Length))
                {
                    if (ts.IsEmpty)
                        ts.Reset(i);

                    ts.Expand();
                }
                else if (c == '$')
                {
                    if (!ts.IsEmpty)
                    {
                        ts.Expand();
                        var s = ts.Cut().ToString();
                        if (isFunction)
                            TryAdd(MacroParameters, s, lineNumber, -1);
                        else
                        {
                            Macros.TryAdd(s, lineNumber);
                            _macroName = s;
                        }
                    }
                }
                else if (c == '(' || c == ';')
                {
                    ts.Reset(i);
                    isFunction = true;
                }
                else if (isFunction && c == ')')
                {
                    if (!string.IsNullOrEmpty(_macroName))
                        MacroProcedures.TryAdd(_macroName, lineContent[(_macroName.Length + 1)..(i + 1)].ToString());

                    isComplete = true;
                }
                else if (c == '=')
                {
                    if (!string.IsNullOrEmpty(_macroName))
                        _macroContents.TryAdd(_macroName, lineContent[(i + 1)..].ToString());

                    _macroName = null;
                    if (isComplete)
                        CompleteMacroParameters(lineNumber);

                    return;
                }
                else if (c != ' ' && c != ';')
                    break;
            }
            if (Macros.Count == 0)
                Macros.Add("Invalid$", -1);
        }

        private static void TryAdd(Dictionary<string, List<int>> dict, string key, int start, int end)
        {
            if (dict.TryGetValue(key, out var bounds))
            {
                if (start <= bounds[^1] + 1)
                    bounds[^1] = end;
                else
                {
                    bounds.Add(start);
                    bounds.Add(end);
                }
            }
            else
                dict.Add(key, [start, end]);
        }

        private void CompleteMacroParameters(int lineNumber)
        {
            foreach (var kvp in MacroParameters)
            {
                var bounds = kvp.Value;
                for (int i = 1, count = bounds.Count; i < count; i += 2)
                {
                    if (bounds[i] == -1)
                        bounds[i] = lineNumber;
                }
            }
        }

        internal bool IsMacroOrParameter(string s, int line) =>
            IsDefined(s, line, Macros, MacroParameters);

        internal bool IsFunction(string s, int line) =>
            IsDefined(s, line, Functions, MacroFunctions);

        internal bool IsVariable(string s, int line) =>
            IsDefined(s, line, Variables, MacroVariables);

        internal bool IsUnit(string s, int line) =>
            IsDefined(s, line, Units, null);


        private static bool IsDefined(string s, int line, Dictionary<string, int> items, Dictionary<string, List<int>> macroItems)
        {
            if (items.TryGetValue(s, out var index))
            {
                if (index <= line)
                    return true;
            }
            if (macroItems is not null && macroItems.TryGetValue(s, out var bounds))
                for (int i = 1, count = bounds.Count; i < count; i += 2)
                    if (line >= bounds[i - 1] && line <= bounds[i])
                        return true;

            return false;
        }
    }
}