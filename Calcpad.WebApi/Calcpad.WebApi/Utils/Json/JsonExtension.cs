using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Calcpad.WebApi.Utils.Json
{
    public static class JsonExtension
    {
        public readonly static JsonSerializerSettings CameCaseJsonSettings = new JsonSetting().WithCameCase();

        /// <summary>
        /// object 对象转换成 json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj, CameCaseJsonSettings);
        }

        /// <summary>
        /// 转换成 JToken
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static JToken ToJToken<T>(this T obj)
        {
            return JToken.FromObject(obj, JsonSerializer.Create(CameCaseJsonSettings));
        }

        /// <summary>
        /// 将 json 转换成指定类型的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T? JsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json, CameCaseJsonSettings);
        }

        /// <summary>
        /// 将 jToken 转换为指定类型的值，如果转换失败则返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jt"></param>
        /// <param name="default_"></param>
        /// <returns></returns>
        public static T ToObjectOrDefault<T>(this JToken jt, T default_)
        {
            if (jt == null) return default_;

            T? value = jt.ToObject<T>();
            if (value == null) return default_;

            return value;
        }

        /// <summary>
        /// 按路径从 Json 中读取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jt"></param>
        /// <param name="path"></param>
        /// <param name="default_"></param>
        /// <returns></returns>
        public static T SelectTokenOrDefault<T>(this JToken jt, string path, T default_)
        {
            if (string.IsNullOrEmpty(path)) return default_;
            if (jt == null) return default_;

            var value = jt.SelectToken(path);
            if (value == null) return default_;
            return value.ToObjectOrDefault(default_);
        }

        /// <summary>
        /// 转换成 query string
        /// </summary>
        /// <param name="jobj"></param>
        /// <returns></returns>
        public static string ToQueryString(this JObject jobj)
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in jobj)
            {
                var value = item.Value?.ToString();
                value ??= string.Empty;
                if (value == "True" || value == "False") value = value.ToLower();

                stringBuilder.Append($"{item.Key}={value}&");
            }
            return stringBuilder.ToString().TrimEnd('&');
        }
    }
}
