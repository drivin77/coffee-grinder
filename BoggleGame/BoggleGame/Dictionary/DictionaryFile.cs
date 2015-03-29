using System;
using System.Collections.Generic;
using System.IO;

namespace BoggleGame.Dictionary
{
    /// <summary>
    /// Opens file on disk containing dictionary words (one per line)
    /// and exposes functionality to retrieve each word.
    /// 
    /// Once the DictionaryFile has been iterated through, it shouldn't be used again;
    /// create a new DictionaryFile if needed.
    /// </summary>
    public class DictionaryFile
    {
        private readonly StreamReader _fileStream;

        public DictionaryFile (String fileName)
        {
            try
            {
                _fileStream = new StreamReader(fileName);
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

        /// <summary>
        /// Allows you to iterate through all words in the file
        /// as if they were in memory. 
        /// ex: foreach (String s in DictionaryFile.Words)...
        /// </summary>
        public IEnumerable<String> Words
        {
            get
            {
                while (_fileStream.Peek() >= 0)
                    yield return _fileStream.ReadLine();
                
                _fileStream.Close();
            }
        }
    }
}
