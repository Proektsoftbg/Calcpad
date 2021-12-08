using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Data.Models
{
    public class Worksheet
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        [Required]
        public Category Category { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [Required, StringLength(255, MinimumLength = 3)]
        public string Description { get; set; }

        [Required]
        public string SourceCode { get; set; }

        [Required]
        public string Html { get; set; }

        public bool IsFree { get; set; }
    }
}
