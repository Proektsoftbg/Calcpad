using System.Numerics;

namespace Calcpad.Tests
{
    internal class TestCalc
    {
        private readonly MathParser _parser;

        public TestCalc(MathSettings settings)
        {
            _parser = new MathParser(settings);
        }  

        public double Run(string expression)
        {
            _parser.Parse(expression);
            _parser.Calculate();
            return _parser.Real;
        }

        public double Run(string[] expressions)
        {
            foreach (var expression in expressions)
            {
                _parser.Parse(expression);
                _parser.Calculate();
            }
            return _parser.Real;
        }

        public double Real => _parser.Real;    
        public double Imaginary => _parser.Imaginary;
        public Complex Complex => _parser.Complex;

        public override string ToString() => _parser.ResultAsString;    
    }
}
