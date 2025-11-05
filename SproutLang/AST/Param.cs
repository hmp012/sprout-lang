namespace SproutLang.AST;

public class Param :Declaration
{
    public TypeSpec Type { get; }
    public Identifier Name { get; }
    public Param(SimpleType type, Identifier name)
    {
        Type = type;
        Name = name;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitParam(this, arg);
    }
}