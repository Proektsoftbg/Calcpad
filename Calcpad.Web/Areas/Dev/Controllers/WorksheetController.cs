using System.IO;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Calcpad.web.Areas.Dev.Models;
using Calcpad.web.Data.Models;
using Calcpad.web.Data.Services;
using Calcpad.web.Helpers;
using Calcpad.web.Services;
using Calcpad.web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Calcpad.web.Areas.Dev.Controllers
{
    [Area("Dev")]
    [Authorize(Roles = Global.Constants.RoleNames.Developer)]
    public class WorksheetController : Controller
    {
        private readonly IWorksheetService _worksheetService;
        private readonly ICategoryService _categoryService;
        private readonly IParserService _parserService;
        private readonly IMapper _mapper;

        public WorksheetController(IWorksheetService worksheetService, ICategoryService categoryService, IParserService parserService, IMapper mapper)
        {
            _worksheetService = worksheetService;
            _categoryService = categoryService;
            _parserService = parserService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int? Id)
        {
            if (!Id.HasValue)
                return BadRequest();

            Category category = await _categoryService.GetByIdAsync(Id.Value);
            if (category == null)
                return NotFound();

            WorksheetInputModel model = new WorksheetInputModel()
            {
                Category = _mapper.Map<CategoryViewModel>(category),
                CategoryId = category.Id
            };
            return View(nameof(Update), model);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            Worksheet worksheet = await _worksheetService.GetByIdAsync(id.Value);
            if (worksheet == null)
                return NotFound();

            WorksheetInputModel model = _mapper.Map<WorksheetInputModel>(worksheet);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            Worksheet worksheet = await _worksheetService.GetByIdAsync(id.Value);
            if (worksheet == null)
                return NotFound();

            WorksheetInputModel model = _mapper.Map<WorksheetInputModel>(worksheet);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            Worksheet worksheet = await _worksheetService.GetByIdAsync(id.Value);
            return new FileContentResult(Encoding.UTF8.GetBytes(worksheet.SourceCode), "application/download")
            {
                FileDownloadName = worksheet.Title + ".clx"
            };
        }

        [HttpPost]
        public IActionResult Upload(IFormCollection form)
        {
            IFormFile file = form.Files[0];
            if (file == null)
                return NotFound();

            string text = TextFileReader.ReadText(file);
            WorksheetInputModel model = new WorksheetInputModel()
            {
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                Description = TextHelper.GetTitle(text),
                SourceCode = text
            };
            return new JsonResult(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(WorksheetInputModel model)
        {
            model.Id = 0;

            if (!ModelState.IsValid)
                return View(nameof(Update), model);

            Worksheet worksheet = _mapper.Map<Worksheet>(model);
            worksheet.Category = null;
            await _worksheetService.AddAsync(worksheet);
            return RedirectToAction(string.Empty, "worksheet", new { Area = "", Controller = "Index", worksheet.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Update(WorksheetInputModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Worksheet worksheet = _mapper.Map<Worksheet>(model);
            await _worksheetService.UpdateAsync(worksheet);
            return RedirectToAction(string.Empty, "worksheet", new { Area = "", Controller = "Index", worksheet.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(WorksheetDeleteModel model)
        {
            await _worksheetService.DeleteAsync(model.Id);
            return RedirectToAction(string.Empty, "category", new { Area = "", Controller = "Index", Id = model.CategoryId });
        }
    }
}