namespace SproutLang.AST;

public sealed class VarDecl :  Declaration
{
    public TypeSpec Type { get; }
    public Identifier Name { get; }

    public VarDecl(TypeSpec type, Identifier identifier)
    {
        Type = type;
        Name = identifier;
    }
}