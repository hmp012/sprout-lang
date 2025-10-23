namespace SproutLang.AST;

public class CallExpr:  Expression
{
    public string Callee { get; }
    public ArgList Arguments { get; }

    public CallExpr(string callee, ArgList arguments)
    {
        Callee = callee;
        Arguments = arguments;
    }
    
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitCallExpr(this, arg);
    }
    
}