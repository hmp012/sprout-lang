using System.Linq.Expressions;

namespace SproutLang.AST;

public interface IAstVisitor
{
    // ---------- Statements ----------
    object VisitProgram(Program n, object arg);
    object VisitBlock(Block n, object arg);
    object VisitArgList(ArgList n, object arg);
    object VisitStatement(Statement n, object arg);

    // ---------- Terminals ----------
    object VisitBoolLiteral(BoolLiteral n, object arg);
    object VisitCharLiteral(CharLiteral n, object arg);
    object VisitIdentifier(Identifier n, object arg);
    object VisitIntLiteral(IntLiteral n, object arg);
    object VisitOperator(Operator n, object arg);
    object VisitParam(Param n, object arg);
    object VisitSimpleType(SimpleType n, object arg);

    // ---------- Expressions ----------
    object VisitExpression(Expression n, object arg); // generic catch-all
    object VisitBinaryExpr(BinaryExpr n, object arg);
    object VisitUnaryExpr(UnaryExpr n, object arg);
    object VisitIntLiteralExpression(IntLiteralExpression n, object arg);
    object VisitBoolLiteralExpression(BoolLiteralExpression n, object arg);
    object VisitCharLiteralExpression(CharLiteralExpression n, object arg);
    object VisitVarExpression(VarExpression n, object arg);
    object VisitCallExpr(CallExpr n, object arg);
}
