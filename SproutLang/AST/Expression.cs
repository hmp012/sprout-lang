namespace SproutLang.AST;

public class Expression :  AST
{
    public Declaration? Declaration { get; set; }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitExpression(this, arg);
    }
}