namespace SproutLang.AST;

public class ArrayAssignment(Identifier name, Expression index) : Assignment
{
    public Identifier Name = name;
    public Expression Index = index;
    public Expression? Expr;

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitArrayAssignment(this, arg);
    }
}