using HtmlAgilityPack;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Calcpad.web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var apiKey = _configuration.GetSection("SendGrid:key").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("proektsoft.bg@gmail.com", "Calcpad");
            var to = new EmailAddress(email, email);
            var plainTextContent = HtmlToPlainText(message);
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var result = await client.SendEmailAsync(msg);
            _logger.LogInformation($"Email sent to: {email}, with subject: {subject}. Status code returned: {result.StatusCode}.");
        }

        private string HtmlToPlainText(string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.InnerText;
        }
    }
}