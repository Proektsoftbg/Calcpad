using System;

namespace Calcpad.Core
{
    internal class ChartParser : PlotParser
    {

        private Func<IValue>[] _fx, _fy;
        internal ChartParser(MathParser parser, PlotSettings settings) : base(parser, settings) { }

        internal override string Parse(ReadOnlySpan<char> script, bool calculate)
        {
            const int n = 4;
            ReadOnlySpan<char> delimiters = ['{', '@', '=', ':', '}'];
            var input = new string[4];
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
                    input[index - 1] += c.ToString();

                if (c == '}')
                    --bracketCount;
            }
            if (index <= 4)
                return string.Format(Messages.Missing_delimiter_0_in_plot_command, delimiters[index].ToString(), script.ToString());

            for (int j = 0; j < n; ++j)
                if (string.IsNullOrWhiteSpace(input[j]))
                    return string.Format(Messages.Missing_0_in_plot_command_1, Parts[j], script.ToString());
            string result;
            if (calculate)
            {
                var charts = input[0].Split('&');
                input[1] = input[1].Trim();
                ReadOnlySpan<Parameter> parameters = [new(input[1])];
                var count = charts.Length;
                _fx = new Func<IValue>[count];
                _fy = new Func<IValue>[count];
                for (int i = 0; i < count; ++i)
                {
                    var xy = charts[i].Split('|');
                    if (xy.Length > 2)
                        return string.Format(Messages.More_than_one_delimiter_in_plot_command, script.ToString());
                    else if (xy.Length > 0)
                    {
                        if (xy.Length == 1)
                            _fy[i] = Parser.Compile(xy[0], parameters);
                        else
                        {
                            _fx[i] = Parser.Compile(xy[0], parameters);
                            _fy[i] = Parser.Compile(xy[1], parameters);
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
                    Throw.InconsistentUnitsException(Unit.GetText(leftUnits), Unit.GetText(rightUnits));

                if (leftUnits is not null)
                {
                    var factor = rightUnits.ConvertTo(leftUnits);
                    right *= factor;
                }
                //Parser.CompileBlocks();
                result = GetHtmlImage(parameters[0].Variable, left, right, leftUnits);
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
                    return string.Format(Messages.More_than_one_delimiter_in_plot_command, input[0]);
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
