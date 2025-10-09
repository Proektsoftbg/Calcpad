using Calcpad.Document.Archive;
using Calcpad.WebApi.Services.Calcpad;
using Calcpad.WebApi.Services.Hosted.Base;
using Calcpad.WebApi.Utils.Calcpad;

namespace Calcpad.WebApi.Services.Hosted
{
    /// <summary>
    /// change the default cpd writer to web cpd writer
    /// </summary>
    /// <param name="writerSettings"></param>
    /// <param name="readerSettings"></param>
    public class RegisterCpdSettings(WebCpdWriterSettings writerSettings,WebCpdReaderSettings readerSettings) : IHostedServiceStartup
    {
        public int Order => 1;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CpdWriterFactory.SetCpdWriterSettings(writerSettings);
            CpdReaderFactory.SetCpdReaderSettings(readerSettings);
        }
    }
}
