using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Calcpad.Document.Core.Segments
{
    public partial class ImageLine : CpdLine
    {
        public static readonly string ImageDirective = "<img ";

        public string Src { get; private set; } = string.Empty;

        private readonly string _originalLine = string.Empty;

        public ImageLine(uint row, string line)
            : base(row)
        {
            if (!IsImageLine(line, out var trimedLine))
                return;

            _originalLine = trimedLine;
            var match = RegexFactory.ImgSrcRegex.Match(trimedLine);
            if (match.Success && match.Groups.Count > 1)
            {
                Src = match.Groups[1].Value;
            }
        }

        public void SetSrc(string src)
        {
            Src = src;
        }

        public override string ToString()
        {
            // Replace the src value in the original line
            return RegexFactory.ImgSrcRegex.Replace(_originalLine, $"src=\"{Src}\"");
        }

        /// <summary>
        /// check if the line is an image line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="trimedLine"></param>
        /// <returns></returns>
        public static bool IsImageLine(string line, out string trimedLine)
        {
            trimedLine = line?.Trim() ?? string.Empty;
            return trimedLine.IndexOf(ImageDirective) > 0;
        }
    }
}
