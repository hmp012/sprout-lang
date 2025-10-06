namespace SproutLang.AST;

public sealed class IfBranch: Statement
{
    public Expression Condition {get;}
    public Block Block {get;}
    public IfBranch(Expression condition, Block block)
    {
        Condition = condition;
        Block = block;
    }
}