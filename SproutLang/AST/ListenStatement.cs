namespace SproutLang.AST;

public class ListenStatement: Statement
{
    public Identifier Identifier;
    public ListenStatement(Identifier identifier)
    {
        Identifier = identifier;
    }
}