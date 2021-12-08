using System.Text;

namespace Calcpad.web.Helpers
{
    public static class TextHelper
    {
        public static string SplitWords(string text, char delimiter = ' ')
        {
            StringBuilder sb = new StringBuilder();
            bool IsFirst = true;
            foreach (char c in text)
            {
                if (IsFirst)
                    IsFirst = false;
                else if (char.IsUpper(c))
                    sb.Append(delimiter);

                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static string GetTitle(string text, char replaceSpaces = ' ')
        {
            if (string.IsNullOrEmpty(text))
                return "document";

            StringBuilder sb = new StringBuilder();
            bool isTag = false, isComment = false, isSpace = false;
            char quote = ' ';
            foreach (char c in text)
            {
                if (c == '\n')
                    return sb.ToString();
                else if (isComment)
                {
                    if (c == quote)
                        isComment = false;
                    else if (isTag)
                    {
                        if (c == '>')
                            isTag = false;
                    }
                    else if (c == '<')
                        isTag = true;
                    else if (char.IsWhiteSpace(c))
                    {
                        if (sb.Length > 100)
                            return sb.ToString();
                        isSpace = true;
                    }
                    else if (char.IsLetterOrDigit(c))
                    {
                        if (isSpace)
                        {
                            sb.Append(replaceSpaces);
                            isSpace = false;
                        }
                        sb.Append(c);
                    }
                }
                else if (c == '\"' || c == '\'')
                {
                    quote = c;
                    isComment = true;
                }
            }
            return sb.ToString();
        }

        public static string SeoFriendly(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c == ' ' || c == '-' || c == '_')
                {
                    sb.Append('-');
                }
                else if (c >= 'A' && c <= 'Z' || c >= 'А' && c <= 'Я')
                {
                    sb.Append((char)(c + 32));
                }
                else if (c >= '0' && c <= '9' || c >= 'a' && c <= 'z' || c >= 'а' && c <= 'я')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
