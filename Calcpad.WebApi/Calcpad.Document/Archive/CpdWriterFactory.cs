using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.Document.Archive
{
    public class CpdWriterFactory
    {
        private static Type _cpdWriterType = typeof(CpdWriter);
        private static CpdWriterSettings _cpdWriterSettings = CpdWriterSettings.Default;

        /// <summary>
        /// create cpd writer instance
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static CpdWriter CreateCpdWriter(CpdWriterSettings? settings = null)
        {
            return (CpdWriter)Activator.CreateInstance(_cpdWriterType, settings ?? _cpdWriterSettings)!;
        }

        /// <summary>
        /// set cpd writer type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void SetCpdWriter<T>() where T : CpdWriter
        {
            _cpdWriterType = typeof(T);
        }

        /// <summary>
        /// set default cpd writer settings
        /// </summary>
        /// <param name="settings"></param>
        public static void SetCpdWriterSettings(CpdWriterSettings settings)
        {
            _cpdWriterSettings = settings;
        }
    }
}
