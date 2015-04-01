Enclosed are the  Bungie's Programmer Test.

The code for this solution is written in C# and has been compiled and tested in Visual Studio 2013.

The program is a console program and takes 2 arguments:
1. dimension (only square boards are accepted, so 2 means 2x2 and 3 is 3x3, etc).
2. the board as a single 1d string.  This is accomplished by appending each row beyond the 1st to the end of the 1st row.

Command line example:
...bin\Release> BoggleGame.exe 2 "abcd"
will produce a board which looks like so:
| a b |
| c d |

Once the program is finished running, it will output the board, the number of words found, and the list of words found (one per line).

To run the program, compile either in debug or release in VS2013 (earlier versions should work as well).  If running directly in VS2013, you'll need to add command arguments, which can be done in the solution properties, under the debug menu.  If running on the command line, you'll need to provide the arguments there (see above).

The dictionary file is included in the Solution and automatically copied to the correct location during the build process.  I'm using the official scrabble player's dictionary.

Please see the code for more information and additional comments.
