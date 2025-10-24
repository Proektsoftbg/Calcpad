using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.Document.Core.Utils
{
    public class FileExtensionsList
    {
        /// <summary>
        /// List of Calcpad document file extensions.
        /// </summary>
        public static readonly string[] CpdFileExtensions = [".cpd", ".cpdz", ".txt"];

        /// <summary>
        /// List of image file extensions.
        /// </summary>
        public static readonly string[] ImageFileExtensions =
        [
            ".png",
            ".jpg",
            ".jpeg",
            ".webp",
            ".svg",
            ".gif"
        ];

        /// <summary>
        /// List of CSV file extensions.
        /// </summary>
        public static readonly string[] CsvFileExtensions = [".csv"];

        /// <summary>
        /// List of Excel file extensions.
        /// </summary>
        public static readonly string[] ExcelFileExtensions = [".xls", ".xlsx", ".xlsm"];
    }
}
