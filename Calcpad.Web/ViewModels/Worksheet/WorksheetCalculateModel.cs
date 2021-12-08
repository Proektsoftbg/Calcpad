using System.Collections.Generic;

namespace Calcpad.web.ViewModels
{
    public class WorksheetCalculateModel : WorksheetBaseModel
    {
        public IEnumerable<string> Var { get; set; }
        public ParserSettings Settings { get; set; }
    }
}
