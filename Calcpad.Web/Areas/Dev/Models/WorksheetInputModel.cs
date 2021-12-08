using Calcpad.web.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.Areas.Dev.Models
{
    public class WorksheetInputModel : WorksheetBaseModel
    {
        [Required, StringLength(255, MinimumLength = 3)]
        public string Description { get; set; }

        [Required, Display(Name = "Source Code")]
        public string SourceCode { get; set; }
    }
}
