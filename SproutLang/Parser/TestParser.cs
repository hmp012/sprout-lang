using SproutLang.Scanner;

namespace SproutLang.Parser;

public class TestParser
{
    public static void Run(string[] args)
    {
        string fileName = args.Length > 0 ? args[0] : "test.txt";

        Console.WriteLine($"Testing Parser with file: {fileName}");
        Console.WriteLine("=" + new string('=', 50));

        try
        {
            Console.WriteLine("Creating source file...");
            // Create source file and scanner
            SourceFile sourceFile = new SourceFile(fileName);

            Console.WriteLine("Creating scanner...");
            Scanner.Scanner scanner = new Scanner.Scanner(sourceFile);

            Console.WriteLine("Creating parser...");
            // Create parser
            Parser parser = new Parser(scanner);

            Console.WriteLine("Starting to parse...");

            // Parse the program
            parser.ParseProgram();

            Console.WriteLine("✅ Parsing completed successfully!");
            Console.WriteLine("The program is syntactically correct.");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"❌ Error: File '{fileName}' not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Parse Error: {ex.Message}");
            Console.WriteLine("The program contains syntax errors.");
        }
    }

    public static void TestWithString(string code, string testName)
    {
        Console.WriteLine($"\n--- Testing: {testName} ---");
        Console.WriteLine($"Code: {code}");

        try
        {
            // Write code to a temporary file
            string tempFile = "temp_test.txt";
            File.WriteAllText(tempFile, code);

            // Create source file and scanner
            SourceFile sourceFile = new SourceFile(tempFile);
            Scanner.Scanner scanner = new Scanner.Scanner(sourceFile);

            // Create parser
            Parser parser = new Parser(scanner);

            // Parse the program
            parser.ParseProgram();

            Console.WriteLine("✅ PASS - Parsed successfully");

            // Clean up
            File.Delete(tempFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FAIL - {ex.Message}");
            
            // Clean up
            if (File.Exists("temp_test.txt"))
                File.Delete("temp_test.txt");
        }
    }
    
    public static void RunTestSuite()
    {
        Console.WriteLine("Running Parser Test Suite");
        Console.WriteLine("=" + new string('=', 50));

        // Test 1: Simple variable declaration with type
        TestWithString("create int x = 42;", "Variable Declaration With Type");
        
        // Test 2: Array variable declaration
        TestWithString("create [int, 10] arr = 0;", "Array Variable Declaration");

        // Test 3: Variable assignment
        TestWithString("x = 10;", "Variable Assignment");

        // Test 4: Output statement
        TestWithString("vomit 42;", "Output Statement");

        // Test 5: If statement
        TestWithString("si (x < 10) vomit x;", "If Statement");

        // Test 6: If-else statement
        TestWithString("si (x < 10) vomit x; sino vomit 0;", "If-Else Statement");

        // Test 7: Block statement
        TestWithString("{ create int x = 5; vomit x; }", "Block Statement");

        // Test 8: While loop
        TestWithString("repeat (x < 10) { x = x + 1; }", "While Loop");

        // Test 9: Expression with operators (with type)
        TestWithString("create int result = x + y * 2;", "Expression with Operators");

        // Test 10: Complex program (with type)
        TestWithString(@"
create int number = 42;
vomit number + 8;
repeat (number < 10) {
    number = number + 1;
    vomit number;
}", "Complex Program");

        // Test 11: Invalid syntax (should fail, missing type)
        TestWithString("create = 42;", "Invalid Syntax (should fail)");
        
        Console.WriteLine("\n" + "=" + new string('=', 50));
        Console.WriteLine("Test Suite Complete");
    }
}