namespace SproutLang.AST;

public class Program: AST
{
    public Block Block { get; }
    
    public Program(Block block)
    {
        Block = block;
    }
}