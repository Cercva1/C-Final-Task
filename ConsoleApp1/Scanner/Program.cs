using System;
using System.IO;

namespace Scanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Scanner agent started.");

            // Example agent logic: list .txt files in a directory
            string dir = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            Console.WriteLine($"Scanning directory: {dir}");

            foreach (var file in Directory.GetFiles(dir, "*.txt"))
            {
                Console.WriteLine($"Found file: {file}");
            }

            // Keep the app running until user presses Enter (optional)
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}