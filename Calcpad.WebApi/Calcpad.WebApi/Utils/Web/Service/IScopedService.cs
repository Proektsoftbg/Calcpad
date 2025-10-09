using System;
using System.Collections.Generic;
using System.Text;

namespace Calcpad.WebApi.Utils.Web.Service
{
    /// <summary>
    /// 生命周期为请求周期内的服务
    /// 服务注册为本身
    /// </summary>
    public interface IScopedService : IService
    {
    }


    /// <summary>
    /// 服务注册为本身和指定类型T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IScopedService<T> : IScopedService
    {

    }
}
