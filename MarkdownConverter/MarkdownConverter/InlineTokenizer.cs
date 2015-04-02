using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkdownConverter
{
    public class InlineTokenizer
    {
        public List<InlineToken> InlineTokens { get; private set; }

        public InlineTokenizer()
        {
            InlineTokens = new List<InlineToken>();
        }

        public void Tokenize(string s, HtmlTag strongTag, HtmlTag emTag)
        {
            var doubleStarTuple =
                new Tuple<InlineToken.TokenType, HtmlTag>(InlineToken.TokenType.DoubleStar, strongTag);
            var doubleUnderscoreTuple =
                new Tuple<InlineToken.TokenType, HtmlTag>(InlineToken.TokenType.DoubleUnderscore, strongTag);

            var starTuple =
                new Tuple<InlineToken.TokenType, HtmlTag>(InlineToken.TokenType.Star, emTag);
            var underscoreTuple =
                new Tuple<InlineToken.TokenType, HtmlTag>(InlineToken.TokenType.Underscore, emTag);

            TokenizeDouble(s, doubleStarTuple);
            TokenizeDouble(s, doubleUnderscoreTuple);
            TokenizeSingle(s, starTuple);
            TokenizeSingle(s, underscoreTuple);

            InlineTokens = InlineTokens.OrderBy(x => x.StrIndex).ToList();
        }

        public InlineToken FindNextOfTheSameToken(int startIndex, InlineToken token, out int indexFound)
        {
            for (var i = startIndex + 1; i < InlineTokens.Count; ++i)
            {
                if (InlineTokens[i].Type.Item1 == token.Type.Item1)
                {
                    indexFound = i;
                    return InlineTokens[i];
                }

            }

            indexFound = -1;
            return null;
        }

        public bool IsOpenTokenValid(string s, InlineToken token)
        {
            var idx = token.StrIndex;
            var numCharsInToken =
                (
                    token.Type.Item1 == InlineToken.TokenType.DoubleStar
                    || token.Type.Item1 == InlineToken.TokenType.DoubleUnderscore
                    )
                    ? 2
                    : 1;

            // we just need to check that there is a character to the right of the open token
            // has a char other than newline
            if (idx <= s.Length - numCharsInToken)
            {
                var nextChar = s[idx + numCharsInToken];
                if (nextChar != ' ' && (nextChar != '\n' || nextChar != '\r'))
                    return true;
            }

            return false;
        }

        public bool IsCloseTokenValid(string s, InlineToken token)
        {
            var idx = token.StrIndex;
            var numCharsInToken =
                (
                    token.Type.Item1 == InlineToken.TokenType.DoubleStar
                    || token.Type.Item1 == InlineToken.TokenType.DoubleUnderscore
                    )
                    ? 2
                    : 1;

            // we just need to check that there is a character to the left of the close token
            // has a char other than newline
            if (idx >= 1)
            {
                var prevChar = s[idx - 1];
                if (prevChar != ' ' && (prevChar != '\n' || prevChar != '\r'))
                    return true;
            }

            return false;
        }

        private void TokenizeDouble(string s, Tuple<InlineToken.TokenType, HtmlTag> type)
        {
            var lastCharMatched = false;

            var chToLookFor =
                (type.Item1 == InlineToken.TokenType.DoubleUnderscore)
                    ? '_'
                    : '*';

            for (var i = 0; i < s.Length; ++i)
            {
                if (s[i].Equals(chToLookFor))
                {
                    // found double _ or double *
                    if (lastCharMatched)
                    {
                        InlineTokens.Add(new InlineToken(i - 1, type));
                        lastCharMatched = false;
                    }

                    else
                    {
                        lastCharMatched = true;
                    }
                }

                else
                {
                    lastCharMatched = false;
                }
            }
        }

        private void TokenizeSingle(string s, Tuple<InlineToken.TokenType, HtmlTag> type)
        {
            var chToLookFor =
                (type.Item1 == InlineToken.TokenType.Underscore)
                    ? '_'
                    : '*';

            for (var i = 0; i < s.Length; ++i)
            {
                if (s[i].Equals(chToLookFor))
                {
                    var curToken = new InlineToken(i, type);
                    var prevToken = new InlineToken(i - 1, type);
                    if (!(InlineTokens.Contains(curToken) || InlineTokens.Contains(prevToken)))
                    {
                        InlineTokens.Add(curToken);
                    }
                }
            }
        }
    }
}