namespace SproutLang.AST;

public class CharLiteral : Terminal
{
    public char Value { get; }
    public CharLiteral(char value) : base(value.ToString())
    {
        Value = value;
    }

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitCharLiteral(this, arg);
    }
}