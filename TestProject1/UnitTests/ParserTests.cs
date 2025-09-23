namespace TestProject1.UnitTests;

using SproutLang.Scanner;
using SproutLang.Parser;
using Xunit;

public class ParserTests
{
    private void AssertParses(string code)
    {
        string tempFile = "temp_test.txt";
        File.WriteAllText(tempFile, code);
        try
        {
            var sourceFile = new SourceFile(tempFile);
            var scanner = new Scanner(sourceFile);
            var parser = new Parser(scanner);
            parser.ParseProgram();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    private void AssertFails(string code)
    {
        string tempFile = "temp_test.txt";
        File.WriteAllText(tempFile, code);
        try
        {
            var sourceFile = new SourceFile(tempFile);
            var scanner = new Scanner(sourceFile);
            var parser = new Parser(scanner);
            Assert.Throws<Exception>(() => parser.ParseProgram());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void VariableDeclarationWithType() =>
        AssertParses("create int x = 42;");

    [Fact]
    public void ArrayVariableDeclaration() =>
        AssertParses("create [int, 10] arr = 0;");

    [Fact]
    public void VariableAssignment() =>
        AssertParses("x = 10;");

    [Fact]
    public void OutputStatement() =>
        AssertParses("vomit 42;");

    [Fact]
    public void IfStatement() =>
        AssertParses("si (x < 10) vomit x;");

    [Fact]
    public void IfElseStatement() =>
        AssertParses("si (x < 10) vomit x; sino vomit 0;");

    [Fact]
    public void BlockStatement() =>
        AssertParses("{ create int x = 5; vomit x; }");

    [Fact]
    public void WhileLoop() =>
        AssertParses("repeat (x < 10) { x = x + 1; }");

    [Fact]
    public void ExpressionWithOperators() =>
        AssertParses("create int result = x + y * 2;");

    [Fact]
    public void ComplexProgram() =>
        AssertParses(@"
create int number = 42;
vomit number + 8;
repeat (number < 10) {
    number = number + 1;
    vomit number;
}");

    [Fact]
    public void InvalidSyntaxShouldFail() =>
        AssertFails("create = 42;");
}