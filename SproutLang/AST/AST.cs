namespace SproutLang.AST;

public abstract class AST
{
    public abstract object? Visit(IAstVisitor v, object? arg);
}