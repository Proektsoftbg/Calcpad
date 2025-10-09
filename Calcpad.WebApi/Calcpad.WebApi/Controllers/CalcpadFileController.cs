using Calcpad.Document;
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
    /// <param name="storagetService"></param>
    public class CalcpadFileController(
        MongoDBContext db,
        TokenService tokenService,
        AppSettings<StorageConfig> storageConfig,
        CpdStorageService storagetService,
        CpdContentService contentService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// verify if a file with the given sha256 exists
        /// if exists, return the file id, otherwise return empty string
        /// if the file exists, also verify if the filePath exists for the user, if not, add a new accessor
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns>file in server</returns>
        [HttpGet("code/{uniqueId}/exists")]
        public async Task<ResponseResult<string>> PresignedObject(string uniqueId)
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
                var relativePath = storagetService.GetCpdObjectName(fileName);

                existObject = new CalcpadFileModel
                {
                    ObjectName = relativePath,
                    FullName = $"%{storageConfig.Value.Environment}%/{relativePath}",
                    FileName = formData.GetFileName(),
                    UniqueId = formData.UniqueId,
                    UserId = tokenService.GetTokenInfo().UserId,
                    IsPublic = formData.IsPublic,
                };
                await db.Collection<CalcpadFileModel>().InsertOneAsync(existObject);
            }
            else
            {
                // updaet is public
                if (existObject.IsPublic != formData.IsPublic)
                    await db.AsFluentUpdate<CalcpadFileModel>()
                        .Where(x => x.Id == existObject.Id)
                        .Set(x => x.IsPublic, formData.IsPublic)
                        .Set(x => x.LastUpdateDate, DateTime.UtcNow)
                        .Set(x => x.FileName, formData.GetFileName())
                        .UpdateOneAsync();
            }

            // save to template path
            var fullPath = Environment.ExpandEnvironmentVariables(existObject.FullName);
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
            var newFileName = $"{toId}{Path.GetExtension(existModel.ObjectName)}";
            var toObjectName = storagetService.GetCpdObjectName(newFileName);
            var toFullPath = storagetService.GetCpdAbsoluteFullName(toObjectName);
            var fromFullPath = storagetService.GetCpdAbsoluteFullName(existModel.ObjectName);

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
                SourceId = existModel.Id,
            };
            await db.Collection<CalcpadFileModel>().InsertOneAsync(newModel);

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// Delete calcpad file
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        [HttpDelete("{uniqueId}")]
        public async Task<bool> DeleteCalcpadFile(string uniqueId)
        {
            var existModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .FirstOrDefaultAsync();
            if (existModel == null)
            {
                return true.ToSuccessResponse();
            }

            var fullPath = storagetService.GetCpdAbsoluteFullName(existModel.ObjectName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet("stream/public/{fileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFileStream(string fileId)
        {
            // validate objectId
            if(!ObjectId.TryParse(fileId,out var fileObjectId))
            {
                return NotFound();
            }

            // 获取文件流
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.Id == fileObjectId)
                .FirstOrDefaultAsync();
            if (fileModel == null)
            {
                return NotFound();
            }

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

        /// <summary>
        /// compile to input form
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        [HttpGet("input-form")]
        public async Task<ResponseResult<string>> CompileToInputForm(string uniqueId, bool simplify = true)
        {
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId && x.IsPublic == false)
                .FirstOrDefaultAsync();
            if (fileModel == null)
                return "Not Found".ToFailResponse("calcpad file not found");

            var fullPath = Environment.ExpandEnvironmentVariables(fileModel.FullName);
            if (!System.IO.File.Exists(fullPath))
                return "Not Found".ToFailResponse("calcpad file not found");

            var cpdExecutor = new CpdExecutor(fullPath);
            var outputText = await cpdExecutor.CompileToInputForm();

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
        /// <returns></returns>
        [HttpPost("{uniqueId}/calculate-html")]
        public async Task<ResponseResult<string>> RunCalculations(
            string uniqueId,
            [FromBody] RunCalculationData data
        )
        {
            var fileModel = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId && x.IsPublic == false)
                .FirstOrDefaultAsync();
            if (fileModel == null)
                return "Not Found".ToFailResponse("calcpad file not found");

            var fullPath = Environment.ExpandEnvironmentVariables(fileModel.FullName);
            if (!System.IO.File.Exists(fullPath))
                return "Not Found".ToFailResponse("calcpad file not found");

            var cpdExecutor = new CpdExecutor(fullPath);
            var outputText = await cpdExecutor.RunCalculation(data.InputFields);

            // compile
            return outputText.ToSuccessResponse();
        }

        /// <summary>
        /// download cpd file and includes
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        [HttpGet("{uniqueId}/origin")]
        public async Task<IActionResult> DownloadCpdFile(string uniqueId)
        {
            var (strem, fileName) = await storagetService.ZipAndDownloadCpdFile(uniqueId);
            if (strem == null)
            {
                return NotFound("file not found");
            }

            return File(strem, "application/zip", fileName);
        }
    }
}
