using AutoMapper;

namespace Calcpad.web.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Core.Settings, ViewModels.ParserSettings>();
            CreateMap<ViewModels.ParserSettings, Core.Settings>();
            CreateMap<Core.MathSettings, ViewModels.MathSettings>();
            CreateMap<ViewModels.MathSettings, Core.MathSettings>();
            CreateMap<Core.PlotSettings, ViewModels.PlotSettings>();
            CreateMap<ViewModels.PlotSettings, Core.PlotSettings>();

            CreateMap<Data.Models.Category, ViewModels.CategoryViewModel>();
            CreateMap<ViewModels.CategoryViewModel, Data.Models.Category>();

            CreateMap<Data.Models.Category, Areas.Dev.Models.CategoryInputModel>();
            CreateMap<Areas.Dev.Models.CategoryInputModel, Data.Models.Category>();
            CreateMap<Data.Models.Worksheet, ViewModels.WorksheetListModel>();
            CreateMap<Data.Models.Worksheet, ViewModels.WorksheetViewModel>();
            CreateMap<Areas.Dev.Models.WorksheetInputModel, Data.Models.Worksheet>();
            CreateMap<Data.Models.Worksheet, Areas.Dev.Models.WorksheetInputModel>();

            CreateMap<Data.Models.Topic, ViewModels.TopicViewModel>();
            CreateMap<ViewModels.TopicViewModel, Data.Models.Topic>();
            CreateMap<Data.Models.Topic, Areas.Admin.Models.TopicInputModel>();
            CreateMap<Areas.Admin.Models.TopicInputModel, Data.Models.Topic>();

            CreateMap<Data.Models.Article, ViewModels.ArticleViewModel>();
            CreateMap<ViewModels.ArticleViewModel, Data.Models.Article>();
            CreateMap<Data.Models.Article, ViewModels.ArticleBaseModel>();
            CreateMap<ViewModels.ArticleBaseModel, Data.Models.Article>();
            CreateMap<Data.Models.Article, Areas.Admin.Models.ArticleInputModel>();
            CreateMap<Areas.Admin.Models.ArticleInputModel, Data.Models.Article>();
        }
    }
}
