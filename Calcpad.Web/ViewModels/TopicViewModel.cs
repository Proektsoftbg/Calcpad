using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.ViewModels
{
    public class TopicsViewModel
    {
        public int TopicId { get; set; }
        public int ArticleId { get; set; }
        public List<TopicViewModel> Topics { get; set; }
    }

    public class TopicViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public IEnumerable<ArticleBaseModel> Articles { get; set; }
    }
}
