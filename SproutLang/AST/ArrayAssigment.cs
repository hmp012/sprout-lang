namespace SproutLang.AST;

public class ArrayAssigment: LValue
{
    public Identifier Name;
    public Expression Index;
    public ArrayAssigment(Identifier name, Expression index)
    {
        Name = name;
        Index = index;
    }
}