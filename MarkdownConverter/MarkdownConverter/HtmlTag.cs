using System;

namespace MarkdownConverter
{
    /// <summary>
    /// represents an html tag which will help us out in conversions
    /// </summary>
    public class HtmlTag
    {
        public string OpenTag { get; private set; }
        public string CloseTag { get; private set; }

        public HtmlTag(String openTag, String closeTag)
        {
            OpenTag = openTag;
            CloseTag = closeTag;
        }
    }
}