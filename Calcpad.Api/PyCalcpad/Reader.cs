using Calcpad.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Calcpad
{
    internal static class Reader
    {
        private static readonly StringBuilder _stringBuilder = new();   
        internal static string Read(string fileName)
        {
            var inputLines = ReadLines(fileName);
            var outputLines = new List<string>();
            var hasForm = false;
            foreach (var line in inputLines)
            {
                ReadOnlySpan<char> s;
                if (line.Contains('\v'))
                {
                    hasForm = true;
                    var n = line.IndexOf('\v');
                    if (n == 0)
                    {
                        SetInputFieldsFromFile(line[1..].EnumerateSplits('\t'), outputLines);
                        break;
                    }
                    else
                    {
                        SetInputFieldsFromFile(line[(n + 1)..].EnumerateSplits('\t'), outputLines);
                        s = line[..n];
                    }
                }
                else
                {
                    s = ReplaceCStyleRelationalOperators(line.TrimStart('\t'));
                    if (!hasForm)
                        hasForm = MacroParser.HasInputFields(s);
                }
                outputLines.Add(s.ToString());
            }
            return string.Join(Environment.NewLine, outputLines);
        }

        private static SpanLineEnumerator ReadLines(string fileName)
        {
            var lines = new SpanLineEnumerator();
            if (Path.GetExtension(fileName).Equals(".cpdz", StringComparison.OrdinalIgnoreCase))
            {
                var f = new FileInfo(fileName)
                {
                    IsReadOnly = false
                };
                using var fs = f.OpenRead();
                lines = Zip.Decompress(fs);
            }
            else
            {
                return File.ReadAllText(fileName).EnumerateLines();
            }
            return lines;
        }

        private static string ReplaceCStyleRelationalOperators(ReadOnlySpan<char> s)
        {
            if (s.IsEmpty)
                return string.Empty;

            _stringBuilder.Clear();
            var commentEnumerator = s.EnumerateComments();
            foreach (var item in commentEnumerator)
            {
                if (!item.IsEmpty && item[0] != '"' && item[0] != '\'')
                {
                    foreach (var c in item)
                    {
                        if (c == '=')
                        {
                            var n = _stringBuilder.Length - 1;
                            if (n < 0)
                            {
                                _stringBuilder.Append(c);
                                break;
                            }
                            switch (_stringBuilder[n])
                            {
                                case '=':
                                    _stringBuilder[n] = '≡';
                                    break;
                                case '!':
                                    _stringBuilder[n] = '≠';
                                    break;
                                case '>':
                                    _stringBuilder[n] = '≥';
                                    break;
                                case '<':
                                    _stringBuilder[n] = '≤';
                                    break;
                                default:
                                    _stringBuilder.Append(c);
                                    break;
                            }
                        }
                        else if (c == '%')
                        {
                            var n = _stringBuilder.Length - 1;
                            if (n >= 0 && _stringBuilder[n] == '%')
                                _stringBuilder[n] = '⦼';
                            else
                                _stringBuilder.Append(c);
                        }
                        else
                            _stringBuilder.Append(c);
                    }
                }
                else
                    _stringBuilder.Append(item);
            }
            return _stringBuilder.ToString();
        }

        private static void SetInputFieldsFromFile(SplitEnumerator fields, List<string> lines)
        {
            if (fields.IsEmpty)
                return;

            _stringBuilder.Clear();
            var values = new Queue<string>();
            foreach (var s in fields)
                values.Enqueue(s.ToString());

            for (int i = 0, n = lines.Count; i < n; ++i)
            {
                if (MacroParser.SetLineInputFields(lines[i], _stringBuilder, values, false))
                    lines[i] = _stringBuilder.ToString();

                _stringBuilder.Clear();
                if (values.Count == 0)
                    return;  
            }
        }

        internal static string Include(string fileName, Queue<string> fields)
        {
            var isLocal = false;
            var s = File.ReadAllText(fileName);
            var j = s.IndexOf('\v');
            var hasForm = j > 0;
            var lines = (hasForm ? s[..j] : s).EnumerateLines();
            var getLines = new List<string>();
            var sf = hasForm ? s[(j + 1)..] : default;
            Queue<string> getFields = GetFields(sf, fields);
            foreach (var line in lines)
            {
                if (Validator.IsKeyword(line, "#local"))
                    isLocal = true;
                else if (Validator.IsKeyword(line, "#global"))
                    isLocal = false;
                else
                {
                    if (!isLocal)
                    {
                        if (Validator.IsKeyword(line, "#include"))
                        {
                            var includeFileName = GetModuleName(line);
                            getLines.Add(fields is null
                                ? Include(includeFileName, null)
                                : Include(includeFileName, new()));
                        }
                        else
                            getLines.Add(line.ToString());

                    }
                }
            }
            if (hasForm && string.IsNullOrWhiteSpace(getLines[^1]))
                getLines.RemoveAt(getLines.Count - 1);

            var len = getLines.Count;
            if (len > 0)
            {
                _stringBuilder.Clear();
                for (int i = 0; i < len; ++i)
                {
                    if (getFields is not null && getFields.Count != 0)
                    {
                        if (MacroParser.SetLineInputFields(getLines[i].TrimEnd(), _stringBuilder, getFields, false))
                            getLines[i] = _stringBuilder.ToString();

                        _stringBuilder.Clear();
                    }
                }
            }
            return string.Join(Environment.NewLine, getLines);
        }

        private static Queue<string> GetFields(ReadOnlySpan<char> s, Queue<string> fields)
        {
            if (fields is null)
                return null;

            if (fields.Count != 0)
            {
                if (!s.IsEmpty)
                {
                    var getFields = MacroParser.GetFields(s, '\t');
                    if (fields.Count < getFields.Count)
                    {
                        for (int i = 0; i < fields.Count; ++i)
                            getFields.Dequeue();

                        while (getFields.Count != 0)
                            fields.Enqueue(getFields.Dequeue());
                    }
                }
                return fields;
            }
            else if (!s.IsEmpty)
                return MacroParser.GetFields(s, '\t');
            else
                return null;
        }
        private static string GetModuleName(ReadOnlySpan<char> s)
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

        private const string ErrorString = "#Error";
        internal static string CodeToHtml(string code)
        {
            const string spaces = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";    
            var errors = new Queue<int>();
            _stringBuilder.Clear();
            var lines = code.EnumerateLines();
            _stringBuilder.AppendLine("<pre class=\"code\">");
            var lineNumber = 0;
            foreach (var line in lines)
            {
                ++lineNumber;
                var i = line.IndexOf('\v');
                var lineText = i < 0 ? line : line[..i];
                var sourceLine = i < 0 ? lineNumber.ToString() : line[(i + 1)..];
                var lineNumText = lineNumber.ToString(CultureInfo.InvariantCulture);
                var n = lineNumText.Length;
                _stringBuilder.Append($"<p class=\"line-text\" id=\"line-{lineNumber}><span title=\"Source line {sourceLine}\">{spaces[(6*n)..]}{lineNumber}</span>&emsp;│&emsp;");
                if (line.StartsWith(ErrorString))
                {
                    errors.Enqueue(lineNumber);
                    _stringBuilder.Append($"<span class=\"err\">{lineText[1..]}</span>");
                }
                else
                {
                    _stringBuilder.Append(lineText);
                }
                _stringBuilder.Append("</p>");
            }
            _stringBuilder.Append("</pre>");
            if (errors.Count != 0 && lineNumber > 30)
            {
                _stringBuilder.AppendLine($"<div class=\"errorHeader\">Found <b>{errors.Count}</b> errors in modules and macros:");
                var count = 0;
                while (errors.Count != 0 && ++count < 20)
                {
                    var line = errors.Dequeue();
                    _stringBuilder.Append($" <span class=\"roundBox\" data-line=\"{line}\">{line}</span>");
                }
                if (errors.Count > 0)
                    _stringBuilder.Append(" ...");

                _stringBuilder.Append("</div>");
                _stringBuilder.AppendLine("<style>body {padding-top:0.5em;} p {margin:0; line-height:1.15em;}</style>");
            }
            else
                _stringBuilder.AppendLine("<style>p {margin:0; line-height:1.15em;}</style>");
            return _stringBuilder.ToString();
        }
    }
}
