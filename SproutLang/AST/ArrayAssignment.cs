namespace SproutLang.AST;

public class ArrayAssignment(Identifier name, IntLiteralExpression index, Expression expr) : Assignment
{
    public Identifier Name { get; set;  } = name;
    public IntLiteralExpression Index { get; set; } = index;
    public Expression Expr {get; set;} = expr;

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitArrayAssignment(this, arg);
    }
}
