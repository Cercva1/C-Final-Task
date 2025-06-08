using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Common;

namespace Master
{
    class Program
    {
        static ConcurrentDictionary<(string, string), int> aggDict = new();

        static void Main(string[] args)
        {
            // Set CPU affinity (e.g., core 2 for Master)
            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)0x4;

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: Master <pipeName1> <pipeName2>");
                return;
            }
            string pipe1 = args[0];
            string pipe2 = args[1];

            var t1 = new Thread(() => PipeListener(pipe1));
            var t2 = new Thread(() => PipeListener(pipe2));
            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            // Print consolidated result
            foreach (var kvp in aggDict)
            {
                Console.WriteLine($"{kvp.Key.Item1}:{kvp.Key.Item2}:{kvp.Value}");
            }
        }

        static void PipeListener(string pipeName)
        {
            using (var server = new NamedPipeServerStream(pipeName, PipeDirection.In))
            {
                server.WaitForConnection();
                var formatter = new BinaryFormatter();
                using (var stream = new BufferedStream(server))
                {
                    while (true)
                    {
                        try
                        {
                            var entry = (WordIndexEntry)formatter.Deserialize(stream);
                            if (entry == null) break;
                            aggDict.AddOrUpdate(
                                (entry.FileName, entry.Word),
                                entry.Count,
                                (k, v) => v + entry.Count
                            );
                        }
                        catch
                        {
                            break; // End of stream
                        }
                    }
                }
            }
        }
    }
}