namespace SproutLang.AST;
public class ArrayType(BaseType elementType, int size) : TypeSpec
{
    public BaseType ElementType { get; } = elementType;
    public int Size { get; } = size;

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitArrayType(this, arg);
    }
}