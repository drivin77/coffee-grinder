using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private static HtmlTag _emTag;
        private static HtmlTag _strongTag;

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

            _emTag = new HtmlTag("<em>", "</em>");
            _strongTag = new HtmlTag("<strong>", "</strong>");
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
                {"*",       _emTag},
                {"_",       _emTag},
                {"**",      _strongTag},
                {"__",      _strongTag}
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

                // for all headers in the markdown file we found in the above statement...
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
                        // header case. we've already added header tags in-place above,
                        // so just write out the header tag, which is the only line we'll see that
                        // begins with '<', since we replace all '<'s above with html encoding.
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

                    // put back in the newlines we stripped above
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
                var replacement = Regex.Replace(listItem, @"^[-|+|*]\s+", _listItemTag.OpenTag);
                replacement = replacement.Insert(replacement.Length, _listItemTag.CloseTag);
                htmlFile.WriteLine(replacement);
            }

            htmlFile.Write(_unorderedListTag.CloseTag);
        }

        /// <summary>
        /// Inlines are emphasis (* or _) strong (** or __)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="htmlFile"></param>
        private static void ParseAndWriteInlines(string token, StreamWriter htmlFile)
        {
            // iterate through token to find special symbols

            var outputString = new StringBuilder();

            // keep a list of indices in the string where we need to insert tag.
            // key: index in token string 
            // value: tag to insert
            var tagSubstitutionLocations = new Dictionary<int, string>();

            var underscoreEmFoundAt = -1;
            var starEmFoundAt = -1;
            var doubleUnderscoreStrongFoundAt = -1;
            var doubleStarStrongFound = -1;            

            for (var i = 0; i < token.Length; ++i)
            {
                switch (token[i])
                {
                    case '_':
                        if ((underscoreEmFoundAt >= 0 || doubleUnderscoreStrongFoundAt >= 0) && i != 0 && !token[i - 1].Equals(' '))
                        {
                            if (i < token.Length - 1 && token[i + 1].Equals('_'))
                            {
                                if (doubleUnderscoreStrongFoundAt >= 0)
                                {
                                    tagSubstitutionLocations[doubleUnderscoreStrongFoundAt] = _strongTag.OpenTag;
                                    tagSubstitutionLocations[i] = _strongTag.CloseTag;
                                    ++i;
                                    doubleUnderscoreStrongFoundAt = -1;
                                }
                            }

                            else
                            {
                                tagSubstitutionLocations[underscoreEmFoundAt] = _emTag.OpenTag;
                                tagSubstitutionLocations[i] = _emTag.CloseTag;
                            
                                underscoreEmFoundAt = -1;     
                            }
                                                  
                        }
                        else
                        {
                            if (i < token.Length - 1)
                            {
                                if (!token[i + 1].Equals(' '))
                                {
                                    if (token[i + 1].Equals('_') && i < token.Length - 2 && !token[i + 2].Equals(' '))
                                    {
                                        doubleUnderscoreStrongFoundAt = i;
                                        ++i;
                                    }

                                    else
                                    {
                                        underscoreEmFoundAt = i;
                                    }
                                }
                                    
                            }
                        }
                        break;

                    case '*':
                        if ((starEmFoundAt >= 0 || doubleStarStrongFound >= 0) && i != 0 && !token[i - 1].Equals(' '))
                        {
                            if (i < token.Length - 1 && token[i + 1].Equals('*'))
                            {
                                if (doubleStarStrongFound >= 0)
                                {
                                    tagSubstitutionLocations[doubleStarStrongFound] = _strongTag.OpenTag;
                                    tagSubstitutionLocations[i] = _strongTag.CloseTag;
                                    ++i;
                                    doubleStarStrongFound = -1;
                                }
                            }

                            else
                            {
                                tagSubstitutionLocations[starEmFoundAt] = _emTag.OpenTag;
                                tagSubstitutionLocations[i] = _emTag.CloseTag;
                               
                                starEmFoundAt = -1;     
                            }
                                                  
                        }
                        else
                        {
                            if (i < token.Length - 1)
                            {
                                if (!token[i + 1].Equals(' '))
                                {
                                    if (token[i + 1].Equals('*') && i < token.Length - 2 && !token[i + 2].Equals(' '))
                                    {
                                        doubleStarStrongFound = i;
                                        ++i;
                                    }

                                    else
                                    {
                                        starEmFoundAt = i;
                                    }
                                }
                                    
                            }
                        }
                        break;
                }
            }

            var reversedDictionary = tagSubstitutionLocations.Reverse();

            var sbToken = token;

            foreach (var tagLocation in reversedDictionary)
            {
                sbToken.Replace() = tagLocation.Value;
            }

            htmlFile.Write(outputString);
            
        }
    }
}
