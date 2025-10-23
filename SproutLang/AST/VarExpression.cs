namespace SproutLang.AST;

public sealed class VarExpression: Expression
{
    public Identifier Name { get; }

    public VarExpression(Identifier name)
    {
        Name = name;
    }
    
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitVarExpression(this, arg);
    }
}