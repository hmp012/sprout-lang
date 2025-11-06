namespace SproutLang.AST;
public class ArrayExpression(Identifier name, IntLiteralExpression index) : Expression
{
    public Identifier Name { get; } = name;
    public IntLiteralExpression Index { get; } = index;

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitArrayExpression(this, arg);
    }
}