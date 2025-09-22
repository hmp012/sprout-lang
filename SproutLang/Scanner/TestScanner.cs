namespace SproutLang.Scanner;

public class TestScanner
{
    public static void Run(string[] args)
    {
        var fileName = "/Users/dkhumape/Documents/7th Semester/CMC/SproutLang/test.txt"; 
        var sourceFile = new SourceFile(fileName);
        var scanner = new Scanner(sourceFile);

        Token token;
        do
        {
            token = scanner.Scan();
            Console.WriteLine(token);
        } while (token.Kind != TokenKind.EOT);
    }
}