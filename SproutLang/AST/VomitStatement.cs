namespace SproutLang.AST;

public class VomitStatement: Statement
{
    public Expression Expression { get; }
    public VomitStatement(Expression expression)
    {
        Expression = expression;
    }
    
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitVomitStatement(this, arg);
    }
}