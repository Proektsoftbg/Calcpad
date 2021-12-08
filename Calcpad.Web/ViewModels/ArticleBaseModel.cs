using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
