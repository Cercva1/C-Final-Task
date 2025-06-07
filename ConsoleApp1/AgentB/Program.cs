using System;
using System.Collections.Concurrent;
using System.Threading;
using Common; // Assuming WordIndexEntry is here

namespace AgentB
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: AgentB <directory>");
                return;
            }

            string directory = args[0];
            var queue = new ConcurrentQueue<WordIndexEntry>();

            // Thread 1: Scan files and enqueue results
            var scannerThread = new Thread(() =>
            {
                var scanner = new FileScanner(directory);
                foreach (var entry in scanner.ScanFiles())
                {
                    queue.Enqueue(entry);
                }
                queue.Enqueue(null); // Signal end
            });

            // Thread 2: Send results to Master via pipe "agent2"
            var senderThread = new Thread(() =>
            {
                PipeSender.Send("agent2", queue);
            });

            scannerThread.Start();
            senderThread.Start();

            scannerThread.Join();
            senderThread.Join();
        }
    }
}
