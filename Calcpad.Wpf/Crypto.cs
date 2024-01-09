using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Calcpad.Wpf
{
    internal static class Crypto
    {
        private const string _key = "Proekt$0FTbg1234"; //Set your own key here (16 bytes)
        public static void EncryptString(string s, Stream stream)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_key);
            aesAlg.IV = new byte[16]; // Initialization Vector (IV)
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using CryptoStream cs = new(stream, encryptor, CryptoStreamMode.Write);
            using StreamWriter sw = new(cs);
            sw.Write(s);
        }

        public static string DecryptString(Stream stream)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.UTF8.GetBytes(_key);
            aesAlg.IV = new byte[16]; // Initialization Vector (IV)

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using CryptoStream cs = new(stream, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new(cs);
            return sr.ReadToEnd();
        }
    }
}
