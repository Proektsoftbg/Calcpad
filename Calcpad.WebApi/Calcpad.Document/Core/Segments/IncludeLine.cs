using System.Text;

namespace Calcpad.Document.Core.Segments
{
    /// <summary>
    /// Formatted line object that include another file
    /// </summary>
    /// <param name="line"></param>
    public class IncludeLine : CpdRow
    {
        public static readonly string IncludeDirective = "#include";

        public string? FilePath { get; private set; }

        // query 参数
        public Dictionary<string, string> Queries { get; private set; } = [];

        // 默认值
        public string DefaultPart { get; private set; } = string.Empty;

        #region Getters
        public bool IsValid => !string.IsNullOrEmpty(FilePath);

        /// <summary>
        /// full path with environment variables expanded
        /// </summary>
        public string FullPath =>
            string.IsNullOrEmpty(FilePath)
                ? string.Empty
                : Path.GetFullPath(Environment.ExpandEnvironmentVariables(FilePath));

        public string Uid => Queries.TryGetValue("uid", out var uid) ? uid : string.Empty;
        #endregion

        /// <summary>
        /// constructor
        /// example:
        /// 1. #include file.calc #{12}
        /// 2. #include file.calc'?param1=val1&param2=val2' #{12}
        /// 3. #include file.calc'?param1=val1&param2=val2'
        /// </summary>
        /// <param name="line"></param>
        public IncludeLine(uint row, string line)
        {
            RowIndex = row;

            if (!IsIncludeLine(line, out var trimedLine))
                return;

            // remove directive
            var purePath = trimedLine[IncludeDirective.Length..].Trim();
            if (string.IsNullOrEmpty(purePath))
                return;

            // 解析默认值（#{...}）
            DefaultPart = string.Empty;
            int defaultStart = purePath.IndexOf("#{");
            if (defaultStart >= 0)
            {
                int defaultEnd = purePath.IndexOf('}', defaultStart + 2);
                if (defaultEnd > defaultStart)
                {
                    DefaultPart = purePath[(defaultStart + 2)..defaultEnd].Trim();
                    purePath = purePath[0..defaultStart].Trim();
                }
            }

            // 解析查询（?）
            string queryPart = string.Empty;
            int queryStart = purePath.IndexOf('?');
            if (queryStart > 0)
            {
                queryPart = purePath[(queryStart + 1)..].Trim();
                purePath = purePath[0..queryStart].Trim('\'', '"', ' ');
            }

            // 设置文件路径
            FilePath = purePath.Trim('\'', '"').Trim();

            // 解析查询参数
            if (!string.IsNullOrEmpty(queryPart))
            {
                var queries = queryPart
                    .Trim('\'', '"', ' ')
                    .Split('&', StringSplitOptions.RemoveEmptyEntries);
                foreach (var query in queries)
                {
                    var qparts = query.Split('=', 2);
                    if (qparts.Length == 2)
                    {
                        var key = qparts[0].Trim();
                        var val = qparts[1].Trim();
                        if (!string.IsNullOrEmpty(key))
                            Queries.Add(key, val);
                    }
                }
            }
        }

        #region public utils
        /// <summary>
        /// add or update query
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddQuery(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            if (!Queries.TryAdd(key, value))
                Queries[key] = value;
        }

        public void AddUid(string uid)
        {
            AddQuery("uid", uid);
        }

        /// <summary>
        /// Set include file path, can be full path or relative path
        /// </summary>
        /// <param name="path"></param>
        public void SetFilePath(string path)
        {
            FilePath = path;
        }

        /// <summary>
        /// Returns a string representation of the object, formatted as an include directive with optional query
        /// parameters and a default part.
        /// </summary>
        /// <remarks>
        /// The returned string includes the file path, followed by query parameters (if any) in
        /// the format `key=value` separated by `&`,  and optionally appends a default part if specified. If the file
        /// path is null or empty, an empty string is returned.
        /// </remarks>
        /// <returns>
        /// A string representing the include directive, including the file path, query parameters, and default part if applicable
        /// </returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(FilePath))
                return string.Empty;

            var sb = new StringBuilder();
            sb.Append("#include ");
            sb.Append(FilePath);

            if (Queries.Count > 0)
            {
                sb.Append("\'?");
                sb.Append(string.Join('&', Queries.Select(kv => $"{kv.Key}={kv.Value}")));
            }

            if (!string.IsNullOrEmpty(DefaultPart))
            {
                sb.Append(' ');
                sb.Append(DefaultPart);
            }

            return sb.ToString();
        }
        #endregion

        #region static helper
        public static bool IsIncludeLine(string? line, out string trimedLine)
        {
            trimedLine = line?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(line))
                return false;

            if (trimedLine.Length < IncludeDirective.Length)
                return false;

            return trimedLine.StartsWith(IncludeDirective, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="row"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static IncludeLine? GetIncludeLine(uint row, string? line)
        {
            if (!IsIncludeLine(line, out _))
                return null;

            return new IncludeLine(row, line!);
        }
        #endregion
    }
}
