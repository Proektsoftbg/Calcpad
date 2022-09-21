using AutoMapper;
using Calcpad.web.Areas.Dev.Models;
using Calcpad.web.Data.Models;
using Calcpad.web.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Areas.Dev.Controllers
{
    [Area("Dev")]
    [Authorize(Roles = Global.Constants.RoleNames.Developer)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            Category parent = await _categoryService.GetByIdAsync(id.Value);
            if (parent == null)
                return NotFound();

            CategoryInputModel model = new()
            {
                Parent = _mapper.Map<CategoryInputModel>(parent),
                ParentId = parent.Id
            };
            return View(nameof(Rename), model);
        }

        [HttpGet]
        public async Task<IActionResult> Rename(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            Category category = await _categoryService.GetByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            if (category.Parent == null)
                return Forbid();

            CategoryInputModel model = _mapper.Map<CategoryInputModel>(category);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            Category category = await _categoryService.GetByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            if (category.Parent == null)
                return Forbid();

            CategoryInputModel model = _mapper.Map<CategoryInputModel>(category);
            model.IsEmpty = !(category.Worksheets.Any() || category.Children.Any());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(CategoryInputModel model)
        {
            model.Id = 0;

            if (!ModelState.IsValid)
                return View(nameof(Rename), model);

            Category category = _mapper.Map<Category>(model);
            await _categoryService.AddAsync(category);
            return RedirectToAction(string.Empty, "category", new { Area = "", Controller = "Index", category.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Rename(CategoryInputModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Category category = _mapper.Map<Category>(model);
            await _categoryService.RenameAsync(category.Id, category.Name);
            return RedirectToAction(string.Empty, "category", new { Area = "", Controller = "Index", category.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(CategoryDeleteModel model)
        {
            await _categoryService.DeleteAsync(model.Id);
            return RedirectToAction(string.Empty, "category", new { Area = "", Controller = "Index", Id = model.ParentId });
        }
    }
}