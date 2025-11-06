namespace SproutLang.AST;

public class IntLiteralExpression: Expression
{
    public IntLiteral Literal { get; }
    public IntLiteralExpression(IntLiteral literal)
    {
        Literal = literal;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitIntLiteralExpression(this, arg);
    }
}