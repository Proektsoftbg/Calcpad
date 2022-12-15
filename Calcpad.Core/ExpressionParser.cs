using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Calcpad.Core
{
    public class ExpressionParser
    {
        private enum Keyword
        {
            None,
            Hide,
            Show,
            Pre,
            Post,
            Val,
            Equ,
            Noc,
            Deg,
            Rad,
            Gra,
            Repeat,
            Loop,
            Break,
            Continue,
            If,
            ElseIf,
            Else,
            EndIf,
            Local,
            Global,
            Round
        }

        private enum KeywordResult
        {
            None,
            Continue,
            Break
        }

        private int _isVal;
        private MathParser _parser;
        //private static readonly string[] NewLines = { "\r\n", "\r", "\n" };
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

        public void Parse(string sourceCode, bool calculate = true) =>
            Parse(sourceCode.AsSpan(), calculate);

        private static Keyword GetKeyword(ReadOnlySpan<char> s)
        {
            if (s[1] == 'i')
            {
                if (s[2] == 'f')
                    return Keyword.If;
                else
                    return Keyword.None;
            }
            var n = s.Length;
            if (n < 4)
                return Keyword.None;
            var c1 = s[1];
            var c2 = s[2];
            var c3 = s[3];
            if (c1 == 'e')
            {
                if (c2 == 'l')
                {
                    if (c3   == 's' && n > 4 &&
                        s[4] == 'e')
                    {
                        if (n > 7 && 
                            s[5] == ' ' && 
                            s[6] == 'i' &&
                            s[7] == 'f')
                            return Keyword.ElseIf;

                        return Keyword.Else;
                    }
                }
                else if (c2 == 'n')
                {
                    if (c3   == 'd' && n > 6 &&
                        s[4] == ' ' && 
                        s[5] == 'i' &&
                        s[6] == 'f')
                        return Keyword.EndIf;
                }
                else if (c2 == 'q' &&
                         c3 == 'u')
                    return Keyword.Equ;
            }
            else if (c1 == 'p')
            {
                if (c2 == 'r')
                {
                    if(c3 == 'e')
                        return Keyword.Pre;
                }
                else if (c2   == 'o' && 
                         c3   == 's' && n > 4 &&
                         s[4] == 't')
                        return Keyword.Post;
            }
            else if (c1 == 'g')
            {
                if (c2 == 'r')
                {
                    if (c3 == 'a')
                        return Keyword.Gra;
                }
                else if (c2   == 'l' &&
                         c3   == 'o' && n > 6 &&
                         s[4] == 'b' &&
                         s[5] == 'a' &&
                         s[6] == 'l')
                         return Keyword.Global;
            }
            else if (c1 == 'r')
            {
                if (c2 == 'a')
                {
                    if (c3 == 'd')
                        return Keyword.Rad;
                }
                else if (c2 == 'o')
                {
                    if (c3 == 'u' && n > 5 &&
                         s[4] == 'n' &&
                         s[5] == 'd')
                        return Keyword.Round;
                }
                else if (c2   == 'e' &&
                         c3   == 'p' && n > 6 &&
                         s[4] == 'e' &&
                         s[5] == 'a' &&
                         s[6] == 't')
                         return Keyword.Repeat;
            }
            else if (c1 == 'l' &&
                     c2 == 'o')
            {
                if (c3 == 'o')
                {
                    if (n > 4 &&
                        s[4] == 'p')
                        return Keyword.Loop;
                }
                else if (c3 == 'c' && n > 5 &&
                         s[4] == 'a' &&
                         s[5] == 'l')
                    return Keyword.Local;
            }
            else
            {
                if (s.StartsWith("#val", StringComparison.Ordinal))
                    return Keyword.Val;
                if (s.StartsWith("#noc", StringComparison.Ordinal))
                    return Keyword.Noc;
                if (s.StartsWith("#hide", StringComparison.Ordinal))
                    return Keyword.Hide;
                if (s.StartsWith("#show", StringComparison.Ordinal))
                    return Keyword.Show;
                if (s.StartsWith("#deg", StringComparison.Ordinal))
                    return Keyword.Deg;
                if (s.StartsWith("#break", StringComparison.Ordinal))
                    return Keyword.Break;
                if (s.StartsWith("#continue", StringComparison.Ordinal))
                    return Keyword.Continue;
            }

            return Keyword.None;
        }

        public void Cancel() => _parser?.Cancel();

        private void Parse(ReadOnlySpan<char> code, bool calculate = true)
        {
            var stringBuilder = new StringBuilder();
            var conditions = new ConditionParser();
            var loops = new Stack<Loop>();

            var lines = new List<int>{0};
            var len = code.Length;
            for (int i = 0; i < len; ++i)
                if (code[i] == '\n')
                    lines.Add(i + 1);

            var lineCount = lines.Count;
            lines.Add(len);
            var line = -1;   
            _parser = new MathParser(Settings.Math)
            {
                IsEnabled = calculate
            };
            _parser.SetVariable("Units", new Value(UnitsFactor()));
            _isVal = 0;
            var isVisible = true;
            var s = ReadOnlySpan<char>.Empty;
            try
            {
                while (++line < lineCount)
                {
                    var expr = code[lines[line]..lines[line + 1]];
                    var htmlId = loops.Any() && loops.Peek().Iteration != 1 ? 
                        string.Empty : 
                        $" id=\"line{line + 1}\"";
                    if (_parser.IsCanceled)
                        break;

                    s = expr.Trim();
                    if (s.IsEmpty)
                    {
                        if (isVisible)
                            stringBuilder.AppendLine($"<p{htmlId}>&nbsp;</p>");

                        continue;
                    }
                    var result = ParseKeyword(s, htmlId, out Keyword keyword);
                    if (result == KeywordResult.Continue)
                        continue;
                    else if (result == KeywordResult.Break)
                        break;

                    if (!ParsePlot(s, htmlId))
                    {
                        if (ParseCondition(s, keyword, htmlId))
                            ParseExpression(s, keyword, htmlId);
                    }
                }                    
                ApplyUnits(stringBuilder, calculate);
                if (conditions.Id > 0 && line == lineCount)
#if BG
                    stringBuilder.Append(ErrHtml($"Грешка: Условният \"#if\" блок не е затворен. Липсва \"#end if\"."));
#else
                    stringBuilder.Append(ErrHtml($"Error: \"#if\" block not closed. Missing \"#end if\"."));
#endif
                if (loops.Any())
#if BG
                    stringBuilder.Append(ErrHtml($"Грешка: Блокът за цикъл \"#repeat\" не е затворен. Липсва \"#loop\"."));
#else
                    stringBuilder.Append(ErrHtml($"<p class=\"err\">Error: \"#repeat\" block not closed. Missing \"#loop\"."));
#endif
            }
            catch (MathParser.MathParserException ex)
            {
                AppendError(s.ToString(), ex.Message);
            }
            catch (Exception ex)
            {
#if BG
                stringBuilder.Append(ErrHtml($"Неочаквана грешка: {ex.Message} Моля проверете коректността на израза."));
#else
                stringBuilder.Append(ErrHtml($"Unexpected error: {ex.Message} Please check the expression consistency."));
#endif
            }
            finally
            {
                HtmlResult = stringBuilder.ToString();
                _parser = null;
            }

            void AppendError(string lineContent, string text) =>
#if BG
                stringBuilder.Append(ErrHtml($"Грешка в \"{lineContent}\" на ред {LineHtml(line)}: {text}</p>"));
#else
                stringBuilder.Append(ErrHtml($"Error in \"{lineContent}\" on line {LineHtml(line)}: {text}</p>"));
#endif
            static string ErrHtml(string text) => $"<p class=\"err\">{text}</p>";
            static string LineHtml(int line) => $"[<a href=\"#0\" data-text=\"{line + 1}\">{line + 1}</a>]";

            KeywordResult ParseKeyword(ReadOnlySpan<char> s, string htmlId, out Keyword keyword)
            {
                keyword = Keyword.None;
                if (s[0] == '#')
                {
                    keyword = GetKeyword(s);
                    if (keyword == Keyword.Hide)
                        isVisible = false;
                    else if (keyword == Keyword.Show)
                        isVisible = true;
                    else if (keyword == Keyword.Pre)
                        isVisible = !calculate;
                    else if (keyword == Keyword.Post)
                        isVisible = calculate;
                    else if (keyword == Keyword.Val)
                        _isVal = 1;
                    else if (keyword == Keyword.Equ)
                        _isVal = 0;
                    else if (keyword == Keyword.Noc)
                        _isVal = -1;
                    else if (keyword == Keyword.Deg)
                        _parser.Degrees = 0;
                    else if (keyword == Keyword.Rad)
                        _parser.Degrees = 1;
                    else if (keyword == Keyword.Gra)
                        _parser.Degrees = 2;
                    else if (keyword == Keyword.Round)
                        ParseKeywordRound(s);
                    else if (keyword == Keyword.Repeat)
                        ParseKeywordRepeat(s, htmlId);
                    else if (keyword == Keyword.Loop)
                        ParseKeywordLoop(s, htmlId);
                    else if (keyword == Keyword.Break)
                    {   
                        if (ParseKeywordBreak(s, htmlId))
                            return KeywordResult.Break;
                    }
                    else if (keyword == Keyword.Continue)
                        ParseKeywordContinue(s, htmlId);
                    else if (keyword != Keyword.Global && keyword != Keyword.Local)
                        return KeywordResult.None;

                    return KeywordResult.Continue;
                }
                return KeywordResult.None;
            }

            void ParseKeywordRound(ReadOnlySpan<char> s)
            {
                if (s.Length > 6)
                {
                    var expr = s[6..].Trim();
                    if (int.TryParse(expr, out int n))
                        Settings.Math.Decimals = n;
                    else
                    {
                        try
                        {
                            _parser.Parse(expr);
                            _parser.Calculate();
                            Settings.Math.Decimals = (int)Math.Round(_parser.Real);
                        }
                        catch (MathParser.MathParserException ex)
                        {
                            AppendError(s.ToString(), ex.Message);
                        }
                    }
                }
            }

            void ParseKeywordRepeat(ReadOnlySpan<char> s, string htmlId)
            {
                ReadOnlySpan<char> expression = s.Length > 7 ?
                    s[7..].Trim() :
                    Span<char>.Empty;

                if (calculate)
                {
                    if (conditions.IsSatisfied)
                    {
                        var count = 0;
                        if (!expression.IsWhiteSpace())
                        {
                            try
                            {
                                _parser.Parse(expression);
                                _parser.Calculate();
                                if (_parser.Real > int.MaxValue)
#if BG
                                    AppendError($"Броят на итерациите е по-голям от максималния {int.MaxValue}.</p>");
#else
                                    AppendError(s.ToString(), $"Number of iterations exceeds the maximum {int.MaxValue}.</p>");
#endif
                                else
                                    count = (int)Math.Round(_parser.Real);
                            }
                            catch (MathParser.MathParserException ex)
                            {
                                AppendError(s.ToString(), ex.Message);
                            }
                        }
                        else
                            count = -1;

                        loops.Push(new Loop(line, count, conditions.Id));
                    }
                }
                else if (isVisible)
                {
                    if (expression.IsWhiteSpace())
                        stringBuilder.Append($"<p{htmlId} class=\"cond\">#repeat</p><div class=\"indent\">");
                    else
                    {
                        try
                        {
                            _parser.Parse(expression);
                            stringBuilder.Append($"<p{htmlId}><span class=\"cond\">#repeat</span> {_parser.ToHtml()}</p><div class=\"indent\">");
                        }
                        catch (MathParser.MathParserException ex)
                        {
                            AppendError(s.ToString(), ex.Message);
                        }
                    }
                }
            }

            void ParseKeywordLoop(ReadOnlySpan<char> s, string htmlId)
            {
                if (calculate)
                {
                    if (conditions.IsSatisfied)
                    {
                        if (!loops.Any())
#if BG
                            AppendError("\"#loop\" без съответен \"#repeat\".");
#else
                            AppendError(s.ToString(), "\"#loop\" without a corresponding \"#repeat\".");
#endif
                        else if (loops.Peek().Id != conditions.Id)
#if BG
                            AppendError("Преплитане на \"#if - #end if\" и \"#repeat - #loop\" блокове.");
#else
                            AppendError(s.ToString(), "Entangled \"#if - #end if\" and \"#repeat - #loop\" blocks.");
#endif
                        else if (!loops.Peek().Iterate(ref line))
                            loops.Pop();
                    }
                }
                else if (isVisible)
                    stringBuilder.Append($"</div><p{htmlId} class=\"cond\">#loop</p>");
            }

            bool ParseKeywordBreak(ReadOnlySpan<char> s, string htmlId)
            {
                if (calculate)
                {
                    if (conditions.IsSatisfied)
                    {
                        if (loops.Any())
                            loops.Peek().Break();
                        else
                            return true;
                    }
                }
                else if (isVisible)
                    stringBuilder.Append($"<p{htmlId} class=\"cond\">#break</p>");

                return false;
            }

            void ParseKeywordContinue(ReadOnlySpan<char> s, string htmlId)
            {
                const int RemoveCondition = Keyword.EndIf - Keyword.If;
                if (calculate)
                {
                    if (conditions.IsSatisfied)
                    {
                        if (!loops.Any())
#if BG
                            AppendError("\"#continue\" без съответен \"#repeat\".");
#else
                            AppendError(s.ToString(), "\"#continue\" without a corresponding \"#repeat\".");
#endif
                        else
                        {
                            var loop = loops.Peek();
                            while (conditions.Id > loop.Id)
                                conditions.SetCondition(RemoveCondition);
                            loop.Iterate(ref line);
                        }

                    }
                }
                else if (isVisible)
                    stringBuilder.Append($"<p{htmlId} class=\"cond\">#continue</p>");
            }

            bool ParsePlot(ReadOnlySpan<char> s, string htmlId)
            {

                if (s.StartsWith("$plot", StringComparison.OrdinalIgnoreCase) ||
                    s.StartsWith("$map", StringComparison.OrdinalIgnoreCase))
                {
                    if (isVisible && (conditions.IsSatisfied || !calculate))
                    {
                        PlotParser plotParser;
                        if (s.StartsWith("$p", StringComparison.OrdinalIgnoreCase))
                            plotParser = new ChartParser(_parser, Settings.Plot);
                        else
                            plotParser = new MapParser(_parser, Settings.Plot);

                        try
                        {
                            _parser.IsPlotting = true;
                            var s1 = plotParser.Parse(s, calculate);
                            stringBuilder.Append(InsertAttribute(s1, htmlId));
                            _parser.IsPlotting = false;
                        }
                        catch (MathParser.MathParserException ex)
                        {
                            AppendError(s.ToString(), ex.Message);
                        }
                    }
                    return true;
                }
                return false;
            }

            bool ParseCondition(ReadOnlySpan<char> s, Keyword keyword, string htmlId)
            {
                conditions.SetCondition(keyword - Keyword.If);
                if (conditions.IsSatisfied && !(loops.Any() && loops.Peek().IsBroken) || !calculate)
                {
                    if (conditions.KeywordLength == s.Length)
                    {
                        if (conditions.IsUnchecked)
#if BG
                            throw new MathParser.MathParserException("Условието не може да бъде празно.");
#else
                            throw new MathParser.MathParserException("Condition cannot be empty.");
#endif
                        if (isVisible && !calculate)
                        {
                            if (keyword == Keyword.Else)
                                stringBuilder.Append($"</div><p{htmlId}>{conditions.ToHtml()}</p><div class = \"indent\">");
                            else
                                stringBuilder.Append($"</div><p{htmlId}>{conditions.ToHtml()}</p>");
                        }
                    }
                    else if (conditions.KeywordLength > 0 &&
                             conditions.IsFound &&
                             conditions.IsUnchecked &&
                             calculate)
                        conditions.Check(0.0);
                    else
                        return true;
                }
                return false;
            }

            void ParseExpression(ReadOnlySpan<char> s, Keyword keyword, string htmlId)
            {
                var kwdLength = conditions.KeywordLength;
                var tokens = GetInput(s[kwdLength..]);
                var lineType = tokens.Any() ?
                    tokens[0].Type :
                    TokenTypes.Text;
                var isOutput = isVisible && (!calculate || kwdLength == 0);
                bool isIndent = keyword == Keyword.ElseIf || keyword == Keyword.EndIf;
                AppendHtmlLineStart(isOutput, lineType, isIndent, htmlId);
                if (isOutput)
                {
                    if (lineType == TokenTypes.Html)
                        tokens[0] = new Token(InsertAttribute(tokens[0].Value, htmlId), TokenTypes.Html);

                    if (kwdLength > 0 && !calculate)
                        stringBuilder.Append(conditions.ToHtml());
                }
                ParseTokens(tokens, isOutput);
                AppendHtmlLineEnd(isOutput, lineType, isIndent);
                if (conditions.IsUnchecked)
                {
                    if (calculate)
                        conditions.Check(_parser.Result);
                    else
                        conditions.Check();
                }
            }

            void AppendHtmlLineStart(bool isOutput, TokenTypes lineType, bool isIndent, string htmlId)
            {
                if (isOutput)
                {
                    if (isIndent)
                        stringBuilder.Append("</div>");

                    if (lineType == TokenTypes.Heading)
                        stringBuilder.Append($"<h3{htmlId}>");
                    else if (lineType != TokenTypes.Html)
                        stringBuilder.Append($"<p{htmlId}>");
                }
            }


            void ParseTokens(List<Token> tokens, bool isOutput)
            {
                foreach (var token in tokens)
                {
                    if (token.Type == TokenTypes.Expression)
                    {
                        try
                        {
                            _parser.Parse(token.Value);
                            if (calculate && _isVal > -1)
                                _parser.Calculate();

                            if (isOutput)
                            {
                                if (_isVal == 1 & calculate)
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
                            if (!calculate && token.Value.Contains('?', StringComparison.Ordinal))
                                errText = token.Value.Replace("?", "<input type=\"text\" size=\"2\" name=\"Var\">");
                            else
                                errText = token.Value;
#if BG
                                            errText = $"Грешка в \"{errText}\" на ред {LineHtml(line)}: {ex.Message}";
#else
                            errText = $"Error in \"{errText}\" on line {LineHtml(line)}: {ex.Message}";
#endif
                            stringBuilder.Append(ErrHtml(errText));
                        }
                    }
                    else if (isVisible)
                        stringBuilder.Append(token.Value);
                }
            }

            void AppendHtmlLineEnd(bool isOutput, TokenTypes lineType, bool indent)
            {
                if (isOutput)
                {
                    if (lineType == TokenTypes.Heading)
                        stringBuilder.Append("</h3>");
                    else if (lineType != TokenTypes.Html)
                        stringBuilder.Append("</p>");

                    if (indent)
                        stringBuilder.Append("<div class = \"indent\">");

                    stringBuilder.AppendLine();
                }
            }
        }

        private static string InsertAttribute(ReadOnlySpan<char> s, string attr)
        {
            if (s.Length > 2 && s[0] == '<' && char.IsLetter(s[1]))
            {
                var i = s.IndexOf('>');
                if (i > 1)
                {
                    var j = i;
                    while (j > 1)
                    {
                        --j;
                        if (s[j] != ' ')
                        {
                            if (s[j] == '/')
                                i = j;

                            break;
                        }
                    };
                    return s[..i].ToString() + attr + s[i..].ToString();
                }
            }
            return s.ToString();
        }

        private void ApplyUnits(StringBuilder sb, bool calculate)
        {
            string unitsHtml = calculate ?
                Settings.Units :
                "<span class=\"Units\">" + Settings.Units + "</span>";

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

        private List<Token> GetInput(ReadOnlySpan<char> s)
        {
            var tokens = new List<Token>();
            var stringBuilder = new StringBuilder();
            var currentSeparator = ' ';
            foreach (char c in s)
            {
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

        private void AddToken(List<Token> tokens, ReadOnlySpan<char> value, char separator)
        {
            var tokenValue = value.ToString();
            var tokenType = GetTokenType(separator);

            if (tokenType == TokenTypes.Expression)
            {
                if (value.IsWhiteSpace())
                    return;
            }
            else if (_isVal < 1)
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
            internal int KeywordLength => _keywordLength;

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

            internal void Break() => _iteration = 0;

            internal bool IsBroken => _iteration == 0;  
        }
    }
}
