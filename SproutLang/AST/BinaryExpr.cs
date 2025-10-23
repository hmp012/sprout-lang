namespace SproutLang.AST;

public class BinaryExpr: Expression
{
    public Expression left { get; }
    public Operator op { get; }
    public Expression right { get; }
    public BinaryExpr(Expression left, Operator op, Expression right)
    {
        this.left = left;
        this.op = op;
        this.right = right;
    }
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitBinaryExpr(this, arg);
    }
}