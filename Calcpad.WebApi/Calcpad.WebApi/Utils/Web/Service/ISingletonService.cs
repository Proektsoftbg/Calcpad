using System;
using System.Collections.Generic;
using System.Text;

namespace Calcpad.WebApi.Utils.Web.Service
{
    /// <summary>
    /// 标记为单例
    /// </summary>
    public interface ISingletonService : IService
    {
    }

    /// <summary>
    /// 标记为单例，并将其注册为 T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISingletonService<T> : ISingletonService
    {
    }
}
