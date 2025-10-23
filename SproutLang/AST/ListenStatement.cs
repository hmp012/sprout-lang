namespace SproutLang.AST;

public class ListenStatement: Statement
{
    public Identifier Identifier;
    public ListenStatement(Identifier identifier)
    {
        Identifier = identifier;
    }
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitListenStatement(this, arg);
    }
}