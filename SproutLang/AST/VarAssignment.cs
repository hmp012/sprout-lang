namespace SproutLang.AST;

public class VarAssignment: Assignment
{
    public Identifier Name { get; }
    public Expression Expr { get; }
    public Declaration? Declaration { get; set; }

    public VarAssignment(Identifier name, Expression expr)
    {
        Name = name;
        Expr = expr;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitVarAssignment(this, arg);
    }
}