﻿using Calcpad.Core;
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
            Macros.Clear();
            MacroParameters.Clear();
            _macroContents.Clear();
            MacroVariables.Clear();
            MacroFunctions.Clear();
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

        internal void Get(SpanLineEnumerator lines, bool isComplex)
        {
            Clear(isComplex);
            var lineNumber = 0;
            foreach (var line in lines)
            {
                ++lineNumber;
                if (!line.IsEmpty)
                    Get(line, lineNumber);
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
            {
                GetMacros(lineContent[4..], lineNumber);
            }
            else if (Validator.IsKeyword(lineContent, "#end def"))
            {
                if (!string.IsNullOrEmpty(_macroName) && _macroName.Length > 0)
                {
                    CompleteMacroParameters(lineNumber);
                    _macroContents.TryAdd(_macroName, _macroBuilder.ToString());
                    GetMacroVariablesAndFunctions(_macroBuilder.ToString().AsSpan(), lineNumber);
                    _macroName = null;
                    _macroBuilder.Clear();
                }
            }
            else if (_macroName is null)
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
                                    Functions.TryAdd(s.ToString(), lineNumber);
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
                    if (item.Contains("$sup", StringComparison.OrdinalIgnoreCase))
                        Variables.TryAdd("x_sup", lineNumber);
                    else if (item.Contains("$inf", StringComparison.OrdinalIgnoreCase))
                        Variables.TryAdd("x_inf", lineNumber);
                }
            }
        }

        private void GetMacroVariablesAndFunctions(ReadOnlySpan<char> content, int lineNumber)
        {
            var lines = content.EnumerateLines();
            var i = lineNumber;
            foreach (var _ in lines)
                --i;

            foreach (var line in lines)
            {
                ++i;
                var commentEnumerator = line.EnumerateComments();
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
                            else if (Validator.IsVarChar(c))
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
                    isComplete = true;
                else if (c == '=')
                {
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