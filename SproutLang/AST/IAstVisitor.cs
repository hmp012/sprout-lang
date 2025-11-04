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
    object? VisitVomitStatement(VomitStatement vomitStatement, object? arg);
    object? VisitListenStatement(ListenStatement listenStatement, object? arg);
    
    object? VisitSubRoutineDecl(SubRoutineDeclar subRoutineDeclar, object? arg);
    object VisitCallStatement(CallStatement callStatement, object? arg);
    
    // ---------- Declarations ----------
    object? VisitDeclaration(Declaration declaration, object? arg);
    object? VisitVarDecl(VarDecl varDecl, object? arg);

    // ---------- Expressions ----------
    object? VisitExpression(Expression expression, object?  arg); // generic catch-all
    object VisitBinaryExpr(BinaryExpr binaryExpr, object? arg);
    object VisitUnaryExpr(UnaryExpr unaryExpr, object? arg);
    object VisitIntLiteralExpression(IntLiteralExpression intLiteralExpression, object? arg);
    object VisitBoolLiteralExpression(BoolLiteralExpression boolLiteralExpression, object? arg);
    object VisitCharLiteralExpression(CharLiteralExpression charLiteralExpression, object? arg);
    object VisitVarExpression(VarExpression varExpression, object? arg);
    object VisitCallExpr(CallExpr callExpr, object? arg);
}
