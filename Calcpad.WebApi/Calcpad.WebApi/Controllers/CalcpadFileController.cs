using Calcpad.Document;
using Calcpad.Document.Archive;
using Calcpad.Document.Core.Segments;
using Calcpad.WebApi.Configs;
using Calcpad.WebApi.Controllers.Base;
using Calcpad.WebApi.Controllers.DTOs;
using Calcpad.WebApi.Models;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Services.Calcpad;
using Calcpad.WebApi.Services.Token;
using Calcpad.WebApi.Utils.Web.ResponseModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Calcpad.WebApi.Controllers
{
    /// <summary>
    /// Calcpad file upload and manage
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tokenService"></param>
    /// <param name="storageConfig"></param>
    /// <param name="appConfig"></param>
    /// <param name="storageService"></param>
    /// <param name="contentService"></param>
    public class CalcpadFileController(
        MongoDBContext db,
        TokenService tokenService,
        AppSettings<StorageConfig> storageConfig,
        AppSettings<AppConfig> appConfig,
        CpdStorageService storageService,
        CpdContentService contentService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// get cpd file resource uri by uniqueId
        /// if file is cpdFile, return formated string for #include
        /// others, return
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns>file in server</returns>
        [HttpGet("uids/{uniqueId}/uri")]
        public async Task<ResponseResult<string>> GetCpdFileResourceUri(string uniqueId)
        {
            var fileObject = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .FirstOrDefaultAsync();
            if (fileObject == null)
            {
                return string.Empty.ToSuccessResponse();
            }

            return contentService.GetFileAccessUri(fileObject).ToSuccessResponse();
        }

        /// <summary>
        /// upload a calcpad file and return the file id
        /// if the file content is changed, you must upload it again with same fileName
        /// if the file contains read or include from relative path, you must invoke <seealso cref="UpdateFilePathsToServicePath"/> to update paths
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ResponseResult<string>> UploadCalcpadCodeFile(
            [FromForm] UploadCalcpadRequestData formData
        )
        {
            if (!formData.IsValidExtensions())
            {
                return string.Empty.ToFailResponse("invalid file extension");
            }

            // verify exist
            var existObject = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == formData.UniqueId)
                .FirstOrDefaultAsync();
            if (existObject == null)
            {
                var fileName = formData.GetUniqueFileName();
                // example: calcpad-files/2024/06/27/xxxxxx.calcpad
                // or: public//2024/06/27/xxxxxx.excel
                var relativePath = storageService.GetCpdObjectName(fileName);
                existObject = new CalcpadFileModel
                {
                    ObjectName = relativePath,
                    FullName = $"%{storageConfig.Value.Environment}%/{relativePath}",
                    FileName = formData.GetFileName(),
                    UniqueId = formData.UniqueId,
                    UserId = tokenService.GetTokenInfo().UserId,
                    IsCpd = formData.IsCpdFile,
                };
                await db.Collection<CalcpadFileModel>().InsertOneAsync(existObject);
            }
            else
            {
                // updaet is public
                if (existObject.IsCpd != formData.IsCpdFile)
                    await db.AsFluentUpdate<CalcpadFileModel>()
                        .Where(x => x.Id == existObject.Id)
                        .Set(x => x.IsCpd, formData.IsCpdFile)
                        .Set(x => x.LastUpdateDate, DateTime.UtcNow)
                        .Set(x => x.FileName, formData.GetFileName())
                        .UpdateOneAsync();
            }

            // save to template path
            var fullPath = Environment.ExpandEnvironmentVariables(existObject.FullName);
            if (formData.IsCpdFile)
            {
                var tempPath = Path.Combine(
                    Path.GetDirectoryName(fullPath)!,
                    $"{Path.GetFileNameWithoutExtension(fullPath)}_temp{Path.GetExtension(fullPath)}"
                );
                formData.Save(tempPath);

                // change cpd include to service path
                var includeUniqueIds = await contentService.SetMeataInfoAndResolvePath(
                    tempPath,
                    existObject.UniqueId
                );
                // replace the original file
                System.IO.File.Move(tempPath, fullPath, true);
                await db.AsFluentUpdate<CalcpadFileModel>()
                    .Where(x => x.Id == existObject.Id)
                    .Set(x => x.IncludeUniqueIds, includeUniqueIds)
                    .UpdateOneAsync();
            }
            else
            {
                formData.Save(fullPath);
            }

            // if is public, return the public url
            return contentService.GetFileAccessUri(existObject).ToSuccessResponse();
        }

        /// <summary>
        /// copy cpd file from fromId to toId
        /// </summary>
        /// <param name="fromId"></param>
        /// <param name="toId"></param>
        /// <returns></returns>
        [HttpPost("copy")]
        public async Task<ResponseResult<bool>> CopyCalcpadCodeFile(string fromId, string toId)
        {
            var existModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == fromId)
                .FirstOrDefaultAsync();
            if (existModel == null)
            {
                return false.ToFailResponse("source file not exists");
            }

            // copy file
            var newFileName = $"{toId}/{Path.GetFileName(existModel.ObjectName)}";
            var toObjectName = storageService.GetCpdObjectName(newFileName);
            var toFullPath = storageService.GetCpdAbsoluteFullName(toObjectName);
            var fromFullPath = storageService.GetCpdAbsoluteFullName(existModel.ObjectName);

            System.IO.File.Copy(fromFullPath, toFullPath, true);

            // copy model
            var newModel = new CalcpadFileModel()
            {
                UniqueId = toId,
                ObjectName = toObjectName,
                FullName = $"%{storageConfig.Value.Environment}%/{toObjectName}",
                FileName = existModel.FileName,
                IncludeUniqueIds = existModel.IncludeUniqueIds,
                LastUpdateDate = existModel.LastUpdateDate,
                IsCpd = existModel.IsCpd,
                SourceId = existModel.Id,
            };
            await db.Collection<CalcpadFileModel>().InsertOneAsync(newModel);

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// update calcpad file macro read from path
        /// user can update the read from macro to change the data source
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="formData"></param>
        /// <returns>new read from path</returns>
        [HttpPut("{uniqueId}/macros/read/from")]
        public async Task<ResponseResult<string>> UpdateCalcpadFileMacroRead(
            [FromRoute] string uniqueId,
            [FromForm] UpdateCpdReadFromData formData
        )
        {
            var cpdFile = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .FirstOrDefaultAsync();
            if (cpdFile == null)
                return string.Empty.ToFailResponse("calcpad file not found");

            // save file
            var newFromPath = storageService.GetReadFromPath(
                cpdFile.FullName,
                formData.File.FileName
            );
            using (var stream = new FileStream(newFromPath, FileMode.Create))
            {
                formData.File.CopyTo(stream);
            }

            if (newFromPath == formData.OldFromPath)
            {
                return newFromPath.ToSuccessResponse();
            }

            // update read from path
            var cpdReader = CpdReaderFactory.CreateCpdReader(cpdFile.FullName);
            var readLines = cpdReader
                .GetReadLines()
                .Where(x => x.FilePath == formData.OldFromPath)
                .ToList();

            if (readLines.Count > 0)
            {
                readLines.ForEach(x => x.SetFilePath(newFromPath));
                var content = CpdWriter.BuildCpdContent(
                    cpdReader.ReadStringLines(),
                    readLines.Cast<CpdLine>()
                );
                var cpdWriter = CpdWriterFactory.CreateCpdWriter();
                cpdWriter.WriteFile(cpdFile.FullName, content);
            }

            // remove old from file
            if (Path.GetDirectoryName(formData.OldFromPath) == Path.GetDirectoryName(newFromPath))
            {
                System.IO.File.Delete(formData.OldFromPath);
            }

            return newFromPath.ToSuccessResponse();
        }

        /// <summary>
        /// Delete calcpad file
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="force">if true will delete file</param>
        /// <returns></returns>
        [HttpDelete("{uniqueId}")]
        public async Task<ResponseResult<bool>> DeleteCalcpadFile(
            string uniqueId,
            bool force = false
        )
        {
            var existModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .FirstOrDefaultAsync();
            if (existModel == null)
            {
                return true.ToSuccessResponse();
            }

            var fullPath = storageService.GetCpdAbsoluteFullName(existModel.ObjectName);
            if (force)
            {
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);

                // remove from database
                await db.Collection<CalcpadFileModel>().DeleteOneAsync(x => x.Id == existModel.Id);
            }
            else
            {
                await db.AsFluentUpdate<CalcpadFileModel>()
                    .Where(x => x.Id == existModel.Id)
                    .Set(x => x.Status, CpdFileStatus.Deleted)
                    .UpdateOneAsync();
            }

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// get stream of public file by fileId(_id)
        /// not support cpd file
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet("stream/public/ids/{fileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFileStreamById(string fileId)
        {
            // validate objectId
            if (!ObjectId.TryParse(fileId, out var fileObjectId))
            {
                return NotFound();
            }

            // query file model
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.Id == fileObjectId && !x.IsCpd)
                .FirstOrDefaultAsync();
            if (fileModel == null)
            {
                return NotFound();
            }

            return await GetFileStream(fileModel);
        }

        /// <summary>
        /// get stream of public file by uniqueId
        /// not support cpd file
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        [HttpGet("stream/public/uids/{uniqueId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFileStreamByUniqueId(string uniqueId)
        {
            // query file model
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId && !x.IsCpd)
                .FirstOrDefaultAsync();
            if (fileModel == null)
            {
                return NotFound();
            }
            return await GetFileStream(fileModel);
        }

        /// <summary>
        /// get file stream from file model
        /// </summary>
        /// <param name="fileModel"></param>
        /// <returns></returns>
        private async Task<IActionResult> GetFileStream(CalcpadFileModel fileModel)
        {
            string fullPath = Environment.ExpandEnvironmentVariables(fileModel.FullName);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            // 根据扩展名推断 content-type，无法识别时回退到 application/octet-stream
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(stream, contentType);
        }

        #region Calculation

        /// <summary>
        /// compile to input form
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="simplify">simplify output html, default true</param>
        /// <returns></returns>
        [HttpGet("input-form")]
        public async Task<ResponseResult<string>> CompileToInputForm(
            string uniqueId,
            bool simplify = true
        )
        {
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId && x.IsCpd == true)
                .FirstOrDefaultAsync();
            if (fileModel == null)
                return "Not Found".ToFailResponse("calcpad file not found");

            var fullPath = Environment.ExpandEnvironmentVariables(fileModel.FullName);
            if (!System.IO.File.Exists(fullPath))
                return "Not Found".ToFailResponse("calcpad file not found");

            var cpdExecutor = new CpdExecutor(fullPath);
            var outputText = await cpdExecutor.CompileToInputForm();

            // replace local link to public path
            outputText = contentService.FormatReadMacroResult(outputText);

            // simplify output html
            // remain row or p element which contains input,select,h1-h6
            if (simplify)
            {
                outputText = contentService.SimplifyHtml(outputText);
            }

            // compile
            return outputText.ToSuccessResponse();
        }

        /// <summary>
        /// run calculations and return the result as html
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("{uniqueId}/calculate-html")]
        public async Task<ResponseResult<string>> RunCalculations(
            string uniqueId,
            [FromBody] RunCalculationData data
        )
        {
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId && x.IsCpd == true)
                .FirstOrDefaultAsync();
            if (fileModel == null)
                return "Not Found".ToFailResponse("calcpad file not found");

            var fullPath = Environment.ExpandEnvironmentVariables(fileModel.FullName);
            if (!System.IO.File.Exists(fullPath))
                return "Not Found".ToFailResponse("calcpad file not found");

            var cpdExecutor = new CpdExecutor(fullPath);
            var outputText = await cpdExecutor.RunCalculation(data.InputFields);
            // replace local link to public path
            outputText = contentService.FormatReadMacroResult(outputText, false);

            // compile
            return outputText.ToSuccessResponse();
        }

        #endregion

        /// <summary>
        /// download cpd file and includes
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        [HttpGet("{uniqueId}/origin")]
        public async Task<IActionResult> DownloadCpdFile(string uniqueId)
        {
            var (strem, fileName) = await storageService.ZipAndDownloadCpdFile(uniqueId);
            if (strem == null)
            {
                return NotFound("file not found");
            }

            return File(strem, "application/zip", fileName);
        }

        #region Update file path to service path

        /// <summary>
        /// get all file paths in the cpd file
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        [HttpGet("{uniqueId}/file-paths")]
        public async Task<ResponseResult<List<string>>> GetFilePathsInCpd(string uniqueId)
        {
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId && x.IsCpd == true)
                .FirstOrDefaultAsync();
            if (fileModel == null)
                return new List<string>().ToFailResponse("calcpad file not found");

            var cpdReader = CpdReaderFactory.CreateCpdReader(fileModel.FullName);

            List<string> filePaths = [];
            filePaths.AddRange(cpdReader.GetIncludeLines().Select(x => x.FilePath));
            filePaths.AddRange(cpdReader.GetReadLines().Select(x => x.FilePath));
            filePaths.AddRange(cpdReader.GetImageLines().Select(x => x.Src));

            // remove ./ at start
            var paths = filePaths.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            return paths.ToSuccessResponse();
        }

        /// <summary>
        /// update file paths to service path by uniqueId
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        [HttpPut("{uniqueId}/file-paths")]
        public async Task<ResponseResult<bool>> UpdateFilePathsToServicePath(
            string uniqueId,
            [FromBody] List<PathFile> filePaths
        )
        {
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId && x.IsCpd == true)
                .FirstOrDefaultAsync();
            if (fileModel == null)
                return false.ToFailResponse("calcpad file not found");

            // find files by uniqueId
            var pathFileModels = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => filePaths.Select(d => d.UniqueId).Contains(x.UniqueId))
                .ToListAsync();

            var cpdReader = CpdReaderFactory.CreateCpdReader(fileModel.FullName);
            var imageLines = cpdReader.GetImageLines().ToList();
            var includeLines = cpdReader.GetIncludeLines().ToList();
            var readLines = cpdReader.GetReadLines().ToList();

            // build path map, map all possible path to server path
            Dictionary<string, (string, ObjectId)> filePathMap = [];
            foreach (var path in filePaths)
            {
                var model = pathFileModels.FirstOrDefault(x => x.UniqueId == path.UniqueId);
                if (model == null)
                {
                    // copy default file from defaults path
                    var defaultFilePath = storageService.GetDefaultFilePath(
                        Path.GetExtension(path.Path)
                    );
                    if (!System.IO.File.Exists(defaultFilePath))
                        continue;
                    model = new CalcpadFileModel() { FullName = Path.GetFullPath(defaultFilePath) };
                }

                var relativePath = storageService.GetRelativePathToCurrentDir(model.FullName);
                filePathMap.TryAdd(path.Path, (relativePath, model.Id));
            }

            // update image lines
            foreach (var img in imageLines)
            {
                // find src
                if (filePathMap.TryGetValue(img.Src, out var newPath))
                {
                    // 转换成图片的 web url
                    var subPath = $"/api/v1/calcpad-file/stream/public/ids/{newPath.Item2}";
                    img.SetSrc(storageService.GetWebUrl(subPath));
                }
            }

            // update include lines
            foreach (var include in includeLines)
            {
                if (string.IsNullOrEmpty(include.FilePath))
                    continue;

                if (filePathMap.TryGetValue(include.FilePath, out var newPath))
                {
                    include.SetFilePath(newPath.Item1);
                }
            }

            // update read lines
            foreach (var read in readLines)
            {
                if (string.IsNullOrEmpty(read.FilePath))
                    continue;

                if (filePathMap.TryGetValue(read.FilePath, out var newPath))
                {
                    read.SetFilePath(newPath.Item1);
                }
            }

            var allLines = new List<CpdLine>();
            allLines.AddRange(imageLines);
            allLines.AddRange(includeLines);
            allLines.AddRange(readLines);

            var content = CpdWriter.BuildCpdContent(cpdReader.ReadStringLines(), allLines);
            var cpdWriter = CpdWriterFactory.CreateCpdWriter();
            cpdWriter.WriteFile(fileModel.FullName, content);

            return true.ToSuccessResponse();
        }
        #endregion
    }
}
