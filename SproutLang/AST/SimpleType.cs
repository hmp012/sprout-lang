namespace SproutLang.AST;

public class SimpleType : TypeSpec
{
    public BaseType Kind { get; }
    public SimpleType(BaseType kind) => Kind = kind;
}