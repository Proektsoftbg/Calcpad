using Newtonsoft.Json;

namespace Calcpad.WebApi.Utils.Json
{
    /// <summary>
    /// json 模型的基类
    /// 主要提供一些序列化和反序列化的方法
    /// </summary>
    public abstract class BaseJsonModel
    {
        #region Json 相关帮助方法
        private readonly static JsonSerializerSettings _cameCaseSetting = new JsonSetting().WithCameCase().WithStringEnumConverter();
        private readonly static JsonSerializerSettings _snakeCaseSetting = new JsonSetting().WithSnakeCase().WithStringEnumConverter();

        public string ToCamelCaseJson()
        {
            return JsonConvert.SerializeObject(this, _cameCaseSetting);
        }

        public string ToSnakeCaseJson()
        {
            return JsonConvert.SerializeObject(this, _snakeCaseSetting);
        }

        public string ToPascalCaseJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static T? FromCamelCaseJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _cameCaseSetting);
        }

        public static T? FromSnakeCaseJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _snakeCaseSetting);
        }

        public static T? FromPascalCaseJson<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        #endregion
    }
}
