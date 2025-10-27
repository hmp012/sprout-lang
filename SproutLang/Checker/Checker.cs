using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SproutLang.AST;

namespace SproutLang.Checker;

public class Checker : IAstVisitor
{

    private IdentificationTable _identificationTable;
    private readonly ILogger _logger;
    
    public Checker(ILogger logger)
    {
        _identificationTable = _identificationTable = new IdentificationTable(logger);
        _logger = logger;
    }
    public void Check(AST.Program program)
    {
        program.Visit(this, null!);
    }
    public object VisitProgram(AST.Program program, object arg)
    {
        _identificationTable.OpenScope();
        
        program.Block.Visit(this, null!);
        
        _identificationTable.CloseScope();
        
        return null!;
        
    }

    public object? VisitBlock(Block block, object arg)
    {
        block.Statements.ForEach(statement => statement.Visit(this, null!));
        return null;
    }

    public object? VisitArgList(ArgList argList, object arg)
    {
        throw new NotImplementedException();
    }

    public object VisitStatement(Statement statement, object arg)
    {
        throw new NotImplementedException();
    }

    public object VisitIfStatement(IfStatement ifStatement, object arg)
    {
        ifStatement.First.Visit(this, arg); 
        
        foreach (var elseIf in ifStatement.ElseIfBranches)
        {
            elseIf.Visit(this, arg);
        }

        if (ifStatement.ElseBlock != null)
        {
            ifStatement.ElseBlock.Visit(this, arg);
        }
        
        return null;
    }

    public object VisitIfBranch(IfBranch ifBranch, object arg)
    {
        ifBranch.Condition.Visit(this, arg);
        ifBranch.Block.Visit(this, arg);
        return null;
    }

    public object VisitRepeatTimes(RepeatTimes repeatTimes, object arg)
    {
        throw new NotImplementedException();
    }

    public object VisitRepeatUntil(RepeatUntil repeatUntil, object arg)
    {
        throw new NotImplementedException();
    }

    public object VisitVarAssignment(VarAssignment varAssignment, object arg)
    {
        throw new NotImplementedException();
    }

    public object? VisitArrayAssignment(ArrayAssignment arrayAssignment, object arg)
    {
        var id = arrayAssignment.Name.Visit(this, arg).ToString();
        if (id != null)
        {
            var varDecl = _identificationTable.Retrieve(id);
        }

        return null;
    }

    public object? VisitBoolLiteral(BoolLiteral boolLiteral, object arg)
    {
        return null;
    }

    public object? VisitCharLiteral(CharLiteral charLiteral, object arg)
    {
        return null;
    }

    public object VisitIdentifier(Identifier identifier, object arg)
    {
        return identifier.Spelling;
    }

    public object? VisitIntLiteral(IntLiteral intLiteral, object arg)
    {
        return null;
    }

    public object VisitOperator(Operator @operator, object arg)
    {
        return @operator.Spelling;
    }

    public object VisitParam(Param param, object arg)
    {
        throw new NotImplementedException();
    }

    public object VisitSimpleType(SimpleType simpleType, object arg)
    {
        throw new NotImplementedException();
    }

    public object? VisitVomitStatement(VomitStatement vomitStatement, object arg)
    {
        vomitStatement.Expression.Visit(this, arg);
        return null;
    }

    public object VisitListenStatement(ListenStatement listenStatement, object arg)
    {
        throw new NotImplementedException();
    }

    public object? VisitSubRoutineDecl(SubRoutineDeclar subRoutineDeclar, object arg)
    {
        var id = subRoutineDeclar.Name.Visit(this, arg).ToString();
        
        _identificationTable.Enter(id, subRoutineDeclar);

        _identificationTable.OpenScope();
        
        foreach (var param in subRoutineDeclar.Params)
        {
            param.Visit(this, arg);
        }
        
        subRoutineDeclar.Body.Visit(this, arg);
        
        _identificationTable.CloseScope();
        
        return null;
    }

    public object VisitCallStatement(CallStatement callStatement, object arg)
    {
        throw new NotImplementedException();
    }

    public object VisitDeclaration(Declaration declaration, object arg)
    {
        return declaration.Visit(this, arg);
    }

    public object? VisitVarDecl(VarDecl varDecl, object arg)
    {
        var id = varDecl.Name.Visit(this, arg).ToString();
        
        _identificationTable.Enter(id, varDecl);
        
        return null;

    }

    public object? VisitExpression(Expression expression, object arg)
    {
        expression.Visit(this, arg);
        return null;
    }

    public object VisitBinaryExpr(BinaryExpr binaryExpr, object arg)
    {
        var op1 = (TypeResult)binaryExpr.left.Visit(this, arg);
        binaryExpr.right.Visit(this, arg);
        var exOp = binaryExpr.op.Visit(this, arg);
        
        if (exOp.Equals("=") && !op1.IsLValue)
        {
            _logger.LogError("Left operand of assignment must be an l-value.");
        }
        
        return new TypeResult(false);
    }

    public object VisitUnaryExpr(UnaryExpr unaryExpr, object arg)
    {
        unaryExpr.Operand.Visit(this, arg);
        var op = unaryExpr.Operator.Visit(this, arg).ToString();
        var allowedUnaryOperators = new HashSet<string?> { "-", "+" };

        // The result of a unary operation (e.g., -x) is a calculated value.
        // It is not a variable location you can assign to, so it's an "r-value".

        if (allowedUnaryOperators.Contains(op))
        {
            _logger.LogError($"Unary operand of unary operator must be allowed.Only allowed {allowedUnaryOperators}");
        }
        
        return new TypeResult(isLValue: false);
        
    }

    public object VisitIntLiteralExpression(IntLiteralExpression intLiteralExpression, object arg)
    {
        intLiteralExpression.Literal.Visit(this, arg);
        return new TypeResult( false);
    }

    public object VisitBoolLiteralExpression(BoolLiteralExpression boolLiteralExpression, object arg)
    {
        boolLiteralExpression.Literal.Visit(this, arg);
        return new TypeResult( false);
    }

    public object VisitCharLiteralExpression(CharLiteralExpression charLiteralExpression, object arg)
    {
        charLiteralExpression.Literal.Visit(this, arg);
        return new TypeResult( false);
    }

    public object VisitVarExpression(VarExpression varExpression, object arg)
    {
        var id = varExpression.Name.Visit(this, arg).ToString();
        
        var decl = _identificationTable.Retrieve(id);
        
        return new TypeResult( true);
    }

    public object VisitCallExpr(CallExpr callExpr, object arg)
    {
        var id = callExpr.Callee.Visit(this, arg).ToString();
        List<Type> args = (List<Type>)callExpr.Arguments.Visit(this, arg);

        if (id != null)
        {
            Declaration? declaration = _identificationTable.Retrieve(id);
            
            if (declaration is SubRoutineDeclar subRoutineDeclar)
            {
                if (subRoutineDeclar.Params.Count != args.Count)
                {
                    _logger.LogError("Function '{Id}' called with incorrect number of arguments. Expected {Expected}, got {Actual}.", id, subRoutineDeclar.Params.Count, args.Count);
                }
            }
            else
            {
                _logger.LogError("Identifier '{Id}' is not a function.", id);
            }
        }

        return new TypeResult(false);
    }
}