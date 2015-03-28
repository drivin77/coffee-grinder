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
        private static readonly BoggleDictionary Diction = new BoggleDictionary();

        private static readonly ArrayList WordsFound = new ArrayList();

        private static void Main(string[] args)
        {
       
          //  char[,] board = { { 'y', 'o', 'x', 'm' }, { 'r', 'b', 'a', 'n' }, { 'v', 'e', 'd', 's' }, {'c', 'f', 'g', 'h'} };
            char[,] board = { { 'y', 'o', 'x', 's', 't' }, { 'r', 'b', 'a', 'c', 'l' }, { 'v', 'e', 'd', 'm', 'j' }, { 'm', 'd', 'f', 'g', 'n' }, { 'b', 'd', 'x', 'y', 'q' } };
          //  char[,] board = { { 'd', 'e' }, { 'a', 'f' } };

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

                var theBoard = new BoggleBoard(dimension, boardInput);
                theBoard.PrintBoard();
                /*
                FindWords(board, board.GetLength(0), board.GetLength(1));

                // sort words found to make output neater
                WordsFound.Sort();

                foreach (var word in WordsFound)
                {
                    Console.WriteLine(word);
                }*/
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
            var isWord = Diction.IsWord(key);
            Console.WriteLine("Is {0} a word? {1}", key, (isWord) ? "yes" : "no");
        }

        /// <summary>
        /// For each letter on the board, recursively search around that letter, forming
        /// strings from those letters and checking if they are words or could potentially
        /// become words.
        /// </summary>
        /// <param name="board">the board.  Doesn't change</param>
        /// <param name="width">the board width.  Doesn't change.</param>
        /// <param name="height">the board height.  Doesn't change.</param>
        private static void FindWords(char[,] board, int width, int height)
        {
            for (var j = 0; j < height; ++j)
            {
                for (var i = 0; i < width; ++i)
                {
                    // bool array gets initialized with default bool value of 'false'
                    var visited = new bool[width, height];

                    FindWordsRecursive(board, i, j, width, height, new StringBuilder(), visited);
                }
            }
        }

        private static void FindWordsRecursive(
            char[,] board,
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
            if (!Diction.IsStartOfWord(curWordStr))
            {
                currentWord.Remove(currentWord.Length - 1, 1);
                visited[j, i] = false;
                return;
            }

            if (Diction.IsWord(curWordStr))
                WordsFound.Add(curWordStr);

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