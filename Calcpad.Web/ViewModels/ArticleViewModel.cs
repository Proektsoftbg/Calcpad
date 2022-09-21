using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.ViewModels
{
    public class ArticleViewModel : ArticleBaseModel
    {
        [Required]
        public TopicViewModel Topic { get; set; }

        [Required]
        public string Html { get; set; }
    }
}
