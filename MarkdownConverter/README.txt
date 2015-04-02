The program compiles to Windows binaries and is a console program which takes 2 arguments:
1. MarkdownFile - the input markdown file to parse and convert.
2. HTMLFile - the html file to create and output.  The program will create the directory and file if not already created.

There was a bit of confusion in the description of the problem for me where I was asked to simply create a function().  I did create that function within the main class, but in order for you to test a markdown file, you would have had to change and compile the code which seemed tedious, so I added command line params.

I created a markdown.txt file in the root of the project, which contains text from the markdown example in the problem description.

The general idea of the program is to regex replace certain elements on the entire file, then break the file up into paragraphs, adding paragraph tags and then lexing and parsing the in-line <em> and <strong> tags.

The last bit of code I added was the Tokenizer class and tokenizer logic for the in-line <em> and <strong> tags.  Initially I was lexing and parsing all in one loop, which was awful to look at and code and had some bugs.  I'm much happier with the refactor and probably could have moved all the parsing to this system, but ran out of time to do so.

Omitted from this solution is allowing paragraphs within lists, which is in the markdown spec. 
Also, escaping valid * and _ locations isn't handled.  
With another day of work I could get these finished, but it's time to turn it in :)

Please see the code for additional comments and notes. 

Thanks for taking the time to look over my solution!

