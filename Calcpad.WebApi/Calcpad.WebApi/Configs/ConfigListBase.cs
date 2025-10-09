using Calcpad.WebApi.Utils.Web.Service;

namespace Calcpad.WebApi.Configs
{
    public class ConfigListBase<T> : List<T>, ISingletonService where T : class, new()
    {
        public ConfigListBase(IConfiguration configuration)
        {
            IConfigurationHelper.BindConfig(configuration, this);
        }
    }
}
