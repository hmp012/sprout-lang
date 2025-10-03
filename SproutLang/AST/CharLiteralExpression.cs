namespace SproutLang.AST;

public class CharLiteralExpression: Expression
{
    public CharLiteral Literal { get; }

    public CharLiteralExpression(CharLiteral literal)
    {
        Literal = literal;
    } 
}