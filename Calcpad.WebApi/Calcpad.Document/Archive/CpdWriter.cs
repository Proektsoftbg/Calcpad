using System.IO.Compression;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using Calcpad.Document.Archive;
using Calcpad.Document.Core;
using Calcpad.Document.Core.Segments;

namespace Calcpad.Document
{
    /// <summary>
    /// calcpad document
    /// dot not store state in writer instance because it's sharing instance
    /// </summary>
    /// <param name="settings"></param>
    /// if fileName is relative, combine with rootDir to get full path
    /// if rootDir is null or empty, use current directory
    /// </param>
    public partial class CpdWriter(CpdWriterSettings settings)
    {
        public virtual void WriteFile(string fileName, string content, string rootDir = "")
        {
            var fullPath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(fileName));
            var isZip = Path.GetExtension(fullPath)
                .Equals(".cpdz", StringComparison.OrdinalIgnoreCase);
            if (isZip)
            {
                Compress(content, fullPath, rootDir);
            }
            else
            {
                using var sw = new StreamWriter(fullPath);
                sw.Write(content);
            }
        }

        #region compress cpdz file

        /// <summary>
        /// compress to full path
        /// </summary>
        /// <param name="text"></param>
        private void Compress(string text, string _fullPath, string rootDir)
        {
            using FileStream zipStream = new(_fullPath, FileMode.Create);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);

            var srcs = Zip.GetLocalUniqueSrcs(text);
            var fileNameWithoutEx = Path.GetFileNameWithoutExtension(_fullPath);

            // save src
            rootDir = !string.IsNullOrEmpty(rootDir) ? Path.GetDirectoryName(_fullPath)! : rootDir;

            var srcsDic = new Dictionary<string, string>();
            foreach (var src in srcs)
            {
                // get full path
                // if path is relative, combine with root dir
                var srcPath = Path.GetFullPath(
                    Path.IsPathRooted(src) ? src : Path.Combine(rootDir, src)
                );
                if (!File.Exists(srcPath))
                    continue;

                // add src file to zip under srcs folder
                var entryPath = Path.Combine(
                    $"{fileNameWithoutEx}_resources",
                    Path.GetFileName(srcPath)
                );
                var entry = archive.CreateEntry(entryPath, CompressionLevel.Fastest);
                using Stream entryFileStream = entry.Open();
                using FileStream fileStream = File.OpenRead(srcPath);
                fileStream.CopyTo(entryFileStream);

                // change src path in text
                var newSrcPath = settings.CreateSrcPath(_fullPath, entryPath, srcPath);
                srcsDic.Add(src, newSrcPath);
            }

            // replace src in text
            text = RegexFactory.ImgSrcRegex.Replace(
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

            // save code.cpd
            ZipArchiveEntry textEntry = archive.CreateEntry(
                Path.GetFileName(Zip.ZipCodeFileName),
                CompressionLevel.Fastest
            );
            using Stream entryStream = textEntry.Open();
            //Crypto.EncryptString(text, entryStream); /Use this if need higher security
            Compress(text, entryStream); //Use this if you need smaller files
        }

        private static void Compress(string text, Stream fs)
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(text);
            sw.Flush();
            ms.Position = 0;
            using var ds = new DeflateStream(fs, CompressionMode.Compress);
            ms.CopyTo(ds);
        }
        #endregion

        #region static helpers
        /// <summary>
        /// build cpd content from lines and updating rows
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="updatingRows"></param>
        /// <returns></returns>
        public static string BuildCpdContent(
            IEnumerable<string> lines,
            IEnumerable<CpdLine>? updatingRows = null
        )
        {
            updatingRows ??= [];

            var totalLength = lines.Sum(x => x.Length) + updatingRows.Count() * 20;

            var sb = new StringBuilder(totalLength);

            var updatingRowsQueue = new Queue<CpdLine>(updatingRows.OrderBy(x => x.RowIndex));
            var index = -1;
            CpdLine? currentInclude = null;
            foreach (var line in lines)
            {
                index++;
                if (updatingRowsQueue.Count > 0 && currentInclude == null)
                {
                    currentInclude = updatingRowsQueue.Dequeue();
                }

                if (currentInclude != null && index == currentInclude.RowIndex)
                {
                    sb.AppendLine(currentInclude.ToString());
                    currentInclude = null;
                    continue;
                }

                sb.AppendLine(line);
            }
            return sb.ToString();
        }
        #endregion
    }
}
