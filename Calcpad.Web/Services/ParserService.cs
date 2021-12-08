using Calcpad.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Calcpad.web.Services
{
    public interface IParserService
    {
        public Task<string> CalculateAsync(string code, Settings settings, IEnumerable<string> inputValues = null);
        public Task<string> ParseAsync(string code, Settings settings, IEnumerable<string> inputValues = null);
        public Task<string> ParseWorksheetAsync(string code);
        public string GetInputTextAndValues(string code, out IEnumerable<string> inputValues);
        public string ReplaceInputValues(string html, string[] values);
    }

    public class ParserService : IParserService
    {
        private readonly ExpressionParser _parser;

        public ParserService()
        {
            _parser = new ExpressionParser();
        }

        public async Task<string> CalculateAsync(string code, Settings settings, IEnumerable<string> inputValues = null)
        {
            return await Task.Run(() => Parse(code, settings, inputValues, true));
        }

        public async Task<string> ParseAsync(string code, Settings settings, IEnumerable<string> inputValues = null)
        {
            return await Task.Run(() => Parse(code, settings, inputValues, false));
        }

        public async Task<string> ParseWorksheetAsync(string code)
        {
            string inputText = GetInputTextAndValues(code, out IEnumerable<string> inputValues);
            Settings settings = new Settings();
            return await ParseAsync(inputText, settings, inputValues);
        }
        public string ReplaceInputValues(string html, string[] values)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int i = 0, i1 = 0, i2 = 0, l = html.Length, n = values.Length;
            while (i1 < l)
            {
                i1 = html.IndexOf("\"Var\"", i2);
                if (i1 < 0)
                {
                    sb.Append(html.Substring(i2));
                    i1 = l;
                }
                else
                {
                    i1 = html.IndexOf("value", i1);
                    i1 = html.IndexOf("\"", i1);
                    sb.Append(html.Substring(i2, i1 - i2 + 1));
                    i2 = html.IndexOf("\"", i1 + 1);
                    if (i < n)
                    {
                        sb.Append(values[i] + '"');
                        i++;
                    }
                }
            }
            return sb.ToString();
        }

        private string Parse(string code, Settings settings, IEnumerable<string> inputValues, bool calculate)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "0";

            if (settings != null)
                _parser.Settings = settings;

            if (inputValues != null)
                SetInputValues(inputValues);

            _parser.Parse(code + '\n', calculate);
            return _parser.HtmlResult;
        }

        public string GetInputTextAndValues(string code, out IEnumerable<string> inputValues)
        {
            if (code.Contains('\v'))
            {
                string[] s = code.Split('\v');
                inputValues = s[1].Split('\t');
                return s[0];
            }
            else
                inputValues = null;
            return code;
        }

        private void SetInputValues(IEnumerable<string> inputValues)
        {
            foreach (string s in inputValues)
            {
                if (string.IsNullOrWhiteSpace(s))
                    _parser.SetInputField("0");
                else
                    _parser.SetInputField(s.Replace(',', '.'));
            }
        }
    }
}
