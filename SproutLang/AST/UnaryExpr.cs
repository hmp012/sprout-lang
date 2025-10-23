namespace SproutLang.AST;

public class UnaryExpr : Expression
{
    public Operator Operator;
    public Expression Operand;
    public UnaryExpr(Operator op, Expression operand)
    {
        Operator = op;
        Operand = operand;
    }
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitUnaryExpr(this, arg);
    }
}