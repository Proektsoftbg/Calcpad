using System.Collections.Generic;
namespace Calcpad.Core
{
    public partial class ExpressionParser
    {
        private readonly struct LineInfo
        {
            internal readonly Keyword Keyword;
            internal readonly List<Token> Tokens;
            internal bool IsCached => Tokens is not null;

            internal LineInfo(List<Token> tokens, Keyword keyword)
            {
                Tokens = tokens;
                Keyword = keyword;
            }
        }
    }
}
