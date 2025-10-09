using Calcpad.Document;
using Calcpad.Document.Archive;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Utils.Encrypt;
using Calcpad.WebApi.Utils.Web.Service;

namespace Calcpad.WebApi.Utils.Calcpad
{
    /// <summary>
    /// cpd writer for web api
    /// feature:
    /// save src as static file
    /// </summary>
    /// <param name="appConfig"></param>
    /// <param name="storageConfig"></param>
    public class WebCpdWriterSettings(AppSettings<AppConfig> appConfig, AppSettings<StorageConfig> storageConfig)
        : CpdWriterSettings,
            ISingletonService
    {
        private readonly string _baseUrl = appConfig.Value.BaseUrl;

        /// <summary>
        /// override to save src as static file
        /// </summary>
        /// <param name="zipFilePath"></param>
        /// <param name="zipSrcEntryPath"></param>
        /// <param name="zipSrcLocalPath"></param>
        /// <returns></returns>
        public override string CreateSrcPath(
            string zipFilePath,
            string zipSrcEntryPath,
            string zipSrcLocalPath
        )
        {
            var publicPath = Path.Combine(
                "public/cpd-resources",
                zipFilePath.ToMD5(),
                Path.GetFileName(zipSrcEntryPath)
            );
            // save src file to public
            var localPublicPath = Path.Combine(storageConfig.Value.Root, publicPath);
            Directory.CreateDirectory(Path.GetDirectoryName(localPublicPath)!);
            // copy file
            File.Copy(zipSrcLocalPath, localPublicPath, true);

            // return web path
            return $"{_baseUrl}/{publicPath.Replace(Path.DirectorySeparatorChar, '/')}";
        }
    }
}
