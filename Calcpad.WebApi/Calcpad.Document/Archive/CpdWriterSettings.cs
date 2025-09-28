using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.Document.Archive
{
    public class CpdWriterSettings
    {
        /// <summary>
        /// create src path for file in zip
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <param name="zipSrcEntryPath"></param>
        /// <param name="zipSrcLocalPath"></param>
        /// <returns></returns>
        public virtual string CreateSrcPath(
            string zipFilePath,
            string zipSrcEntryPath,
            string zipSrcLocalPath
        )
        {
            return $"./{zipSrcEntryPath}";
        }

        public static readonly CpdWriterSettings Default = new();
    }
}
