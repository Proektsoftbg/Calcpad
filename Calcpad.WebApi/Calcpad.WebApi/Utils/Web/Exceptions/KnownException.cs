namespace Calcpad.WebApi.Utils.Web.Exceptions
{
    /// <summary>
    /// 已知的异常
    /// 通常在编程中主动抛出，用于控制流程
    /// </summary>
    public class KnownException(string message) : Exception(message)
    {
        public int Code { get; set; } = StatusCodes.Status500InternalServerError;
    }
}
