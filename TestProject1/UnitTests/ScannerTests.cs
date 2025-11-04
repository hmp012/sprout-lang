using SproutLang.Scanner;

namespace TestProject1.UnitTests;

public class ScannerTests
{
    private Token[] ScanAll(string code)
    {
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, code);
        try
        {
            var sourceFile = new SourceFile(tempFile);
            var scanner = new Scanner(sourceFile);
            var tokens = new List<Token>();
            Token token;
            do
            {
                token = scanner.Scan();
                tokens.Add(token);
            } while (token.Kind != TokenKind.EOT);
            return tokens.ToArray();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
    
    [Fact]
    public void ScansSimpleDeclaration()
    {
        var tokens = ScanAll("create int x = 5;");
        Assert.Equal(TokenKind.Create, tokens[0].Kind);
        Assert.Equal(TokenKind.Int, tokens[1].Kind);
        Assert.Equal(TokenKind.Identifier, tokens[2].Kind);
        Assert.Equal(TokenKind.Assign, tokens[3].Kind);
        Assert.Equal(TokenKind.IntLiteral, tokens[4].Kind);
        Assert.Equal(TokenKind.Semicolon, tokens[5].Kind);
        Assert.Equal(TokenKind.EOT, tokens[6].Kind);
    }

    [Fact]
    public void ScansLiterals()
    {
        var tokens = ScanAll("123");
        Assert.Equal(TokenKind.IntLiteral, tokens[0].Kind);
        Assert.Equal(TokenKind.EOT, tokens[1].Kind);
    }

    [Fact]
    public void ScansIdentifiers()
    {
        var tokens = ScanAll("myVar");
        Assert.Equal(TokenKind.Identifier, tokens[0].Kind);
        Assert.Equal(TokenKind.EOT, tokens[1].Kind);
    }

    [Fact]
    public void ScansTypes()
    {
        var tokens = ScanAll("int bool char");
        Assert.Equal(TokenKind.Int, tokens[0].Kind);
        Assert.Equal(TokenKind.Bool, tokens[1].Kind);
        Assert.Equal(TokenKind.Char, tokens[2].Kind);
        Assert.Equal(TokenKind.EOT, tokens[3].Kind);
    }

    [Fact]
    public void ScansKeywords()
    {
        var code = "create vomit listenCarefully si o sino repeat until sprout bloom times";
        var expected = new[]
        {
            TokenKind.Create, TokenKind.Vomit, TokenKind.ListenCarefully, TokenKind.Si,
            TokenKind.O, TokenKind.Sino, TokenKind.Repeat, TokenKind.Until,
            TokenKind.Sprout, TokenKind.Bloom, TokenKind.Times, TokenKind.EOT
        };
        var tokens = ScanAll(code);
        for (int i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], tokens[i].Kind);
    }

    [Fact]
    public void ScansOperators()
    {
        var tokens = ScanAll("+ - * / == != < > && || ! = ");
        var expected = new[]
        {
            TokenKind.Plus, TokenKind.Minus, TokenKind.Multiply, TokenKind.Divide,
            TokenKind.Equals, TokenKind.NotEquals, TokenKind.LessThan, TokenKind.GreaterThan,
            TokenKind.And, TokenKind.Or, TokenKind.Not, TokenKind.Assign, TokenKind.EOT
        };
        for (int i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], tokens[i].Kind);
    }

    [Fact]
    public void ScansSymbols()
    {
        var tokens = ScanAll(", ; ( ) [ ] { }");
        var expected = new[]
        {
            TokenKind.Comma, TokenKind.Semicolon,
            TokenKind.LParenthesis, TokenKind.RParenthesis,
            TokenKind.LBracket, TokenKind.RBracket,
            TokenKind.LBrace, TokenKind.RBrace,
            TokenKind.EOT
        };
        for (int i = 0; i < expected.Length; i++)
            Assert.Equal(expected[i], tokens[i].Kind);
    }

    [Fact]
    public void ScansEOT()
    {
        var tokens = ScanAll("");
        Assert.Single(tokens);
        Assert.Equal(TokenKind.EOT, tokens[0].Kind);
    }

    [Fact]
    public void ScansErrorToken()
    {
        var tokens = ScanAll("@");
        Assert.Equal(TokenKind.Error, tokens[0].Kind);
        Assert.Equal(TokenKind.EOT, tokens[1].Kind);
    }
    
    [Fact]
    public void ScansBoolLiterals()
    {
        var tokens = ScanAll("true false");
        Assert.Equal(TokenKind.BoolLiteral, tokens[0].Kind);
        Assert.Equal(TokenKind.BoolLiteral, tokens[1].Kind);
        Assert.Equal(TokenKind.EOT, tokens[2].Kind);
    }

    [Fact]
    public void ScansCharLiterals()
    {
        var tokens = ScanAll("'A' 'z' '1'");
        Assert.Equal(TokenKind.CharLiteral, tokens[0].Kind);
        Assert.Equal("A", tokens[0].Spelling);
        Assert.Equal(TokenKind.CharLiteral, tokens[1].Kind);
        Assert.Equal("z", tokens[1].Spelling);
        Assert.Equal(TokenKind.CharLiteral, tokens[2].Kind);
        Assert.Equal("1", tokens[2].Spelling);
        Assert.Equal(TokenKind.EOT, tokens[3].Kind);
    }
}