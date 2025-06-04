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
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x1;

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: AgentA <directoryPath>");
                return;
            }
            string dirPath = args[0];
            string pipeName = "agent1"; // Unique for AgentA

            var queue = new ConcurrentQueue<WordIndexEntry>();
            var readThread = new Thread(() => FileScanner(dirPath, queue));
            var sendThread = new Thread(() => PipeSender(pipeName, queue));

            readThread.Start();
            sendThread.Start();

            readThread.Join();
            // Signal end of data by enqueueing null
            queue.Enqueue(null);
            sendThread.Join();
            Console.WriteLine("AgentA finished.");

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

        static void PipeSender(string pipeName, ConcurrentQueue<WordIndexEntry> queue)
        {
            using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                client.Connect();
                var formatter = new BinaryFormatter();
                using (var stream = new BufferedStream(client))
                {
                    while (true)
                    {
                        if (queue.TryDequeue(out var entry))
                        {
                            if (entry == null) break; // End signal
                            formatter.Serialize(stream, entry);
                            stream.Flush();
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                }
            }

        }
    }
}