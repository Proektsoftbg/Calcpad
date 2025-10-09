using Microsoft.AspNetCore.Routing;
using System;
using System.Text.RegularExpressions;

namespace Calcpad.WebApi.Utils.Web.Convention
{
    /// <summary>
    /// 将路由转成破折号连接的小写样式
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object? value)
        {
            if (value == null) return string.Empty;

            // Slugify value
            return Regex.Replace(value.ToString()!,
                                 "([a-z])([A-Z])",
                                 "$1-$2",
                                 RegexOptions.CultureInvariant,
                                 TimeSpan.FromMilliseconds(100)).ToLowerInvariant();
        }
    }
}
