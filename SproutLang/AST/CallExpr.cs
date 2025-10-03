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
    
}