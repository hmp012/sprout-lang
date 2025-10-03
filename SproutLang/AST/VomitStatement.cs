namespace SproutLang.AST;

public class VomitStatement: Statement
{
    public Expression Expression { get; }
    public VomitStatement(Expression expression)
    {
        Expression = expression;
    }
    
}