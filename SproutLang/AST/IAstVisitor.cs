namespace SproutLang.AST;

public interface IAstVisitor
{
    // ---------- Statements ----------
    object  VisitProgram(Program program, object? arg);
    object?  VisitBlock(Block block, object? arg);
    object VisitArgList(ArgList argList, object? arg);
    object? VisitStatement(Statement statement, object? arg);
    object? VisitIfStatement(IfStatement ifStatement, object? arg);
    object? VisitIfBranch(IfBranch ifBranch, object? arg);
    object? VisitRepeatTimes(RepeatTimes repeatTimes, object? arg);
    object? VisitRepeatUntil(RepeatUntil repeatUntil, object? arg);
    object? VisitVarAssignment(VarAssignment varAssignment, object? arg);
    object? VisitArrayAssignment(ArrayAssignment arrayAssignment, object? arg);

    // ---------- Terminals ----------
    object? VisitBoolLiteral(BoolLiteral boolLiteral, object? arg);
    object? VisitCharLiteral(CharLiteral charLiteral, object? arg);
    object VisitIdentifier(Identifier identifier, object? arg);
    object? VisitIntLiteral(IntLiteral intLiteral, object? arg);
    object VisitOperator(Operator @operator, object? arg);
    object? VisitParam(Param param, object? arg);
    object VisitSimpleType(SimpleType simpleType, object? arg);
    object VisitArrayType(ArrayType n, object arg);
    object? VisitVomitStatement(VomitStatement vomitStatement, object? arg);
    object? VisitListenStatement(ListenStatement listenStatement, object? arg);
    
    object? VisitSubRoutineDecl(SubRoutineDeclar subRoutineDeclar, object? arg);
    object VisitCallStatement(CallStatement callStatement, object? arg);
    
    // ---------- Declarations ----------
    object VisitDeclaration(Declaration n, object arg);
    object VisitVarDecl(VarDecl n, object arg);

    // ---------- Expressions ----------
    object VisitExpression(Expression n, object arg); // generic catch-all
    object VisitBinaryExpr(BinaryExpr n, object arg);
    object VisitUnaryExpr(UnaryExpr n, object arg);
    object VisitIntLiteralExpression(IntLiteralExpression n, object arg);
    object VisitBoolLiteralExpression(BoolLiteralExpression n, object arg);
    object VisitCharLiteralExpression(CharLiteralExpression n, object arg);
    object VisitArrayExpression(ArrayExpression n, object arg);
    object VisitVarExpression(VarExpression n, object arg);
    object VisitCallExpr(CallExpr n, object arg);
    object? VisitArrayType(ArrayType arrayType, object arg);
    object? VisitArrayExpression(ArrayExpression arrayExpression, object arg);
}
