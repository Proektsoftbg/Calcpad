namespace PyCalcpad
{
    public class PlotSettings
    {
        private bool _shadows;
        public bool IsAdaptive { get; set; }
        public double ScreenScaleFactor { get; set; } = 2.0;
        public string ImagePath { get; set; }
        public string ImageUri { get; set; }
        public bool VectorGraphics { get; set; }
        public int ColorScale { get; set; }
        public bool SmoothScale { get; set; }
        public bool Shadows
        {
            set => _shadows = value;
            get => _shadows && ColorScale != (int)ColorScales.Gray || ColorScale == (int)ColorScales.None;
        }
        public LightDirections LightDirection { get; set; }

        public enum LightDirections
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest
        }

        public enum ColorScales
        {
            None,
            Gray,
            Rainbow,
            Terrain,
            VioletToYellow,
            GreenToYellow,
            Blues
        }

        public PlotSettings()
        {
            IsAdaptive = true;
            ImagePath = string.Empty;
            ImageUri = string.Empty;
            VectorGraphics = false;
            ColorScale = (int)ColorScales.Rainbow;
            SmoothScale = false;
            Shadows = true;
            LightDirection = LightDirections.NorthWest;
        }
    }
}