using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Calcpad.web.Areas.Identity.IdentityHostingStartup))]
namespace Calcpad.web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}