namespace SproutLang.AST;

public class BoolLiteral: Terminal
{
    public bool Value { get; }
    
    public BoolLiteral(bool value) : base(value.ToString().ToLower())
    {
        Value = value;
    }
}
