using Calcpad.web.Services;
using Calcpad.web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Calcpad.web.Controllers
{
    [ApiController]
    [Route("api/calculate")]
    public class CalculateController : ControllerBase
    {
        private readonly IParserService _parser;

        public CalculateController(IParserService parser)
        {
            _parser = parser;
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(CalculateViewModel model)
        {
            string output = string.Empty;
            if (!ModelState.IsValid)
            {
                output = "<p class=\"text-danger\">" +
                            string.Join("<br />", ModelState.Values
                            .SelectMany(x => x.Errors)
                            .Select(x => x.ErrorMessage)) + "</p>";
            }
            else if (!string.IsNullOrWhiteSpace(model.Input))
            {
                Core.Settings settings = new();
                settings.Math.IsComplex = model.IsComplex;
                settings.Math.Degrees = model.Degrees;
                output = await _parser.CalculateAsync(model.Input, settings);
            }
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = output
            };
        }
    }
}