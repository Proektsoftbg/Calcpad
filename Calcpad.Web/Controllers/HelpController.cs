using System.Threading.Tasks;
using AutoMapper;
using Calcpad.web.Data.Models;
using Calcpad.web.Data.Services;
using Calcpad.web.Helpers;
using Calcpad.web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Calcpad.web.Controllers
{
    public class HelpController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IMapper _mapper;

        public HelpController(IArticleService articleService, IMapper mapper)
        {
            _articleService = articleService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index(int? id, string slug)
        {
            if (!id.HasValue)
                id = 0;

            Article article = await _articleService.GetByIdAsync(id.Value);
            if (article == null)
                return NotFound();

            if (slug == null || id == 0)
            {
                id = article.Id;
                slug = TextHelper.SeoFriendly(article.Title);
                return RedirectToRoute("slug", new { id, slug });
            }
            ArticleViewModel model = _mapper.Map<ArticleViewModel>(article);
            return View(model);
        }
    }
}