The program compiles to Windows binaries and is a console program which takes 2 arguments:
1. dimension, an integer value (only square boards are accepted, so 2 means 2x2 and 3 is 3x3, etc).
2. board, a string.  This is a 1d representation of the board, which is accomplished by appending each row beyond the 1st to the end of the 1st row.  Important Note: the letter 'q' is automatically transformed to 'qu' and if you put 'qu' in the board that will actually become 'quu' and count as 3 letters.  The program will alert you if you didn't intend to do this, as you'll likely have mis-aligned the board.

Command line example:
...bin\Release> BoggleGame.exe 2 "abcd"
will produce a board which looks like so:
| a b |
| c d |

...bin\Release> BoggleGame.exe 2 "abcq"
will produce a board which looks like so:
| a b |
| c qu|

Once the program has finished running, it will output the dictionary used, number of words in the dictionary, the board, the number of words found, and the list of words found (one per line).

To run the program, compile either in debug or release in VS2013 (earlier versions should work as well).  If running directly in VS2013, you'll need to add command arguments, which can be done in the solution properties, under the debug menu.  If running on the command line, you'll need to provide the arguments there (see above).

The dictionary file is included in the Solution and automatically copied to the binary output location during the build process.  I'm using the official scrabble player's dictionary.

The key data structure used to represent the words in the dictionary is a Ternary Search Trie (TST), which allows for very fast lookup of string values, especially with dictionary-length words.  I choose this over the O(1) lookup time of a hashtable because there's no easy or efficient way to throw out a partial word as we go through the boggle board with a hashtable.  If the keys are inserted in random order, the lookup of a key is O(ln n), which is only 20 operations for 1,000,000,000 keys, not bad!  TSTs are also more memory efficient than R-way tries, though require more iterations to find the letter you're looking for (though still very efficient).

Please see the code for more information and additional comments.
