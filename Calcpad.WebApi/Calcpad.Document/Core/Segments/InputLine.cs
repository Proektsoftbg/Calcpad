using System;
using System.Collections.Generic;
using System.Text;
using Calcpad.Core;
using Calcpad.Document.Core.Segments.Input;

namespace Calcpad.Document.Core.Segments
{
    public class InputLine : CpdLine
    {
        // 搜索 #include 指令
        private static readonly System.Buffers.SearchValues<char> s_includeSearchValues =
            System.Buffers.SearchValues.Create("#include");

        private readonly List<StringSegment> _lineSegments = [];
        public List<InputField> Fields { get; private set; } = [];

        public bool ContainsInputs => Fields.Count > 0;

        public InputLine(uint rowIndex, string line)
            : base(rowIndex)
        {
            // resolve input fields from the line
            if (string.IsNullOrEmpty(line))
                return;

            ExtractLineInputFields(line);
        }

        #region 提取变量值
        /// <summary>
        /// extract input fields from a line
        /// </summary>
        /// <param name="s"></param>
        /// <param name="values"></param>
        private void ExtractLineInputFields(ReadOnlySpan<char> s)
        {
            Fields.Clear();

            if (s.Length == 0)
                return;

            // handle #include first as #{val1;val2;}
            var trimmed = s.TrimEnd();
            var lastHashIndex = trimmed.LastIndexOf('#');
            var lastLeftBrace = trimmed.LastIndexOf('{');
            var lastRightBrace = trimmed.LastIndexOf('}');
            if (
                lastHashIndex > 0
                && lastHashIndex + 1 == lastLeftBrace
                && lastLeftBrace < lastRightBrace
            )
            {
                var valuesStr = trimmed[(lastLeftBrace + 1)..lastRightBrace];
                var values = valuesStr.ToString().Split(';');
                var name = trimmed[..lastHashIndex].ToString().Trim();
                AddInputField(new InputField(values, name) { Type = InputFieldType.Include });
                return;
            }

            // 处理注释中的 variable = ? {val}
            var commentEnumerator = s.EnumerateComments();
            // 非 include 时处理
            foreach (var item in commentEnumerator)
            {
                bool isInput = false;
                if (!item.IsEmpty && item[0] != '"' && item[0] != '\'')
                {
                    var inputChar = '\0';
                    var braceStart = -1;
                    for (int j = 0; j < item.Length; ++j)
                    {
                        var c = item[j];
                        if (c == '?')
                        {
                            inputChar = c;
                        }
                        else if (c == '{' && inputChar == '?')
                        {
                            inputChar = '{';
                            braceStart = j + 1;
                        }
                        else if (c == '}' && inputChar == '{')
                        {
                            var val = item[braceStart..j].ToString().Trim();
                            var equalIndex = item.IndexOf('=');

                            var name = item[..(equalIndex - 1)].ToString().Trim();
                            AddInputField(
                                new InputField([val], name) { Type = InputFieldType.Variable }
                            );
                            isInput = true;
                            inputChar = '\0';
                            braceStart = -1;
                        }
                    }
                }

                // 添加原始字符
                if (!isInput)
                    _lineSegments.Add(new StringSegment(item.ToString()));
            }
        }

        private void AddInputField(InputField field)
        {
            Fields.Add(field);
            _lineSegments.Add(field);
        }
        #endregion


        public override string ToString()
        {
            return string.Concat(_lineSegments);
        }

        public static bool IsInputLine(ReadOnlySpan<char> line)
        {
            // check for ? {...} pattern
            var index = line.IndexOf('?');
            if (index >= 0 && line[index + 2] == '{')
                return true;

            // check for #include directive
            if (line.Contains(IncludeLine.IncludeDirective, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}
