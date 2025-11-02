namespace SproutLang.AST;
public class ArrayExpression(Identifier name, Expression index) : Expression
{
    public Identifier Name { get; } = name;
    public Expression Index { get; } = index;

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitArrayExpression(this, arg);
    }
}
