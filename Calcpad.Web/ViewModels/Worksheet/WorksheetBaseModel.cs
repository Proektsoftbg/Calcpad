using System.ComponentModel.DataAnnotations;

namespace Calcpad.web.ViewModels
{

    public abstract class WorksheetBaseModel
    {
        public WorksheetBaseModel() { }
        public WorksheetBaseModel(WorksheetBaseModel model)
        {
            Id = model.Id;
            Category = model.Category;
            Title = model.Title;
            IsFree = model.IsFree;
        }

        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [Required]
        public CategoryViewModel Category { get; set; }

        [Required, StringLength(50, MinimumLength = 3)]
        public string Title { get; set; }

        [Display(Name = "Free")]
        public bool IsFree { get; set; }
    }
}
