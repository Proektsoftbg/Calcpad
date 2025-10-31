using System.Text.RegularExpressions;

namespace Calcpad.Document.Core
{
    public partial class RegexFactory
    {
        [GeneratedRegex(
            @"src\s*=\s*[""']([^""']*)[""']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            "zh-CN"
        )]
        private static partial Regex GeneratedImgSrcRegex();

        /// <summary>
        /// image src regex
        /// </summary>
        public static readonly Regex ImgSrcRegex = GeneratedImgSrcRegex();
    }
}
