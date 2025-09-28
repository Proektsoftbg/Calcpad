using Calcpad.WebApi.Utils.Json;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Calcpad.WebApi.Utils.Web.ResponseModel
{
    /// <summary>
    /// 结果基类
    /// </summary>
    public abstract class ResultBase : IActionResult
    {
        public async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "application/json;charset=utf-8";

            await response.WriteAsync(this.ToJson());
        }

        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool Ok { get; set; }

        /// <summary>
        /// 是否不通过
        /// </summary>
        public bool NotOk => !Ok;

        /// <summary>
        /// 附带的消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public virtual object GetData()
        {
            return null;
        }


        /// <summary>
        /// ResultFlag 转换成 bool
        /// </summary>
        /// <param name="resultFlag"></param>
        public static implicit operator bool(ResultBase resultFlag) => resultFlag.Ok;

        /// <summary>
        /// bool 转换成 ResultFlag
        /// </summary>
        /// <param name="value"></param>
        public static explicit operator ResultBase(bool value)
        {
            return new Result<object>(value, string.Empty, string.Empty);
        }
    }
}
