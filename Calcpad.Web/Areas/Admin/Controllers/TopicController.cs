using AutoMapper;
using Calcpad.web.Areas.Admin.Models;
using Calcpad.web.Data.Models;
using Calcpad.web.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Global.Constants.RoleNames.Administrator)]
    public class TopicController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly IMapper _mapper;

        public TopicController(ITopicService topicService, IMapper mapper)
        {
            _topicService = topicService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Add()
        {
            TopicInputModel model = new TopicInputModel()
            {
                Id = 0,
                Name = string.Empty,
                IsEmpty = true
            };
            return View(nameof(Rename), model);
        }

        [HttpGet]
        public async Task<IActionResult> Rename(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            Topic topic = await _topicService.GetByIdAsync(id.Value);
            if (topic == null)
                return NotFound();

            TopicInputModel model = _mapper.Map<TopicInputModel>(topic);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            Topic topic = await _topicService.GetByIdAsync(id.Value);
            if (topic == null)
                return NotFound();

            TopicInputModel model = _mapper.Map<TopicInputModel>(topic);
            model.IsEmpty = !topic.Articles.Any();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(TopicInputModel model)
        {
            model.Id = 0;
            
            if (!ModelState.IsValid)
                return View(nameof(Rename), model);

            Topic topic = _mapper.Map<Topic>(model);
            await _topicService.AddAsync(topic);
            return RedirectToAction(string.Empty, "help", new { Area = "", Controller = "Index", topic.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Rename(TopicInputModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Topic topic = _mapper.Map<Topic>(model);
            await _topicService.RenameAsync(topic.Id, topic.Name);
            return RedirectToAction(string.Empty, "help", new { Area = "", Controller = "Index"});
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _topicService.DeleteAsync(id);
            return RedirectToAction(string.Empty, "help", new { Area = "", Controller = "Index"});
        }
    }
}