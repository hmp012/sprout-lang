using System.Text;

namespace SproutLang.Scanner;

public class Scanner : IScanner
{
    private SourceFile _sourceFile;
    
    private char currentChar;
    private StringBuilder currentSpelling = new StringBuilder();

    public Scanner(SourceFile sourceFile)
    {
        _sourceFile = sourceFile;

        currentChar = sourceFile.GetSource();
    }
    
    private void TakeIt()
    {
        currentSpelling.Append(currentChar);
        currentChar = _sourceFile.GetSource();
    }
    
    private void scanSeparator()
    {
       switch (currentChar)
       {
           case '#':
            TakeIt();
            while ( currentChar != SourceFile.EOL && currentChar != SourceFile.EOT )
            {
                TakeIt();
            }
            if ( currentChar == SourceFile.EOL )
            {
                TakeIt();
            }
            break;
           case ' ': 
           case '\n': 
           case '\r': 
           case '\t':
               TakeIt();
               break;
       }
    }
    
    private bool isLetter( char c )
    {
        return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
    }
    private bool isDigit( char c )
    {
        return c >= '0' && c <= '9';
    }
    

    private TokenKind ScanToken()
    {
        if (isLetter(currentChar))
        {
            TakeIt();
            while ( isLetter(currentChar) || isDigit(currentChar) )
            {
                TakeIt();
            }

            string spelling = currentSpelling.ToString();
            return spelling switch
            {
                "int" => TokenKind.Int,
                "bool" => TokenKind.Bool,
                "char" => TokenKind.Char,
                _ => TokenKind.Identifier
            };
        }
        else if (isDigit(currentChar))
        {
            TakeIt();
            while (isDigit(currentChar))
            {
                TakeIt();
            }

            return TokenKind.IntLiteral;
        }

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
                    return TokenKind.Equals;  // ==
                }
                return TokenKind.Assign;  // =
            case '!':
                TakeIt();
                if (currentChar == '=')
                {
                    TakeIt();
                    return TokenKind.NotEquals;  // !=
                }
                return TokenKind.Not;  // !
            case '&':
                TakeIt();
                if (currentChar == '&')
                {
                    TakeIt();
                    return TokenKind.And;  // &&
                }
                else
                {
                    return TokenKind.Error;
                }
            case '|':
                TakeIt();
                if (currentChar == '|')
                {
                    TakeIt();
                    return TokenKind.Or;  // ||
                }
                else
                {
                    return TokenKind.Error;
                }
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
            case '<':
                TakeIt();
                return TokenKind.LessThan;
            case '>':
                TakeIt();
                return TokenKind.GreaterThan;
            case SourceFile.EOT:
                return TokenKind.EOT;
            default:
                TakeIt();
                return TokenKind.Error;
        }
    }

    public Token Scan()
    {
        while(currentChar == '#' || currentChar == ' ' || currentChar == '\n' || currentChar == '\r' || currentChar == '\t')
        {
            scanSeparator();
        }
        
        currentSpelling.Clear();
        TokenKind kind = ScanToken(); 
        return new Token(kind, currentSpelling.ToString());
    }
    
}