﻿using System;

namespace BoggleGame.Game
{
    /// <summary>
    /// Represents the board and the rules of the board.
    /// Rules:
    /// - board must be n x n dimensions.  
    /// - the char 'q' when defining the board will represent 'qu', but only takes up one spot on the board.
    /// - board only contains letters: no special characters, non-letter characters, or whitespace!
    /// - all characters on the board will be stored in lower case (converted if not already)
    /// </summary>
    public class BoggleBoard
    {
        /// <summary>
        /// Since dimension is always n x n, we only store one axis length
        /// </summary>
        public int Dimension { get; private set; }

        /// <summary>
        /// Indexer can be used like array accessor brackes on "this" object.
        /// For example if you have a variable:
        /// BoggleBoard theBoard = new BoggleBoard(...)
        /// you can access theBoard like this: theBoard[0,0].
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public String this[int i, int j]
        {
            get { return _board[i, j]; }
        }

        // store as a string in order to represent "qu".
        // This will take up more space than chars, but 
        // the boggle board will never be that big in reality,
        // so the extra space is negligable.
        // for a 2x2 board, the indices will look like this:
        // | (0,0)  (0,1) |
        // | (1,0)  (1,1) |
        private readonly String[,] _board;

        public void PrintBoard()
        {
            for (var i = 0; i < Dimension; ++i)
            {
                for (var j = 0; j < Dimension; ++j)
                {
                    if (j == 0)
                        Console.Write("| ");

                    Console.Write(_board[i, j] + " ");
                    
                    if (j == Dimension - 1)
                        Console.Write("|");                   
                }

                Console.WriteLine();
            }
        }

        public BoggleBoard(int dimension, String board)
        {
            // is board null, empty, or only whitespace?
            if (String.IsNullOrWhiteSpace(board))
                throw new ArgumentException(
                    "The board passed in is empty or consists of only empty characters.",
                    "board"
                );

            // bounds check 
            // dimension * dimension == board.Length
            if ((dimension * dimension != board.Length))
                throw new ArgumentException(
                    String.Format(
                        "The board dimension passed in ({0}) and the board passed " +
                         " in ({1}) don't match up in length.",
                         dimension,
                         board
                    ),
                    "dimension"
                );

            Dimension = dimension;

            // init board memory
            _board = new string[dimension, dimension];

            var lowerCaseBoard = board.ToLower();

            // initialize board, swapping 'q' with "qu" and converting
            // all chars to Strings.
            for (var i = 0; i < dimension; ++i)
            {
                for (var j = 0; j < dimension; ++j)
                {
                    // convert from 2d -> 1d
                    var stringIndex = i * dimension + j;
                    var curChar = lowerCaseBoard[stringIndex];

                    if (! Char.IsLetter(curChar))
                        throw new ArgumentException(
                            String.Format(
                                "Non-alphabetic character found in board '{0}'",
                                curChar
                            ),
                            board);

                    _board[i, j] = (curChar.Equals('q')) ? "qu" : curChar.ToString();
                }
            }
        }
    }
}
