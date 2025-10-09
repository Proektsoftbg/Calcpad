using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.WebApi.Utils.Web.ResponseModel
{
    /// <summary>
    /// 响应失败
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ErrorResponse<T>: ResponseResult<T>
    {
        public ErrorResponse()
        {
            Code = (int)HttpStatusCode.InternalServerError;
            Ok = false;
        }

        /// <summary>
        /// 返回错误
        /// </summary>
        /// <param name="message"></param>
        public ErrorResponse(string message):this()
        {
            Message = message;
        }

        public ErrorResponse(string message,T data):this(message)
        {
            Data = data;
        }
    }
}
