namespace SproutLang.AST;

public sealed class Block : AST
{

    public List<Statement> Statements { get; }

    public Block(List<Statement> statements)
    {
        Statements = statements;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitBlock(this, arg);
    }
}