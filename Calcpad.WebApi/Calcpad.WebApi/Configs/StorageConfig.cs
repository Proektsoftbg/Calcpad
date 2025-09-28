namespace Calcpad.WebApi.Configs
{
    public class StorageConfig
    {
        public string Environment { get; set; } = "CALCPAD_STORAGEROOT";

        /// <summary>
        /// 存储路径
        /// </summary>
        public string Root { get; set; } = "StoragetRoot";
    }
}
