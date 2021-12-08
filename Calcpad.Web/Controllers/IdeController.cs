using System.Net;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Calcpad.web.ViewModels;
using Calcpad.web.Services;
using AutoMapper;
using System.Text;
using Microsoft.AspNetCore.Http;
using Calcpad.web.Helpers;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Controllers
{
    public class IdeController : Controller
    {
        private readonly IParserService _parser;
        private readonly IMapper _mapper;
        private readonly IOpenXmlService _openXml;

        public IdeController(IParserService parser, IMapper mapper, IOpenXmlService openXml)
        {
            _parser = parser;
            _mapper = mapper;
            _openXml = openXml;
        }

        public IActionResult Index(ParseViewModel model)
        {
            if (model == null)
                model = new ParseViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Parse(ParseViewModel model)
        {
            string output = string.Empty;
            if (!ModelState.IsValid)
            {
                output = "<p class=\"text-danger\">" +
                            string.Join("<br />", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage)) + "</p>";
            }
            else if (model.Code != null)
            {
                Core.Settings settings = _mapper.Map<Core.Settings>(model.Settings);
                output = await _parser.CalculateAsync(model.Code, settings);
            }
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = output
            };
        }

        [HttpPost]
        public IActionResult Save(ParseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            string text = model.Code;
            if (text == null)
                return View(nameof(Index), model);

            string fileName = TextHelper.GetTitle(text);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "code";

            return new FileContentResult(Encoding.UTF8.GetBytes(text), "application/download")
            {
                FileDownloadName = fileName + ".txt"
            };
        }

        [HttpPost]
        public IActionResult Open(IFormCollection form)
        {
            IFormFile file = form.Files[0];
            if (file != null)
                return Content(TextFileReader.ReadText(file));

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Preview(ParseViewModel model)
        {
            if (ModelState.IsValid)
            {
                Core.Settings settings = _mapper.Map<Core.Settings>(model.Settings);
                string output = await _parser.CalculateAsync(model.Code, settings);
                model.Output = new HtmlString(output);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Download(ParseViewModel model)
        {
            if (!ModelState.IsValid)
                return View(nameof(Index), model);

            model.Settings.Plot.VectorGraphics = false;
            Core.Settings settings = _mapper.Map<Core.Settings>(model.Settings);
            string text = model.Code;
            string output = await _parser.CalculateAsync(text, settings);
            string fileName = TextHelper.GetTitle(text) + ".docx";
            string url = $"{Request.Scheme}://{Request.Host.Value}{Request.PathBase}";
            FileContentResult file = await _openXml.GetDocxFileAsync(output, fileName, url);
            return file;
        }
    }
}