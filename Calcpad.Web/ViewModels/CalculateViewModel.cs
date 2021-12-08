using Calcpad.web.Validation;

namespace Calcpad.web.ViewModels
{
    public class CalculateViewModel
    {
        public CalculatorMode Mode;

        public string Input { get; set; }
        public bool Degrees { get; set; }
        public bool IsComplex { get; set; }    

        public enum CalculatorMode
        {
            Basic = 1,
            Scientific = 2,
            Graphical = 3
        }
    }

}
