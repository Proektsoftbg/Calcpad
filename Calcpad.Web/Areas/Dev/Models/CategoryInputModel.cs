using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Areas.Dev.Models
{
    public class CategoryInputModel
    {
        public int Id { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public int ParentId { get; set; }

        public CategoryInputModel Parent { get; set; }

        public string Icon { get; set; }

        public bool IsEmpty { get; set; }
    }
}
