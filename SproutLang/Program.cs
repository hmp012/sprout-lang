// See https://aka.ms/new-console-template for more information

using SproutLang.Scanner;
using SproutLang.Parser;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("SproutLang Compiler");
        Console.WriteLine("Choose test mode:");
        Console.WriteLine("1. Test Scanner (s)");
        Console.WriteLine("2. Test Parser with file (p)");
        Console.WriteLine("3. Run Parser test suite (t)");
        Console.WriteLine();
        
        if (args.Length > 0)
        {
            switch (args[0].ToLower())
            {
                case "s":
                case "scanner":
                    TestScanner.Run(args.Skip(1).ToArray());
                    break;
                case "p":
                case "parser":
                    TestParser.Run(args.Skip(1).ToArray());
                    break;
                case "t":
                case "test":
                    TestParser.RunTestSuite();
                    break;
                default:
                    TestParser.Run(args);
                    break;
            }
        }
        else
        {
            // Default: run parser test suite
            TestParser.RunTestSuite();
        }
    }
}