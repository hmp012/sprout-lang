namespace SproutLang.Scanner;

public class Token
{
    public TokenKind Kind { get; }
    public string Spelling { get; }

    public Token(TokenKind kind, string spelling)
    {
        Kind = kind;
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