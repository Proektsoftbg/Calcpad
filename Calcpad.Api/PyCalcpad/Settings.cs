namespace PyCalcpad
{
    public class Settings
    {
        public MathSettings Math { get; set; } = new();
        public PlotSettings Plot { get; set; } = new();
        public string Units { get; set; } = "m";
    }
}