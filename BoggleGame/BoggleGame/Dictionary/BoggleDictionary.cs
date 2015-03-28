using System;
using BoggleGame.DataStructures;

namespace BoggleGame.Dictionary
{
    public class BoggleDictionary
    {
        private readonly TST _wordTrie;

        public uint NumWords
        {
            get { return _wordTrie.NumElements; }
        }

        /// <summary>
        /// Initializes the dictionary from a DictionaryFile.
        /// </summary>
        public BoggleDictionary()
        {
            _wordTrie = new TST();

            var df = new DictionaryFile("ospd.txt");

            foreach (var word in df.Words)
            {
                _wordTrie.Put(word);
            }
        }

        /// <summary>
        /// Is testString a valid full word in the dictionary?
        /// We don't allow one letter words.
        /// </summary>
        /// <param name="testString">string to test for existance</param>
        /// <returns>true if in dictionary, false otherwise</returns>
        public Boolean IsWord(String testString)
        {
            return testString.Length > 1 && _wordTrie.Get(testString);
        }

        public bool IsStartOfWord(String prefix)
        {
            return _wordTrie.IsStartOfKey(prefix);
        }
    }
}
