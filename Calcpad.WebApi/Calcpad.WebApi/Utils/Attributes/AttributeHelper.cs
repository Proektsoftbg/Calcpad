using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.WebApi.Utils.Attributes
{
    /// <summary>
    /// Attribute 的扩展
    /// </summary>
    public class AttributeHelper
    {
        /// <summary>
        /// 获取类上指定类型的所有特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetAttributes<T>(Type targetType) where T : Attribute
        {
            return targetType.GetCustomAttributes(typeof(T), true).OfType<T>();
        }

        /// <summary>
        /// 获取类型上一个自定义特性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targe"></param>
        /// <returns></returns>
        public static T? GetAttribute<T>(Type targetType) where T : Attribute
        {
            return GetAttributes<T>(targetType).FirstOrDefault();
        }
    }
}
