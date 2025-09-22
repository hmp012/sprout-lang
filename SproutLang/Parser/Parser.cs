using SproutLang.Scanner;

namespace SproutLang.Parser;

public class Parser
{
    private Scanner.Scanner _scanner;
    private Token _currentToken;

    public Parser(Scanner.Scanner scanner)
    {
        _scanner = scanner;
        _currentToken = _scanner.Scan();
    }

    private void accept(TokenKind expectedKind)
    {
        if (_currentToken.Kind == expectedKind)
        {
            _currentToken = _scanner.Scan();
        }
        else
        {
            throw new Exception($"Expected {expectedKind}, but found {_currentToken.Kind}");
        }
    }

    // Program ::= Statement*
    public void parseProgram()
    {
        while (isStarterOfStatement(_currentToken.Kind))
        {
            parseStatement();
        }
    }

    // Statement ::= Declaration | Assignment | Expression | IfStatement | LoopStatement | OutputStatement | BlockStatement | BloomStatement
    public void parseStatement()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.Create:
                parseDeclaration();
                break;
            case TokenKind.Identifier:
                parseAssignment();
                break;
            case TokenKind.Si:
                parseIfStatement();
                break;
            case TokenKind.Repeat:
                parseLoopStatement();
                break;
            case TokenKind.Vomit:
                parseOutputStatement();
                break;
            case TokenKind.Bloom:
                parseBloomStatement();
                break;
            case TokenKind.LBrace:
                parseBlockStatement();
                break;
            default:
                parseExpressionStatement();
                break;
        }
    }

    // Declaration ::= "create" Identifier "=" Expression ";"
    public void parseDeclaration()
    {
        accept(TokenKind.Create);
        parseIdentifier();
        accept(TokenKind.Assign);
        parseExpression();
        accept(TokenKind.Semicolon);
    }

    // Assignment ::= Identifier "=" Expression ";"
    public void parseAssignment()
    {
        parseIdentifier();
        accept(TokenKind.Assign);
        parseExpression();
        accept(TokenKind.Semicolon);
    }

    // IfStatement ::= "si" "(" Expression ")" Statement ("sino" Statement)?
    public void parseIfStatement()
    {
        accept(TokenKind.Si);
        accept(TokenKind.LParenthesis);
        parseExpression();
        accept(TokenKind.RParenthesis);
        parseStatement();
        
        if (_currentToken.Kind == TokenKind.Sino)
        {
            accept(TokenKind.Sino);
            parseStatement();
        }
    }

    // LoopStatement ::= "repeat" "(" Expression ")" Statement | "repeat" Statement "until" "(" Expression ")" ";"
    public void parseLoopStatement()
    {
        accept(TokenKind.Repeat);
        
        if (_currentToken.Kind == TokenKind.LParenthesis)
        {
            // while-style loop
            accept(TokenKind.LParenthesis);
            parseExpression();
            accept(TokenKind.RParenthesis);
            parseStatement();
        }
        else
        {
            // do-while style loop
            parseStatement();
            accept(TokenKind.Until);
            accept(TokenKind.LParenthesis);
            parseExpression();
            accept(TokenKind.RParenthesis);
            accept(TokenKind.Semicolon);
        }
    }

    // OutputStatement ::= "vomit" Expression ";"
    public void parseOutputStatement()
    {
        accept(TokenKind.Vomit);
        parseExpression();
        accept(TokenKind.Semicolon);
    }

    // BloomStatement ::= "bloom" ";"
    public void parseBloomStatement()
    {
        accept(TokenKind.Bloom);
        accept(TokenKind.Semicolon);
    }

    // BlockStatement ::= "{" Statement* "}"
    public void parseBlockStatement()
    {
        accept(TokenKind.LBrace);
        while (_currentToken.Kind != TokenKind.RBrace)
        {
            parseStatement();
        }
        accept(TokenKind.RBrace);
    }

    // ExpressionStatement ::= Expression ";"
    public void parseExpressionStatement()
    {
        parseExpression();
        accept(TokenKind.Semicolon);
    }

    // Expression ::= LogicalOrExpression
    public void parseExpression()
    {
        parseLogicalOrExpression();
    }

    // LogicalOrExpression ::= LogicalAndExpression ("||" LogicalAndExpression)*
    public void parseLogicalOrExpression()
    {
        parseLogicalAndExpression();
        while (_currentToken.Kind == TokenKind.Or)
        {
            accept(TokenKind.Or);
            parseLogicalAndExpression();
        }
    }

    // LogicalAndExpression ::= EqualityExpression ("&&" EqualityExpression)*
    public void parseLogicalAndExpression()
    {
        parseEqualityExpression();
        while (_currentToken.Kind == TokenKind.And)
        {
            accept(TokenKind.And);
            parseEqualityExpression();
        }
    }

    // EqualityExpression ::= RelationalExpression (("==" | "!=") RelationalExpression)*
    public void parseEqualityExpression()
    {
        parseRelationalExpression();
        while (_currentToken.Kind == TokenKind.Equals || _currentToken.Kind == TokenKind.NotEquals)
        {
            if (_currentToken.Kind == TokenKind.Equals)
                accept(TokenKind.Equals);
            else
                accept(TokenKind.NotEquals);
            parseRelationalExpression();
        }
    }

    // RelationalExpression ::= AdditiveExpression (("<" | ">") AdditiveExpression)*
    public void parseRelationalExpression()
    {
        parseAdditiveExpression();
        while (_currentToken.Kind == TokenKind.LessThan || _currentToken.Kind == TokenKind.GreaterThan)
        {
            if (_currentToken.Kind == TokenKind.LessThan)
                accept(TokenKind.LessThan);
            else
                accept(TokenKind.GreaterThan);
            parseAdditiveExpression();
        }
    }

    // AdditiveExpression ::= MultiplicativeExpression (("+" | "-") MultiplicativeExpression)*
    public void parseAdditiveExpression()
    {
        parseMultiplicativeExpression();
        while (_currentToken.Kind == TokenKind.Plus || _currentToken.Kind == TokenKind.Minus)
        {
            if (_currentToken.Kind == TokenKind.Plus)
                accept(TokenKind.Plus);
            else
                accept(TokenKind.Minus);
            parseMultiplicativeExpression();
        }
    }

    // MultiplicativeExpression ::= UnaryExpression (("*" | "/") UnaryExpression)*
    public void parseMultiplicativeExpression()
    {
        parseUnaryExpression();
        while (_currentToken.Kind == TokenKind.Multiply || _currentToken.Kind == TokenKind.Divide)
        {
            if (_currentToken.Kind == TokenKind.Multiply)
                accept(TokenKind.Multiply);
            else
                accept(TokenKind.Divide);
            parseUnaryExpression();
        }
    }

    // UnaryExpression ::= ("!" | "-")? PrimaryExpression
    public void parseUnaryExpression()
    {
        if (_currentToken.Kind == TokenKind.Not || _currentToken.Kind == TokenKind.Minus)
        {
            if (_currentToken.Kind == TokenKind.Not)
                accept(TokenKind.Not);
            else
                accept(TokenKind.Minus);
        }
        parsePrimaryExpression();
    }

    // PrimaryExpression ::= Literal | Identifier | "(" Expression ")" | FunctionCall
    public void parsePrimaryExpression()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.IntLiteral:
            case TokenKind.CharLiteral:
            case TokenKind.StringLiteral:
                parseLiteral();
                break;
            case TokenKind.Identifier:
                if (isNextToken(TokenKind.LParenthesis))
                    parseFunctionCall();
                else
                    parseIdentifier();
                break;
            case TokenKind.LParenthesis:
                accept(TokenKind.LParenthesis);
                parseExpression();
                accept(TokenKind.RParenthesis);
                break;
            default:
                throw new Exception($"Unexpected token in primary expression: {_currentToken.Kind}");
        }
    }

    // FunctionCall ::= Identifier "(" ArgumentList? ")"
    public void parseFunctionCall()
    {
        parseIdentifier();
        accept(TokenKind.LParenthesis);
        if (_currentToken.Kind != TokenKind.RParenthesis)
        {
            parseArgumentList();
        }
        accept(TokenKind.RParenthesis);
    }

    // ArgumentList ::= Expression ("," Expression)*
    public void parseArgumentList()
    {
        parseExpression();
        while (_currentToken.Kind == TokenKind.Comma)
        {
            accept(TokenKind.Comma);
            parseExpression();
        }
    }

    // Literal ::= IntLiteral | CharLiteral | StringLiteral
    public void parseLiteral()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.IntLiteral:
                accept(TokenKind.IntLiteral);
                break;
            case TokenKind.CharLiteral:
                accept(TokenKind.CharLiteral);
                break;
            case TokenKind.StringLiteral:
                accept(TokenKind.StringLiteral);
                break;
            default:
                throw new Exception($"Expected literal, but found {_currentToken.Kind}");
        }
    }

    // Identifier ::= IDENTIFIER
    public void parseIdentifier()
    {
        accept(TokenKind.Identifier);
    }

    // Type ::= "int" | "bool" | "char" | "string"
    public void parseType()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.Int:
                accept(TokenKind.Int);
                break;
            case TokenKind.Bool:
                accept(TokenKind.Bool);
                break;
            case TokenKind.Char:
                accept(TokenKind.Char);
                break;
            case TokenKind.String:
                accept(TokenKind.String);
                break;
            default:
                throw new Exception($"Expected type, but found {_currentToken.Kind}");
        }
    }

    // Helper method to check next token without consuming it
    private bool isNextToken(TokenKind kind)
    {
        // This would require lookahead - simplified implementation
        return false;
    }

    // Starter checking pattern for statements
    private bool isStarterOfStatement(TokenKind kind)
    {
        return kind == TokenKind.Create ||
               kind == TokenKind.Identifier ||
               kind == TokenKind.Si ||
               kind == TokenKind.Repeat ||
               kind == TokenKind.Vomit ||
               kind == TokenKind.Bloom ||
               kind == TokenKind.LBrace ||
               isStarterOfExpression(kind);
    }
    
    // Helper method to check if current token is a starter of Expression
    private bool isStarterOfExpression(TokenKind kind)
    {
        return kind == TokenKind.IntLiteral ||
               kind == TokenKind.CharLiteral ||
               kind == TokenKind.StringLiteral ||
               kind == TokenKind.Identifier ||
               kind == TokenKind.LParenthesis ||
               kind == TokenKind.Not ||
               kind == TokenKind.Minus;
    }
}