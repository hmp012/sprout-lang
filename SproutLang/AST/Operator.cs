namespace SproutLang.AST;

public class Operator: Terminal
{
    public Operator(string spelling) : base(spelling)
    {
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitOperator(this, arg);
    }
}