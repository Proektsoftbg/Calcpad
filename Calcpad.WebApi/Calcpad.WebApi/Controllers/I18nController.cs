using Calcpad.Document;
using Calcpad.WebApi.Controllers.Base;
using Calcpad.WebApi.Controllers.DTOs;
using Calcpad.WebApi.Models;
using Calcpad.WebApi.Models.Base;
using Calcpad.WebApi.Services.AI;
using Calcpad.WebApi.Utils.Web.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Calcpad.WebApi.Controllers
{
    public class I18nController(
        MongoDBContext db,
        CpdI18nContentService i18NService,
        AIService aIService
    ) : ControllerBaseV1
    {
        /// <summary>
        /// Generates internationalization (i18n) language resources for a specified Calcpad file using the provided
        /// language code.
        /// </summary>
        /// <remarks>The operation requires that the AI service is enabled and that the specified Calcpad
        /// file exists. If the file is not found or the AI service is disabled, the response will indicate failure with
        /// an appropriate error message.</remarks>
        /// <param name="uniqueId">The unique identifier of the Calcpad file for which to generate i18n language resources.</param>
        /// <param name="lang">The language code representing the target language for the generated resources. For example, "en" for
        /// English or "fr" for French.</param>
        /// <returns>A response result containing <see langword="true"/> if the i18n language resources were generated
        /// successfully or no keys required translation; otherwise, <see langword="false"/> if the operation failed.
        /// The response includes error information if applicable.</returns>
        [HttpPost("{uniqueId}/translations/{lang}")]
        public async Task<ResponseResult<bool>> GenerateI18nLangs(string uniqueId, string lang)
        {
            if (!aIService.IsEnabled)
            {
                return false.ToFailResponse("AI Service is not enabled");
            }

            var cpdFile = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .Where(x => x.IsCpd == true)
                .FirstOrDefaultAsync();

            if (cpdFile == null)
            {
                return false.ToFailResponse("Calcpad file not found");
            }

            var fullPath = Environment.ExpandEnvironmentVariables(cpdFile.FullName);
            if (!System.IO.File.Exists(fullPath))
                return false.ToFailResponse("Calcpad file not found");

            var keys = await i18NService.GetCpdFileI18nKeys(fullPath);
            if (keys.Count == 0)
            {
                return true.ToSuccessResponse();
            }

            // get existing i18n docs
            var existingTrans = await db.AsQueryable<CalcpadLangModel>()
                .Where(x => x.UniqueId == uniqueId && x.Lang == lang && x.IsByAI == true)
                .ToListAsync();
            // remove existing
            var newKeys = keys.Except(existingTrans.Select(x => x.Key)).ToList();
            if (newKeys.Count == 0)
            {
                return true.ToSuccessResponse();
            }

            // generate translations
            var translations = await aIService.TranslateToLang(newKeys, lang);

            // update parallelly
            var newLangs = translations.Select(x =>
            {
                return new CalcpadLangModel
                {
                    UniqueId = uniqueId,
                    Lang = lang,
                    Key = x.Item1,
                    Value = x.Item2,
                    Version = cpdFile.Version,
                    IsByAI = true,
                };
            });
            await db.InserManyAsync(newLangs);

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// Translates the most recent calculation result of the specified Calcpad file to the given language.
        /// </summary>
        /// <remarks>If the specified Calcpad file does not exist or has no calculation result, the
        /// response will indicate the error. When the language is set to "default", the original calculation result is
        /// returned without translation.</remarks>
        /// <param name="uniqueId">The unique identifier of the Calcpad file whose calculation result is to be translated.</param>
        /// <param name="lang">The target language code for translation. Use "default" to return the original result without translation.</param>
        /// <returns>A response containing the translated HTML content of the last calculation result. If the file is not found
        /// or no calculation result exists, the response contains an error message.</returns>
        [HttpPut("{uniqueId}/translate-result-to/{lang}")]
        public async Task<ResponseResult<string>> TranslateResultToLang(
            string uniqueId,
            string lang
        )
        {
            var cpdFile = await db.AsQueryable<CalcpadFileModel>()
                .Where(x => x.UniqueId == uniqueId)
                .Where(x => x.IsCpd == true)
                .FirstOrDefaultAsync();

            if (cpdFile == null)
            {
                return "".ToFailResponse("Calcpad file not found");
            }

            var lastHtml = cpdFile.LastCalculationHtml;
            if (string.IsNullOrEmpty(lastHtml))
            {
                return "".ToFailResponse(
                    "No calculation result found, Please run calculation first"
                );
            }

            if (lang == "default")
                return lastHtml.ToSuccessResponse();

            // get tranlated keys
            var translatedContent = await i18NService.TranslateHtmlContentToLang(
                cpdFile.UniqueId,
                lastHtml,
                lang
            );

            return translatedContent.ToSuccessResponse();
        }

        /// <summary>
        /// Retrieves all available language models associated with the specified unique Calcpad file identifier,
        /// including those from its source file if applicable.
        /// </summary>
        /// <param name="uniqueId">The unique identifier of the Calcpad file for which to retrieve language models. Cannot be null or empty.</param>
        /// <returns>A response result containing a list of language models associated with the specified Calcpad file and its
        /// source file. The list is empty if no matching file is found.</returns>
        [HttpGet("{uniqueId}/langs")]
        public async Task<ResponseResult<List<CalcpadLangModel>>> GetAllLangsByUniqueId(
            string uniqueId
        )
        {
            var (self, source) = await i18NService.GetSelfAndSourceCpdFile(uniqueId);
            if (self == null)
                return new List<CalcpadLangModel>().ToFailResponse("Calcpad file not found");

            var uniqueIds = new List<string> { self.UniqueId };
            if (source != null)
                uniqueIds.Add(source.UniqueId);
            var langs = await db.AsQueryable<CalcpadLangModel>()
                .Where(x => uniqueIds.Contains(x.UniqueId))
                .OrderBy(x => x.Id)
                .ToListAsync();
            return langs.ToSuccessResponse();
        }

        /// <summary>
        /// Updates the value of the specified language entry.
        /// </summary>
        /// <param name="langId">The unique identifier of the language entry to update.</param>
        /// <param name="langValue">The new value to assign to the language entry. Cannot be null.</param>
        /// <returns>A response result indicating whether the update operation was successful. The value is <see
        /// langword="true"/> if the update succeeded; otherwise, <see langword="false"/>.</returns>
        [HttpPut("langs/{langId}")]
        public async Task<ResponseResult<bool>> UpdateLangValue(
            string langId,
            [FromBody] UpdateLangValueData data
        )
        {
            if (!ObjectId.TryParse(langId, out var langObjId))
            {
                return false.ToFailResponse("Invalid language ID");
            }

            await db.AsFluentMongo<CalcpadLangModel>()
                .Where(x => x.Id == langObjId)
                .Set(x => x.Value, data.LangValue)
                .Set(x => x.LastModifyDate, DateTime.UtcNow)
                .UpdateOneAsync();
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// Deletes the language document identified by the specified language ID.
        /// It will also delete the corresponding language document from the source file if applicable with the same key.
        /// </summary>
        /// <param name="langId">The unique identifier of the language document to delete.</param>
        /// <returns>A response result containing <see langword="true"/> if the language document was successfully deleted;
        /// otherwise, <see langword="false"/> if the document was not found.</returns>
        [HttpDelete("langs/{langId}")]
        public async Task<ResponseResult<bool>> DeleteLangById(string langId)
        {
            if (!ObjectId.TryParse(langId, out var langObjId))
            {
                return false.ToFailResponse("Invalid language ID");
            }

            var langDoc = await db.AsQueryable<CalcpadLangModel>()
                .Where(x => x.Id == langObjId)
                .FirstOrDefaultAsync();

            if (langDoc == null)
            {
                return false.ToFailResponse("Language document not found");
            }

            var (selfCpd, sourceCpd) = await i18NService.GetSelfAndSourceCpdFile(langDoc.UniqueId);
            var cpdUids = new List<string> { langDoc.UniqueId };
            if (sourceCpd != null)
            {
                cpdUids.Add(sourceCpd.UniqueId);
            }

            await db.AsFluentMongo<CalcpadLangModel>()
                .Where(x => cpdUids.Contains(x.UniqueId) && x.Key == langDoc.Key)
                .DeleteOneAsync();
            return true.ToSuccessResponse();
        }
    }
}
