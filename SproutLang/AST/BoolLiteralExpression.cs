namespace SproutLang.AST;

public class BoolLiteralExpression: Expression
{
    public BoolLiteral Literal { get; }
    
    public BoolLiteralExpression(BoolLiteral literal)
    {
        Literal = literal;
    }
}