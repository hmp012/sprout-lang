namespace SproutLang.AST;

public class SimpleType : TypeSpec
{
    public BaseType Kind { get; }
    public SimpleType(BaseType kind) => Kind = kind;
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitSimpleType(this, arg);
    }
}