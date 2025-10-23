namespace SproutLang.AST;

public class ArgList: AST
{
    public List<Expression> Arguments { get; }
    
    public ArgList()
    {
        Arguments = new List<Expression>();
    }

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitArgList(this, arg);
    }
}