using System;
using Microsoft.AspNetCore.Html;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Calcpad.web.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Calcpad.web.Validation;

namespace Calcpad.web.ViewModels
{
    public class ParseViewModel
    {
        public ParseViewModel()
        {
            Code = string.Empty;
            Settings = new ParserSettings();
            Output = new HtmlString("0");
        }

        [NoScript]
        public string Code { get; set; }
        public ParserSettings Settings { get; set; }
        public HtmlString Output { get; set; }    }

    public class ParserSettings
    {
        public MathSettings Math { get; set; }
        public PlotSettings Plot { get; set; }
        public string Units { get; set; }

        public ParserSettings()
        {
            Math = new MathSettings();
            Plot = new PlotSettings();
            Units = "m";
        }
    }

    public class MathSettings
    {   
        [Range(0, 16)]
        public int Decimals { get; set; }
        public bool Degrees { get; set; }
        public bool IsComplex { get; set; }
        public bool Substitute { get; set; }

        [Display(Name = "Format equatuions")]
        public bool FormatEquations { get; set; }

        public MathSettings()
        {
            Decimals = 2;
            Degrees = true;
            IsComplex = false;
            Substitute = true;
            FormatEquations = true;
        }
    }

    public class PlotSettings
    {
        public string ImagePath { get; set; }
        public string ImageUri { get; set; }
        public bool VectorGraphics { get; set; }

        [Display(Name = "Scale")]
        public ColorScales ColorScale { get; set; }

        [Display(Name = "Smooth")]
        public bool SmoothScale { get; set; }
        public bool Shadows { get; set; }

        [Display(Name = "Light direction")]
        public LightDirections LightDirection { get; set; }

        public IEnumerable<SelectListItem> LightDirectionsSelectListItems =>
            EnumListMapper<LightDirections>.Map(LightDirection);
        public IEnumerable<SelectListItem> ColorScalesSelectListItems =>
            EnumListMapper<ColorScales>.Map(ColorScale);

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
            ImagePath = string.Empty;
            ImageUri = string.Empty;
            VectorGraphics = false;
            this.ColorScale = ColorScales.Rainbow;
            SmoothScale = false;
            Shadows = true;
            LightDirection = LightDirections.NorthWest;
        }
    }
}
