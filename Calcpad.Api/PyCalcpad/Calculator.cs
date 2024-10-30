using Calcpad.Core;

namespace PyCalcpad
{
    public class Calculator
    {
        private readonly MathParser _parser;

        public Calculator(MathSettings settings) => _parser = new(ConvertMathSettings(settings));

        public string Eval(string code)
        {
            _parser.Parse(code);
            _parser.Calculate();
            _parser.SetVariable("ans", _parser.Real); 
            return _parser.ResultAsString;
        }

        public string Run(string code)
        {
            _parser.Parse(code);
            _parser.Calculate();
            return _parser.ToString();
        }

        public void SetVariable(string name, double value) => _parser.SetVariable(name, value);

        internal static Calcpad.Core.MathSettings ConvertMathSettings(MathSettings settings) =>
            new()
            {
                Decimals = settings.Decimals,
                Degrees = settings.Degrees,
                IsComplex = settings.IsComplex,
                Substitute = settings.Substitute,
                FormatEquations = settings.FormatEquations,
                ZeroSmallMatrixElements = settings.ZeroSmallMatrixElements,
                MaxOutputCount = settings.MaxOutputCount,
            };
    }
}
