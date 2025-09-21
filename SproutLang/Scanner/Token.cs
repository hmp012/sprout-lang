namespace SproutLang.Scanner;

public class Token
{
    public TokenKind Kind { get; }
    public string Spelling { get; }

    public Token(TokenKind kind, string spelling)
    {
        var keywords = new[]
        {
            TokenKind.Create, TokenKind.Vomit, TokenKind.ListenCarefully,
            TokenKind.Si, TokenKind.O, TokenKind.Sino, TokenKind.Repeat,
            TokenKind.Until, TokenKind.Sprout, TokenKind.Bloom, TokenKind.Times
        };
        TokenKind actualKind = kind;
        
        if (kind == TokenKind.Identifier)
        {
            foreach (var keyword in keywords)
            {
                if (spelling == keyword.GetSpelling())
                {
                    actualKind = keyword;
                    break;
                }
            }
        }
        Kind = actualKind;
        Spelling = spelling;
    }

    public override string ToString()
    {
        return $"{Kind} ('{Spelling}')";
    }

    public bool IsOperator()
    {
        return Kind is TokenKind.Plus or TokenKind.Minus or TokenKind.Multiply or TokenKind.Divide
            or TokenKind.Equals or TokenKind.NotEquals or TokenKind.LessThan or TokenKind.GreaterThan
            or TokenKind.And or TokenKind.Or or TokenKind.Not or TokenKind.Assign;
    }

    public bool IsLiteral()
    {
        return Kind is TokenKind.IntLiteral or TokenKind.CharLiteral or TokenKind.StringLiteral;
    }

    public bool IsKeyword()
    {
        return Kind switch
        {
            TokenKind.Create or TokenKind.Vomit or TokenKind.ListenCarefully or
                TokenKind.Si or TokenKind.O or TokenKind.Sino or TokenKind.Repeat or
                TokenKind.Until or TokenKind.Sprout or TokenKind.Bloom or TokenKind.Times => true,
            _ => false
        };
    }
}