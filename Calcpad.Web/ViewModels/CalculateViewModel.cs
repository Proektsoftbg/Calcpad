namespace Calcpad.web.ViewModels
{
    public class CalculateViewModel
    {
        public CalculatorMode Mode;

        public string Input { get; set; }
        public int Degrees { get; set; }
        public bool IsComplex { get; set; }

        public enum CalculatorMode
        {
            Basic = 1,
            Scientific = 2,
            Graphing = 3
        }
    }

}
