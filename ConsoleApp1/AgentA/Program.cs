using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Common;

namespace AgentA
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set CPU affinity (e.g., core 0 for AgentA)

        }

         static void FileScanner(string dirPath, ConcurrentQueue<WordIndexEntry> queue)
        {
            foreach (var file in Directory.GetFiles(dirPath, "*.txt"))
            {
                var wordCount = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in File.ReadLines(file))
                {
                    foreach (var word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        wordCount.AddOrUpdate(word, 1, (k, v) => v + 1);
                    }
                }
                foreach (var kvp in wordCount)
                {
                    queue.Enqueue(new WordIndexEntry
                    {
                        FileName = Path.GetFileName(file),
                        Word = kvp.Key,
                        Count = kvp.Value
                    });
                }
            }
        }

    }
}