namespace SproutLang.AST;

public class Expression :  AST
{
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitExpression(this, arg);
    }
}