using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
