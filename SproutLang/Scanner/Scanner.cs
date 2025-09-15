namespace SproutLang.Scanner;

using System;
using System.Collections.Generic;
using System.Text;

public class Scanner
{
    private SourceFile source;
    private char currentChar;
    private StringBuilder currentSpelling = new StringBuilder();

    private static readonly Dictionary<string, TokenKind> Keywords = new(StringComparer.Ordinal)
    {
        { "int", TokenKind.Int },
        { "bool", TokenKind.Bool },
        { "char", TokenKind.Char },
        { "string", TokenKind.String },

        { "create", TokenKind.Create },
        { "vomit", TokenKind.Vomit },
        { "listenCarefully", TokenKind.ListenCarefully },
        { "si", TokenKind.Si },
        { "o", TokenKind.O },
        { "sino", TokenKind.Sino },
        { "repeat", TokenKind.Repeat },
        { "until", TokenKind.Until },
        { "sprout", TokenKind.Sprout },
        { "bloom", TokenKind.Bloom },
        { "times", TokenKind.Times }
    };

    public Scanner(SourceFile source)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        currentChar = source.GetSource();
    }

    // Convenience constructor: open by filename
    public Scanner(string sourceFileName) : this(new SourceFile(sourceFileName)) { }

    private void TakeIt()
    {
        currentSpelling.Append(currentChar);
        currentChar = source.GetSource();
    }

    private static bool IsLetter(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }

    private static bool IsDigit(char c)
    {
        return (c >= '0' && c <= '9');
    }

    private void ScanSeparator()
    {
        switch (currentChar)
        {
            case '#':
                // comment until end of line
                TakeIt();
                while (currentChar != SourceFile.EOL && currentChar != SourceFile.EOT)
                    TakeIt();
                if (currentChar == SourceFile.EOL)
                    TakeIt();
                break;

            case ' ':
            case '\n':
            case '\r':
            case '\t':
                TakeIt();
                break;
        }
    }

    private TokenKind ScanToken()
    {
        if (IsLetter(currentChar))
        {
            TakeIt();
            while (IsLetter(currentChar) || IsDigit(currentChar))
                TakeIt();

            var spelling = currentSpelling.ToString();
            if (Keywords.TryGetValue(spelling, out var kw))
                return kw;
            return TokenKind.Identifier;
        }
        else if (IsDigit(currentChar))
        {
            TakeIt();
            while (IsDigit(currentChar))
                TakeIt();

            return TokenKind.IntLiteral;
        }
        else
        {
            switch (currentChar)
            {
                case '+':
                    TakeIt();
                    return TokenKind.Plus;
                case '-':
                    TakeIt();
                    return TokenKind.Minus;
                case '*':
                    TakeIt();
                    return TokenKind.Multiply;
                case '/':
                    TakeIt();
                    return TokenKind.Divide;

                case '=':
                    TakeIt();
                    if (currentChar == '=')
                    {
                        TakeIt();
                        return TokenKind.Equals;
                    }
                    return TokenKind.Assign;

                case '!':
                    TakeIt();
                    if (currentChar == '=')
                    {
                        TakeIt();
                        return TokenKind.NotEquals;
                    }
                    return TokenKind.Not;

                case '&':
                    TakeIt();
                    if (currentChar == '&')
                    {
                        TakeIt();
                        return TokenKind.And;
                    }
                    // single & not supported in language
                    return TokenKind.Error;

                case '|':
                    TakeIt();
                    if (currentChar == '|')
                    {
                        TakeIt();
                        return TokenKind.Or;
                    }
                    return TokenKind.Error;

                case '<':
                    TakeIt();
                    return TokenKind.LessThan;
                case '>':
                    TakeIt();
                    return TokenKind.GreaterThan;

                case ',':
                    TakeIt();
                    return TokenKind.Comma;
                case ';':
                    TakeIt();
                    return TokenKind.Semicolon;
                case '(':
                    TakeIt();
                    return TokenKind.LParenthesis;
                case ')':
                    TakeIt();
                    return TokenKind.RParenthesis;
                case '[':
                    TakeIt();
                    return TokenKind.LBracket;
                case ']':
                    TakeIt();
                    return TokenKind.RBracket;
                case '{':
                    TakeIt();
                    return TokenKind.LBrace;
                case '}':
                    TakeIt();
                    return TokenKind.RBrace;

                case '\'':
                    // char literal: 'a' or '\n' etc. Keep simple and accept until closing '\'' or EOT
                    TakeIt(); // opening '
                    if (currentChar != SourceFile.EOT && currentChar != SourceFile.EOL)
                    {
                        // accept possibly escaped char
                        if (currentChar == '\\')
                        {
                            TakeIt();
                            if (currentChar != SourceFile.EOT && currentChar != SourceFile.EOL)
                                TakeIt();
                        }
                        else
                            TakeIt();
                    }
                    if (currentChar == '\'')
                        TakeIt();
                    return TokenKind.CharLiteral;

                case '"':
                    // string literal
                    TakeIt(); // opening "
                    while (currentChar != '"' && currentChar != SourceFile.EOT)
                    {
                        if (currentChar == '\\')
                        {
                            TakeIt();
                            if (currentChar != SourceFile.EOT)
                                TakeIt();
                        }
                        else
                            TakeIt();
                    }
                    if (currentChar == '"')
                        TakeIt();
                    return TokenKind.StringLiteral;

                case SourceFile.EOT:
                    return TokenKind.EOT;

                default:
                    // unknown single char -> produce error token but still consume
                    TakeIt();
                    return TokenKind.Error;
            }
        }
    }

    public Token Scan()
    {
        while (currentChar == '#' || currentChar == '\n' || currentChar == '\r' || currentChar == '\t' || currentChar == ' ')
            ScanSeparator();

        currentSpelling = new StringBuilder();
        TokenKind kind = ScanToken();

        string spelling = kind == TokenKind.EOT ? TokenKind.EOT.GetSpelling() : currentSpelling.ToString();
        return new Token(kind, spelling);
    }
}

