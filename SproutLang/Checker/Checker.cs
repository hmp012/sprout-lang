using Microsoft.Extensions.Logging;
using SproutLang.AST;

namespace SproutLang.Checker;

public class Checker : IAstVisitor
{
    private IdentificationTable _identificationTable;
    private readonly ILogger _logger;

    public Checker(ILogger logger)
    {
        _identificationTable = new IdentificationTable(logger);
        _logger = logger;
    }

    public void Check(AST.Program program)
    {
        program.Visit(this, null!);
    }

    public object VisitProgram(AST.Program program, object? arg)
    {
        _identificationTable.OpenScope();

        program.Block.Visit(this, null!);

        _identificationTable.CloseScope();

        return null!;
    }

    public object? VisitBlock(Block block, object? arg)
    {
        block.Statements.ForEach(statement => statement.Visit(this, null!));
        return null;
    }

    public object VisitArgList(ArgList argList, object? arg)
    {
        var types = new List<BaseType?>();

        foreach (var expr in argList.Arguments)
        {
            var result = expr.Visit(this, arg) as TypeResult;
            types.Add(result?.Type);
        }

        return types;
    }

    public object? VisitStatement(Statement statement, object? arg)
    {
        statement.Visit(this, arg);
        return null;
    }

    public object? VisitIfStatement(IfStatement ifStatement, object? arg)
    {
        ifStatement.First.Visit(this, arg);

        foreach (var elseIf in ifStatement.ElseIfBranches)
        {
            elseIf.Visit(this, arg);
        }

        if (ifStatement.ElseBlock != null)
        {
            _identificationTable.OpenScope();
            ifStatement.ElseBlock.Visit(this, arg);
            _identificationTable.CloseScope();
        }

        return null;
    }

    public object? VisitIfBranch(IfBranch ifBranch, object? arg)
    {
        ifBranch.Condition.Visit(this, arg);
        _identificationTable.OpenScope();
        ifBranch.Block.Visit(this, arg);
        _identificationTable.CloseScope();
        return null;
    }

    public object? VisitRepeatTimes(RepeatTimes repeatTimes, object? arg)
    {
        repeatTimes.Times.Visit(this, arg);
        repeatTimes.Body.Visit(this, arg);
        return null;
    }

    public object? VisitRepeatUntil(RepeatUntil repeatUntil, object? arg)
    {
        repeatUntil.Body.Visit(this, arg);
        repeatUntil.Condition.Visit(this, arg);

        return null;
    }

    public object? VisitVarAssignment(VarAssignment varAssignment, object? arg)
    {
        var id = varAssignment.Name.Visit(this, arg)?.ToString();
        if (id != null)
        {
            var varDecl = _identificationTable.Retrieve(id);
            
            if (varDecl is VarDecl vd)
            {
                var declaredType = (vd.Type as SimpleType)?.Kind;
                var exprResult = varAssignment.Expr.Visit(this, arg) as TypeResult;

                if (declaredType != null && exprResult != null && exprResult.Type != null)
                {
                    if (declaredType != exprResult.Type)
                    {
                        _logger.LogError("Type mismatch in assignment to '{Id}'. Expected {Expected}, got {Actual}.",
                            id, declaredType, exprResult.Type);
                    }
                }
            }
            else
            {
                _logger.LogError("Variable '{Id}' not declared as a variable", id);
            }
        }

        varAssignment.Expr.Visit(this, arg);

        return null;
    }

    public object? VisitArrayAssignment(ArrayAssignment arrayAssignment, object? arg)
    {
        var id = arrayAssignment.Name.Visit(this, arg)?.ToString();
        if (id != null)
        {
            if (_identificationTable.Retrieve(id) is VarDecl array)
            {
                if (array.Type.Visit(this, arg) is ArrayType type)
                {
                    var size = type.Size;

                    var expressionType = arrayAssignment.Expr.Visit(this, arg) as TypeResult;

                    if (expressionType != null && type.ElementType != expressionType.Type)
                    {
                        _logger.LogError(
                            "Type mismatch in array assignment to '{Id}'. Expected {Expected}, got {Actual}.",
                            id, type.ElementType, expressionType.Type);
                    }

                    if (arrayAssignment.Index.Visit(this,arg) is TypeResult)
                    {
                        var indexValue = arrayAssignment.Index.Literal.Value;
                        if (indexValue < 0 || indexValue >= size)
                        {
                            _logger.LogError("Array index {Index} out of bounds for array '{Id}' of size {Size}.",
                                indexValue, id, size);
                        }
                    }

                }
            }
        }
        return null;
    }

    public object? VisitBoolLiteral(BoolLiteral boolLiteral, object? arg)
    {
        return null;
    }

    public object? VisitCharLiteral(CharLiteral charLiteral, object? arg)
    {
        return null;
    }

    public object VisitIdentifier(Identifier identifier, object? arg)
    {
        return identifier.Spelling;
    }

    public object? VisitIntLiteral(IntLiteral intLiteral, object? arg)
    {
        return null;
    }

    public object VisitOperator(Operator @operator, object? arg)
    {
        return @operator.Spelling;
    }

    public object? VisitParam(Param param, object? arg)
    {
        param.Name.Visit(this, arg);
        param.Type.Visit(this, arg);

        _identificationTable.Enter(param.Name.Spelling, param);
        return null;
    }

    public object VisitSimpleType(SimpleType simpleType, object? arg)
    {
        return simpleType;
    }

    public object? VisitArrayType(ArrayType arrayType, object? arg)
    {
        return arrayType;
    }

    public object? VisitVomitStatement(VomitStatement vomitStatement, object? arg)
    {
        vomitStatement.Expression.Visit(this, arg);
        return null;
    }

    public object? VisitListenStatement(ListenStatement listenStatement, object? arg)
    {
        listenStatement.Identifier.Visit(this, arg);
        return null;
    }

    public object? VisitSubRoutineDecl(SubRoutineDeclar subRoutineDeclar, object? arg)
    {
        var id = subRoutineDeclar.Name.Visit(this, arg)?.ToString();

        if (id != null)
        {
            _identificationTable.Enter(id, subRoutineDeclar);
        }

        _identificationTable.OpenScope();

        foreach (var param in subRoutineDeclar.Params)
        {
            param.Visit(this, arg);
        }

        subRoutineDeclar.Body.Visit(this, arg);

        _identificationTable.CloseScope();

        return null;
    }

    public object VisitCallStatement(CallStatement callStatement, object? arg)
    {
        throw new NotImplementedException();
    }

    public object? VisitDeclaration(Declaration declaration, object? arg)
    {
        return declaration.Visit(this, arg);
    }

    public object? VisitVarDecl(VarDecl varDecl, object? arg)
    {
        var id = varDecl.Name.Visit(this, arg)?.ToString();

        varDecl.Type.Visit(this, arg);

        if (id != null)
        {
            _identificationTable.Enter(id, varDecl);
        }

        return null;
    }

    public object? VisitExpression(Expression expression, object? arg)
    {
        expression.Visit(this, arg);
        return null;
    }

    public object VisitBinaryExpr(BinaryExpr binaryExpr, object? arg)
    {
        var leftResult = binaryExpr.left.Visit(this, arg) as TypeResult;
        var rightResult = binaryExpr.right.Visit(this, arg) as TypeResult;
        var exOp = binaryExpr.op.Visit(this, arg)?.ToString();

        if (exOp != null && exOp.Equals("="))
        {
            if (leftResult != null && !leftResult.IsLValue)
            {
                _logger.LogError("Left operand of assignment must be an l-value.");
            }

            CheckTypeMismatch(leftResult, rightResult, "assignment");
            return new TypeResult(leftResult?.Type, false);
        }

        var comparisonOperators = new HashSet<string?> { "==", "!=", "<", ">" };

        CheckTypeMismatch(leftResult, rightResult, $"binary operation '{exOp}'");

        var resultType = comparisonOperators.Contains(exOp) ? BaseType.Bool : leftResult?.Type;
        return new TypeResult(resultType, false);
    }

    private void CheckTypeMismatch(TypeResult? leftResult, TypeResult? rightResult, string? operation)
    {
        if (leftResult?.Type != null && rightResult?.Type != null)
        {
            if (leftResult.Type != rightResult.Type)
            {
                _logger.LogError("Type mismatch in {Operation}. Left: {Left}, Right: {Right}.",
                    operation, leftResult.Type, rightResult.Type);
            }
        }
    }


    public object VisitUnaryExpr(UnaryExpr unaryExpr, object? arg)
    {
        var operandResult = unaryExpr.Operand.Visit(this, arg) as TypeResult;
        var op = unaryExpr.Operator.Visit(this, arg)?.ToString();
        var allowedUnaryOperators = new HashSet<string?> { "-", "+" };

        // The result of a unary operation (e.g., -x) is a calculated value.
        // It is not a variable location you can assign to, so it's an "r-value".

        if (!allowedUnaryOperators.Contains(op))
        {
            _logger.LogError($"Unary operand of unary operator must be allowed.Only allowed {allowedUnaryOperators}");
        }

        return new TypeResult(operandResult?.Type, isLValue: false);
    }

    public object VisitIntLiteralExpression(IntLiteralExpression intLiteralExpression, object? arg)
    {
        intLiteralExpression.Literal.Visit(this, arg);
        return new TypeResult(BaseType.Int, false);
    }

    public object VisitBoolLiteralExpression(BoolLiteralExpression boolLiteralExpression, object? arg)
    {
        boolLiteralExpression.Literal.Visit(this, arg);
        return new TypeResult(BaseType.Bool, false);
    }

    public object VisitCharLiteralExpression(CharLiteralExpression charLiteralExpression, object? arg)
    {
        charLiteralExpression.Literal.Visit(this, arg);
        return new TypeResult(BaseType.Char, false);
    }

    public object VisitVarExpression(VarExpression varExpression, object? arg)
    {
        var id = varExpression.Name.Visit(this, arg)?.ToString();

        if (id == null)
        {
            _logger.LogError("Variable name is null.");
            return new TypeResult(null, false);
        }

        var decl = _identificationTable.Retrieve(id);
        
        // Store the declaration reference in the AST node for the encoder to use
        varExpression.Declaration = decl;

        if (decl is VarDecl varDecl)
        {
            var type = (varDecl.Type as SimpleType)?.Kind;
            return new TypeResult(type, true);
        }

        if (decl is Param param)
        {
            var type = param.Type.Kind;
            return new TypeResult(type, true);
        }

        _logger.LogError("Variable '{Id}' not declared.", id);
        return new TypeResult(null, false);
    }

    public object VisitCallExpr(CallExpr callExpr, object? arg)
    {
        var id = callExpr.Callee.Visit(this, arg)?.ToString();
        var args = callExpr.Arguments.Visit(this, arg) as List<BaseType?>;

        if (id != null)
        {
            Declaration? declaration = _identificationTable.Retrieve(id);

            if (declaration is SubRoutineDeclar subRoutineDeclar)
            {
                if (args != null && subRoutineDeclar.Params.Count != args.Count)
                {
                    _logger.LogError(
                        "Function '{Id}' called with incorrect number of arguments. Expected {Expected}, got {Actual}.",
                        id, subRoutineDeclar.Params.Count, args.Count);
                }
            }
            else
            {
                _logger.LogError("Identifier '{Id}' is not a function.", id);
            }
        }

        return new TypeResult(null, false);
    }

    public object VisitArrayExpression(ArrayExpression arrayExpression, object? arg)
    {
        var id = arrayExpression.Name.Visit(this, arg)?.ToString();

        if (id == null)
        {
            _logger.LogError("Array name is null.");
            return new TypeResult(null, false);
        }

        var decl = _identificationTable.Retrieve(id);

        if (decl is VarDecl varDecl)
        {
            if (varDecl.Type is ArrayType arrayType)
            {
                var indexResult = arrayExpression.Index.Visit(this, arg) as TypeResult;

                if (indexResult?.Type != null && indexResult.Type != BaseType.Int)
                {
                    _logger.LogError("Array index must be of type Int. Got {Type}.", indexResult.Type);
                }

                return new TypeResult(arrayType.ElementType, true);
            }
            else
            {
                _logger.LogError("Variable '{Id}' is not an array.", id);
                return new TypeResult(null, false);
            }
        }

        _logger.LogError("Array '{Id}' not declared.", id);
        return new TypeResult(null, false);
    }
}