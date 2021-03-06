﻿using System;
using System.Collections.Generic;
using System.Text;
using BoggleGame.Dictionary;
using BoggleGame.Game;

namespace BoggleGame
{
    /// <summary>
    /// Main boggle game class
    /// Rules:
    /// - Using standard Boggle word find rules.  No variant rules.
    /// </summary>
    internal static class BoggleGame
    {
        private static BoggleDictionary _diction;

        private static BoggleBoard _theBoard;

        private static List<string> _wordsFoundUniqueOrdered;

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

                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();

                return;
            }

            try
            {
                var dimension = Int32.Parse(args[0]);
                var boardInput = args[1];

                _diction = new BoggleDictionary();
                Console.WriteLine("Dictionary name: '{0}'", _diction.DictionaryFileName);
                Console.WriteLine("Words in dictionary: {0}", _diction.NumWords);

                _wordsFoundUniqueOrdered = new List<string>();

                _theBoard = new BoggleBoard(dimension, boardInput);
                _theBoard.PrintBoard();
                
                FindWords();

                if (_wordsFoundUniqueOrdered.Count > 0)
                {
                    Console.WriteLine("{0} words found.", _wordsFoundUniqueOrdered.Count);
                    foreach (var word in _wordsFoundUniqueOrdered)
                        Console.WriteLine(word);
                }

                else
                {
                    Console.WriteLine("No words with 3 or more letters found!");
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.InnerException != null)
                    Console.WriteLine(e.InnerException.Message);

                Console.WriteLine();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// For each letter on the board, recursively search around that letter, forming
        /// strings from those letters and checking if they are words or could potentially
        /// become words.
        /// </summary>
        private static void FindWords()
        {
            var height = _theBoard.Dimension;
            var width = _theBoard.Dimension;

            for (var j = 0; j < height; ++j)
            {
                for (var i = 0; i < width; ++i)
                {
                    // bool array gets initialized with default bool value of 'false'
                    var visited = new bool[width, height];

                    FindWordsRecursive(i, j, width, height, new StringBuilder(), visited);
                }
            }
        }

        /// <summary>
        /// Recursive function to search board for words.
        /// </summary>
        /// <param name="i">current i position on board</param>
        /// <param name="j">current j position on board</param>
        /// <param name="width">width of board (fixed)</param>
        /// <param name="height">height of board (fixed)</param>
        /// <param name="currentWord">the current letters we have strung together 
        /// from recursing</param>
        /// <param name="visited">2d array the size of board marking locations we've 
        /// recursed in this pass, so we don't reuse a letter</param>
        private static void FindWordsRecursive(
            int i,
            int j,
            int width,
            int height,
            StringBuilder currentWord,
            bool[,] visited
            )
        {
            currentWord.Append(_theBoard[j, i]);

            // quick exit case if the current string isn't a word
            var curWordStr = currentWord.ToString();

            // don't check the trie if the str length is only one because that
            // case is always true.
            if (curWordStr.Length > 1 && !_diction.IsStartOfWord(curWordStr))
            {
                RemoveChars(_theBoard[j, i], currentWord);
                visited[j, i] = false;
                return;
            }

            if (_diction.IsWord(curWordStr))
                AddUniqueOrderedWord(curWordStr);
                
            visited[j, i] = true;

            // left
            if (i - 1 >= 0 && !visited[j, i - 1])
                FindWordsRecursive(i - 1, j, width, height, currentWord, visited);

            // up and left
            if (i - 1 >= 0 && j - 1 >= 0 && !visited[j - 1, i - 1])
                FindWordsRecursive(i - 1, j - 1, width, height, currentWord, visited);

            // up
            if (j - 1 >= 0 && !visited[j - 1, i])
                FindWordsRecursive(i, j - 1, width, height, currentWord, visited);

            // up and right
            if (i + 1 < width && j - 1 >= 0 && !visited[j - 1, i + 1])
                FindWordsRecursive(i + 1, j - 1, width, height, currentWord, visited);

            // right
            if (i + 1 < width && !visited[j, i + 1])
                FindWordsRecursive(i + 1, j, width, height, currentWord, visited);

            // right and down
            if (i + 1 < width && j + 1 < height && !visited[j + 1, i + 1])
                FindWordsRecursive(i + 1, j + 1, width, height, currentWord, visited);

            // down
            if (j + 1 < height && !visited[j + 1, i])
                FindWordsRecursive(i, j + 1, width, height, currentWord, visited);

            // down and left
            if (i - 1 >= 0 && j + 1 < height && !visited[j + 1, i - 1])
                FindWordsRecursive(i - 1, j + 1, width, height, currentWord, visited);

            // "rewind" the state to continue searching
            RemoveChars(_theBoard[j,i], currentWord);
            visited[j, i] = false;
        }

        /// <summary>
        /// Add word to _wordsFoundUniqueOrdered, keeping _wordsFoundUniqueOrdered ordered 
        /// and not allowing any duplicates.  We do this by BinarySearch-ing for an existing
        /// word in the list and do nothing if it exists, otherwise we insert
        /// the new word at the index we get from List<T>.BinarySearch().
        /// BinarySearch() returns a negative value if the item doesn't exist, which is
        /// the binary compliment of the index of the next element that is larger
        /// than word.
        /// </summary>
        /// <param name="word">a full word found in our traversal</param>
        private static void AddUniqueOrderedWord(string word)
        {
            int inverseIndex;

            // if inverseIndex is >= 0, then we have a duplicate word.
            // Simply do nothing in this case.
            if ((inverseIndex = _wordsFoundUniqueOrdered.BinarySearch(word)) < 0)
            {
                _wordsFoundUniqueOrdered.Insert(~inverseIndex, word);
            }
        }

        /// <summary>
        /// Helper function to remove characters from recursively built string depending
        /// on if the letter is 'qu' or not.
        /// </summary>
        /// <param name="curChar">current char to remove from the currentWord</param>
        /// <param name="currentWord">the word to remove characters from.  Will be modified.</param>
        private static void RemoveChars(string curChar, StringBuilder currentWord)
        {
            var removeChars = (curChar.Equals("qu")) ? 2 : 1;
            currentWord.Remove(currentWord.Length - removeChars, removeChars);
        }
    }
}