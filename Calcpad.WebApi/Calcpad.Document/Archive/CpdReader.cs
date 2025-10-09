using System.IO.Compression;
using System.Text;
using Calcpad.Core;
using Calcpad.Document.Core.Segments;

namespace Calcpad.Document.Archive
{
    /// <summary>
    /// read cpd or cpdz file
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="settings"></param>
    public class CpdReader(string filePath, CpdReaderSettings settings)
    {
        private readonly string _fullPath = Path.GetFullPath(
            Environment.ExpandEnvironmentVariables(filePath)
        );
        private string _content = string.Empty;

        /// <summary>
        /// read all text from cpd file
        /// the result will be cached
        /// </summary>
        /// <returns></returns>
        public string ReadAllText()
        {
            if (!string.IsNullOrEmpty(_content))
                return _content;

            if (
                string.Equals(
                    Path.GetExtension(_fullPath),
                    ".cpdz",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                // check if it is really a composite file
                if (IsComposite(_fullPath))
                    return DecompressWithSrcs();

                var f = new FileInfo(_fullPath) { IsReadOnly = true };
                using var fs = f.OpenRead();
                _content = DecompressToString(fs);
            }
            else
            {
                using var sr = new StreamReader(_fullPath);
                _content = sr.ReadToEnd();
            }
            return _content;
        }

        /// <summary>
        /// decompress cpdz file with srcs
        /// cpdz file is a zip file with code and resources
        /// </summary>
        /// <returns></returns>
        private string DecompressWithSrcs()
        {
            var fileDir = Path.GetDirectoryName(_fullPath)!;
            var resourceTempDir = GetResourcesTempDir();
            Directory.CreateDirectory(resourceTempDir);

            var srcsDic = new Dictionary<string, string>();

            string text = string.Empty;
            using ZipArchive archive = ZipFile.OpenRead(_fullPath);
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.Length == 0)
                {
                    // It's a directory
                    Directory.CreateDirectory(Path.Combine(resourceTempDir, entry.FullName));
                    continue;
                }

                // files
                if (string.Equals(entry.Name, Zip.ZipCodeFileName, StringComparison.Ordinal))
                {
                    using Stream entryStream = entry.Open();
                    //text = Crypto.DecryptString(entryStream);     //Use this if need higher security
                    text = DecompressToString(entryStream); //Use this if you need smaller files
                    continue;
                }

                var srcKey = $"./{entry.FullName}";
                if (srcsDic.ContainsKey(srcKey))
                    continue;

                // other files, extract to temp dir
                string entryPath = Path.Combine(resourceTempDir, entry.FullName);
                Directory.CreateDirectory(Path.GetDirectoryName(entryPath)!);
                entry.ExtractToFile(entryPath, true);

                var srcPath = settings.CreateSrcPath(_fullPath, entry.FullName, entryPath);

                // save path
                srcsDic.Add(srcKey, srcPath);
            }

            // replace src in text
            text = Zip.HtmlImgAnyRegex.Replace(
                text,
                match =>
                {
                    var originalSrc = match.Groups[1].Value;
                    if (srcsDic.TryGetValue(originalSrc, out var value))
                    {
                        return match.Value.Replace(originalSrc, value);
                    }
                    return match.Value;
                }
            );

            // remove empty temp dirs
            if (IsDirectoryEmpty(resourceTempDir))
                Directory.Delete(resourceTempDir, true);

            return text;
        }

        private static bool IsDirectoryEmpty(string path)
        {
            if (!Directory.Exists(path))
                return true;

            var options = new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = false
            };

            using var e = Directory.EnumerateFileSystemEntries(path, "*", options).GetEnumerator();
            try
            {
                return !e.MoveNext(); // 如果没有任何条目则为空目录
            }
            catch (IOException)
            {
                // 遇到 IO 错误时保守处理：认为目录不为空以避免误删，或根据需要改为 true
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // 权限问题也保守处理
                return false;
            }
        }

        /// <summary>
        /// read lines from cpd file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SpanLineEnumerator ReadLines()
        {
            var code = ReadAllText();
            return code.EnumerateLines();
        }

        public List<string> ReadStringLines()
        {
            var lines = new List<string>();
            var spanLines = ReadLines();
            foreach (var line in spanLines)
            {
                lines.Add(line.ToString());
            }
            return lines;
        }

        /// <summary>
        /// read include lines from cpd file
        /// </summary>
        /// <returns></returns>
        public List<IncludeLine> GetIncludeLines()
        {
            var code = ReadAllText();
            if (string.IsNullOrEmpty(code))
                return [];

            var spanLines = ReadLines();

            var includes = new List<IncludeLine>();
            var includeSpan = IncludeLine.IncludeDirective.AsSpan();

            uint index = 0;
            foreach (var line in spanLines)
            {
                var trimmed = line.TrimStart();
                if (
                    !trimmed.IsEmpty
                    && trimmed.Length >= includeSpan.Length
                    && trimmed.StartsWith(includeSpan, StringComparison.OrdinalIgnoreCase)
                )
                {
                    includes.Add(new IncludeLine(index, line.ToString()));
                }

                index++;
            }

            return includes;
        }

        /// <summary>
        /// get read lines from cpd file
        /// </summary>
        /// <returns></returns>
        public List<ReadLine> GetReadLines()
        {
            var code = ReadAllText();
            if (string.IsNullOrEmpty(code))
                return [];

            var spanLines = ReadLines();

            var reads = new List<ReadLine>();
            var readSpan = ReadLine.ReadDirective.AsSpan();

            uint index = 0;
            foreach (var line in spanLines)
            {
                var trimmed = line.TrimStart();
                if (
                    !trimmed.IsEmpty
                    && trimmed.Length >= readSpan.Length
                    && trimmed.StartsWith(readSpan, StringComparison.OrdinalIgnoreCase)
                )
                {
                    reads.Add(new ReadLine(index, line.ToString()));
                }

                index++;
            }

            return reads;
        }

        private string GetResourcesTempDir()
        {
            return Path.Combine(
                Path.GetDirectoryName(_fullPath)!,
                ".temp",
                Path.GetFileNameWithoutExtension(_fullPath)
            );
        }

        /// <summary>
        /// clear temp files
        /// </summary>
        public void ClearTemp()
        {
            var tempDir = GetResourcesTempDir();
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        #region  static helper

        /// <summary>
        /// check if the file is a composite file (cpdz)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsComposite(string fileName)
        {
            var signature = "PK"u8; // Signature for ZIP files
            using FileStream fileStream = File.OpenRead(fileName);
            byte[] fileSignature = new byte[2];
            fileStream.ReadExactly(fileSignature, 0, 2);
            return signature.SequenceEqual(fileSignature);
        }

        /// <summary>
        /// decompress stream to string
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static string DecompressToString(Stream fs)
        {
            using var ms = new MemoryStream();
            using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                ds.CopyTo(ms);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }
        #endregion
    }
}
