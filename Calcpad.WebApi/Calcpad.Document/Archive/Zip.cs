using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Calcpad.Document.Core;

namespace Calcpad.Document.Archive
{
    /// <summary>
    /// Zip compression and decompression
    /// </summary>
    public static partial class Zip
    {
        public static readonly string ZipCodeFileName = "code.cpd";

        /// <summary>
        /// get images from src
        /// </summary>
        /// <param name="contet"></param>
        /// <returns></returns>
        public static string[] GetLocalUniqueSrcs(string contet)
        {
            MatchCollection matches = RegexFactory.ImgSrcRegex.Matches(contet);
            return
            [
                .. matches
                    .Select(x => x.Groups[1].Value)
                    .Where(x => !x.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    .Distinct()
            ];
        }
    }
}
