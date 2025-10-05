namespace SproutLang.AST;

public class IntLiteralExpression: Expression
{
    public IntLiteral Literal { get; }
    public IntLiteralExpression(IntLiteral literal)
    {
        Literal = literal;
    }
}