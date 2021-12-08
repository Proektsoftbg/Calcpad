using System.Threading.Tasks;
using AutoMapper;
using Calcpad.web.Areas.Admin.Models;
using Calcpad.web.Data.Models;
using Calcpad.web.Data.Services;
using Calcpad.web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calcpad.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Global.Constants.RoleNames.Developer)]
    public class ArticleController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ITopicService _topicService;
        private readonly IMapper _mapper;

        public ArticleController(IArticleService articleService, ITopicService topicService, IMapper mapper)
        {
            _articleService = articleService;
            _topicService = topicService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Add(int? Id)
        {
            if (!Id.HasValue)
                return BadRequest();

            Topic topic = await _topicService.GetByIdAsync(Id.Value);
            if (topic == null)
                return NotFound();

            ArticleViewModel model = new ArticleViewModel()
            {
                Topic = _mapper.Map<TopicViewModel>(topic),
                TopicId = topic.Id
            };
            return View(nameof(Update), model);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            Article article = await _articleService.GetByIdAsync(id.Value);
            if (article == null)
                return NotFound();

            ArticleViewModel model = _mapper.Map<ArticleViewModel>(article);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            Article article = await _articleService.GetByIdAsync(id.Value);
            if (article == null)
                return NotFound();

            ArticleViewModel model = _mapper.Map<ArticleViewModel>(article);
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Add(ArticleInputModel model)
        {
            model.Id = 0;

            if (!ModelState.IsValid)
                return View(nameof(Update), model);

            Article article = _mapper.Map<Article>(model);
            article.Topic = null;
            await _articleService.AddAsync(article);
            return RedirectToAction(string.Empty, "help", new { Area = "", Controller = "Index", article.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Update(ArticleInputModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Article article = _mapper.Map<Article>(model);
            await _articleService.UpdateAsync(article);
            return RedirectToAction(string.Empty, "help", new { Area = "", Controller = "Index", article.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ArticleDeleteModel model)
        {
            await _articleService.DeleteAsync(model.Id);
            return RedirectToAction(string.Empty, "help", new { Area = "", Controller = "Index", Id = model.TopicId });
        }
    }
}