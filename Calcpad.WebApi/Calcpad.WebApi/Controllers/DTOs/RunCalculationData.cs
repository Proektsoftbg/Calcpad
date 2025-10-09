namespace Calcpad.WebApi.Controllers.DTOs
{
    public class RunCalculationData
    {
        public string CpdUid { get; set; }

        /// <summary>
        /// 输入字段
        /// </summary>
        public string[] InputFields { get; set; } = [];
    }
}
