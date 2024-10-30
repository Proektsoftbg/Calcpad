using System;

namespace Calcpad.Core
{
    public partial class ExpressionParser
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
            VarSub,
            NoSub,
            NoVar,
            Split,
            Wrap,
            Deg,
            Rad,
            Gra,
            If,
            ElseIf,
            Else,
            EndIf,
            While,
            For,
            Repeat,
            Loop,
            Break,
            Continue,
            Local,
            Global,
            Round,
            Pause,
            Input
        }
        private enum KeywordResult
        {
            None,
            Continue,
            Break
        }

        private Keyword _previousKeyword = Keyword.None;
        private static string[] KeywordStrings;

        private static void InitKeyWordStrings()
        {
            KeywordStrings = Enum.GetNames(typeof(Keyword));
            for (int i = 0, len = KeywordStrings.Length; i < len; ++i)
                KeywordStrings[i] = '#' + KeywordStrings[i].ToLowerInvariant();

            KeywordStrings[(int)Keyword.ElseIf] = "#else if";
            KeywordStrings[(int)Keyword.EndIf] = "#end if";
        }

        private static Keyword GetKeyword(ReadOnlySpan<char> s)
        {
            for (int i = 1, len = KeywordStrings.Length; i < len; ++i)
                if (s.StartsWith(KeywordStrings[i], StringComparison.OrdinalIgnoreCase))
                    return (Keyword)i;

            return Keyword.None;
        }

        KeywordResult ParseKeyword(ReadOnlySpan<char> s, out Keyword keyword)
        {
            if (s[0] == '#' || _isPausedByUser)
            {
                keyword = _isPausedByUser ? Keyword.Pause : GetKeyword(s);
                if (keyword == Keyword.Hide)
                    _isVisible = false;
                else if (keyword == Keyword.Show)
                    _isVisible = true;
                else if (keyword == Keyword.Pre)
                    _isVisible = !_calculate;
                else if (keyword == Keyword.Post)
                    _isVisible = _calculate;
                else if (keyword == Keyword.Input)
                    return ParseKeywordInput();
                else if (keyword == Keyword.Pause)
                    return ParseKeywordPause();
                else if (keyword == Keyword.Val)
                    _isVal = 1;
                else if (keyword == Keyword.Equ)
                    _isVal = 0;
                else if (keyword == Keyword.Noc)
                    _isVal = -1;
                else if (keyword == Keyword.VarSub)
                    _parser.VariableSubstitution = MathParser.VariableSubstitutionOptions.VariablesAndSubstitutions;
                else if (keyword == Keyword.NoSub)
                    _parser.VariableSubstitution = MathParser.VariableSubstitutionOptions.VariablesOnly;
                else if (keyword == Keyword.NoVar)
                    _parser.VariableSubstitution = MathParser.VariableSubstitutionOptions.SubstitutionsOnly;
                else if (keyword == Keyword.Split)
                    _parser.Split = true;
                else if (keyword == Keyword.Wrap)
                    _parser.Split = false;
                else if (keyword == Keyword.Deg)
                    _parser.Degrees = 0;
                else if (keyword == Keyword.Rad)
                    _parser.Degrees = 1;
                else if (keyword == Keyword.Gra)
                    _parser.Degrees = 2;
                else if (keyword == Keyword.Round)
                    ParseKeywordRound(s);
                else if (keyword == Keyword.Repeat)
                    ParseKeywordRepeat(s);
                else if (keyword == Keyword.For)
                    ParseKeywordFor(s);
                else if (keyword == Keyword.While)
                    ParseKeywordWhile(s);
                else if (keyword == Keyword.Loop)
                    ParseKeywordLoop(s);
                else if (keyword == Keyword.Break)
                {
                    if (ParseKeywordBreak())
                        return KeywordResult.Break;
                }
                else if (keyword == Keyword.Continue)
                    ParseKeywordContinue(s);
                else if (keyword != Keyword.Global && keyword != Keyword.Local)
                    return KeywordResult.None;

                return KeywordResult.Continue;
            }
            keyword = Keyword.None;
            return KeywordResult.None;
        }

        KeywordResult ParseKeywordInput()
        {
            if (_condition.IsSatisfied)
            {
                _previousKeyword = Keyword.Input;
                if (_calculate)
                {
                    _startLine = _currentLine + 1;
                    _pauseCharCount = _sb.Length;
                    _calculate = false;
                    return KeywordResult.Continue;
                }
                return KeywordResult.Break;
            }
            return _calculate ? KeywordResult.Continue : KeywordResult.Break;
        }

        KeywordResult ParseKeywordPause()
        {
            if (_condition.IsSatisfied && (_calculate || _startLine > 0))
            {
                if (_calculate)
                {
                    if (_isPausedByUser)
                        _startLine = _currentLine;
                    else
                        _startLine = _currentLine + 1;
                }

                if (_previousKeyword != Keyword.Input)
                    _pauseCharCount = _sb.Length;

                _previousKeyword = Keyword.Pause;
                _isPausedByUser = false;
                return KeywordResult.Break;
            }
            if (_isVisible && !_calculate)
                _sb.Append($"<p{HtmlId} class=\"cond\">#pause</p>");

            return KeywordResult.Continue;
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
                        Settings.Math.Decimals = (int)Math.Round(_parser.Real, MidpointRounding.AwayFromZero);
                    }
                    catch (MathParser.MathParserException ex)
                    {
                        AppendError(s.ToString(), ex.Message, _currentLine);
                    }
                }
            }
        }

        void ParseKeywordRepeat(ReadOnlySpan<char> s)
        {
            ReadOnlySpan<char> expression = s.Length > 7 ? //#repeat - 7    
                s[7..].Trim() :
                [];

            if (_calculate)
            {
                if (_condition.IsSatisfied)
                {
                    var count = 0;
                    if (!expression.IsWhiteSpace())
                    {
                        try
                        {
                            _parser.Parse(expression);
                            _parser.Calculate();
                            if (_parser.Real > int.MaxValue)
                                AppendError(s.ToString(), string.Format(Messages.Number_of_iterations_exceeds_the_maximum_0, int.MaxValue), _currentLine);
                            else
                                count = (int)Math.Round(_parser.Real, MidpointRounding.AwayFromZero);
                        }
                        catch (MathParser.MathParserException ex)
                        {
                            AppendError(s.ToString(), ex.Message, _currentLine);
                        }
                    }
                    else
                        count = -1;

                    _loops.Push(new RepeatLoop(_currentLine, count, _condition.Id));
                }
            }
            else if (_isVisible)
            {
                if (expression.IsWhiteSpace())
                    _sb.Append($"<p{HtmlId} class=\"cond\">#repeat</p><div class=\"indent\">");
                else
                {
                    try
                    {
                        _parser.Parse(expression);
                        _sb.Append($"<p{HtmlId}><span class=\"cond\">#repeat</span> <span class=\"eq\">{_parser.ToHtml()}</span></p><div class=\"indent\">");
                    }
                    catch (MathParser.MathParserException ex)
                    {
                        AppendError(s.ToString(), ex.Message, _currentLine);
                    }
                }
            }
        }

        void ParseKeywordFor(ReadOnlySpan<char> s)
        {
            ReadOnlySpan<char> expression = s.Length > 4 ? //#for - 4
                s[4..].Trim() :
                [];

            if (expression.IsWhiteSpace())
                Throw.ExpressionEmptyException();

            (int loopStart, int loopEnd) = GetForLoopLimits(expression);
            if (loopStart > -1 &&
                loopEnd > loopStart)
            {
                var varName = expression[..loopStart].Trim().ToString();
                var startExpr = expression[(loopStart + 1)..loopEnd].Trim();
                var endExpr = expression[(loopEnd + 1)..].Trim();
                if (Validator.IsVariable(varName))
                {
                    if (_calculate)
                    {
                        if (_condition.IsSatisfied)
                        {
                            try
                            {
                                _parser.Parse(startExpr);
                                _parser.Calculate();
                                var start = new Value(_parser.Result, _parser.Units);
                                _parser.Parse(endExpr);
                                _parser.Calculate();
                                var end = new Value(_parser.Result, _parser.Units);
                                _loops.Push(new ForLoop(_currentLine, start, end, varName, _condition.Id));
                                _parser.SetVariable(varName, start);
                            }
                            catch (MathParser.MathParserException ex)
                            {
                                AppendError(s.ToString(), ex.Message, _currentLine);
                            }
                        }
                    }
                    else if (_isVisible)
                    {
                        try
                        {
                            var varHtml = new HtmlWriter().FormatVariable(varName, string.Empty);
                            _parser.Parse(startExpr);
                            var startHtml = _parser.ToHtml();
                            _parser.Parse(endExpr);
                            var endHtml = _parser.ToHtml();
                            _sb.Append($"<p{HtmlId}><span class=\"cond\">#for</span> <span class=\"eq\">{varHtml} = {startHtml} : {endHtml}</span></p><div class=\"indent\">");
                        }
                        catch (MathParser.MathParserException ex)
                        {
                            AppendError(s.ToString(), ex.Message, _currentLine);
                        }
                    }
                }
            }
        }

        void ParseKeywordWhile(ReadOnlySpan<char> s)
        {
            ReadOnlySpan<char> expression = s.Length > 6 ? //#while - 6
                s[7..].Trim() :
                [];

            if (expression.IsWhiteSpace())
                Throw.ExpressionEmptyException();

            if (_calculate)
            {
                if (_condition.IsSatisfied)
                {
                    try
                    {
                        _parser.Parse(expression);
                        _parser.Calculate();
                        _condition.SetCondition(Keyword.While - Keyword.If);
                        _condition.Check(_parser.Result);
                        if (_condition.IsSatisfied)
                        {
                            _loops.Push(new WhileLoop(_currentLine, expression.ToString(), _condition.Id));
                            ParseWhileResidualExpressions(expression, false);
                        }
                    }
                    catch (MathParser.MathParserException ex)
                    {
                        AppendError(s.ToString(), ex.Message, _currentLine);
                    }
                }
            }
            else if (_isVisible)
            {
                try
                {
                    _sb.Append($"<p{HtmlId}><span class=\"cond\">#while</span> ");
                    ParseTokens(GetTokens(expression), true, false);
                    _sb.Append("</p><div class=\"indent\">");
                }
                catch (MathParser.MathParserException ex)
                {
                    AppendError(s.ToString(), ex.Message, _currentLine);
                }
            }
        }

        void ParseKeywordLoop(ReadOnlySpan<char> s)
        {
            if (_calculate)
            {
                if (_condition.IsSatisfied)
                {
                    if (_loops.Count == 0)
                        AppendError(s.ToString(), Messages.loop_without_a_corresponding_repeat, _currentLine);
                    else
                    {
                        var next = _loops.Peek();
                        if (next.Id != _condition.Id)
                            AppendError(s.ToString(), Messages.Entangled_if__end_if__and_repeat__loop_blocks, _currentLine);
                        else
                        {
                            if (next is ForLoop forLoop)
                            {
                                var value = _parser.GetVariable(forLoop.VarName);
                                var delta = forLoop.End - forLoop.Start;
                                value += new Value(new Complex(Math.Sign(delta.Re), Math.Sign(delta.Im)), delta.Units);
                                _parser.SetVariable(forLoop.VarName, value);

                            }
                            else if (next is WhileLoop whileLoop)
                            {
                                var condition = whileLoop.Condition.AsSpan();
                                _parser.Parse(condition);
                                _parser.Calculate();
                                _condition.Check(_parser.Result);
                                if (_condition.IsSatisfied)
                                    ParseWhileResidualExpressions(condition, false);
                                else
                                {
                                    _condition.SetCondition(Condition.RemoveConditionKeyword);
                                    next.Break();
                                }
                            }
                            if (next.Iterate(ref _currentLine))
                                _parser.ResetStack();
                            else
                                _loops.Pop();
                        }
                    }
                }
                else if (_condition.IsLoop)
                    _condition.SetCondition(Condition.RemoveConditionKeyword);
            }
            else if (_isVisible)
                _sb.Append($"</div><p{HtmlId} class=\"cond\">#loop</p>");
        }

        private void ParseWhileResidualExpressions(ReadOnlySpan<char> condition, bool isOutput)
        {
            var commentIndex = condition.IndexOfAny(['\'', '"']);
            if (commentIndex > -1 && commentIndex < condition.Length - 1)
            {
                condition = condition[commentIndex..];
                var tokens = GetTokens(condition);
                ParseTokens(tokens, isOutput, false);
            }
        }

        private bool ParseKeywordBreak()
        {
            if (_calculate)
            {
                if (_condition.IsSatisfied)
                {
                    if (_loops.Count != 0)
                        _loops.Peek().Break();
                    else
                        return true;
                }
            }
            else if (_isVisible)
                _sb.Append($"<p{HtmlId} class=\"cond\">#break</p>");

            return false;
        }

        void ParseKeywordContinue(ReadOnlySpan<char> s)
        {
            if (_calculate)
            {
                if (_condition.IsSatisfied)
                {
                    if (_loops.Count == 0)
                        AppendError(s.ToString(), Messages.continue_without_a_corresponding_repeat, _currentLine);
                    else
                    {
                        var loop = _loops.Peek();
                        while (_condition.Id > loop.Id)
                            _condition.SetCondition(Condition.RemoveConditionKeyword);
                        loop.Iterate(ref _currentLine);
                    }

                }
            }
            else if (_isVisible)
                _sb.Append($"<p{HtmlId} class=\"cond\">#continue</p>");
        }

        private static (int, int) GetForLoopLimits(ReadOnlySpan<char> expression)
        {
            (int start, int end) = (-1, -1);
            int n1 = 0, n2 = 0, n3 = 0;
            for (int i = 0, len = expression.Length; i < len; ++i)
            {
                switch (expression[i])
                {
                    case '=': start = i; break;
                    case ':' when n1 == 0 && n2 == 0 && n3 == 0: end = i; return (start, end);
                    case '(': ++n1; break;
                    case ')': --n1; break;
                    case '{': ++n2; break;
                    case '}': --n2; break;
                    case '[': ++n3; break;
                    case ']': --n3; break;
                }
            }
            return (start, end);
        }
    }
}