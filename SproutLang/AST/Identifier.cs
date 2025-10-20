namespace SproutLang.AST;

public class Identifier : Terminal
{
    public Identifier(string spelling) : base(spelling)
    {
    }

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitIdentifier(this, arg);
    }
}