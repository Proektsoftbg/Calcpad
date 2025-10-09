using System.Text.RegularExpressions;

namespace Calcpad.WebApi.Utils.Attributes
{
    /// <summary>
    /// 用于标记集合名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionNameAttribute(string name) : Attribute
    {
        public string Name { get; set; } = name;

        /// <summary>
        /// 获取集合名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ResolveCollectionName(Type type)
        {
            var att = AttributeHelper.GetAttribute<CollectionNameAttribute>(type);
            if (att == null)
            {
                // 将末尾的 Model 去掉
                string modelName = Regex.Replace(type.Name, "Model$", "");
                return modelName;
            }
            return att.Name;
        }
    }
}
