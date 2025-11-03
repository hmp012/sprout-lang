namespace SproutLang.AST;

public class Statement : AST
{
    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitStatement(this, arg);
    }
}
