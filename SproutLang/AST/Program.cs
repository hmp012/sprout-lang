namespace SproutLang.AST;

public class Program: AST
{
    public Block Block { get; }
    
    public Program(Block block)
    {
        Block = block;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitProgram(this, arg);
    }
}