using System;
using System.IO;
using BoggleGame.DataStructures;

namespace BoggleGame.Dictionary
{
    /// <summary>
    /// The dictionary.
    /// Rules:
    /// - We'll use the Official Scrabble Player's Dictionary (ospd.txt) as our dictionary, which is 
    ///   also suitable as a Boggle official dictionary (according to Wikipedia).
    /// - Words must be 3 characters or longer, so we'll only store 3 letter or greater words.  This
    ///   will save us space in the data structure.
    /// </summary>
    public class BoggleDictionary
    {
        private readonly TST _wordTrie;

        public uint NumWords
        {
            get { return _wordTrie.NumElements; }
        }

        public string DictionaryFileName
        {
            get { return "ospd.txt"; }
        }
        
        /// <summary>
        /// Initializes the dictionary from a DictionaryFile.
        /// </summary>
        public BoggleDictionary()
        {
            _wordTrie = new TST();

            // ospd.txt is copied into the executable directory on 
            // successful compile to path Dictionary/ospd.txt.
            // GetCurrentDirectory() will return us the exe path.
            var df = new DictionaryFile(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Dictionary",
                    DictionaryFileName)
                );

            foreach (var word in df.Words)
            {
                if (word.Length >= 3)
                {
                    try
                    {
                        _wordTrie.Put(word);
                    }
                    catch (ArgumentException e)
                    {
                        const string message = "Unacceptable word found in dictionary file. " +
                                               "See inner-exception for details.";
                        throw new ArgumentException(message, e);
                    }
                }
                    
            }
        }

        /// <summary>
        /// Is testString a valid full word in the dictionary?
        /// We don't allow one letter words.
        /// </summary>
        /// <param name="testString">string to test for existance</param>
        /// <returns>true if in dictionary, false otherwise</returns>
        public Boolean IsWord(string testString)
        {
            return testString.Length > 1 && _wordTrie.Get(testString);
        }

        /// <summary>
        /// Is prefix the start of a valid word?  Will be useful for
        /// iterating through the Boggle board and quickly exiting a
        /// search path if we know it's not a word.
        /// </summary>
        /// <param name="prefix">String to check for potential word existance</param>
        /// <returns>true if prefix could be a word, false otherwise</returns>
        public bool IsStartOfWord(string prefix)
        {
            return _wordTrie.IsStartOfKey(prefix);
        }
    }
}
