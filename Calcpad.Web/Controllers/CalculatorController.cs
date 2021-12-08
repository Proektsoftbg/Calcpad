using Calcpad.web.Services;
using Calcpad.web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Calcpad.web.Controllers
{
    public class CalculatorController : Controller
    {
        public IActionResult Index(int? id)
        {
            CalculateViewModel.CalculatorMode mode;
            if (!(id.HasValue && Enum.IsDefined(typeof(CalculateViewModel.CalculatorMode), id)))
                mode = CalculateViewModel.CalculatorMode.Scientific;
            else
                mode = (CalculateViewModel.CalculatorMode)id.Value;

            CalculateViewModel model = new CalculateViewModel()
            {
                Input = string.Empty,
                Mode = mode
            };
            return View(model);
        }
    }
}