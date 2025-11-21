namespace SproutLang.AST;

public abstract class Assignment: Statement
{
    public Declaration? Declaration { get; set; }
}