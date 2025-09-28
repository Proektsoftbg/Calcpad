namespace Calcpad.WebApi.Services.Hosted.Base
{
    /// <summary>
    /// Will execute all IHostedServiceStartup services in order when the application starts
    /// </summary>
    /// <param name="ssf"></param>
    public class HostServicesManager(IServiceScopeFactory ssf) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = ssf.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var postServices = serviceProvider.GetServices<IHostedServiceStartup>()
                .OrderBy(x => x.Order);
            foreach (var postService in postServices)
            {
                await postService.ExecuteAsync(stoppingToken);
            }
        }
    }
}
