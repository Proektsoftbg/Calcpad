using System.Text.RegularExpressions;

namespace Calcpad.WebApi.Configs
{
    public static class IConfigurationHelper
    {
        /// <summary>
        /// 通过配置获取配置项
        /// 要求配置项的名称和配置项的类型名称一致，若类型有 Config 后缀，配置中不应有 Config 后缀
        /// </summary>
        /// <typeparam name="TConfig"></typeparam>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static TConfig GetConfig<TConfig>(this IConfiguration configuration) where TConfig : class
        {
            // 实例化配置项
            var config = Activator.CreateInstance<TConfig>();
            BindConfig(configuration, config);
            return config;
        }


        public static void BindConfig(IConfiguration configuration, object configInstance)
        {
            var section = configuration.GetSection(GetConfigSectionName(configInstance.GetType()));
            if (section.Exists())
            {
                section.Bind(configInstance);
            }
        }

        private static string GetConfigSectionName(Type configType)
        {
            var regex = new Regex("Config$", RegexOptions.IgnoreCase);
            return regex.Replace(configType.Name, "");
        }
    }
}
