namespace SproutLang.AST;

public class Declaration :  Statement
{
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitDeclaration(this, arg);
    }
}