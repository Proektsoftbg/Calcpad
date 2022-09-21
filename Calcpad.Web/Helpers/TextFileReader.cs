using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace Calcpad.web.Helpers
{
    public static class TextFileReader
    {
        public static string ReadText(IFormFile file)
        {
            using Stream stream = file.OpenReadStream();
            byte[] buffer = new byte[stream.Length];
            for (int i = 0; i < stream.Length; i++)
                buffer[i] = (byte)stream.ReadByte();

            string s = Encoding.UTF8.GetString(buffer);
            foreach (char c in s)
            {
                if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t' && c != '\v')
                    return "\'The file contains non-text characters. Only UTF-8 encoded text files are allowed.";
            }
            return s;
        }
    }
}
