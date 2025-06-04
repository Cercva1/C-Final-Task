using system;

namespace Common
{   
    [Serializable]
    public class WordIndexEntry
    {
        public string FileName { get; set; }
        public string Word { get; set; }
        public int Count { get; set; }

    }   
    
}

