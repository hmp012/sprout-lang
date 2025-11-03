namespace SproutLang.AST;

public class BoolLiteralExpression: Expression
{
    public BoolLiteral Literal { get; }
    
    public BoolLiteralExpression(BoolLiteral literal)
    {
        Literal = literal;
    }
    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitBoolLiteralExpression(this, arg);
    }
    
}