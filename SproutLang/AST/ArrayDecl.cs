namespace SproutLang.AST;
public sealed class ArrayDecl(ArrayType type, Identifier identifier) : Declaration
{
    public ArrayType Type { get; } = type;
    public Identifier Name { get; } = identifier;

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitArrayDecl(this, arg);
    }
}
