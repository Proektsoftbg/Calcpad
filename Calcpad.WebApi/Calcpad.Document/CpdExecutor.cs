using System.Globalization;
using System.Text;
using Calcpad.Core;
using Calcpad.Document.Archive;
using Calcpad.Document.Include;

namespace Calcpad.Document
{
    public class CpdExecutor(
        string fullName,
        Settings? settings = null,
        IIncludeResolver? includeResolver = null
    )
    {
        #region private fields
        private readonly Settings _settings = settings ?? new Settings();
        #endregion


        #region Parsers
        private readonly ExpressionParser _parser = new();
        private readonly MacroParser _macroParser =
            new() { Include = (includeResolver ?? new LocalFileIncludeResolver()).Include };
        #endregion

        /// <summary>
        /// compile file to input form
        /// </summary>
        /// <returns></returns>
        public async Task<string> CompileToInputForm(string sourceCode = "", bool calculate = false)
        {
            if (string.IsNullOrEmpty(sourceCode))
            {
                var cpdReader = CpdReaderFactory.CreateCpdReader(fullName);
                sourceCode = cpdReader.ReadAllText();
            }
            if (string.IsNullOrEmpty(sourceCode))
                return string.Empty;

            // use settings
            _parser.Settings = _settings;
            var sourceCodeTemp = await ParseMacros(sourceCode);

            var timeoutTask = Task.Run(async () =>
            {
#if DEBUG
                // ignore timeout in debug
#else
                await Task.Delay(10 * 1000);
                _parser.Cancel();
#endif
            });
            var parseTask = Task.Run(() =>
            {
                _parser.Parse(sourceCodeTemp, calculate);
            });
            Task.WaitAny([timeoutTask, parseTask]);

            return _parser.HtmlResult;
        }

        private async Task<string> ParseMacros(string sourceCode)
        {
            return await Task.Run(() =>
            {
                var hasErrors = _macroParser.Parse(sourceCode, out var outputText, null, 0, true);
                if (hasErrors)
                {
                    return sourceCode;
                }
                return outputText;
            });
        }

        /// <summary>
        /// 执行计算
        /// </summary>
        /// <param name="inputFields"></param>
        /// <returns></returns>
        public async Task<string> RunCalculation(string[] inputFields)
        {
            // update input fields
            var inputText = SetInputFields(inputFields);
            // override file
            var cpdWriter = CpdWriterFactory.CreateCpdWriter();
            cpdWriter.WriteFile(fullName, inputText);

            return await CompileToInputForm(inputText, true);
        }

        /// <summary>
        /// read cpd file and update fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private string SetInputFields(string[] fields)
        {
            var cpdReader = CpdReaderFactory.CreateCpdReader(fullName);
            var allTexts = cpdReader.ReadAllText();

            if (
                fields is null
                || fields.Length == 0
                || fields.Length == 1 && string.IsNullOrEmpty(fields[0])
            )
                return allTexts;

            if (!ValidateInputFields(fields, out _))
                return allTexts;

            // 解析成 (fline, value) 列表，fline == 0 表示未指定行（按顺序应用）
            var pairs = new List<(int fline, string value)>(fields.Length);
            foreach (var f in fields)
            {
                if (string.IsNullOrEmpty(f))
                {
                    pairs.Add((0, string.Empty));
                    continue;
                }

                var s = f.AsSpan();
                var j = s.IndexOf(':');
                if (j >= 0 && int.TryParse(s[..j], out var parsed))
                    pairs.Add((parsed, s[(j + 1)..].ToString().Trim()));
                else
                    pairs.Add((0, s.ToString().Trim()));
            }

            // 将 SpanLineEnumerator 收集为行字符串列表（便于逐行索引与跳转）
            var lines = cpdReader.ReadStringLines();

            var sbAll = new StringBuilder(allTexts.Length + fields.Length * 10);
            var sbLine = new StringBuilder();
            var fieldIndex = 0; // index into pairs
            var line = 0; // 1-based

            while (line < lines.Count && fieldIndex < pairs.Count)
            {
                ++line;
                var values = new Queue<string>();
                var jumpFline = 0;

                // 收集适用于当前行的值；遇到指定未来行则停止并记下行号用于跳转
                while (fieldIndex < pairs.Count)
                {
                    var (fline, val) = pairs[fieldIndex];
                    if (fline > line)
                    {
                        jumpFline = fline;
                        break;
                    }
                    values.Enqueue(val);
                    ++fieldIndex;
                }

                var currentText = lines[line - 1];
                if (values.Count != 0)
                {
                    if (MacroParser.SetLineInputFields(currentText.TrimEnd(), sbLine, values, true))
                    {
                        currentText = sbLine.ToString();
                    }
                    sbLine.Clear();
                }

                sbAll.AppendLine(currentText);

                if (jumpFline > line)
                {
                    // 跳过到目标行之前（目标行将在下一循环中处理）
                    if (jumpFline - 1 > lines.Count)
                    {
                        // 如果目标行超出范围，则把剩余行全部追加并结束
                        for (var k = line; k < lines.Count; ++k)
                            sbAll.AppendLine(lines[k]);
                        line = lines.Count;
                        break;
                    }
                    else
                    {
                        for (var k = line; k < jumpFline - 1 && k < lines.Count; ++k)
                            sbAll.AppendLine(lines[k]);
                        line = jumpFline - 1;
                    }
                }
            }

            // 把剩余未处理的原始行追加上
            if (line < lines.Count)
            {
                for (var k = line; k < lines.Count; ++k)
                    sbAll.AppendLine(lines[k]);
            }

            return sbAll.ToString();
        }

        /// <summary>
        /// validate input fields
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        private static bool ValidateInputFields(string[] fields, out int errorLine)
        {
            errorLine = -1;
            if (fields is null || fields.Length == 0)
                return true;

            for (int i = 0, len = fields.Length; i < len; ++i)
            {
                var s = fields[i].AsSpan();
                if (s.Length > 0)
                {
                    var j = s.IndexOf(':');
                    if (j > 0)
                        s = s[(j + 1)..];
                }
                if (
                    s.Length == 0
                    || s[0] == '+'
                    || !double.TryParse(
                        s,
                        NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                        CultureInfo.InvariantCulture,
                        out var _
                    )
                )
                {
                    errorLine = i;
                    return false;
                }
            }
            return true;
        }
    }
}
