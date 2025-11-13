using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Calcpad.WebApi.Utils.Web.Swagger
{
    /// <summary>
    /// 将 summary 内容移动到 description 中
    /// 将 tag 值作为 summary
    /// </summary>
    public class DotNETSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || context == null)
                return;

            // 若已有 summary（来自 XML 注释），先保留到 description 中
            if (!string.IsNullOrWhiteSpace(operation.Summary))
            {
                if (string.IsNullOrWhiteSpace(operation.Description))
                {
                    operation.Description = operation.Summary;
                }
            }

            // 使用方法名作为 summary（例如：GetUser、CreateItem）
            var methodInfo = context.MethodInfo;
            if (methodInfo != null)
            {
                operation.Summary = methodInfo.Name;
            }
        }
    }
}
