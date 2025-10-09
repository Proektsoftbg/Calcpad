using Calcpad.WebApi.Utils.Web.Service;

namespace Calcpad.WebApi.Services.Hosted.Base
{
    public interface IHostedServiceStartup : IScopedService<IHostedServiceStartup>
    {
        /// <summary>
        /// 优先级
        /// </summary>
        int Order { get; }

        Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
