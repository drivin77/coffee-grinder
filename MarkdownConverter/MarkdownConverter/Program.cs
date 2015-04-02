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
        private static Dictionary<String, HtmlTag> _headerHash;

        private static HtmlTag _paragraphTag;
        private static HtmlTag _unorderedListTag;
        private static HtmlTag _orderedListTag;
        private static HtmlTag _listItemTag;
        private static HtmlTag _emTag;
        private static HtmlTag _strongTag;

        const string OlListItemReString = @"^\s{0,3}\d+\.\s+";
        const string UlListItemReString = @"^\s{0,3}[-|+|*]\s+";

        static void Main(string[] args)
        {
            if (args.Length != 2 || (args.Length == 1 && args[0].Equals("?")))
            {
                Console.WriteLine("Usage: MarkdownConverter <MarkdownFile> <HTMLFile>");
                Console.WriteLine();
                Console.WriteLine("  <MarkdownFile>  path including filename of markdown file to read.");
                Console.WriteLine("  <HTMLFile>      path including filename of html output file. Will be overwritten.");

                Console.ReadKey();

                return;
            }
                
            try
            {
                // get the command line args
                var mdFile = args[0];
                var htmlFile = args[1];

                // make sure the input file exists
                if (! File.Exists(mdFile))
                    throw new Exception(String.Format("Input file couldn't be found! ({0})", mdFile));

                // try to create the directory for the output file if it 
                // doesn't already exist.
                var outFileDir = Path.GetDirectoryName(htmlFile);
                if (!string.IsNullOrWhiteSpace(outFileDir) && !Directory.Exists(outFileDir))
                {
                    try
                    {
                        Directory.CreateDirectory(outFileDir);
                    }
                    catch (Exception e)
                    {                           
                        throw new Exception(
                            String.Format(
                                "Couldn't create directory ('{0}') for html output file: ('{1}')",
                                outFileDir,
                                e.Message
                            )
                        );
                    }
                }
                
                // the main function to kick off the conversion
                MarkdownToHtml(mdFile, htmlFile);

                Console.WriteLine("Converted markdown file: '{0}' to html: '{1}'", mdFile, htmlFile);

                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception found: " + e.Message);
                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }      
        }

        private static void MarkdownToHtml(string markdownFilePath, string htmlFilePath)
        {
            _emTag = new HtmlTag("<em>", "</em>");
            _strongTag = new HtmlTag("<strong>", "</strong>");
            _paragraphTag = new HtmlTag("<p>", "</p>");
            _unorderedListTag = new HtmlTag("<ul>", "</ul>");
            _orderedListTag = new HtmlTag("<ol>", "</ol>");
            _listItemTag = new HtmlTag("<li>", "</li>");

            _headerHash = new Dictionary<String, HtmlTag>
            {
                {"#",       new HtmlTag("<h1>", "</h1>")},
                {"##",      new HtmlTag("<h2>", "</h2>")},
                {"###",     new HtmlTag("<h3>", "</h3>")},
                {"####",    new HtmlTag("<h4>", "</h4>")},
                {"#####",   new HtmlTag("<h5>", "</h5>")},
                {"######",  new HtmlTag("<h6>", "</h6>")}
            };

            var markupFile = new StreamReader(markdownFilePath);
            var htmlFile = new StreamWriter(htmlFilePath, false);

            using (htmlFile)
            {
                // read entire file into a string object
                var fileString = markupFile.ReadToEnd();

                // replace tab chars with 4 spaces
                fileString = Regex.Replace(fileString, @"\t", @"    ");

                // file-wide replacement for &, making sure to leave html codes alone
                fileString = Regex.Replace(fileString, @"&(?!#?\w+;)", @"&amp;");

                // file-wide replacement for <.  
                fileString = Regex.Replace(fileString, @"<", @"&lt;");

                // file-wide replacement for numbered-list escapes (^1\. in markdown converst to 1.)
                var numListEscape = new Regex(@"^\s{0,3}(\d+)\\\.", RegexOptions.Multiline);
                fileString = numListEscape.Replace(fileString, @"$1.");

                // match all headers and replace in-place file-wide
                var matches = Regex.Matches(
                    fileString,
                    @"^(#{1,6})\s*(.*?)\s*#*\s*(?:\r\n?|\n|$)",
                    RegexOptions.Multiline
                );

                // for all headers in the markdown file we found in the above statement...
                foreach (Match match in matches)
                {
                    // quick lookup of which header tag to use based on first match in header
                    // string: match.Groups[1].Value will be "#", "##", ... "######".
                    // our Dictionary keeps which html tag we should use based on how many '#'s.
                    var headerTag = _headerHash[match.Groups[1].Value];

                    // replace the markdown header syntax with html tags
                    var replacementString = String.Format(
                        "{0}{1}{2}", headerTag.OpenTag,
                        match.Groups[2].Value.Trim(),
                        headerTag.CloseTag
                    );

                    // actually run the replacement
                    var escapedRegex = Regex.Escape(match.ToString().Trim());
                    var replacementRegex = new Regex(escapedRegex);

                    fileString =
                        replacementRegex.Replace(fileString, replacementString, 1);

                }

                // split file into paragraph tokens
                var tokens = Regex.Split(fileString, "\r\n?\r\n?|\n\n");

                foreach (var token in tokens)
                {
                    SplitParagraphToken(token, htmlFile);

                    // put back in the newlines we stripped above in the Regex split
                    htmlFile.WriteLine();
                    htmlFile.WriteLine();
                }
            }
            markupFile.Close();
        }

        /// <summary>
        /// Here we check the first char of the paragraph and process accordingly
        /// </summary>
        /// <param name="token">the paragraph token</param>
        /// <param name="htmlFile">output file to write to</param>
        private static void SplitParagraphToken(string token, StreamWriter htmlFile)
        {
            // if paragraph doesn't start with #, or a list symbol, then it's a paragraph
            switch (token[0])
            {
                // header case. we've already added header tags in-place above,
                // so just write out the header tag, which is the only line we'll see that
                // begins with '<', since we replace all '<'s above with html encoding.
                case '<':
                    // check if there are non-blank lines after the header and process the next
                    // line on as a new paragraph
                    var newlineRegex = new Regex("\r\n?|\n");
                    var hiddenParagraphInHeaderParagraph = newlineRegex.Split(token, 2);
                    if (hiddenParagraphInHeaderParagraph.Length > 1)
                    {
                        htmlFile.WriteLine(hiddenParagraphInHeaderParagraph[0]);

                        // add an extra newline to differentiate the header from the paragraph below
                        htmlFile.WriteLine();
                        SplitParagraphToken(hiddenParagraphInHeaderParagraph[1], htmlFile);
                    }

                    else
                    {
                        htmlFile.Write(token);
                    }
                        
                    break;

                // unordered list case.  Must have a space after the list symbol.
                case '-':
                case '+':
                case '*':
                case ' ':
                    if (Regex.IsMatch(token, UlListItemReString))
                    {
                        CreateList(token, htmlFile, _unorderedListTag, UlListItemReString);
                        break;
                    }

                    goto default;

                default:
                    // we can't have a regex in a case statement, so the ordered
                    // list has to go in default as the ordered list can specify any
                    // number followed by a period and a space.
                    if (Regex.IsMatch(token, OlListItemReString))
                        CreateList(token, htmlFile, _orderedListTag, OlListItemReString);

                    else
                        CreateParagraph(token, htmlFile);

                    break;
            }
        }

        private static void CreateParagraph(string token, StreamWriter htmlFile)
        {
            htmlFile.Write(_paragraphTag.OpenTag);
            ParseAndWriteInlines(token, htmlFile);
            htmlFile.Write(_paragraphTag.CloseTag);
        }

        /// <summary>
        /// Creates lists for ordered or unordered list tokens
        /// </summary>
        /// <param name="token">an entire chunk of list</param>
        /// <param name="htmlFile">file to write to</param>
        /// <param name="outerListTag">the HtmlTag of the outer list <ol></ol> or <ul></ul> for example</param>
        /// <param name="listItemRegex">the regex of the particular list type to find list items</param>
        private static void CreateList(
            string token,
            StreamWriter htmlFile,
            HtmlTag outerListTag,
            String listItemRegex
        )
        {
            htmlFile.WriteLine(outerListTag.OpenTag);

            // go through each list item in the paragraph, allowing for multi-line list formatting
            foreach (var listItem in Regex.Split(token, "\r\n?|\n" + listItemRegex, RegexOptions.Multiline))
            {
                // remove list marker
                var replacement = Regex.Replace(listItem, listItemRegex, "");

                // insert html at beginning and end
                replacement = replacement.Insert(0, _listItemTag.OpenTag);
                replacement = replacement.Insert(replacement.Length, _listItemTag.CloseTag);

                // remove any white space in front of new lines
                replacement = Regex.Replace(replacement, @"(\r\n?|\n)\s{1,3}", "$1");

                // encode any in-lines in the list
                ParseAndWriteInlines(replacement, htmlFile);
                htmlFile.WriteLine();
            }

            htmlFile.Write(outerListTag.CloseTag);
        }

        /// <summary>
        /// Inlines are: emphasis (* or _) strong (** or __)
        /// </summary>
        /// <param name="token">the current token to parse</param>
        /// <param name="htmlFile">the output file to write to</param>
        private static void ParseAndWriteInlines(string token, StreamWriter htmlFile)
        {
            // keep a list of indices in the string where we need to insert the tag.
            // key: index into input string to begin replacement at
            // value: number of characers to remove from index and tag to insert there
            var tagSubstitutionLocations = new Dictionary<int, Tuple<int, string>>();

            // state variables
            var underscoreEmFoundAt = -1;
            var starEmFoundAt = -1;
            var doubleUnderscoreStrongFoundAt = -1;
            var doubleStarStrongFound = -1;            

            // ok, not very pretty logic here, but this is the only place we need to get messy.
            // build up the tagSubstitutionLocations dictionary with valid indices of
            // *, **, _, __ tokens.
            for (var i = 0; i < token.Length; ++i)
            {
                switch (token[i])
                {
                    case '_':
                        if ((underscoreEmFoundAt >= 0 || doubleUnderscoreStrongFoundAt >= 0) && i != 0 && !token[i - 1].Equals(' '))
                        {
                            if (i < token.Length - 1 && token[i + 1].Equals('_'))
                            {
                                tagSubstitutionLocations[doubleUnderscoreStrongFoundAt] = new Tuple<int, string>(2, _strongTag.OpenTag);
                                tagSubstitutionLocations[i] = new Tuple<int, string>(2, _strongTag.CloseTag);
                                ++i;
                                doubleUnderscoreStrongFoundAt = -1;
                            }

                            else 
                            {
                                tagSubstitutionLocations[underscoreEmFoundAt] = new Tuple<int, string>(1, _emTag.OpenTag);
                                tagSubstitutionLocations[i] = new Tuple<int, string>(1, _emTag.CloseTag);
                            
                                underscoreEmFoundAt = -1;     
                            }
                                                  
                        }
                        else
                        {
                            if (i < token.Length - 1)
                            {
                                if (!token[i + 1].Equals(' '))
                                {
                                    // escaped _, do nothing
                                    if (i != 0 && token[i - 1].Equals('\\'))
                                    {
                                        // remove the '\' character
                                        tagSubstitutionLocations[i - 1] = new Tuple<int, string>(1, "");
                                        break;
                                    }

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
                                tagSubstitutionLocations[doubleStarStrongFound] = new Tuple<int, string>(2, _strongTag.OpenTag);
                                tagSubstitutionLocations[i] = new Tuple<int, string>(2, _strongTag.CloseTag);
                                ++i;
                                doubleStarStrongFound = -1;
                            }

                            else if (doubleStarStrongFound == -1)
                            {
                                tagSubstitutionLocations[starEmFoundAt] = new Tuple<int, string>(1, _emTag.OpenTag);
                                tagSubstitutionLocations[i] = new Tuple<int, string>(1, _emTag.CloseTag);
                               
                                starEmFoundAt = -1;     
                            }
                                                  
                        }
                        else
                        {
                            if (i < token.Length - 1)
                            {
                                if (!token[i + 1].Equals(' '))
                                {
                                    // escaped *, do nothing
                                    if (i != 0 && token[i - 1].Equals('\\'))
                                    {
                                        // remove the '\' character
                                        tagSubstitutionLocations[i - 1] = new Tuple<int, string>(1, "");
                                        break;
                                    }

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

            // no <em> or <strong> found
            if (tagSubstitutionLocations.Count == 0)
            {
                htmlFile.Write(token);
                return;
            }              

            // found <em> and/or <strong>, so remove the *,**,_,__
            // chars and replace with <em> and <strong>
            var sbToken = new StringBuilder(token);

            // go backwards through the tokens so we don't have to do
            // any math on the indices to be replaced.  If we go in ascending order
            // then the indices we stored above won't work any more.
            foreach (var tagLocation in tagSubstitutionLocations.OrderByDescending(i => i.Key))
            {
                // grab local vars from the Dictionary we populated above.
                var startIndexToReplace = tagLocation.Key;
                var tuple = tagLocation.Value;
                var numCharsToRemove = tuple.Item1;
                var tagToInsert = tuple.Item2;

                // remove the *,**,_, or __.  The dictionary entry tells us how many to remove
                sbToken.Remove(startIndexToReplace, numCharsToRemove);

                // insert the html <em>, </em>, <strong>, or </strong> tag
                sbToken.Insert(startIndexToReplace, tagToInsert);
            }

            htmlFile.Write(sbToken.ToString());         
        }
    }
}
