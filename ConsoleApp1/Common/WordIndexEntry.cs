using System;

namespace Common
{
    [Serializable]
    public class WordIndexEntry
    {
        public string FileName { get; set; }
        public string Word { get; set; }
        public int Count { get; set; }

        public WordIndexEntry(string fileName, string word, int count)
        {
            FileName = fileName;
            Word = word;
            Count = count;
        }

        // For deserialization
        public WordIndexEntry()
        {
            FileName = "";
            Word = "";
            Count = 0;
        }
    }
}