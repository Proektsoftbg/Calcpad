using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Calcpad.web.Services
{
    public interface IReCaptchaService
    {
        public string GetClientHtml();
        public Task<bool> ValidateCaptchaAsync(string gRecaptchaResponse);
    }

    public class ReCapthaService : IReCaptchaService
    {
        private readonly IConfiguration _configuration;

        private class ResponseData
        {
            public bool Success { get; set; }
            public DateTime Challenge_ts { get; set; }
            public string Hostname { get; set; }
        }

        public ReCapthaService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetClientHtml()
        {
            string key = _configuration.GetSection("GoogleReCaptcha:key").Value;
            return $"<div class=\"g-recaptcha\" data-sitekey=\"{key}\"></div>";
        }

        public async Task<bool> ValidateCaptchaAsync(string gRecaptchaResponse)
        {
            var result = false;
            string secretKey = _configuration.GetSection("GoogleReCaptcha:secret").Value;
            var apiUrl = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";
            var requestUri = string.Format(apiUrl, secretKey, gRecaptchaResponse);
            var request = (HttpWebRequest)WebRequest.Create(requestUri);

            using (WebResponse response = await request.GetResponseAsync())
            {
                using StreamReader stream = new(response.GetResponseStream());
                JObject jResponse = JObject.Parse(await stream.ReadToEndAsync());
                var isSuccess = jResponse.Value<bool>("success");
                result = isSuccess;
            }
            return result;
        }
    }
}
