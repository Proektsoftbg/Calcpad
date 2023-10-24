using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Calcpad.Wpf
{
    internal static class Zip
    {
        internal static void Compress(string text, Stream fs)
        {
            using var ms = new MemoryStream();
            using var sw = new StreamWriter(ms);
            sw.Write(text);
            sw.Flush();
            ms.Position = 0;
            using var ds = new DeflateStream(fs, CompressionMode.Compress);
            ms.CopyTo(ds);
        }

        internal static void CompressWithImages(string text, string[] images, string fileName)
        {
            using (FileStream zipStream = new FileStream(fileName, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    var textFile = Path.GetTempFileName();
                    File.WriteAllText(textFile, Crypto.EncryptString(text));
                    ZipArchiveEntry textEntry = archive.CreateEntry(Path.GetFileName(textFile));
                    using (Stream entryStream = textEntry.Open())
                    {
                        using (var fileStream = File.OpenRead(textFile))
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                    if (images is null)
                        return;
                    
                    var sourcePath = Path.GetDirectoryName(fileName);
                    var sourceParent = Directory.GetDirectoryRoot(sourcePath);
                    if (!string.Equals(sourceParent, sourcePath, StringComparison.OrdinalIgnoreCase))
                        sourceParent = Directory.GetParent(sourcePath).FullName;

                    var regexString = @"src\s*=\s*""\s*\.\./";
                    for (int i = 0; i < 2; ++i)
                    {
                        foreach (var image in images)
                        {
                            var m = Regex.Match(image, regexString, RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                var n = m.Length;
                                var imageFileName = image[n..^1].Replace('/', '\\');
                                var imageFilePath = Path.Combine(sourceParent, imageFileName);
                                if (File.Exists(imageFilePath))
                                {
                                    ZipArchiveEntry imageEntry = archive.CreateEntry(imageFileName);
                                    using (Stream entryStream = imageEntry.Open())
                                    {
                                        using (FileStream fileStream = File.OpenRead(imageFilePath))
                                        {
                                            fileStream.CopyTo(entryStream);
                                        }
                                    }
                                }
                            }
                        }
                        regexString = @"src\s*=\s*""\s*\./";
                        if (string.Equals(sourceParent, sourcePath, StringComparison.OrdinalIgnoreCase))
                            return;
                        sourceParent = sourcePath;
                    }
                }
            }
        }

        internal static SpanLineEnumerator Decompress(Stream fs) =>
            DecompressToString(fs).AsSpan().EnumerateLines();

        internal static string DecompressToString(Stream fs)
        {
            using var ms = new MemoryStream();
            using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                ds.CopyTo(ms);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        internal static bool IsComposite(string fileName)
        {
            byte[] signature = new byte[] { 0x50, 0x4B }; // Signature for ZIP files

            using (FileStream fileStream = File.OpenRead(fileName))
            {
                byte[] fileSignature = new byte[2];
                fileStream.Read(fileSignature, 0, 2);
                return fileSignature[0] == signature[0] && fileSignature[1] == signature[1];
            }
        }

        internal static string DecompressWithImages(string fileName)
        {
            var filePath = Path.GetDirectoryName(fileName);
            string text = string.Empty;
            using (ZipArchive archive = ZipFile.OpenRead(fileName))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string entryPath = Path.Combine(filePath, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(entryPath));
                    if (entry.Length == 0) // It's a directory
                        Directory.CreateDirectory(entryPath);
                    else // It's a file
                    {
                        entry.ExtractToFile(entryPath, true);
                        if (string.Equals(Path.GetExtension(entry.Name), ".tmp", StringComparison.OrdinalIgnoreCase))
                        {
                            text = Crypto.DecryptString(File.ReadAllText(entryPath));
                            File.Delete(entryPath);
                        }
                    }
                }
            }
            return text;
        }
    }
}