using System;
using System.IO;
using System.Reflection;
namespace Calcpad
{
    internal class Program
    {
        private static readonly string _currentCultureName = "en";
        internal static readonly string AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string AddCultureExt(string ext) => string.Equals(_currentCultureName, "en", StringComparison.Ordinal) ?
            $".{ext}" :
            $".{_currentCultureName}.{ext}";
    }
}