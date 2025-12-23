namespace Calcpad.Document.Core.Segments
{
    public enum ReadType
    {
        // For the #read command, TYPE can be any of the following capital letters:
        // rectangular matrix(default)
        R,

        // column matrix
        C,

        // diagonal matrix
        D,

        // symmetric skyline matrix
        S,

        // lower triangular matrix
        L,

        //upper triangular matrix
        U,

        // vector
        V,
    }

    /// <summary>
    /// format:
    /// #read Name from Path.Ext@Sheet!Start:End Type=T
    /// #read M from filename.xlsx@Sheet1!A1:B2 type=[R|C|D|L|U|S]
    /// #read M from filename.xlsx@Sheet1 (when range is missing the whole sheet is read)
    /// #read M from filename.xlsm!A1:B2 (when sheet is missing the first sheet is read)
    /// #read M from filename.xls@Sheet1!A1:B2 (when type is missing R is assumed)
    /// #read M from filename.csv@R1C1:R2C2 type=[R|C|D|L|U|S] sep=,
    /// #read M from filename.txt@R1C1:R2C2 type=[R|C|D|L|U|S] sep=
    ///
    /// #write Name to Path.Ext@Sheet!Start:End Type=T
    /// #write M to filename.xlsx@Sheet1!A1:B2 type=[Y|N]
    /// #write M to filename.xlsm@Sheet1!A1:B2 (when type is missing N is assumed)
    /// #write M to filename.xls@Sheet1!A1:B2
    ///
    /// #write Name to Path.Ext@Sheet!Start:End Type=T sep=S
    /// #write M to filename.csv@R1C1:R2C2 type=[Y|N] sep=,
    /// #write M to filename.txt@R1C1:R2C2 type=[Y|N] sep=
    ///
    /// #append Name to Path.Ext@Sheet!Start:End Type=T sep=S
    /// #append M to filename.csv@R1C1:R2C2 type=[Y|N] sep=,
    /// #append M to filename.txt@R1C1:R2C2 type=[Y|N] sep=
    ///
    /// reference:<see cref="ReadWriteOptions"/>
    /// </summary>
    public class ReadLine : CpdLine
    {
        public static readonly string ReadDirective = "#read";

        /// <summary>
        /// variable name to read into
        /// </summary>
        public string Name { get; private set; } = string.Empty;

        public string FilePath { get; private set; } = string.Empty;

        public string Filter { get; private set; } = string.Empty;

        public Dictionary<string, string> Options { get; private set; } = [];

        public string Uid
        {
            get => Options.TryGetValue("uid", out var uid) ? uid : string.Empty;
        }

        public ReadLine(uint row, string line)
            : base(row)
        {
            if (!IsReadLine(line))
                return;
            var trimedLine = line.Trim();

            // read variable name
            var fromIndex = trimedLine.IndexOf("from", StringComparison.OrdinalIgnoreCase);
            var variableStartIndex = trimedLine.IndexOf(' ');
            Name = trimedLine[(variableStartIndex + 1)..fromIndex].Trim();

            // parse options
            string afterFrom = trimedLine[(fromIndex + 4)..].TrimStart();
            string[] parts = afterFrom.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<string> optionsParts = [.. parts.Where(x => x.Contains('='))];
            foreach (var opt in optionsParts)
            {
                var kv = opt.Split('=', 2);
                if (kv.Length == 2)
                {
                    Options.TryAdd(kv[0].ToLower(), kv[1]);
                }
            }

            var filterEndIndex =
                optionsParts.Count > 0 ? trimedLine.IndexOf(optionsParts[0]) : trimedLine.Length;
            var filePathAndFilter = trimedLine[(fromIndex + 4)..filterEndIndex].Trim();
            var atIndex = filePathAndFilter.IndexOf('@');
            if (atIndex > 0)
            {
                FilePath = filePathAndFilter[..atIndex].Trim();
                Filter = filePathAndFilter[(atIndex + 1)..].Trim();
            }
            else
            {
                FilePath = filePathAndFilter;
                Filter = string.Empty;
            }
        }

        #region public utils
        public void SetFilePath(string path)
        {
            FilePath = path;
        }

        public override string ToString()
        {
            var parts = new List<string>
            {
                ReadDirective,
                Name,
                "from",
                FilePath + (string.IsNullOrEmpty(Filter) ? string.Empty : "@" + Filter)
            };
            foreach (var option in Options)
            {
                parts.Add($"{option.Key}={option.Value}");
            }
            return string.Join(' ', parts);
        }
        #endregion

        #region static helpers
        /// <summary>
        /// check if a line is a read directive line
        /// example:
        /// 1. #read m from file.calc
        /// </summary>
        /// <param name="line"></param>
        /// <param name="trimedLine"></param>
        /// <returns></returns>
        public static bool IsReadLine(ReadOnlySpan<char> line)
        {
            var trimedLine = line.Trim();

            if (trimedLine.IsEmpty)
                return false;

            if (trimedLine.Length < ReadDirective.Length)
                return false;

            return trimedLine.StartsWith(ReadDirective, StringComparison.OrdinalIgnoreCase)
                && trimedLine.IndexOf("from", StringComparison.OrdinalIgnoreCase)
                    > ReadDirective.Length;
        }

        #endregion
    }
}
