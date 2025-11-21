// See https://aka.ms/new-console-template for more information

using SproutLang.Scanner;
using SproutLang.Parser;
using SproutLang.Compiler;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("SproutLang Compiler");
        Console.WriteLine("===================");
        Console.WriteLine();
        
        if (args.Length > 0)
        {
            // Compile the file specified in command-line arguments
            string sourcePath = args[0];
            
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Error: File not found: {sourcePath}");
                Environment.Exit(1);
            }
            
            bool success = Compiler.CompileFile(sourcePath);
            Environment.Exit(success ? 0 : 1);
        }
        else
        {
            // Interactive mode - prompt for file path
            Console.WriteLine("Enter the path to the SproutLang source file:");
            string? sourcePath = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                Console.WriteLine("No file specified. Exiting.");
                Environment.Exit(1);
            }
            
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Error: File not found: {sourcePath}");
                Environment.Exit(1);
            }
            
            bool success = Compiler.CompileFile(sourcePath);
            
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            
            Environment.Exit(success ? 0 : 1);
        }
    }
}