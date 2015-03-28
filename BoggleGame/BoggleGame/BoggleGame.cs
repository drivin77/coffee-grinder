using System;
using System.Collections;
using System.Text;
using BoggleGame.Dictionary;
using BoggleGame.Game;

namespace BoggleGame
{
    /// <summary>
    /// Main boggle game class
    /// </summary>
    internal class BoggleGame
    {
        private static BoggleDictionary _diction;

        private static ArrayList _wordsFound;

        private static void Main(string[] args)
        {
            if (args.Length != 2 || (args.Length == 1 && args[0].Equals("?")))
            {
                Console.WriteLine("Usage: BoggleGame <dimension> <board>");
                Console.WriteLine(" ");
                Console.WriteLine("  <dimension>  a positive integer value representing a dimension x dimension board.");
                Console.WriteLine("  <board>      a string representing the board.  Append all subsequent rows to the first row.");
                Console.WriteLine("               Ex: If the board dimension is 2 and the letters are");
                Console.WriteLine("               | a b |");
                Console.WriteLine("               | c d |");
                Console.WriteLine("               then board will be 'abcd'");

                return;
            }

            try
            {
                var dimension = Int32.Parse(args[0]);
                var boardInput = args[1];

                _diction = new BoggleDictionary();
                _wordsFound = new ArrayList();

                var theBoard = new BoggleBoard(dimension, boardInput);
                theBoard.PrintBoard();
                
                FindWords(theBoard);

                if (_wordsFound.Count > 0)
                {
                    // sort words found to make output neater
                    _wordsFound.Sort();

                    foreach (var word in _wordsFound)
                    {
                        Console.WriteLine(word);
                    }
                }

                else
                {
                    Console.WriteLine("No words found!");
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// helper function which writes to stdout whether or not key exists in dictionary
        /// </summary>
        /// <param name="key">string to check dictionary for</param>
        private static void IsWord(string key)
        {
            var isWord = _diction.IsWord(key);
            Console.WriteLine("Is {0} a word? {1}", key, (isWord) ? "yes" : "no");
        }

        /// <summary>
        /// For each letter on the board, recursively search around that letter, forming
        /// strings from those letters and checking if they are words or could potentially
        /// become words.
        /// </summary>
        /// <param name="theBoard">The BoggleBoard object to search for words in</param>
        private static void FindWords(BoggleBoard theBoard)
        {
            var height = theBoard.Dimension;
            var width = theBoard.Dimension;

            for (var j = 0; j < height; ++j)
            {
                for (var i = 0; i < width; ++i)
                {
                    // bool array gets initialized with default bool value of 'false'
                    var visited = new bool[width, height];

                    FindWordsRecursive(theBoard, i, j, width, height, new StringBuilder(), visited);
                }
            }
        }

        private static void FindWordsRecursive(
            BoggleBoard board,
            int i,
            int j,
            int width,
            int height,
            StringBuilder currentWord,
            bool[,] visited
            )
        {
            currentWord.Append(board[j, i]);

            // quick exit case if the current string isn't a word
            var curWordStr = currentWord.ToString();
            if (!_diction.IsStartOfWord(curWordStr))
            {
                currentWord.Remove(currentWord.Length - 1, 1);
                visited[j, i] = false;
                return;
            }

            if (_diction.IsWord(curWordStr))
                _wordsFound.Add(curWordStr);

            visited[j, i] = true;

            // left
            if (i - 1 >= 0 && !visited[j, i - 1])
                FindWordsRecursive(board, i - 1, j, width, height, currentWord, visited);

            // up and left
            if (i - 1 >= 0 && j - 1 >= 0 && !visited[j - 1, i - 1])
                FindWordsRecursive(board, i - 1, j - 1, width, height, currentWord, visited);

            // up
            if (j - 1 >= 0 && !visited[j - 1, i])
                FindWordsRecursive(board, i, j - 1, width, height, currentWord, visited);

            // up and right
            if (i + 1 < width && j - 1 >= 0 && !visited[j - 1, i + 1])
                FindWordsRecursive(board, i + 1, j - 1, width, height, currentWord, visited);

            // right
            if (i + 1 < width && !visited[j, i + 1])
                FindWordsRecursive(board, i + 1, j, width, height, currentWord, visited);

            // right and down
            if (i + 1 < width && j + 1 < height && !visited[j + 1, i + 1])
                FindWordsRecursive(board, i + 1, j + 1, width, height, currentWord, visited);

            // down
            if (j + 1 < height && !visited[j + 1, i])
                FindWordsRecursive(board, i, j + 1, width, height, currentWord, visited);

            // down and left
            if (i - 1 >= 0 && j + 1 < height && !visited[j + 1, i - 1])
                FindWordsRecursive(board, i - 1, j + 1, width, height, currentWord, visited);

            currentWord.Remove(currentWord.Length - 1, 1);
            visited[j, i] = false;
        }
    }
}