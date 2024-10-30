using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Calcpad
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

        internal static SpanLineEnumerator Decompress(Stream fs)
        {
            using var ms = new MemoryStream();
            using (var ds = new DeflateStream(fs, CompressionMode.Decompress))
                ds.CopyTo(ms);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            return sr.ReadToEnd().AsSpan().EnumerateLines();
        }
    }
}