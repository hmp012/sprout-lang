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

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitIfBranch(this, arg);
    }
}