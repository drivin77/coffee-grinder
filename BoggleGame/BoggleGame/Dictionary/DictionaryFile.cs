using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BoggleGame.Dictionary
{
    /// <summary>
    /// Opens file on disk containing dictionary words (one per line)
    /// and exposes functionality to retrieve each word.
    /// 
    /// It's important to randomize the insert order of the words into the
    /// TST, so we'll randomize the order of the words to get a well-balanced
    /// trie
    /// </summary>
    public class DictionaryFile
    {
        private readonly List<string> _dictionaryFileLines;

        /// <summary>
        /// Allows you to iterate through all words in the file
        /// as if they were in memory. 
        /// ex: foreach (String s in DictionaryFile.Words)...
        /// </summary>
        public IEnumerable<string> Words
        {
            get
            {
                // this is a linq shortcut which allows us to return 
                // one element at a time.  Basically the List<string>
                // is auto-converted to an IEnumerable<String>
                return _dictionaryFileLines;
            }
        }

        public DictionaryFile (string fileName)
        {
            try
            {
                // we need to randomize the words in the dictionary
                // for a well-balanced trie
                _dictionaryFileLines = File.ReadAllLines(fileName).ToList();

                // easy way to "shuffle" the word array
                var r = new Random();
                _dictionaryFileLines = _dictionaryFileLines.OrderBy(x => r.Next()).ToList();
            }

            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(
                    String.Format(
                        "Dictionary file couldn't be found ('{0}').",
                        fileName
                    )
                );
            }         
        }
    }
}
