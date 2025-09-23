using System.Text;

namespace SproutLang.Scanner;

public class Scanner
{
    private SourceFile _sourceFile;
    
    private char currentChar;
    private StringBuilder currentSpelling = new StringBuilder();

    public Scanner(SourceFile sourceFile)
    {
        _sourceFile = sourceFile;

        currentChar = sourceFile.GetSource();
    }
    
    private void takeIt()
    {
        currentSpelling.Append(currentChar);
        currentChar = _sourceFile.GetSource();
    }
    
    private void scanSeparator()
    {
       switch (currentChar)
       {
           case '#':
            takeIt();
            while ( currentChar != SourceFile.EOL && currentChar != SourceFile.EOT )
            {
                takeIt();
            }
            if ( currentChar == SourceFile.EOL )
            {
                takeIt();
            }
            break;
           case ' ': 
           case '\n': 
           case '\r': 
           case '\t':
               takeIt();
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
            takeIt();
            while ( isLetter(currentChar) || isDigit(currentChar) )
            {
                takeIt();
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
            takeIt();
            while (isDigit(currentChar))
            {
                takeIt();
            }

            return TokenKind.IntLiteral;
        }

        switch (currentChar)
        {
            case '+':
                takeIt();
                return TokenKind.Plus;
            case '-':
                takeIt();
                return TokenKind.Minus;
            case '*':
                takeIt();
                return TokenKind.Multiply;
            case '/':
                takeIt();
                return TokenKind.Divide;
            case '=':
                takeIt();
                if (currentChar == '=')
                {
                    takeIt();
                    return TokenKind.Equals;  // ==
                }
                return TokenKind.Assign;  // =
            case '!':
                takeIt();
                if (currentChar == '=')
                {
                    takeIt();
                    return TokenKind.NotEquals;  // !=
                }
                return TokenKind.Not;  // !
            case '&':
                takeIt();
                if (currentChar == '&')
                {
                    takeIt();
                    return TokenKind.And;  // &&
                }
                else
                {
                    return TokenKind.Error;
                }
            case '|':
                takeIt();
                if (currentChar == '|')
                {
                    takeIt();
                    return TokenKind.Or;  // ||
                }
                else
                {
                    return TokenKind.Error;
                }
            case ',':
                takeIt();
                return TokenKind.Comma;
            case ';':
                takeIt();
                return TokenKind.Semicolon;
            case '(':
                takeIt();
                return TokenKind.LParenthesis;
            case ')':
                takeIt();
                return TokenKind.RParenthesis;
            case '[':
                takeIt();
                return TokenKind.LBracket;
            case ']':
                takeIt();
                return TokenKind.RBracket;
            case '{':
                takeIt();
                return TokenKind.LBrace;
            case '}':
                takeIt();
                return TokenKind.RBrace;
            case '<':
                takeIt();
                return TokenKind.LessThan;
            case '>':
                takeIt();
                return TokenKind.GreaterThan;
            case SourceFile.EOT:
                return TokenKind.EOT;
            default:
                takeIt();
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