/**
 * Class TST - Ternary Search Trie
 * Fast lookup (reTRIEval) of string objects
 * More space efficient than a standard trie data structure.
 * 
 * This will be the main data structure to store our dictionary
 * as we can quickly look up if a word is valid (and even more quickly
 * look up if a word is invalid) as we move through the Boggle board.
 * 
 * Slightly customized version of traditional TST.
 * We're not interested in storing Values in the trie, but 
 * we store 0 to mark a valid word.  For example if the root
 * node has key 'n', the node's value will be null.  This is how our
 * dictionary knows 'n' isn't a word.  Then if we look at N's node->RightChild
 * and see 'o', this node will have a 0 meaning that 'no' is in fact 
 * a valid word.
**/

using System;

namespace BoggleGame.DataStructures
{
    public class TST
    {
        private Node _root;
        public UInt32 NumElements { get; private set; }

        /// <summary>
        /// Object type for each node in the trie
        /// </summary>
        private class Node
        {           
            public char Ch { get; set; }
            public short? Value {  get;  set; }
            public Node LeftChild {  get;  set; }
            public Node MiddleChild {  get;  set; }
            public Node RightChild {  get;  set; }
        }

        /// <summary>
        /// Determines if 'prefix' is a valid start to a key.  
        /// </summary>
        /// <param name="prefix">are the characters in this string valid in the TST?</param>
        /// <returns>true if the characters exist as part of a key, false otherwise.</returns>
        public bool IsStartOfKey(string prefix)
        {
            if (String.IsNullOrWhiteSpace(prefix))
                return false;

            var getNode = Get(_root, prefix, 0);

            // if getNode is NULL, then the last character found doesn't exist.  'key' isn't a word
            // and can't be a word.  Otherwise this could potentially be a word
            return getNode != null;
        }

        /// <summary>
        /// Does 'key' exist in the trie?
        /// </summary>
        /// <param name="key">string to search for in the trie</param>
        /// <returns>true if key exists, false otherwise</returns>
        public bool Get (string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                return false;

            var getNode = Get(_root, key, 0);

            // if getNode is NULL, then the last character found doesn't exist.  'key' isn't a word
            // and can't be a word.
            if (getNode == null)
                return false;

            return getNode.Value != null;
        }

        private Node Get (Node root, string key, int idx)
        {
            if (String.IsNullOrWhiteSpace(key))
                return null;

            if (idx >= key.Length)
                throw new ArgumentOutOfRangeException(
                    "idx",
                    "idx is bigger than the length of the string"
                );

            if (root == null)
                return null;

            var curChar = key[idx];

            if (!Char.IsLetter(curChar))
            {
                throw new ArgumentException(
                    String.Format(
                        "non-alphabetic character found in search key ({0})",
                        key
                    ),
                "key");
            }


            if (curChar < root.Ch)
                return Get(root.LeftChild, key, idx);

            if (curChar > root.Ch)
                return Get(root.RightChild, key, idx);

            if (idx < key.Length - 1)
                return Get(root.MiddleChild, key, idx + 1);
            
            return root;

        }
    
        /// <summary>
        /// Puts key into the trie, if it's not there already
        /// </summary>
        /// <param name="key">string to be inserted</param>
        public void Put (string key)
        {
            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentException();

            ++NumElements;
            _root = Put(_root, key, 0);
        }

        private static Node Put (Node root, string key, int idx)
        {
            var curChar = key[idx];

            if (! Char.IsLetter(curChar))
                throw new ArgumentException(
                    String.Format(
                        "Can't add a key with non-alphabetic characters ({0})", key),
                        "key"
                    );

            if (root == null)
                root = new Node {Ch = curChar};

            if (curChar < root.Ch)
                root.LeftChild = Put(root.LeftChild, key, idx);

            else if (curChar > root.Ch)
                root.RightChild = Put(root.RightChild, key, idx);

            else if (idx < key.Length - 1)
                root.MiddleChild = Put(root.MiddleChild, key, idx + 1);
 
            else
                root.Value = 0;

            return root;
        }
    }
}
