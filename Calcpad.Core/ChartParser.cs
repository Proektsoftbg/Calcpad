using System;

namespace Calcpad.Core
{
    internal class ChartParser : PlotParser
    {
       
        private Func<Value>[] _fx, _fy;
        internal ChartParser(MathParser parser, PlotSettings settings) : base(parser, settings) { }

        internal override string Parse(string script, bool calculate)
        {
            const int n = 4;
            char[] delimiters = { '{', '@', '=', ':', '}' };
            var input = new string[4];
            var index = 0;
            for (int i = 0, len = script.Length; i < len; ++i)
            {
                var c = script[i];
                if (c == delimiters[index])
                {
                    index++;
                    if (index > n)
                        break;
                }
                else if (index > 0)
                    input[index - 1] += c.ToString();
            }
            if (index <= 4)
#if BG
                return $"<span class = \"err\">Липсва разделител \"{delimiters[index]}\" в команда за графика \"{script}\".</span>";
#else            
                return $"<span class = \"err\">Missing delimiter \"{delimiters[index]}\" in plot command \"{script}\".</span>";
#endif

            for (int j = 0; j < n; ++j)
                if (string.IsNullOrWhiteSpace(input[j]))
#if BG
                    return $"<span class = \"err\">Липсва {Parts[j]} в команда за графика \"{script}\".</span>.";
#else           
                    return $"<span class = \"err\">Missing {Parts[j]} in plot command \"{script}\".</span>.";
#endif
            string result;
            if (calculate)
            {
                var charts = input[0].Split('&');
                input[1] = input[1].Trim();
                Parameter[] p = { new(input[1]) };
                p[0].SetValue(Value.Zero);
                var count = charts.Length;
                _fx = new Func<Value>[count];
                _fy = new Func<Value>[count];
                for (int i = 0; i < count; ++i)
                {
                    var xy = charts[i].Split('|');
                    if (xy.Length > 2)
#if BG
                        return $"<span class = \"err\">Разделителите \"|\" са повече от един в команда за графика {script}.</span>";
#else
                        return $"<span class = \"err\">More than one delimiter \"|\" in plot command {script}.</span>";
#endif
                    else if (xy.Length > 0)
                    {
                        if (xy.Length == 1)
                        {
                            _fx[i] = Parser.CompileRpn(input[1], p);
                            _fy[i] = Parser.CompileRpn(xy[0], p);
                        }
                        else
                        {
                            _fx[i] = Parser.CompileRpn(xy[0], p);
                            _fy[i] = Parser.CompileRpn(xy[1], p);
                        }
                    }
                }
                Parser.Parse(input[2]);
                var left = Parser.CalculateReal();
                var leftUnits = Parser.Units;
                Parser.Parse(input[3]);
                var right = Parser.CalculateReal();
                var rightUnits = Parser.Units;
                if (!Unit.IsConsistent(leftUnits, rightUnits))
#if BG
                    throw new MathParser.MathParserException($"Несъвместими мерни единици: \"{Unit.GetText(leftUnits)}\" и \"{Unit.GetText(rightUnits)}\".");
#else
                    throw new MathParser.MathParserException($"Inconsistent units: \"{Unit.GetText(leftUnits)}\" and \"{Unit.GetText(rightUnits)}\".");
#endif
                if (leftUnits is not null)
                {
                    var factor = rightUnits.ConvertTo(leftUnits);
                    right *= factor;
                }
                Parser.Compile();
                result = GetHtmlImage(p[0], left, right, leftUnits);
            }
            else
                result = GetHtmlText(input);

            return result;
        }

        private string GetHtmlImage(Variable var, double left, double right, Unit u)
        {
            var chartPlotter = new ChartPlotter(Parser, Settings);
            var plotResult = chartPlotter.Plot(_fx, _fy, var, left, right, u);
            return plotResult;
        }

        private string GetHtmlText(string[] input)
        {
            var charts = input[0].Split('&');
            var index = 0;
            input[1] = input[1].Trim();
            foreach (var s in charts)
            {
                var xy = s.Split('|');
                if (xy.Length > 2)
#if BG
                    return $"<span class = \"err\">Има повече от един разделител \"|\" в команда за графика {input[0]}.</span>";
#else
                    return $"<span class = \"err\">More than one delimiter \"|\" in plot command {input[0]}.</span>";
#endif
                else if (xy.Length > 0)
                {
                    string sx, sy;
                    if (xy.Length == 1)
                    {
                        Parser.Parse(input[1]);
                        sx = Parser.ToHtml();
                        Parser.Parse(xy[0]);
                        sy = Parser.ToHtml();
                    }
                    else
                    {
                        Parser.Parse(xy[0]);
                        sx = Parser.ToHtml();
                        Parser.Parse(xy[1]);
                        sy = Parser.ToHtml();
                    }
                    charts[index] = sx + " | " + sy;
                    index++;
                }
            }
            input[0] = string.Join(" & ", charts);
            for (int i = 1, len = input.Length; i < len; ++i)
            {
                Parser.Parse(input[i]);
                input[i] = Parser.ToHtml();
            }
            return string.Concat("<span class = \"eq\"><span class=\"cond\">$Plot</span>{", input[0], " @ ", input[1], " ∈ [", input[2], "; ", input[3] + "]}</span>");
        }
    }
}
