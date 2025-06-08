using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Common;

namespace ScannerA
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: ScannerA <directoryPath>");
                return;
            }

            string directoryPath = args[0];
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist: {directoryPath}");
                return;
            }

            Console.WriteLine($"ScannerA scanning directory: {directoryPath}");

            var entries = new List<WordIndexEntry>();

            foreach (var file in Directory.GetFiles(directoryPath, "*.txt"))
            {
                var wordCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var line in File.ReadLines(file))
                {
                    var words = line.Split(new[] { ' ', '\t', '.', ',', ';', ':', '!', '?', '-', '_', '"', '(', ')', '[', ']', '{', '}', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        var trimmed = word.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed)) continue;
                        if (!wordCounts.ContainsKey(trimmed))
                            wordCounts[trimmed] = 0;
                        wordCounts[trimmed]++;
                    }
                }
                foreach (var kvp in wordCounts)
                {
                    entries.Add(new WordIndexEntry
                    {
                        FileName = Path.GetFileName(file) ?? "",
                        Word = kvp.Key,
                        Count = kvp.Value
                    });
                }
            }

            Console.WriteLine($"Found {entries.Count} word entries. Connecting to master...");

            try
            {
                using (var client = new TcpClient("127.0.0.1", 5000))
                using (var stream = client.GetStream())
                {
                    string jsonString = JsonSerializer.Serialize(entries);
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                    stream.Write(jsonBytes, 0, jsonBytes.Length);
                    Console.WriteLine("Data sent to master successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to master: {ex.Message}");
            }

            Console.WriteLine("ScannerA finished.");
        }
    }
}