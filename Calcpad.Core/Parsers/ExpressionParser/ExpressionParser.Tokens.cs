using System;
using System.Collections.Generic;

namespace Calcpad.Core
{
    public partial class ExpressionParser
    {
        private class Token
        {
            internal string Value { get; }
            internal TokenTypes Type;
            internal int CacheID = -1;
            internal Token(string value, TokenTypes type)
            {
                Value = value;
                Type = type;
            }
            public override string ToString() => Value;
        }

        private enum TokenTypes
        {
            Expression,
            Heading,
            Text,
            Html,
            Error
        }

        private List<Token> GetTokens(ReadOnlySpan<char> s)
        {
            var tokens = new List<Token>();
            var ts = new TextSpan(s);
            var currentSeparator = ' ';
            for (int i = 0, len = s.Length; i < len; ++i)
            {
                var c = s[i];
                if (c == '\'' || c == '\"')
                {
                    if (currentSeparator == ' ' || currentSeparator == c)
                    {
                        if (!ts.IsEmpty)
                            AddToken(tokens, ts.Cut(), currentSeparator);

                        ts.Reset(i + 1);
                        currentSeparator = currentSeparator == c ? ' ' : c;
                    }
                    else if (currentSeparator != ' ')
                        ts.Expand();
                }
                else
                    ts.Expand();
            }
            if (!ts.IsEmpty)
                AddToken(tokens, ts.Cut(), currentSeparator);

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
                if (tokens.Count == 0)
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
    }
}