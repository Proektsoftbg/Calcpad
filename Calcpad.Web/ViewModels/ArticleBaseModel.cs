using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.ViewModels
{
    public class ArticleBaseModel
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }
    }
}
