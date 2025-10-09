using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.Document.Archive
{
    public class CpdReaderFactory
    {
        private static Type _cpdReaderType = typeof(CpdReader);
        private static CpdReaderSettings _cpdReaderSettings = CpdReaderSettings.Default;

        /// <summary>
        /// create cpd reader instance
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static CpdReader CreateCpdReader(string fullPath, CpdReaderSettings? settings = null)
        {
            return (CpdReader)Activator.CreateInstance(_cpdReaderType, fullPath, settings ?? _cpdReaderSettings)!;
        }

        /// <summary>
        /// set cpd reader type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetCpdReader<T>() where T : CpdReader
        {
            _cpdReaderType = typeof(T);
        }

        /// <summary>
        /// set default cpd writer settings
        /// </summary>
        /// <param name="settings"></param>
        public static void SetCpdReaderSettings(CpdReaderSettings settings)
        {
            _cpdReaderSettings = settings;
        }
    }
}
