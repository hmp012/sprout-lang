namespace SproutLang.AST;

public sealed class IfBranch: AST
{
    public Expression Condition {get;}
    public Block Block {get;}
    public IfBranch(Expression condition, Block block)
    {
        Condition = condition;
        Block = block;
    }
}