using AutoMapper;
using Calcpad.web.Data.Models;
using Calcpad.web.Data.Services;
using Calcpad.web.Helpers;
using Calcpad.web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcpad.web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IWorksheetService _worksheetService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IWorksheetService worksheetService, IMapper mapper)
        {
            _categoryService = categoryService;
            _worksheetService = worksheetService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id, string slug)
        {
            if (!id.HasValue)
                id = 1;

            Category category = await _categoryService.GetByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            if (slug == null)
            {
                slug = TextHelper.SeoFriendly(category.Name);
                return RedirectToRoute("slug", new { id, slug });
            }
            CategoryViewModel model = _mapper.Map<CategoryViewModel>(category);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            ViewData["q"] = q;
            IEnumerable<Worksheet> resutls = await _worksheetService.SearchAsync(q);
            IEnumerable<WorksheetListModel> model = _mapper.Map<IEnumerable<WorksheetListModel>>(resutls);
            return View(model);
        }
    }
}