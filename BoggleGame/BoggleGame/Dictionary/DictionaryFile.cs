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
    /// I'm assuming the file containing the dictionary has
    /// no repeating words.
    /// 
    /// It's important to randomize the insert order of the words into the
    /// TST, so we'll randomize the order of the words to get a well-balanced
    /// trie.
    /// </summary>
    public class DictionaryFile
    {
        // store the dictionary words as an IEnumerable, so we don't read the 
        // entire file in to memory.  This acts as a stream, so we only keep
        // a small number of words in memory at once.
        private readonly IEnumerable<string> _dictionaryFileLines;

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
                // one word at a time.  The entire file isn't in memory,
                // instead we stream the file via the IEnumerator and
                // ReadLines function.
                return _dictionaryFileLines;
            }
        }

        public DictionaryFile (string fileName)
        {
            try
            {
                // File.ReadLines returns an IEnumerable, which is faster
                // and more memory efficient than allocating memory
                // for the entire file at once.  As we enumerate, 
                // the file is read in sections so we don't have to
                // load a large dictionary all at once.
                _dictionaryFileLines = File.ReadLines(fileName);
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
