using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Calcpad.Core
{
    public class ExpressionParser
    {
        private enum Keywords { None, Hide, Show, Pre, Post, Val, Equ, Deg, Rad, Repeat, Loop, Break, If, ElseIf, Else, EndIf, Def, EndDef }
        private readonly List<string> _inputFields = new();
        private int _currentField;
        private bool _isVal;
        private Stack<Macros> _callStack = new();
        private Stack<string> _definitionMacrosStack = new();
        private Dictionary<string, Macros> _definedMacros = new();  // TODO: move to parser
        private MathParser _parser;
        private const int CallStackSize = 100;
        private static readonly string[] NewLines = { "\r\n", "\r", "\n" };
        private static readonly Regex MacrosNamePattern = new(@"^([∡°a-zA-Zα-ωΑ-Ω][a-zA-Zα-ωΑ-Ω,_′″‴⁗øØ°∡0-9.]*\$)");
        private static readonly Regex VariableNamePattern = new(@"^([∡°a-zA-Zα-ωΑ-Ω][a-zA-Zα-ωΑ-Ω,_′″‴⁗øØ°∡0-9.]*)");
        private static readonly Regex MacrosCallPattern = new(@"([∡°a-zA-Zα-ωΑ-Ω][a-zA-Zα-ωΑ-Ω,_′″‴⁗øØ°∡0-9.]*\$)\((.*?)\)");
        public Settings Settings { get; set; }
        public string HtmlResult { get; private set; }
        public static bool IsUs
        {
            get => Unit.IsUs;
            set => Unit.IsUs = value;
        }
        public ExpressionParser()
        {
            Settings = new Settings();
        }

        public void ClearInputFields()
        {
            _inputFields.Clear();
            _currentField = 0;
        }

        public void SetInputField(string value) => _inputFields.Add(value);

        public string GetInputField()
        {
            if (!_inputFields.Any())
                return "?";

            if (_currentField >= _inputFields.Count)
                _currentField = 0;

            return _inputFields[_currentField++];
        }

        public void Parse(string expressions, bool calculate = true) =>
            Parse(expressions.Split(NewLines, StringSplitOptions.None), calculate);

        private static Keywords GetKeyword(string s)
        {

            if (s[1] == 'i' && s[2] == 'f')
                return Keywords.If;
            if (s.Length < 4)
                return Keywords.None;
            if (s[1] == 'e')
            {
                if (s[2] == 'l' && s[3] == 's' && s.Length > 4 && s[4] == 'e')
                {
                    if (s.Length > 7 && s[5] == ' ' && s[6] == 'i' && s[7] == 'f')
                        return Keywords.ElseIf;

                    return Keywords.Else;
                }
                if (s[2] == 'n' && s[3] == 'd' && s.Length > 6 && s[4] == ' ' && s[5] == 'i' && s[6] == 'f')
                    return Keywords.EndIf;
                if (s[2] == 'n' && s[3] == 'd' && s.Length > 7 && s[4] == ' ' && s[5] == 'd' && s[6] == 'e' && s[7] == 'f')
                    return Keywords.EndDef;
                if (s[2] == 'q' && s[3] == 'u')
                    return Keywords.Equ;
            }
            else if (s[1] == 'v' && s[2] == 'a' && s[3] == 'l')
                return Keywords.Val;
            else if (s[1] == 'd' && s[2] == 'e' && s[3] == 'f')
                return Keywords.Def;
            else if (s[1] == 'h' && s[2] == 'i' && s[3] == 'd' && s.Length > 4 && s[4] == 'e')
                return Keywords.Hide;
            else if (s[1] == 's' && s[2] == 'h' && s[3] == 'o' && s.Length > 4 && s[4] == 'w')
                return Keywords.Show;
            else if (s[1] == 'p')
            {
                if (s[2] == 'r' && s[3] == 'e')
                    return Keywords.Pre;

                if (s[2] == 'o' && s[3] == 's' && s.Length > 4 && s[4] == 't')
                    return Keywords.Post;
            }
            else if (s[1] == 'd' && s[2] == 'e' && s[3] == 'g')
                return Keywords.Deg;
            else if (s[1] == 'r')
            {
                if (s[2] == 'a' && s[3] == 'd')
                    return Keywords.Rad;

                if (s[2] == 'e' && s[3] == 'p' && s.Length > 6 && s[4] == 'e' && s[5] == 'a' && s[6] == 't')
                    return Keywords.Repeat;
            }
            else if (s[1] == 'l' && s[2] == 'o' && s[3] == 'o' && s.Length > 4 && s[4] == 'p')
                return Keywords.Loop;
            else if (s[1] == 'b' && s[2] == 'r' && s[3] == 'e' && s.Length > 5 && s[4] == 'a' && s[5] == 'k')
                return Keywords.Break;

            return Keywords.None;
        }
        public void Cancel() => _parser?.Cancel();
        
        public void Parse(string[] expressions, bool calculate = true, MathParser parser = null, int start = 0, int? end = null)
        {
            var stringBuilder = new StringBuilder(expressions.Length * 80);
            var condition = new ConditionParser();
            var isSubCall = false;
            if (parser is null)
            {
                _parser = new MathParser(Settings.Math);
            }
            else
            {
                _parser = parser;
                isSubCall = true;
            }
            var loops = new Stack<Loop>();
            _isVal = false;
            var isVisible = true;
            _parser.IsEnabled = calculate;
            _parser.GetInputField += GetInputField;
            _parser.SetVariable("Units", new Value(UnitsFactor()));
            var line = 0;
            end ??= expressions.Length - 1;
            var s = string.Empty;
            try
            {
                for (var i = start; i < end; ++i)
                {
                    line = i + 1;
                    var id = loops.Any() && loops.Peek().Iteration != 1 ? "" : $" id=\"line{i+1}\"";
                    if (_parser.IsCanceled)
                        break;

                    s = expressions[i].Trim();
                    Debug.WriteLine(s);
                    if (string.IsNullOrEmpty(s))
                    {
                        if (isVisible)
                            stringBuilder.AppendLine($"<p{id}>&nbsp;</p>");

                        continue;
                    }
                    var lower = s.ToLowerInvariant();
                    var keyword = Keywords.None;
                    if (s[0] == '#')
                    {
                        var isKeyWord = true;
                        keyword = GetKeyword(lower);
                        if (keyword == Keywords.Hide)
                            isVisible = false;
                        else if (keyword == Keywords.Show)
                            isVisible = true;
                        else if (keyword == Keywords.Pre)
                            isVisible = !calculate;
                        else if (keyword == Keywords.Post)
                            isVisible = calculate;
                        else if (keyword == Keywords.Val)
                            _isVal = true;
                        else if (keyword == Keywords.Equ)
                            _isVal = false;
                        else if (keyword == Keywords.Deg)
                            _parser.Degrees = true;
                        else if (keyword == Keywords.Rad)
                            _parser.Degrees = false;
                        else if (keyword == Keywords.Repeat)
                        {
                            var expression = string.Empty;
                            if (s.Length > 7)
                                expression = s[7..].Trim();

                            if (calculate)
                            {
                                if (condition.IsSatisfied)
                                {
                                    var count = 0;
                                    if (!string.IsNullOrWhiteSpace(expression))
                                    {
                                        try
                                        {
                                            _parser.Parse(expression);
                                            _parser.Calculate();
                                            if (_parser.Real > int.MaxValue)
#if BG
                                                stringBuilder.Append($"<p class=\"err\">Грешка в \"{s}\" на ред {Line(line)}: Броят на итерациите е по-голям от максималния {int.MaxValue}.</p>");
#else
                                                stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line{Line(line)}: Number of iterations exceeds the maximum {int.MaxValue}.</p>");
#endif
                                            else
                                                count = (int)Math.Round(_parser.Real);
                                        }
                                        catch (MathParser.MathParserException ex)
                                        {
#if BG
                                            stringBuilder.Append($"<p class=\"err\">Грешка в \"{s}\" на ред {Line(line)}: {ex.Message}</p>");
#else
                                            stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line {Line(line)}: {ex.Message}</p>");
#endif                                        
                                        }
                                    }
                                    else
                                        count = -1;

                                    loops.Push(new Loop(i, count, condition.Id));
                                }
                            }
                            else if (isVisible)
                            {
                                if (string.IsNullOrWhiteSpace(expression))
                                    stringBuilder.Append($"<p{id} class=\"cond\">#repeat</p><div class=\"indent\">");
                                else
                                {
                                    try
                                    {
                                        _parser.Parse(expression);
                                        stringBuilder.Append($"<p{id}><span class=\"cond\">#repeat</span> {_parser.ToHtml()}</p><div class=\"indent\">");
                                    }
                                    catch (MathParser.MathParserException ex)
                                    {
#if BG
                                        stringBuilder.Append($"<p class=\"err\">Грешка в \"{s}\" на ред {Line(line)}: {ex.Message}</p>");
#else                                        
                                        stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line {Line(line)}: {ex.Message}</p>");
#endif                                 
                                    }
                                }
                            }
                        }
                        else if (keyword == Keywords.Loop)
                        {
                            if (calculate)
                            {
                                if (condition.IsSatisfied)
                                {
                                    if (!loops.Any())
#if BG
                                        stringBuilder.Append($"<p class=\"err\">Грешка в \"{s}\" на ред {Line(line)}: \"#loop\" без съответен \"#repeat\".</p>");
#else                                    
                                        stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line {Line(line)}: \"#loop\" without a corresponding \"#repeat\".</p>");
#endif                                    
                                    else if (loops.Peek().Id != condition.Id)
#if BG
                                        stringBuilder.Append($"<p class=\"err\">Грешка в \"{s}\" на ред {Line(line)}: Преплитане на \"#if - #end if\" и \"#repeat - #loop\" блокове .</p>");
#else
                                        stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line {Line(line)}: Entangled \"#if - #end if\" and \"#repeat - #loop\" blocks.</p>");
#endif
                                    else if (condition.IsSatisfied && !loops.Peek().Iterate(ref i))
                                        loops.Pop();
                                }
                            }
                            else if (isVisible)
                            {
                                stringBuilder.Append($"</div><p{id} class=\"cond\">#loop</p>");
                            }
                        }
                        else if (keyword == Keywords.Break)
                        {
                            if (calculate)
                            {
                                if (condition.IsSatisfied)
                                {
                                    if (loops.Any())
                                        loops.Peek().Disable();
                                    else
                                        break;
                                }
                            }
                            else if (isVisible)
                                stringBuilder.Append($"<p{id} class=\"cond\">#break</p>");
                        }
                        else if (keyword == Keywords.Def)
                        {
                            var endOfName = s.IndexOf("$");
                            if (endOfName == -1)
                            {
                                // TODO: BG
                                stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line{Line(line)}: Missing \"$\" in macros name.</p>");
                            }
                            else if (endOfName < 6)
                            {
                                // TODO: BG
                                stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line{Line(line)}: Empty macros name.</p>");
                            }
                            else
                            {
                                var macrosName = s[5..(endOfName+1)];
                                var closeBracket = s.IndexOf(")");
                                if (!MacrosNamePattern.IsMatch(macrosName))
                                {
                                    // TODO: BG
                                    stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line{Line(line)}: Invalid macros name \"{macrosName}\"</p>");
                                }
                                else if (s[endOfName + 1] != '(')
                                {
                                    // TODO: BG
                                    stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line{Line(line)}: Missing \"(\" in macros definition.</p>");
                                }
                                else if (closeBracket == -1)
                                {
                                    // TODO: BG
                                    stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line{Line(line)}: Missing \")\" in macros definition.</p>");
                                }
                                else
                                {
                                    var args = s[(endOfName + 1 + 1)..closeBracket];
                                    var argsNames = args.Split(";",
                                            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var argName in argsNames)
                                    {
                                        if (!VariableNamePattern.IsMatch(argName))
                                        {
                                            // TODO: BG
                                            stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line{Line(line)}: invalid argument name: \"{argName}\"</p>");
                                            break;
                                        }
                                    }
                                    
                                    _definedMacros[macrosName] = new Macros(line, argsNames.ToArray());
                                    _definitionMacrosStack.Push(macrosName);
                                }
                            }
                            
                        }
                        else if (keyword == Keywords.EndDef)
                        {
                            if (_definitionMacrosStack.Count == 0)
                            {
                                // TODO: BG
                                stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line {Line(line)}: \"#end def\" without a corresponding \"#def\".</p>");
                            }
                            else
                            {
                                var macrosName = _definitionMacrosStack.Pop();
                                _definedMacros[macrosName].EndDefLine = line;
                            }
                        }
                        else
                            isKeyWord = false;

                        if (isKeyWord)
                            continue;
                    }
                    if (_definitionMacrosStack.Any()) continue;
                    if (lower.StartsWith("$plot") || lower.StartsWith("$map"))
                    {
                        if (isVisible && (condition.IsSatisfied || !calculate))
                        {
                            PlotParser plotParser;
                            if (lower.StartsWith("$p"))
                                plotParser = new ChartParser(_parser, Settings.Plot);
                            else
                                plotParser = new MapParser(_parser, Settings.Plot);

                            try
                            {
                                _parser.IsPlotting = true;
                                s = plotParser.Parse(s, calculate);
                                stringBuilder.Append(InsertAttribute(s, id));
                                _parser.IsPlotting = false;
                            }
                            catch (MathParser.MathParserException ex)
                            {
#if BG                                
                                stringBuilder.Append($"<p class=\"err\">Грешка в \"{s}\" на ред {Line(line)}: {ex.Message}</p>");
#else                                
                                stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line {Line(line)}: {ex.Message}</p>");
#endif                            
                            }
                        }
                    }
                    else
                    {
                        condition.SetCondition(keyword - Keywords.If);
                        if (condition.IsSatisfied || !calculate)
                        {
                            var kwdLength = condition.KeyWordLength;
                            if (kwdLength == s.Length)
                            {
                                if (condition.IsUnchecked)
#if BG
                                    throw new MathParser.MathParserException("Условието не може да бъде празно.");
#else
                                    throw new MathParser.MathParserException("Condition cannot be empty.");
#endif
                                if (isVisible && !calculate)
                                {
                                    if (keyword == Keywords.Else)
                                        stringBuilder.Append($"</div><p{id}>{condition.ToHtml()}</p><div class = \"indent\">");
                                    else
                                        stringBuilder.Append($"</div><p{id}>{condition.ToHtml()}</p>");
                                }
                            }
                            else if (kwdLength > 0 && condition.IsFound && condition.IsUnchecked && calculate)
                                condition.Check(0.0);
                            else
                            {
                                var tokens = GetInput(s, kwdLength);
                                var lineType = TokenTypes.Text;
                                if (tokens.Any())
                                    lineType = tokens[0].Type;
                                var isOutput = isVisible && (!calculate || kwdLength == 0);
                                if (isOutput)
                                {
                                    if (keyword == Keywords.ElseIf || keyword == Keywords.EndIf)
                                        stringBuilder.Append("</div>");

                                    if (lineType == TokenTypes.Heading)
                                        stringBuilder.Append($"<h3{id}>");
                                    else if (lineType == TokenTypes.Html)
                                        tokens[0] = new Token(InsertAttribute(tokens[0].Value, id), TokenTypes.Html);
                                    else if (lineType == TokenTypes.Text && isSubCall)
                                        stringBuilder.Append($"<span{id}>");
                                    else
                                        stringBuilder.Append($"<p{id}>");

                                    if (kwdLength > 0 && !calculate)
                                        stringBuilder.Append(condition.ToHtml());
                                }

                                foreach (var token in tokens)
                                {
                                    var match = MacrosCallPattern.Match(token.Value);
                                    if (match.Success)
                                    {
                                        var name = match.Groups[1].Value;
                                        if (!_definedMacros.ContainsKey(name))
                                        {
#if BG
                                            stringBuilder.Append($"<p class=\"err\">Грешка в \"{token.Value}\" на ред \"{Line(line)}\": Недефинирано макроси \"{name}$\"</p>");
#else
                                            stringBuilder.Append($"<p class=\"err\">Error in \"{token.Value}\" on line \"{Line(line)}\": Undefined macros \"{name}$\"</p>");
#endif
                                            continue;
                                        }

                                        if (_callStack.Count > CallStackSize)
                                        {
                                            // TODO: BG
                                            stringBuilder.Append($"<p class=\"err\">Error in \"{token.Value}\" on line \"{Line(line)}\": Stack overflow: more than {CallStackSize} calls!</p>");
                                            continue;
                                        }
                                        var macros = _definedMacros[name];
                                        var argsValues = match.Groups[2].Value.Split(";", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                                        if (argsValues.Length != macros.ArgumentNames.Length)
                                        {
                                            // TODO: BG
                                            stringBuilder.Append($"<p class=\"err\">Error in \"{token.Value}\" on line \"{Line(line)}\": Expected {macros.ArgumentNames.Length} arguments but got {argsValues.Length}</p>");
                                            continue;
                                        }
                                        var localExpressions = macros.ReplaceArgumentInBody(argsValues, expressions);
                                        _callStack.Push(macros);
                                        Parse(localExpressions, calculate, _parser, macros.DefLine, macros.EndDefLine-1);
                                        _callStack.Pop();
                                        stringBuilder.Append(HtmlResult);
                                        continue;
                                    }
                                    if (token.Type == TokenTypes.Expression)
                                    {
                                        try
                                        {
                                            _parser.Parse(token.Value);
                                            if (calculate)
                                                _parser.Calculate();

                                            if (isOutput)
                                            {
                                                if (_isVal & calculate)
                                                    stringBuilder.Append(Complex.Format(_parser.Result, Settings.Math.Decimals, OutputWriter.OutputFormat.Html));
                                                else
                                                {
                                                    if (Settings.Math.FormatEquations)
                                                        stringBuilder.Append($"<span class=\"eq\" data-xml=\'{_parser.ToXml()}\'>{_parser.ToHtml()}</span>");
                                                    else
                                                        stringBuilder.Append($"<span class=\"eq\">{_parser.ToHtml()}</span>");

                                                }
                                            }
                                        }
                                        catch (MathParser.MathParserException ex)
                                        {
                                            string errText;
                                            if (!calculate && token.Value.Contains('?'))
                                                errText = token.Value.Replace("?", "<input type=\"text\" size=\"2\" name=\"Var\">");
                                            else
                                                errText = token.Value;
#if BG
                                            errText = $"<p class=\"err\">Грешка в \"{errText}\" на ред {Line(line)}: {ex.Message}</p>";
#else      
                                            errText = $"<p class=\"err\">Error in \"{errText}\" on line {Line(line)}: {ex.Message}</p>";
#endif
                                            stringBuilder.Append(errText);
                                        }
                                    }
                                    else if (isVisible)
                                        stringBuilder.Append(token.Value);
                                }
                                if (isOutput)
                                {
                                    if (lineType == TokenTypes.Heading)
                                        stringBuilder.Append("</h3>");
                                    else if (lineType == TokenTypes.Text && isSubCall)
                                        stringBuilder.Append($"<span{id}>");
                                    else if (lineType != TokenTypes.Html)
                                        stringBuilder.Append("</p>");

                                    if (keyword == Keywords.If || keyword == Keywords.ElseIf)
                                        stringBuilder.Append("<div class = \"indent\">");

                                    stringBuilder.AppendLine();
                                }
                                if (condition.IsUnchecked)
                                {
                                    if (calculate)
                                        condition.Check(_parser.Result);
                                    else
                                        condition.Check();
                                }
                            }
                        }
                        else if (calculate)
                            PurgeObsoleteInput(s);
                    }
                }
                ApplyUnits(stringBuilder, calculate);
                if (condition.Id > 0 && line == end)
#if BG
                    stringBuilder.Append($"<p class=\"err\">Грешка: Условният \"#if\" блок не е затворен. Липсва \"#end if\"</p>");
#else
                    stringBuilder.Append($"<p class=\"err\">Error: \"#if\" block not closed. Missing \"#end if\"</p>");
#endif
                if (loops.Any())
#if BG
                    stringBuilder.Append($"<p class=\"err\">Грешка: Блокът за цикъл \"#repeat\" не е затворен. Липсва \"#loop\"</p>");
#else
                    stringBuilder.Append($"<p class=\"err\">Error: \"#repeat\" block not closed. Missing \"#loop\"</p>");
                if (_definitionMacrosStack.Any())
                    // TODO: BG
                    stringBuilder.Append($"<p class=\"err\">Error: \"#def\" block not closed. Missing \"#end def\"</p>");

#endif
            }
            catch (MathParser.MathParserException ex)
            {
#if BG
                stringBuilder.Append($"<p class=\"err\">Грешка в \"{s}\" на ред {Line(line)}: {ex.Message}</p>");
#else
                stringBuilder.Append($"<p class=\"err\">Error in \"{s}\" on line {Line(line)}: {ex.Message}</p>");
#endif
            }
            catch (Exception ex)
            {
#if BG
                stringBuilder.Append($"<p class=\"err\">Неочаквана грешка: {ex.Message} Моля проверете коректността на израза.</p>");
#else
                stringBuilder.Append($"<p class=\"err\">Unexpected error: {ex.Message} Please check the expression consistency.</p>");
#endif
            }
            finally
            {
                if (!isSubCall)
                {
                    _parser = null;
                    _callStack.Clear();
                    _definedMacros.Clear();
                    _definitionMacrosStack.Clear();
                }
                HtmlResult = stringBuilder.ToString();
            }

            static string Line(int line) => $"[<a href=\"#0\" data-text=\"{line}\">{line}</a>]";

        }

        private static string InsertAttribute(string s, string attr)
        {
            if (s.StartsWith('<') && s.Length > 2)
            {
                if (char.IsLetter(s[1]))
                {
                    var i = s.IndexOf('>');
                    if (i > 1)
                        return s[..i] + attr + s[i..];
                }
            }
            return s;
        }

        private void ApplyUnits(StringBuilder sb, bool calculate)
        {
            string unitsHtml;
            if (calculate)
                unitsHtml = Settings.Units;
            else
                unitsHtml = "<span class=\"Units\">" + Settings.Units + "</span>";

            long len = sb.Length;
            sb.Replace("%u", unitsHtml);
            if (calculate || sb.Length == len)
                return;

            sb.Insert(0, "<select id=\"Units\" name=\"Units\"><option value=\"m\"> m </option><option value=\"cm\"> cm </option><option value=\"mm\"> mm </option></select>");
        }

        private double UnitsFactor()
        {
            return Settings.Units switch
            {
                "mm" => 1000,
                "cm" => 100,
                "m" => 1,
                _ => 0
            };
        }

        private void PurgeObsoleteInput(string s)
        {
            var isExpression = true;
            for (int i = 0, len = s.Length; i < len; ++i)
            {
                var c = s[i];
                if (c == '\'' || c == '\"')
                    isExpression = !isExpression;
                else if (c == '?' && isExpression)
                    GetInputField();
            }
        }

        private List<Token> GetInput(string s, int startIndex)
        {
            var tokens = new List<Token>();
            var stringBuilder = new StringBuilder();
            var currentSeparator = ' ';
            for (int i = startIndex, len = s.Length; i < len; ++i)
            {
                var c = s[i];
                if (c == '\'' || c == '\"')
                {
                    if (currentSeparator == ' ' || currentSeparator == c)
                    {
                        if (stringBuilder.Length != 0)
                        {
                            AddToken(tokens, stringBuilder.ToString(), currentSeparator);
                            stringBuilder.Clear();
                        }
                        if (currentSeparator == c)
                            currentSeparator = ' ';
                        else
                            currentSeparator = c;
                    }
                    else if (currentSeparator != ' ')
                        stringBuilder.Append(c);
                }
                else
                    stringBuilder.Append(c);
            }
            if (stringBuilder.Length != 0)
                AddToken(tokens, stringBuilder.ToString(), currentSeparator);

            return tokens;
        }

        private void AddToken(List<Token> tokens, string value, char separator)
        {
            var tokenValue = value;
            var tokenType = GetTokenType(separator);

            if (tokenType == TokenTypes.Expression)
            {
                if (string.IsNullOrWhiteSpace(tokenValue))
                    return;
            }
            else if (!_isVal)
            {
                if (!tokens.Any())
                    tokenValue += ' ';
                else
                    tokenValue = ' ' + tokenValue + ' ';
            }

            var token = new Token(tokenValue, tokenType);
            if (token.Type == TokenTypes.Text)
            {
                tokenValue = tokenValue.TrimStart();
                if (tokenValue.Length > 0 && tokenValue[0] == '<')
                    token.Type = TokenTypes.Html;
            }
            tokens.Add(token);
        }

        private static TokenTypes GetTokenType(char separator)
        {
            return separator switch
            {
                ' ' => TokenTypes.Expression,
                '\"' => TokenTypes.Heading,
                '\'' => TokenTypes.Text,
                _ => TokenTypes.Error,
            };
        }

        private struct Token
        {
            internal string Value { get; }
            internal TokenTypes Type;
            internal Token(string value, TokenTypes type)
            {
                Value = value;
                Type = type;
            }
        }

        private enum TokenTypes
        {
            Expression,
            Heading,
            Text,
            Html,
            Error
        }

        private class ConditionParser
        {
            private enum Types
            {
                None,
                If,
                ElseIf,
                Else,
                EndIf
            }
            private readonly struct Item
            {
                internal bool Value { get; }
                internal Types Type { get; }
                internal Item(bool value, Types type)
                {
                    Type = type;
                    Value = value;
                }
            }

            private int _count;
            private string _keyword;
            private int _keywordLength;
            private readonly Item[] _conditions = new Item[20];
            private Types Type => _conditions[Id].Type;
            internal int Id { get; private set; }
            internal bool IsUnchecked { get; private set; }
            internal bool IsSatisfied => _conditions[_count].Value;
            internal bool IsFound { get; private set; }
            internal int KeyWordLength => _keywordLength;

            internal ConditionParser()
            {
                _conditions[0] = new Item(true, Types.None);
                _keyword = string.Empty;
            }
            private void Add(bool value)
            {
                ++Id;
                _conditions[Id] = new Item(value, Types.If);
                if (IsSatisfied)
                {
                    ++_count;
                    IsFound = false;
                }
            }

            private void Remove()
            {
                --Id;
                if (_count > Id)
                {
                    --_count;
                    IsFound = true;

                }
            }

            private void Change(bool value, Types type)
            {
                _conditions[Id] = new Item(value, type);
            }

            internal void SetCondition(int index)
            {
                if (index < 0 || index >= (int)Types.EndIf)
                {
                    if (_keywordLength > 0)
                    {
                        _keywordLength = 0;
                        _keyword = string.Empty;
                    }
                    return;
                }

                var type = (Types)(index + 1);
                _keywordLength = GetKeywordLength(type);
                _keyword = GetKeyword(type);
                IsUnchecked = type == Types.If || type == Types.ElseIf;
                if (type > Types.If && _count == 0)
#if BG
                    throw new MathParser.MathParserException("Условният блок не е инициализиран с \"#if\".");
#else                    
                    throw new MathParser.MathParserException("Condition block not initialized with \"#if\".");
#endif
                if (Type == Types.Else)
                {
                    if (type == Types.Else)
#if BG
                        throw new MathParser.MathParserException("Може да има само едно \"#else\" в условен блок.");
#else                         
                        throw new MathParser.MathParserException("Duplicate \"#else\" in condition block.");
#endif
                    if (type == Types.ElseIf)
#if BG
                        throw new MathParser.MathParserException("Не може да има \"#else if\" след \"#else\" в условен блок.");
#else                             
                        throw new MathParser.MathParserException("\"#else if\" is not allowed after \"#else\" in condition block.");
#endif
                }
                switch (type)
                {
                    case Types.If:
                        Add(true);
                        break;
                    case Types.ElseIf:
                        Change(true, Types.If);
                        break;
                    case Types.Else:
                        Change(!IsFound, type);
                        break;
                    case Types.EndIf:
                        Remove();
                        break;
                }
            }

            internal void Check(Complex value)
            {
                if (!value.IsReal)
#if BG                    
                    throw new MathParser.MathParserException("Условието не може да бъде комплексно число.");
#else                    
                    throw new MathParser.MathParserException("Condition cannot evaluate to a complex number.");
#endif
                var d = value.Re;
                if (double.IsNaN(d) || double.IsInfinity(d))
#if BG
                    throw new MathParser.MathParserException($"Невалиден резултат от проверка на условие: {d}.");
#else
                    throw new MathParser.MathParserException($"Condition result is invalid: {d}.");
#endif
                var result = Math.Abs(d) > 1e-12;
                if (result)
                    IsFound = true;
                Change(result, Type);
                IsUnchecked = false;
            }

            internal void Check() => IsUnchecked = false;

            public override string ToString() => _keyword;

            internal string ToHtml()
            {
                if (string.IsNullOrEmpty(_keyword))
                    return _keyword;
                return "<span class=\"cond\">" + _keyword + "</span>";
            }

            private static int GetKeywordLength(Types type)
            {
                return type switch
                {
                    Types.If => 3,
                    Types.ElseIf => 8,
                    Types.Else => 5,
                    Types.EndIf => 7,
                    _ => 0,
                };
            }
            private static string GetKeyword(Types type)
            {
                return type switch
                {
                    Types.If => "#if ",
                    Types.ElseIf => "#else if ",
                    Types.Else => "#else",
                    Types.EndIf => "#end if",
                    _ => string.Empty,
                };
            }
        }

        private class Loop
        {
            private readonly int _startLine;
            private int _iteration;
            internal int Id { get; }
            internal int Iteration => _iteration;
            internal Loop(int startLine, int count, int id)
            {
                _startLine = startLine;
                if (count < 0)
                    count = 100000;

                _iteration = count;
                Id = id;
            }

            internal bool Iterate(ref int currentLine)
            {
                if (_iteration <= 1)
                    return false;

                currentLine = _startLine;
                --_iteration;
                return true;
            }

            internal void Disable() => _iteration = 0;  
        }

        private class Macros
        {
            internal int DefLine { get; private set; }
            internal int EndDefLine { get; set; }
            internal string[] ArgumentNames { get; private set; }
            internal Macros(int defLine, string[] argumentNames)
            {
                DefLine = defLine;
                ArgumentNames = argumentNames;
            }

            internal string[] ReplaceArgumentInBody(string[] argumentValues, string[] expressions)
            {
                var localExpressions = expressions.ToArray();
                for (var j = DefLine; j < EndDefLine - 1; ++j)
                {
                    var commentSep = '\0';
                    var newExpression = new StringBuilder();
                    var commentGroup = new StringBuilder();
                    foreach (var value in expressions[j])
                    {
                        if (commentSep == '\0' && "'\"".Contains(value))
                        {
                            commentSep = value;
                            commentGroup = ReplaceArguments(argumentValues, commentGroup);
                            newExpression.Append(commentGroup);
                            commentGroup.Clear();
                        }
                        else if (value == commentSep)
                        {
                            newExpression.Append(commentGroup);
                            commentGroup.Clear();
                            commentSep = '\0';
                        }
                        commentGroup.Append(value);
                    }

                    if (commentSep == '\0')
                    {
                        commentGroup = ReplaceArguments(argumentValues, commentGroup);
                        newExpression.Append(commentGroup);
                    }
                    else
                    {
                        newExpression.Append(commentGroup);
                    }
                    localExpressions[j] = newExpression.ToString();
                }

                return localExpressions;
            }
            private StringBuilder ReplaceArguments(string[] argumentValues, StringBuilder str) => Enumerable
                .Range(0, argumentValues.Length)
                .OrderByDescending(x => ArgumentNames[x].Length)  // don't overwrite 
                .Aggregate(str,
                    (current, index)
                        => current.Replace(ArgumentNames[index], argumentValues[index]));
        }
    }
}
