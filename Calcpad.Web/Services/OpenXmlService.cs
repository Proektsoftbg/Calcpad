using Microsoft.AspNetCore.Mvc;
using Calcpad.OpenXml;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Calcpad.web.Services
{
    public interface IOpenXmlService
    {
        public Task<FileContentResult> GetDocxFileAsync(string html, string fileName, string url);
    }

    public class OpenXmlService : IOpenXmlService
    {
        public async Task<FileContentResult> GetDocxFileAsync(string html, string fileName, string url)
        {
            using Stream stream = new MemoryStream();
            OpenXmlWriter writer = new();
            string result = await Task.Run(() => writer.Convert(html, stream, url));
            if (result.Length == 0)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Position = 0;
                for (int i = 0; i < stream.Length; i++)
                {
                    buffer[i] = (byte)stream.ReadByte();
                }
                return new FileContentResult(buffer, "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    FileDownloadName = fileName
                };
            }
            return new FileContentResult(Encoding.UTF8.GetBytes(result), "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                FileDownloadName = "errors.txt"
            };
        }
    }
}
