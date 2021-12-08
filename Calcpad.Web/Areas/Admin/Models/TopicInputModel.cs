using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Areas.Admin.Models
{
    public class TopicInputModel
    {
        public int Id { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public bool IsEmpty { get; set; }
    }
}
