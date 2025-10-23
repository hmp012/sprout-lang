namespace SproutLang.AST;

public class CallStatement: Statement
{
    public CallExpr Call { get; }

    public CallStatement(CallExpr call)
    {
        Call = call;
    }
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitCallStatement(this, arg);
    }
}