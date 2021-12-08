using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Calcpad.Wpf
{
    internal class Zip
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

        internal static List<string> Decompress(Stream fs)
        {
            var lines = new List<string>();
            using var ms = new MemoryStream();
            using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                ds.CopyTo(ms);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            while (!sr.EndOfStream)
            {
                var s = sr.ReadLine().TrimStart('\t');
                lines.Add(s);
            }
            return lines;
        }
    }
}