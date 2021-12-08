namespace Calcpad.web.ViewModels
{
    public class WorksheetViewModel : WorksheetBaseModel
    {
        public WorksheetViewModel() { }
        public WorksheetViewModel(WorksheetCalculateModel model) : base(model) 
        {
            Settings = model.Settings;
        }
        public string Html { get; set; }
        public ParserSettings Settings { get; set; }
    }
}
