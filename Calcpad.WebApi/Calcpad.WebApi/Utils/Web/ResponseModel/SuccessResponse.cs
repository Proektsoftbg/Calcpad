using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.WebApi.Utils.Web.ResponseModel
{
    /// <summary>
    /// 响应成功
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SuccessResponse<T> : ResponseResult<T>
    {
        public SuccessResponse()
        {
            Code = (int)HttpStatusCode.OK;
        }

        public SuccessResponse(T data):this()
        {
            Data = data;
        }
    }
}
