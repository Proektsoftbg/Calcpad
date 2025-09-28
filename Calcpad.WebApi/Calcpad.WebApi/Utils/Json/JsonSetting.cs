using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Calcpad.WebApi.Utils.Json
{
    public class JsonSetting : JsonSerializerSettings
    {
        public JsonSetting WithCameCase()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver();
            return this;
        }

        public JsonSetting WithSnakeCase()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            return this;
        }

        public JsonSetting WithPascalCase()
        {
            return this;
        }

        private bool _isStringEnumConverterAdded = false;
        /// <summary>
        /// 枚举转为字符串
        /// 可以使用 EnumMemberAttribute 来指定字符串
        /// </summary>
        /// <returns></returns>
        public JsonSetting WithStringEnumConverter()
        {
            if (_isStringEnumConverterAdded) return this;

            _isStringEnumConverterAdded = true;
            Converters.Add(new StringEnumConverter());
            return this;
        }
    }
}
