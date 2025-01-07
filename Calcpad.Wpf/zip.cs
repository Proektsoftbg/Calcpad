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

        private const string codeFileName = "code.cpd";
        internal static void CompressWithImages(string text, string[] images, string fileName)
        {
            using FileStream zipStream = new(fileName, FileMode.Create);
            using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);
            ZipArchiveEntry textEntry = archive.CreateEntry(Path.GetFileName(codeFileName), CompressionLevel.Fastest);
            using (Stream entryStream = textEntry.Open())
            {
                //Crypto.EncryptString(text, entryStream);         //Use this if need higher security
                Compress(text, entryStream);                       //Use this if you need smaller files
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
                            ZipArchiveEntry imageEntry = archive.CreateEntry(imageFileName, CompressionLevel.Fastest);
                            using Stream entryStream = imageEntry.Open();
                            using FileStream fileStream = File.OpenRead(imageFilePath);
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }
                regexString = @"src\s*=\s*""\s*\./";
                if (string.Equals(sourceParent, sourcePath, StringComparison.OrdinalIgnoreCase))
                    return;
                sourceParent = sourcePath;
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
            var signature = "PK"u8; // Signature for ZIP files
            using FileStream fileStream = File.OpenRead(fileName);
            byte[] fileSignature = new byte[2];
            fileStream.ReadExactly(fileSignature, 0, 2);
            return signature.SequenceEqual(fileSignature);
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
                        if (string.Equals(entry.Name, codeFileName, StringComparison.Ordinal))
                        {
                            using Stream entryStream = entry.Open();
                            //text = Crypto.DecryptString(entryStream);     //Use this if need higher security
                            text = DecompressToString(entryStream);         //Use this if you need smaller files
                        }
                        else
                            entry.ExtractToFile(entryPath, true);
                    }
                }
            }
            return text;
        }
    }
}