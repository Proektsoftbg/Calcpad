using System;

namespace Calcpad.Core
{
    internal class MapParser : PlotParser
    {
        private Func<IValue> _function;
        internal MapParser(MathParser parser, PlotSettings settings) : base(parser, settings) { }

        internal override string Parse(ReadOnlySpan<char> script, bool calculate)
        {
            const int n = 7;
            ReadOnlySpan<char> delimiters = ['{', '@', '=', ':', '&', '=', ':', '}'];
            var input = new string[n];
            string result;
            var index = 0;
            var bracketCount = 0;
            for (int i = 0, len = script.Length; i < len; ++i)
            {
                var c = script[i];
                if (c == '{')
                    ++bracketCount;

                if (c == delimiters[index] && bracketCount == 1)
                {
                    index++;
                    if (index > n)
                        break;
                }
                else if (index > 0)
                    input[index - 1] += c;

                if (c == '}')
                    --bracketCount;
            }
            if (index <= n)
                return string.Format(Messages.Missing_delimiter_0_in_surface_map_command_1, delimiters[index].ToString(), script.ToString());
            for (int i = 0; i < n; ++i)
            {
                if (string.IsNullOrWhiteSpace(input[i]))
                {
                    var part = i;
                    if (part > 3)
                        part -= 3;

                    Throw.MissingMapItemException(Parts[part]);
                }
            }

            input[1] = input[1].Trim();
            input[4] = input[4].Trim();
            if (calculate)
            {
                Parser.Parse(input[2]);
                var startX = Parser.CalculateReal();
                var xUnits = Parser.Units;
                Parser.Parse(input[3]);
                var endX = Parser.CalculateReal();
                var endUnits = Parser.Units;
                if (!Unit.IsConsistent(xUnits, endUnits))
                    Throw.InconsistentUnitsException(Unit.GetText(xUnits), Unit.GetText(endUnits));

                if (endUnits is not null)
                {
                    var factor = endUnits.ConvertTo(xUnits);
                    endX *= factor;
                }
                Parser.Parse(input[5]);
                var startY = Parser.CalculateReal();
                var yUnits = Parser.Units;
                Parser.Parse(input[6]);
                var endY = Parser.CalculateReal();
                endUnits = Parser.Units;
                if (!Unit.IsConsistent(yUnits, endUnits))
                    Throw.InconsistentUnitsException(Unit.GetText(yUnits), Unit.GetText(endUnits));

                if (endUnits is not null)
                {
                    var factor = endUnits.ConvertTo(yUnits);
                    endY *= factor;
                }
                if (startX.AlmostEquals(endX) || startY.AlmostEquals(endY))
                    Throw.PlotLimitsIdenticalException();

                ReadOnlySpan<Parameter> parameters =
                [
                    new(input[1]),
                    new(input[4])
                ];
                _function = Parser.Compile(input[0], parameters);
                result = GetHtmlImage(parameters[0].Variable, parameters[1].Variable, startX, endX, startY, endY, xUnits, yUnits);
            }
            else
                result = GetHtmlText(input);

            return result;
        }

        private string GetHtmlImage(Variable varX, Variable varY, double startX, double endX, double startY, double endY, Unit xUnits, Unit yUnits)
        {
            return new MapPlotter(Parser, Settings).Plot(_function, varX, varY, startX, endX, startY, endY, xUnits, yUnits);
        }

        private string GetHtmlText(string[] input)
        {
            for (int i = 0, len = input.Length; i < len; ++i)
            {
                Parser.Parse(input[i]);
                input[i] = Parser.ToHtml();
            }
            return string.Concat("<span class = \"eq\"><span class=\"cond\">$Map</span>", '{', input[0], " @ ", input[1], " ∈ [", input[2], "; ", input[3], "]}</span>");
        }

    }
}
