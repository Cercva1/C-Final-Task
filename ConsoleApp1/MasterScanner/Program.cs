using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Common;

namespace MasterScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MasterScanner server starting on port 5000...");
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();

            while (true)
            {
                Console.WriteLine("Waiting for agent connection...");
                using (TcpClient client = server.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, bytesRead);
                            if (!stream.DataAvailable) break;
                        }
                        string jsonString = Encoding.UTF8.GetString(ms.ToArray());
                        try
                        {
                            List<WordIndexEntry>? entries = JsonSerializer.Deserialize<List<WordIndexEntry>>(jsonString);
                            Console.WriteLine($"Received {entries?.Count ?? 0} word entries from agent:");
                            if (entries != null)
                            {
                                foreach (var entry in entries)
                                {
                                    Console.WriteLine($"{entry.FileName}: {entry.Word} - {entry.Count}");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to parse received data: " + e.Message);
                        }
                    }
                }
            }
        }
    }
}