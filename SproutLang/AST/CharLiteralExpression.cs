namespace SproutLang.AST;

public class CharLiteralExpression: Expression
{
    public CharLiteral Literal { get; }

    public CharLiteralExpression(CharLiteral literal)
    {
        Literal = literal;
    } 
    
    
    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitCharLiteralExpression(this, arg);
    }
}