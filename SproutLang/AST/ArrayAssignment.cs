namespace SproutLang.AST;

public class ArrayAssignment: Assignment
{
    public Identifier Name;
    public Expression Index;
    public ArrayAssignment(Identifier name, Expression index)
    {
        Name = name;
        Index = index;
    }
}