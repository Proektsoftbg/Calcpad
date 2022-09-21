using Calcpad.Core;
using System;
using System.Threading.Tasks;

namespace Calcpad.web.Services
{
    public interface ICalculatorService
    {
        public Task<string> CalculateAsync(string code, MathSettings settings);
        public Task<string> ParseAsync(string code, MathSettings settings);
    }

    public class CalculatorService : ICalculatorService
    {
        public async Task<string> ParseAsync(string code, MathSettings settings)
        {
            return await Task.Run(() => Parse(code, settings, false));
        }

        public async Task<string> CalculateAsync(string code, MathSettings settings)
        {
            return await Task.Run(() => Parse(code, settings, true));
        }

        private string Parse(string code, MathSettings settings, bool calculate)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "0";

            try
            {
                MathParser _parser = new(settings);
                string equation = code.Replace(',', '.');
                _parser.Parse(equation);
                _parser.Calculate();
                return calculate ?
                    _parser.ResultAsString.Replace('·', '*').Trim() :
                    $"<p class=\"eq\">{_parser.ToHtml()}</p>";
            }
            catch (Exception e)
            {
                return calculate ?
                    "Undefined" :
                    $"<p class=\"err\">{e.Message}</p>";
            }
        }
    }
}
