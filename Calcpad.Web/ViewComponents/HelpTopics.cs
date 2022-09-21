using AutoMapper;
using Calcpad.web.Data.Models;
using Calcpad.web.Data.Services;
using Calcpad.web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcpad.web.ViewComponents
{
    public class HelpTopics : ViewComponent
    {
        private readonly ITopicService _topicService;
        private readonly IMapper _mapper;

        public HelpTopics(ITopicService topicService, IMapper mapper)
        {
            _topicService = topicService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync(int topicId, int articleId)
        {
            List<Topic> topics = await _topicService.GetAllAsync();
            TopicsViewModel model = new()
            {
                TopicId = topicId,
                ArticleId = articleId,
                Topics = new List<TopicViewModel>()
            };
            foreach (Topic topic in topics)
                model.Topics.Add(_mapper.Map<TopicViewModel>(topic));

            return View(model);
        }
    }
}
