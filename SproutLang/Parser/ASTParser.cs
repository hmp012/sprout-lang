using SproutLang.Scanner;
using SproutLang.AST;

namespace SproutLang.Parser;

public class ASTParser
{
    private readonly Scanner.Scanner _scanner;
    private Token _currentToken;

    public ASTParser(Scanner.Scanner scanner)
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
            throw new ParserException($"Expected {expectedKind}, but found {_currentToken.Kind}");
        }
    }

    public AST.Program ParseProgram()
    {
        var statements = new List<Statement>();

        while (IsStarterOfStatement(_currentToken.Kind))
        {
            statements.Add(ParseOneStatement());
        }

        return new AST.Program(new Block(statements));
    }

    private Statement ParseOneStatement()
    {
        return _currentToken.Kind switch
        {
            TokenKind.Create => ParseDeclaration(),
            TokenKind.Si => ParseIfStatement(),
            TokenKind.Repeat => ParseLoopStatement(),
            TokenKind.Vomit => ParseVomitStatement(),
            TokenKind.ListenCarefully => ParseListenStatement(),
            TokenKind.Sprout => ParseSubroutineDecl(),
            TokenKind.Bloom => ParseCallStatement(),
            _ when IsStarterOfExpression(_currentToken.Kind) => ParseAssignmentOrCall(),
            _ => throw new ParserException($"Unexpected token: {_currentToken.Kind}")
        };
    }

    private Declaration ParseDeclaration()
    {
        Accept(TokenKind.Create);

        Identifier name;

        if (_currentToken.Kind == TokenKind.LBracket)
        {
            Accept(TokenKind.LBracket);
            SimpleType elementType = ParseType() as SimpleType
                                     ?? throw new Exception("Expected simple type for array element");
            Accept(TokenKind.Comma);
            int size = int.Parse(_currentToken.Spelling);
            Accept(TokenKind.IntLiteral);
            Accept(TokenKind.RBracket);

            ArrayType arrayType = new ArrayType(elementType.Kind, size);
            name = ParseIdentifier();
            Accept(TokenKind.Semicolon);

            return new VarDecl(arrayType, name);
        }
        
        var type = ParseType();
        name = ParseIdentifier();
        Accept(TokenKind.Semicolon);

        return new VarDecl(type, name);
    }

    private Statement ParseAssignmentOrCall()
    {
        Identifier id = ParseIdentifier();
        
        if (_currentToken.Kind == TokenKind.LBracket)
        {
            Accept(TokenKind.LBracket);
            Expression intLiteral = ParseExpression();
            Accept(TokenKind.RBracket);
            Accept(TokenKind.Assign);
            Expression expr = ParseExpression();
            Accept(TokenKind.Semicolon);

            if (intLiteral is IntLiteralExpression intLiteralExpression)
            {
                ArrayAssignment arrayAssignment = new ArrayAssignment(id, intLiteralExpression, expr);
                return arrayAssignment;
            }
            throw new Exception("Array index must be an integer literal.");
        }

        if (_currentToken.Kind == TokenKind.Assign)
        {
            Accept(TokenKind.Assign);
            Expression expr = ParseExpression();
            Accept(TokenKind.Semicolon);
            return new VarAssignment(id, expr);
        }

        if (_currentToken.Kind == TokenKind.LParenthesis)
        {
            Accept(TokenKind.LParenthesis);
            var args = ParseArguments();
            Accept(TokenKind.RParenthesis);
            Accept(TokenKind.Semicolon);
            return new CallStatement(new CallExpr(new Identifier(id.Spelling), args));
        }

        throw new ParserException("Invalid assignment or call.");
    }

    private IfStatement ParseIfStatement()
    {
        Accept(TokenKind.Si);
        Accept(TokenKind.LParenthesis);
        Expression condition = ParseExpression();
        Accept(TokenKind.RParenthesis);
        Block thenBlock = ParseBlock();

        var first = new IfBranch(condition, thenBlock);

        var elseIfs = new List<IfBranch>();
        while (_currentToken.Kind == TokenKind.O)
        {
            Accept(TokenKind.O);
            Accept(TokenKind.Sino);
            Accept(TokenKind.LParenthesis);
            Expression elseIfCond = ParseExpression();
            Accept(TokenKind.RParenthesis);
            Block elseIfBlock = ParseBlock();
            elseIfs.Add(new IfBranch(elseIfCond, elseIfBlock));
        }

        Block? elseBlock = null;
        if (_currentToken.Kind == TokenKind.Sino)
        {
            Accept(TokenKind.Sino);
            elseBlock = ParseBlock();
        }

        return new IfStatement(first, elseIfs, elseBlock);
    }

    private LoopStatement ParseLoopStatement()
    {
        Accept(TokenKind.Repeat);

        if (_currentToken.Kind == TokenKind.IntLiteral)
        {
            string value = _currentToken.Spelling;
            Accept(TokenKind.IntLiteral);
            Accept(TokenKind.Times);
            Block block = ParseBlock();
            return new RepeatTimes(new IntLiteralExpression(new IntLiteral(int.Parse(value))), block);
        }
        else
        {
            Accept(TokenKind.Until);
            Accept(TokenKind.LParenthesis);
            Expression condition = ParseExpression();
            Accept(TokenKind.RParenthesis);
            Block block = ParseBlock();
            return new RepeatUntil(condition, block);
        }
    }

    private VomitStatement ParseVomitStatement()
    {
        Accept(TokenKind.Vomit);
        Expression expr = ParseExpression();
        Accept(TokenKind.Semicolon);
        return new VomitStatement(expr);
    }

    private ListenStatement ParseListenStatement()
    {
        Accept(TokenKind.ListenCarefully);
        Identifier id = ParseIdentifier();
        Accept(TokenKind.Semicolon);
        return new ListenStatement(id);
    }

    private SubRoutineDeclar ParseSubroutineDecl()
    {
        Accept(TokenKind.Sprout);
        Identifier name = ParseIdentifier();
        Accept(TokenKind.LParenthesis);

        List<Param> parameters = new();
        if (_currentToken.Kind != TokenKind.RParenthesis)
        {
            parameters.Add(ParseParam());
            while (_currentToken.Kind == TokenKind.Comma)
            {
                Accept(TokenKind.Comma);
                parameters.Add(ParseParam());
            }
        }

        Accept(TokenKind.RParenthesis);
        Block body = ParseBlock();
        return new SubRoutineDeclar(name, parameters, body);
    }

    private Param ParseParam()
    {
        SimpleType type = ParseType();
        Identifier id = ParseIdentifier();
        return new Param(type, id);
    }

    private CallStatement ParseCallStatement()
    {
        Accept(TokenKind.Bloom);
        Identifier id = ParseIdentifier();
        Accept(TokenKind.LParenthesis);
        var args = ParseArguments();
        Accept(TokenKind.RParenthesis);
        Accept(TokenKind.Semicolon);
        return new CallStatement(new CallExpr(new Identifier(id.Spelling), args));
    }

    private ArgList ParseArguments()
    {
        var args = new ArgList();
        if (IsStarterOfExpression(_currentToken.Kind))
        {
            args.Arguments.Add(ParseExpression());
            while (_currentToken.Kind == TokenKind.Comma)
            {
                Accept(TokenKind.Comma);
                args.Arguments.Add(ParseExpression());
            }
        }

        return args;
    }

    private Block ParseBlock()
    {
        Accept(TokenKind.LBrace);
        var statements = new List<Statement>();
        while (_currentToken.Kind != TokenKind.RBrace)
        {
            statements.Add(ParseOneStatement());
        }

        Accept(TokenKind.RBrace);
        return new Block(statements);
    }

    private Expression ParseExpression()
    {
        return ParseAssignmentExpression();
    }

    private Expression ParseAssignmentExpression()
    {
        Expression left = ParseLogicalOrExpression();

        while (_currentToken.Kind == TokenKind.Assign)
        {
            Accept(TokenKind.Assign);
            Expression right = ParseLogicalOrExpression();
            left = new BinaryExpr(left, new Operator("="), right);
        }

        return left;
    }

    private Expression ParseLogicalOrExpression()
    {
        Expression left = ParseLogicalAndExpression();

        while (_currentToken.Kind == TokenKind.Or)
        {
            Accept(TokenKind.Or);
            Expression right = ParseLogicalAndExpression();
            left = new BinaryExpr(left, new Operator("||"), right);
        }


        return left;
    }

    private Expression ParseLogicalAndExpression()
    {
        Expression left = ParseEqualityExpression();

        while (_currentToken.Kind == TokenKind.And)
        {
            Accept(TokenKind.And);
            Expression right = ParseEqualityExpression();
            left = new BinaryExpr(left, new Operator("&&"), right);
        }


        return left;
    }


    private Expression ParseEqualityExpression()
    {
        Expression left = ParseRelationalExpression();

        while (_currentToken.Kind == TokenKind.Equals || _currentToken.Kind == TokenKind.NotEquals)
        {
            var op = _currentToken.Kind == TokenKind.Equals ? "==" : "!=";
            Accept(_currentToken.Kind);
            Expression right = ParseRelationalExpression();
            left = new BinaryExpr(left, new Operator(op), right);
        }

        return left;
    }


    private Expression ParseRelationalExpression()
    {
        Expression left = ParseAdditiveExpression();


        while (_currentToken.Kind == TokenKind.LessThan || _currentToken.Kind == TokenKind.GreaterThan)
        {
            var op = _currentToken.Kind == TokenKind.LessThan ? "<" : ">";
            Accept(_currentToken.Kind);
            Expression right = ParseAdditiveExpression();
            left = new BinaryExpr(left, new Operator(op), right);
        }

        return left;
    }

    private Expression ParseAdditiveExpression()
    {
        Expression left = ParseMultiplicativeExpression(); // Changed from ParseUnaryExpression

        while (_currentToken.Kind == TokenKind.Plus ||
               _currentToken.Kind == TokenKind.Minus) // Changed from Multiply/Divide
        {
            var op = _currentToken.Kind == TokenKind.Plus ? "+" : "-";
            Accept(_currentToken.Kind);
            Expression right = ParseMultiplicativeExpression(); // Changed from ParseUnaryExpression
            left = new BinaryExpr(left, new Operator(op), right);
        }

        return left;
    }

    private Expression ParseMultiplicativeExpression() // New method
    {
        Expression left = ParseUnaryExpression();

        while (_currentToken.Kind == TokenKind.Multiply || _currentToken.Kind == TokenKind.Divide)
        {
            var op = _currentToken.Kind == TokenKind.Multiply ? "*" : "/";
            Accept(_currentToken.Kind);
            Expression right = ParseUnaryExpression();
            left = new BinaryExpr(left, new Operator(op), right);
        }

        return left;
    }


    private Expression ParseUnaryExpression()
    {
        if (_currentToken.Kind == TokenKind.Not || _currentToken.Kind == TokenKind.Minus)
        {
            string op = _currentToken.Kind == TokenKind.Not ? "!" : "-";
            Accept(_currentToken.Kind);
            Expression right = ParseUnaryExpression();
            return new UnaryExpr(new Operator(op), right);
        }


        return ParsePrimaryExpression();
    }


    private Expression ParsePrimaryExpression()
    {
        if (_currentToken.Kind == TokenKind.IntLiteral)
        {
            string value = _currentToken.Spelling;
            Accept(TokenKind.IntLiteral);
            return new IntLiteralExpression(new IntLiteral(int.Parse(value)));
        }
        else if (_currentToken.Kind == TokenKind.CharLiteral)
        {
            string value = _currentToken.Spelling;
            Accept(TokenKind.CharLiteral);
            return new CharLiteralExpression(new CharLiteral(char.Parse(value)));
        }
        else if (_currentToken.Kind == TokenKind.Identifier)
        {
            return ParseVariableOrFunctionCall();
        }
        else if (_currentToken.Kind == TokenKind.LParenthesis)
        {
            return ParseGroupedExpression();
        }
        else if (_currentToken.Kind == TokenKind.BoolLiteral)
        {
            string value = _currentToken.Spelling;
            Accept(TokenKind.BoolLiteral);
            return new BoolLiteralExpression(new BoolLiteral(bool.Parse(value)));
        }
        else
        {
            throw new ParserException($"Unexpected token in primary expression: {_currentToken.Kind}");
        }
    }


    private Expression ParseVariableOrFunctionCall()
    {
        Identifier id = ParseIdentifier();

        if (_currentToken.Kind == TokenKind.LParenthesis)
        {
            Accept(TokenKind.LParenthesis);
            var args = ParseArguments();
            Accept(TokenKind.RParenthesis);
            return new CallExpr(new Identifier(id.Spelling), args);
        }
        
        else if (_currentToken.Kind == TokenKind.LBracket)
        {
            Accept(TokenKind.LBracket);
            Expression index = ParseExpression();
            Accept(TokenKind.RBracket);
            return new ArrayExpression(id, index);
        }

        return new VarExpression(new Identifier(id.Spelling));
    }


    private Expression ParseGroupedExpression()
    {
        Accept(TokenKind.LParenthesis);
        Expression expr = ParseExpression();
        Accept(TokenKind.RParenthesis);
        return expr;
    }

    private SimpleType ParseType()
    {
        switch (_currentToken.Kind)
        {
            case TokenKind.Int:
                Accept(TokenKind.Int);
                return new SimpleType(BaseType.Int);
            case TokenKind.Bool:
                Accept(TokenKind.Bool);
                return new SimpleType(BaseType.Bool);
            case TokenKind.Char:
                Accept(TokenKind.Char);
                return new SimpleType(BaseType.Char);
            default:
                throw new ParserException($"Unexpected type: {_currentToken.Kind}");
        }
    }

    private Identifier ParseIdentifier()
    {
        var name = _currentToken.Spelling;
        Accept(TokenKind.Identifier);
        return new Identifier(name);
    }

    private bool IsStarterOfStatement(TokenKind kind)
    {
        return kind is TokenKind.Create or TokenKind.Identifier or TokenKind.Si or TokenKind.Repeat or TokenKind.Vomit
            or TokenKind.ListenCarefully or TokenKind.Bloom or TokenKind.Sprout or TokenKind.LBrace
            or TokenKind.IntLiteral or TokenKind.CharLiteral or TokenKind.StringLiteral;
    }

    private bool IsStarterOfExpression(TokenKind kind)
    {
        return kind is TokenKind.IntLiteral or TokenKind.CharLiteral or TokenKind.StringLiteral or TokenKind.Identifier
            or TokenKind.LParenthesis or TokenKind.Not or TokenKind.Minus or TokenKind.BoolLiteral;
    }
}