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

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitArrayAssignment(this, arg);
    }
}