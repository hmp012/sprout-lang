namespace SproutLang.AST;

public class Param :AST
{
    public TypeSpec Type { get; }
    public Identifier Name { get; }
    public Param(TypeSpec type, Identifier name)
    {
        Type = type;
        Name = name;
    }
}