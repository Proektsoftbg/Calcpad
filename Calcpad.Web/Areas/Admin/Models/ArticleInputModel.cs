using Calcpad.web.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Areas.Admin.Models
{
    public class ArticleInputModel : ArticleBaseModel
    {
        [Required]
        public string Html { get; set; }
    }
}
