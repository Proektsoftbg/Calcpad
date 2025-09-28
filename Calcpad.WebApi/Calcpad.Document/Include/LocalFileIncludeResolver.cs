using System.Text;
using Calcpad.Core;
using Calcpad.Document.Archive;

namespace Calcpad.Document.Include
{
    public class LocalFileIncludeResolver : IIncludeResolver
    {
        private readonly StringBuilder _stringBuilder = new(10000);

        /// <summary>
        /// default include action
        /// features:
        /// 1. support query, e.g. path/test.cpd?id=1
        /// 2. support web cpd
        /// 3. support relative path
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public string Include(string fileName, Queue<string>? fields)
        {
            var isLocal = false;
            var cpdReader = CpdReaderFactory.CreateCpdReader(fileName);
            var s = cpdReader.ReadAllText();
            var j = s.IndexOf('\v');
            var hasForm = j > 0;
            var lines = (hasForm ? s[..j] : s).EnumerateLines();
            var getLines = new List<string>();
            var sf = hasForm ? s[(j + 1)..] : default;
            Queue<string>? getFields = GetFields(sf, fields);
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
                            var includeFileName = GetIncludeFileName(line);
                            var includeFilePath = Path.GetFullPath(
                                Environment.ExpandEnvironmentVariables(includeFileName)
                            );
                            if (!File.Exists(includeFilePath))
                                throw new FileNotFoundException(
                                    $"{Messages.File_not_found}: {includeFileName}."
                                );

                            getLines.Add(Include(includeFilePath, fields == null ? null : new()));
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
                        if (
                            MacroParser.SetLineInputFields(
                                getLines[i].TrimEnd(),
                                _stringBuilder,
                                getFields,
                                false
                            )
                        )
                            getLines[i] = _stringBuilder.ToString();

                        _stringBuilder.Clear();
                    }
                }
            }
            return string.Join(Environment.NewLine, getLines);
        }

        private static Queue<string>? GetFields(ReadOnlySpan<char> s, Queue<string>? fields)
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

        public virtual string GetIncludeFileName(ReadOnlySpan<char> line)
        {
            return UserDefined.GetFileName(line);
        }
    }
}
