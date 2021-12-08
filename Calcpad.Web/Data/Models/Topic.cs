using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Data.Models
{
    public class Topic
    {
        public int Id { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }
        public IEnumerable<Article> Articles { get; }
    }
}
