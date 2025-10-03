namespace SproutLang.AST;

public class ArgList: AST
{
    public List<Expression> Arguments { get; }
    
    public ArgList()
    {
        Arguments = new List<Expression>();
    }
}