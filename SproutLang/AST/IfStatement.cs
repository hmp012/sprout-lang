namespace SproutLang.AST;

public class IfStatement: Statement
{
    public IfBranch First { get; }
    public List<IfBranch> ElseIfBranches { get; }
    public Block? ElseBlock { get; }
    
    public IfStatement(IfBranch first, List<IfBranch> elseIfBranches, Block? elseBlock)
    {
        First = first;
        ElseIfBranches = elseIfBranches;
        ElseBlock = elseBlock;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitIfStatement(this, arg);
    }
}
