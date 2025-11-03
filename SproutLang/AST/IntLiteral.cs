namespace SproutLang.AST;

public class IntLiteral:  Terminal
{
    public int Value { get; }
    public IntLiteral(int value) : base(value.ToString())
    {
        Value = value;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitIntLiteral(this, arg);
    }
}