namespace SproutLang.AST;

public class IntLiteralExpression: Expression
{
    public IntLiteral Lieteral { get; }
    public IntLiteralExpression(IntLiteral lieteral)
    {
        Lieteral = lieteral;
    }
}