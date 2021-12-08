using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Data.Models
{
    public class Article
    {
        public int Id { get; set; }

        public int TopicId { get; set; }

        [Required]
        public Topic Topic { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        public string Html { get; set; }
    }
}
