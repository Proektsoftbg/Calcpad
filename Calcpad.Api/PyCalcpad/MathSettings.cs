using System;

namespace PyCalcpad
{
    public class MathSettings
    {
        private int _decimals;
        private int _maxOutputCount;
        public int Decimals
        {
            get => _decimals;
            set
            {
                _decimals = value switch
                {
                    <= 0 => 0,
                    >= 15 => 15,
                    _ => value
                };
            }
        }
        public int Degrees { get; set; }
        public bool IsComplex { get; set; }
        public bool Substitute { get; set; }
        public bool FormatEquations { get; set; }
        public bool ZeroSmallMatrixElements { get; set; } 
        public int MaxOutputCount
        {
            get => _maxOutputCount;
            set
            {
                _maxOutputCount = value switch
                {
                    <= 5 => 5,
                    >= 100 => 100,
                    _ => value
                };
            }
        }

        public MathSettings()
        {
            Decimals = 2;
            Degrees = (int)TrigUnits.Deg;
            IsComplex = false;
            Substitute = true;
            FormatEquations = true;
            ZeroSmallMatrixElements = true;
            MaxOutputCount = 20;
        }

        public enum TrigUnits
        {
            Deg,
            Rad,
            Grad
        }
    }
}