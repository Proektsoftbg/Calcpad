using Calcpad.WebApi.Utils.Json;

namespace Calcpad.WebApi.Utils.Web.ResponseModel
{
    /// <summary>
    /// IResult 相关的转换器
    /// </summary>
    public static class ToResponseExtension
    {
        /// <summary>
        /// 封装成请求成功的返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ResponseResult<T> ToSuccessResponse<T>(this T data)
        {
            return new SuccessResponse<T>(data);
        }

        /// <summary>
        /// 封装成请求失败的返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static ResponseResult<T> ToFailResponse<T>(this T data, string errorMessage)
        {
            return new ErrorResponse<T>(errorMessage, data);
        }

        /// <summary>
        /// 根据数据是否为空，封装成请求成功或失败的返回值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static ResponseResult<T> ToSmartResponse<T>(this T data, string errorMessage = "数据不存在")
        {
            if (data == null)
            {
                return new ErrorResponse<T>(errorMessage, data);
            }
            return new SuccessResponse<T>(data);
        }

        /// <summary>
        /// 将 HttpResponseMessage 中的 Content 转换成 ResponseResult
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpResponseMessage"></param>
        /// <returns></returns>
        public static async Task<ResponseResult<T>> ToResponseResult<T>(this HttpResponseMessage httpResponseMessage)
        {
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return new ErrorResponse<T>(httpResponseMessage.ReasonPhrase);
            }

            var content = await httpResponseMessage.Content.ReadAsStringAsync();
            var response = content.JsonTo<ResponseResult<T>>();
            if (response == null)
            {
                return new ErrorResponse<T>("无法解析返回结果");
            }
            return response;
        }
    }
}
