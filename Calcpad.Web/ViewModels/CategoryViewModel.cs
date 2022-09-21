using System.Collections.Generic;

namespace Calcpad.web.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }

        public int ParentId { get; set; }

        public CategoryViewModel Parent { get; set; }

        public IEnumerable<CategoryViewModel> Children { get; set; }

        public IEnumerable<WorksheetListModel> Worksheets { get; set; }
    }
}
