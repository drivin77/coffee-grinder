using System;

namespace MarkdownConverter
{
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