namespace SproutLang.Scanner;

public enum TokenKind
{
    // Literals
    IntLiteral,
    CharLiteral,
    StringLiteral,

    // Identifiers
    Identifier,

    // Types
    Int,
    Bool,
    Char,
    String,

    // Keywords
    Create,
    Vomit,
    ListenCarefully,
    Si,
    O,
    Sino,
    Repeat,
    Until,
    Sprout,
    Bloom,
    Times,

    // Operators
    Operators,

    // Symbols
    Comma, Semicolon,
    LParenthesis, RParenthesis,
    LBracket, RBracket,
    LBrace, RBrace,

    // Special
    EOT,
    Error
}

public static class TokenKindExtensions
{
    private static readonly Dictionary<TokenKind, string> Spellings = new()
    {
        { TokenKind.Int, "int" },
        { TokenKind.Bool, "bool" },
        { TokenKind.Char, "char" },
        { TokenKind.String, "string" },

        { TokenKind.Create, "create" },
        { TokenKind.Vomit, "vomit" },
        { TokenKind.ListenCarefully, "listenCarefully" },
        { TokenKind.Si, "si" },
        { TokenKind.O, "o" },
        { TokenKind.Sino, "sino" },
        { TokenKind.Repeat, "repeat" },
        { TokenKind.Until, "until" },
        { TokenKind.Sprout, "sprout" },
        { TokenKind.Bloom, "bloom" },
        { TokenKind.Times, "times" },

        { TokenKind.Operators, "operators"},

        { TokenKind.Comma, "," },
        { TokenKind.Semicolon, ";" },
        { TokenKind.LParenthesis, "(" },
        { TokenKind.RParenthesis, ")" },
        { TokenKind.LBracket, "[" },
        { TokenKind.RBracket, "]" },
        { TokenKind.LBrace, "{" },
        { TokenKind.RBrace, "}" },
        
        { TokenKind.EOT, "EOT" },
        {TokenKind.Error, "Error" }
    };

    public static string GetSpelling(this TokenKind kind)
    {
        return Spellings.TryGetValue(kind, out var spelling) ? spelling : kind.ToString();
    }
}