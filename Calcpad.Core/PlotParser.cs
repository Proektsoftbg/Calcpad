﻿namespace Calcpad.Core
{
    internal abstract class PlotParser
    {
        protected MathParser Parser;
        protected PlotSettings Settings;

        protected PlotParser(MathParser parser, PlotSettings settings)
        {
            Parser = parser;
            Settings = settings;
        }

        internal abstract string Parse(string script, bool calculate);

        protected static string[] Parts =
        {
            "function",
            "variable name",
            "left margin",
            "right margin"
        };
    }
}