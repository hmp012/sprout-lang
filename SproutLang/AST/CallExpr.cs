namespace SproutLang.AST;

public class CallExpr:  Expression
{
    public Identifier Callee { get; }
    public ArgList Arguments { get; }

    public CallExpr(Identifier callee, ArgList arguments)
    {
        Callee = callee;
        Arguments = arguments;
    }
    
    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitCallExpr(this, arg);
    }
    
}