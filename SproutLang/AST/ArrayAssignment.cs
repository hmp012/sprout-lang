namespace SproutLang.AST;

public class ArrayAssignment(Identifier name, Expression index, Expression? expr = null) : Assignment
{
    public Identifier Name { get; set;  } = name;
    public Expression Index { get; set; } = index;
    public Expression? Expr {get; set;} = expr;

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitArrayAssignment(this, arg);
    }
}