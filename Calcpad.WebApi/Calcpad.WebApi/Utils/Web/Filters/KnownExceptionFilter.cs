using Calcpad.WebApi.Utils.Web.Exceptions;
using Calcpad.WebApi.Utils.Web.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Calcpad.WebApi.Utils.Web.Filters
{
    /// <summary>
    /// 对 KnownException 异常进行处理
    /// </summary>
    public class KnownExceptionFilter : IAsyncExceptionFilter
    {
        private readonly static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// 重写OnExceptionAsync方法，定义自己的处理逻辑
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task OnExceptionAsync(ExceptionContext context)
        {
            // 如果异常没有被处理则进行处理
            if (context.ExceptionHandled == false && context.Exception is KnownException knownException)
            {
                // 定义返回类型
                var result = new ResponseResult<string>
                {
                    Code = knownException.Code,
                    Message = knownException.Message
                };
                context.Result = new ContentResult
                {
                    // 返回状态码设置为200，表示成功
                    StatusCode = StatusCodes.Status200OK,
                    // 设置返回格式
                    ContentType = "application/json;charset=utf-8",
                    Content = JsonSerializer.Serialize(result, _jsonSerializerOptions)
                };

                // 设置为true，表示异常已经被处理了
                context.ExceptionHandled = true;
            }
            
            return Task.CompletedTask;
        }
    }
}
