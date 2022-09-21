using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Data.Models
{
    public class Country
    {
        public int Id { get; set; }

        [Required, StringLength(2, MinimumLength = 2)]
        public string Code { get; set; }

        [Required, StringLength(35, MinimumLength = 3)]
        public string Name { get; set; }
        public bool IsEU { get; set; }
    }
}
