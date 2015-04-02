using System;

namespace MarkdownConverter
{
    public class InlineToken : IEquatable<InlineToken>
    {
        public int StrIndex { get; private set; }
        public Tuple<TokenType, HtmlTag> Type { get; private set; }

        public enum TokenType
        {
            Star,
            DoubleStar,
            Underscore,
            DoubleUnderscore
        }

        public InlineToken(int strIndex, Tuple<TokenType, HtmlTag> type)
        {
            StrIndex = strIndex;
            Type = type;
        }

        public bool Equals(InlineToken other)
        {
            return (StrIndex == other.StrIndex);
        }
    }
}