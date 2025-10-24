using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Services.Hosted.Base;

namespace Calcpad.WebApi.Services.Hosted
{
    /// <summary>
    /// copy default files from Data directory to StorageRoot/defaults
    /// </summary>
    public class InitializeDefaultFiles(AppSettings<StorageConfig> storageConfig) : IHostedServiceStartup
    {
        public int Order => 3;

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var sourceFiles = Directory.GetFiles("data/defaults/files");
            foreach(var sourceFile in sourceFiles)
            {
                var targetFilePath = $"{storageConfig.Value.Root}/defaults/files/{Path.GetFileName(sourceFile)}";
                Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath)!);
                // copy to path
                File.Copy(sourceFile, targetFilePath, true);
            }

            return Task.CompletedTask;
        }
    }
}
