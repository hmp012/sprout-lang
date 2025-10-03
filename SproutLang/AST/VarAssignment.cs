namespace SproutLang.AST;

public class VarAssignment: LValue
{
    public Identifier Name { get; }
    public Expression Expr { get; }

    public VarAssignment(Identifier name, Expression expr)
    {
        Name = name;
        Expr = expr;
    }
}