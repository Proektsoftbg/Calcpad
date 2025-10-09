using Calcpad.WebApi.Utils.Web.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace Calcpad.WebApi.Utils.Web.Filters
{
    public class TokenExpiredFilter : IAsyncExceptionFilter
    {
        private readonly static JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Task OnExceptionAsync(ExceptionContext context)
        {
            // 如果异常没有被处理则进行处理
            if (context.ExceptionHandled == false && context.Exception is SecurityTokenExpiredException expiredTokenException)
            {
                // 定义返回类型
                var result = new ResponseResult<string>
                {
                    Code = StatusCodes.Status401Unauthorized,
                    Message = expiredTokenException.Message
                };
                context.Result = new ContentResult
                {
                    // 返回状态码设置为200，表示成功
                    StatusCode = StatusCodes.Status401Unauthorized,
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
