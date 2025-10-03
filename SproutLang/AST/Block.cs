namespace SproutLang.AST;

public sealed class Block : AST
{

    public List<Statement> Statements { get; }

    public Block(List<Statement> statements)
    {
        Statements = statements;
    }
}