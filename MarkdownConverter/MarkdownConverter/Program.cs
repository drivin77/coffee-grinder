using System;
using System.Collections;
using System.IO;

namespace MarkdownConverter
{
    static class Program
    {
        private static Hashtable _symbolHash;

        static void Main(string[] args)
        {
            var emTag = new HtmlTag("<em>", "</em>");
            var strongTag = new HtmlTag("<strong>", "</strong>");
            _symbolHash = new Hashtable
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

            try
            {
                var markupFile = new StreamReader("Markdown.txt");
                var htmlFile = new FileStream("htmlConversion.html", FileMode.Create);
                while (markupFile.Peek() >= 0)
                {
                    var line = markupFile.ReadLine();

                    var tokens = line.Split(new[] {' '}, StringSplitOptions.None);

                    foreach (var token in tokens)
                    {
                        Console.WriteLine(token);
                    }
                    Console.WriteLine();
                    
                }

                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception found: " + e.Message);
                throw;
            }
            
        }
    }
}
