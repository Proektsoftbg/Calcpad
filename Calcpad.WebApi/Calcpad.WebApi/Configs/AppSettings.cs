using Calcpad.WebApi.Utils.Web.Service;

namespace Calcpad.WebApi.Configs
{
    /// <summary>
    /// Gets options from appsettings.json and registers as transient
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="configuration"></param>
    public class AppSettings<T>(IConfiguration configuration) : ITransientService where T : class, new()
    {
        public T Value { get; set; } = configuration.GetConfig<T>();
    }
}
