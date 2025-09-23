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

    private void Accept(TokenKind expectedKind)
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
    public void ParseProgram()
    {
        while (IsStarterOfStatement(_currentToken.Kind))
        {
            ParseStatement();
        }
    }

    // Statement ::= Declaration | Assignment | Expression | IfStatement | LoopStatement | OutputStatement | BlockStatement | BloomStatement
    private void ParseStatement()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.Create:
                ParseDeclaration();
                break;
            case TokenKind.Identifier:
                ParseAssignment();
                break;
            case TokenKind.Si:
                ParseIfStatement();
                break;
            case TokenKind.Repeat:
                ParseLoopStatement();
                break;
            case TokenKind.Vomit:
                ParseOutputStatement();
                break;
            case TokenKind.Bloom:
                ParseBloomStatement();
                break;
            case TokenKind.LBrace:
                ParseBlockStatement();
                break;
            default:
                ParseExpressionStatement();
                break;
        }
    }

    // Declaration ::= "create" Identifier "=" Expression ";"
    private void ParseDeclaration()
    {
        Accept(TokenKind.Create);

        if (_currentToken.Kind == TokenKind.LBracket)
        {
            // Array declaration: [Type, IntLiteral] VarName
            Accept(TokenKind.LBracket);
            ParseType();
            Accept(TokenKind.Comma);
            Accept(TokenKind.IntLiteral);
            Accept(TokenKind.RBracket);
            ParseIdentifier();
        }
        else
        {
            // Simple variable: Type VarName
            ParseType();
            ParseIdentifier();
        }

        Accept(TokenKind.Assign);
        ParseExpression();
        Accept(TokenKind.Semicolon);
    }

    // Assignment ::= Identifier "=" Expression ";"
    private void ParseAssignment()
    {
        ParseIdentifier();
        Accept(TokenKind.Assign);
        ParseExpression();
        Accept(TokenKind.Semicolon);
    }

    // IfStatement ::= "si" "(" Expression ")" Statement ("sino" Statement)?
    private void ParseIfStatement()
    {
        Accept(TokenKind.Si);
        Accept(TokenKind.LParenthesis);
        ParseExpression();
        Accept(TokenKind.RParenthesis);
        ParseStatement();
        
        if (_currentToken.Kind == TokenKind.Sino)
        {
            Accept(TokenKind.Sino);
            ParseStatement();
        }
    }

    // LoopStatement ::= "repeat" "(" Expression ")" Statement | "repeat" Statement "until" "(" Expression ")" ";"
    private void ParseLoopStatement()
    {
        Accept(TokenKind.Repeat);
        
        if (_currentToken.Kind == TokenKind.LParenthesis)
        {
            // while-style loop
            Accept(TokenKind.LParenthesis);
            ParseExpression();
            Accept(TokenKind.RParenthesis);
            ParseStatement();
        }
        else
        {
            // do-while style loop
            ParseStatement();
            Accept(TokenKind.Until);
            Accept(TokenKind.LParenthesis);
            ParseExpression();
            Accept(TokenKind.RParenthesis);
            Accept(TokenKind.Semicolon);
        }
    }

    // OutputStatement ::= "vomit" Expression ";"
    private void ParseOutputStatement()
    {
        Accept(TokenKind.Vomit);
        ParseExpression();
        Accept(TokenKind.Semicolon);
    }

    // BloomStatement ::= "bloom" ";"
    private void ParseBloomStatement()
    {
        Accept(TokenKind.Bloom);
        Accept(TokenKind.Semicolon);
    }

    // BlockStatement ::= "{" Statement* "}"
    private void ParseBlockStatement()
    {
        Accept(TokenKind.LBrace);
        while (_currentToken.Kind != TokenKind.RBrace)
        {
            ParseStatement();
        }
        Accept(TokenKind.RBrace);
    }

    // ExpressionStatement ::= Expression ";"
    private void ParseExpressionStatement()
    {
        ParseExpression();
        Accept(TokenKind.Semicolon);
    }

    // Expression ::= LogicalOrExpression
    private void ParseExpression()
    {
        ParseLogicalOrExpression();
    }

    // LogicalOrExpression ::= LogicalAndExpression ("||" LogicalAndExpression)*
    private void ParseLogicalOrExpression()
    {
        ParseLogicalAndExpression();
        while (_currentToken.Kind == TokenKind.Or)
        {
            Accept(TokenKind.Or);
            ParseLogicalAndExpression();
        }
    }

    // LogicalAndExpression ::= EqualityExpression ("&&" EqualityExpression)*
    private void ParseLogicalAndExpression()
    {
        ParseEqualityExpression();
        while (_currentToken.Kind == TokenKind.And)
        {
            Accept(TokenKind.And);
            ParseEqualityExpression();
        }
    }

    // EqualityExpression ::= RelationalExpression (("==" | "!=") RelationalExpression)*
    private void ParseEqualityExpression()
    {
        ParseRelationalExpression();
        while (_currentToken.Kind == TokenKind.Equals || _currentToken.Kind == TokenKind.NotEquals)
        {
            if (_currentToken.Kind == TokenKind.Equals)
                Accept(TokenKind.Equals);
            else
                Accept(TokenKind.NotEquals);
            ParseRelationalExpression();
        }
    }

    // RelationalExpression ::= AdditiveExpression (("<" | ">") AdditiveExpression)*
    private void ParseRelationalExpression()
    {
        ParseAdditiveExpression();
        while (_currentToken.Kind == TokenKind.LessThan || _currentToken.Kind == TokenKind.GreaterThan)
        {
            if (_currentToken.Kind == TokenKind.LessThan)
                Accept(TokenKind.LessThan);
            else
                Accept(TokenKind.GreaterThan);
            ParseAdditiveExpression();
        }
    }

    // AdditiveExpression ::= MultiplicativeExpression (("+" | "-") MultiplicativeExpression)*
    private void ParseAdditiveExpression()
    {
        ParseMultiplicativeExpression();
        while (_currentToken.Kind == TokenKind.Plus || _currentToken.Kind == TokenKind.Minus)
        {
            if (_currentToken.Kind == TokenKind.Plus)
                Accept(TokenKind.Plus);
            else
                Accept(TokenKind.Minus);
            ParseMultiplicativeExpression();
        }
    }

    // MultiplicativeExpression ::= UnaryExpression (("*" | "/") UnaryExpression)*
    private void ParseMultiplicativeExpression()
    {
        ParseUnaryExpression();
        while (_currentToken.Kind == TokenKind.Multiply || _currentToken.Kind == TokenKind.Divide)
        {
            if (_currentToken.Kind == TokenKind.Multiply)
                Accept(TokenKind.Multiply);
            else
                Accept(TokenKind.Divide);
            ParseUnaryExpression();
        }
    }

    // UnaryExpression ::= ("!" | "-")? PrimaryExpression
    private void ParseUnaryExpression()
    {
        if (_currentToken.Kind == TokenKind.Not || _currentToken.Kind == TokenKind.Minus)
        {
            if (_currentToken.Kind == TokenKind.Not)
                Accept(TokenKind.Not);
            else
                Accept(TokenKind.Minus);
        }
        ParsePrimaryExpression();
    }

    // PrimaryExpression ::= Literal | Identifier | "(" Expression ")" | FunctionCall
    private void ParsePrimaryExpression()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.IntLiteral:
            case TokenKind.CharLiteral:
            case TokenKind.StringLiteral:
                ParseLiteral();
                break;
            case TokenKind.Identifier:
                if (IsNextToken(TokenKind.LParenthesis))
                    ParseFunctionCall();
                else
                    ParseIdentifier();
                break;
            case TokenKind.LParenthesis:
                Accept(TokenKind.LParenthesis);
                ParseExpression();
                Accept(TokenKind.RParenthesis);
                break;
            default:
                throw new Exception($"Unexpected token in primary expression: {_currentToken.Kind}");
        }
    }

    // FunctionCall ::= Identifier "(" ArgumentList? ")"
    private void ParseFunctionCall()
    {
        ParseIdentifier();
        Accept(TokenKind.LParenthesis);
        if (_currentToken.Kind != TokenKind.RParenthesis)
        {
            ParseArgumentList();
        }
        Accept(TokenKind.RParenthesis);
    }

    // ArgumentList ::= Expression ("," Expression)*
    private void ParseArgumentList()
    {
        ParseExpression();
        while (_currentToken.Kind == TokenKind.Comma)
        {
            Accept(TokenKind.Comma);
            ParseExpression();
        }
    }

    // Literal ::= IntLiteral | CharLiteral | StringLiteral
    private void ParseLiteral()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.IntLiteral:
                Accept(TokenKind.IntLiteral);
                break;
            case TokenKind.CharLiteral:
                Accept(TokenKind.CharLiteral);
                break;
            case TokenKind.StringLiteral:
                Accept(TokenKind.StringLiteral);
                break;
            default:
                throw new Exception($"Expected literal, but found {_currentToken.Kind}");
        }
    }

    // Identifier ::= IDENTIFIER
    private void ParseIdentifier()
    {
        Accept(TokenKind.Identifier);
    }

    // Type ::= "int" | "bool" | "char" | "string"
    private void ParseType()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.Int:
                Accept(TokenKind.Int);
                break;
            case TokenKind.Bool:
                Accept(TokenKind.Bool);
                break;
            case TokenKind.Char:
                Accept(TokenKind.Char);
                break;
            case TokenKind.String:
                Accept(TokenKind.String);
                break;
            default:
                throw new Exception($"Expected type, but found {_currentToken.Kind}");
        }
    }

    // Helper method to check next token without consuming it
    private bool IsNextToken(TokenKind kind)
    {
        // This would require lookahead - simplified implementation
        return false;
    }

    // Starter checking pattern for statements
    private bool IsStarterOfStatement(TokenKind kind)
    {
        return kind == TokenKind.Create ||
               kind == TokenKind.Identifier ||
               kind == TokenKind.Si ||
               kind == TokenKind.Repeat ||
               kind == TokenKind.Vomit ||
               kind == TokenKind.Bloom ||
               kind == TokenKind.LBrace ||
               IsStarterOfExpression(kind);
    }
    
    // Helper method to check if current token is a starter of Expression
    private bool IsStarterOfExpression(TokenKind kind)
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