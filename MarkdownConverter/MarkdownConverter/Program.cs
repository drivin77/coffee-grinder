using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MarkdownConverter
{
    static class Program
    {
        private static Regex _emRegex;
        private static Regex _em2Regex;
        private static Regex _strongRegex;
        private static Regex _strong2Regex;

        private static Dictionary<String, HtmlTag> _symbolHash;

        private static HtmlTag _paragraphTag;
        private static HtmlTag _unorderedListTag;
        private static HtmlTag _orderedListTag;
        private static HtmlTag _listItemTag;

        static void Main(string[] args)
        {
            try
            {
                MarkdownToHtml("Markdown.txt", "htmlConversion.html");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception found: " + e.Message);
            }      
        }

        private static void MarkdownToHtml(string markdownFilePath, string htmlFilePath)
        {
            _emRegex = new Regex(@"(\*\w+|\w+\*)");
            _em2Regex = new Regex(@"(_\w+|\w+_)");
            _strongRegex = new Regex(@"(\*\*\w+|\w+\*\*)");
            _strong2Regex = new Regex(@"(__\w+|\w+__)");

            var emTag = new HtmlTag("<em>", "</em>");
            var strongTag = new HtmlTag("<strong>", "</strong>");
            _paragraphTag = new HtmlTag("<p>", "</p>");
            _unorderedListTag = new HtmlTag("<ul>", "</ul>");
            _orderedListTag = new HtmlTag("<ol>", "</ol>");
            _listItemTag = new HtmlTag("<li>", "</li>");

            _symbolHash = new Dictionary<String, HtmlTag>
            {
                {"#",       new HtmlTag("<h1>", "</h1>")},
                {"##",      new HtmlTag("<h2>", "</h2>")},
                {"###",     new HtmlTag("<h3>", "</h3>")},
                {"####",    new HtmlTag("<h4>", "</h4>")},
                {"#####",   new HtmlTag("<h5>", "</h5>")},
                {"#######", new HtmlTag("<h6>", "</h6>")},
                {"*",       emTag},
                {"_",       emTag},
                {"**",      strongTag},
                {"__",      strongTag}
            };

            var markupFile = new StreamReader(markdownFilePath);
            var htmlFile = new StreamWriter(htmlFilePath, false);

            using (htmlFile)
            {
                // read entire file into a string object
                var fileString = markupFile.ReadToEnd();

                // replace tab chars
                fileString = Regex.Replace(fileString, @"\t", @"    ");

                // file-wide replacement for &, making sure to leave html codes alone
                fileString = Regex.Replace(fileString, @"&(?!#?\w+;)", @"&amp;");

                // file-wide replacement for <.  
                fileString = Regex.Replace(fileString, @"<", @"&lt;");

                // match all headers and replace in-place file-wide
                var matches = Regex.Matches(
                    fileString,
                    @"^(#{1,6})\s*(.*?)\s*#*\s*(?:\n|$)",
                    RegexOptions.Multiline
                );

                // for all headers in the markdown file...
                foreach (Match match in matches)
                {
                    // quick lookup of which header tag to use based on first match in header
                    // string: match.Groups[1].Value will be "#", "##", ... "######".
                    // our Dictionary keeps which html tag we should use based on how many '#'s.
                    var headerTag = _symbolHash[match.Groups[1].Value];

                    // replace the markdown header syntax with html tags
                    var replacementString = String.Format(
                        "{0}{1}{2}", headerTag.OpenTag,
                        match.Groups[2].Value.Trim(),
                        headerTag.CloseTag
                    );

                    // actually run the replacement
                    fileString = 
                        Regex.Replace(fileString, match.ToString().Trim(), replacementString);
                }

                // split file into paragraph tokens
                var tokens = Regex.Split(fileString, "\r\n\r\n");

                foreach (var token in tokens)
                {
                    // if paragraph doesn't start with #, or a list symbol, then it's a paragraph
                    switch (token[0])
                    {
                        // header case.  Must have a space after the header symbol.
                        case '<':
                            htmlFile.Write(token);
                            break;
                          

                        // unordered list case.  Must have a space after the list symbol.
                        case '-':
                        case '+':
                        case '*':
                            if (Regex.IsMatch(token, @"[\-|\+|\*]\s"))
                            {
                                CreateUnorderedList(token, htmlFile);
                                break;
                            }

                            goto default;

                        default:
                            // we can't have a regex in a case statement, so the ordered
                            // list has to go in default as the ordered list can specify any
                            // number folowed by a period and a space.
                            if (Regex.IsMatch(token, @"^\d+\.\s"))
                                CreateOrderedList(token, htmlFile);

                            else
                                CreateParagraph(token, htmlFile);

                            break;
                    }
                    htmlFile.WriteLine();
                    htmlFile.WriteLine();
                }  
            }
            markupFile.Close();
        }

        private static void CreateParagraph(string token, StreamWriter htmlFile)
        {
            htmlFile.Write(_paragraphTag.OpenTag);
            ParseAndWriteInlines(token, htmlFile);
            htmlFile.Write(_paragraphTag.CloseTag);
        }

        private static void CreateOrderedList(string token, StreamWriter htmlFile)
        {
            htmlFile.WriteLine(_orderedListTag.OpenTag);

            // go through each list item in the paragraph
            foreach (var listItem in Regex.Split(token, "\r\n"))
            {
                var replacement = Regex.Replace(listItem, @"^\d+\.\s+", _listItemTag.OpenTag);
                replacement = replacement.Insert(replacement.Length, _listItemTag.CloseTag);
                htmlFile.WriteLine(replacement);
            }

            htmlFile.Write(_orderedListTag.CloseTag);
          
        }

        private static void CreateUnorderedList(string token, StreamWriter htmlFile)
        {
            htmlFile.WriteLine(_unorderedListTag.OpenTag);

            // go through each list item in the paragraph
            foreach (var listItem in Regex.Split(token, "\r\n"))
            {
                var replacement = Regex.Replace(listItem, @"^[\-|\+|\*]\s+", _listItemTag.OpenTag);
                replacement = replacement.Insert(replacement.Length, _listItemTag.CloseTag);
                htmlFile.WriteLine(replacement);
            }

            htmlFile.Write(_unorderedListTag.CloseTag);
        }

        /// <summary>
        /// Inlines are emphasis (* or _) strong (** or __), or ampersand (&) and less than (&lt;)
        /// escaping.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="htmlFile"></param>
        private static void ParseAndWriteInlines(string token, StreamWriter htmlFile)
        {
            htmlFile.Write(token);
            // keep closing tags on a stack for emphasis and strong for nesting.
         /*   var htmlTagStack = new Stack<string>();

            // line to write to output file
            var fileWriteLine = new StringBuilder();

            // iterate through every token (separated by whitespace in the line and transform 
            // in-lines in place
            var wordTokens = token.Split(' ');
            foreach (var wordToken in wordTokens)
            {
                // em case
                if (Regex.IsMatch(token, "^(\*|_)\w+"))
                {
                    wordToken.
                }
            }*/
            /*
            LastCharSeen lastChar;

            var tokenLength = token.Length;
            for (var i = 0; i < tokenLength; ++i)
            {
                switch (token[i])
                {
                    case '*':
                    case '_':
                        var isStrong = false;
                        var isEm = false;
                        // check for valid <em> or <strong>
                        if (i < tokenLength - 1)
                        {
                            // <strong> case (potentially)
                            if (token[i].Equals(token[i + 1]))
                            {
                                if (i < tokenLength - 2 && ! token[i + 2].Equals(' '))
                                {
                                    ++i;
                                    isStrong = true;
                                }
                            }

                            // <em> case
                            else if (!token[i + 1].Equals(' '))
                            {
                                isEm = true;
                            }
                        }

                    default:

                }
            }*/
        }
    }
}
