using System;

namespace Calcpad.Core
{
    internal class MapParser : PlotParser
    {
        private Func<Value> _function;
        internal MapParser(MathParser parser, PlotSettings settings) : base(parser, settings) { }

        internal override string Parse(ReadOnlySpan<char> script, bool calculate)
        {
            const int n = 7;
            char[] delimiters = { '{', '@', '=', ':', '&', '=', ':', '}' };
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
#if BG
                return $"<span class = \"err\">Липсва разделител \"{delimiters[index]}\" в команда за 2D графика \"{script}\".</span>";
#else            
                return $"<span class = \"err\">Missing delimiter \"{delimiters[index]}\" in surface map command \"{script}\".</span>";
#endif
            for (int i = 0; i < n; ++i)
            {
                if (string.IsNullOrWhiteSpace(input[i]))
                {
                    var part = i;
                    if (part > 3)
                        part -= 3;
#if BG
                    throw new MathParser.MathParserException($"Липсва {Parts[part]} в команда за 2D графика.");
#else                     
                    throw new MathParser.MathParserException($"Missing {Parts[part]} in surface map command.");
#endif
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
#if BG                   
                    throw new MathParser.MathParserException($"Несъвместими мерни единици: '{Unit.GetText(xUnits)}' и '{Unit.GetText(endUnits)}'.");
#else                
                    throw new MathParser.MathParserException($"Inconsistent units: '{Unit.GetText(xUnits)}' and '{Unit.GetText(endUnits)}'.");
#endif
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
#if BG
                    throw new MathParser.MathParserException($"Несъвместими мерни единици: '{Unit.GetText(yUnits)}' и '{Unit.GetText(endUnits)}'.");
#else                
                    throw new MathParser.MathParserException($"Inconsistent units: '{Unit.GetText(yUnits)}' and '{Unit.GetText(endUnits)}'.");
#endif
                if (endUnits is not null)
                {
                    var factor = endUnits.ConvertTo(yUnits);
                    endY *= factor;
                }
                if (startX.EqualsBinary(endX) || startY.EqualsBinary(endY))
#if BG
                    throw new MathParser.MathParserException($"Границите на чертожната площ съвпадат.");
#else                
                    throw new MathParser.MathParserException($"The limits of plot area are identical.");
#endif
                Parameter[] parameters =
                {
                    new(input[1]),
                    new(input[4])
                };
                _function = Parser.Compile(input[0], parameters);
                //Parser.CompileBlocks();
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
