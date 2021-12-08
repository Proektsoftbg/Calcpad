using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Data.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public int Count { get; set; }

        public int? ParentId { get; set; }

        public Category Parent { get; set; }

        public IEnumerable<Category> Children { get; }

        public IEnumerable<Worksheet> Worksheets { get; }

        public string Icon { get; set; }
    }
}
