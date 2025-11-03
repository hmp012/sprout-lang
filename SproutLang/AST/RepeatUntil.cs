namespace SproutLang.AST;

public class RepeatUntil: LoopStatement
{
    public Expression Condition {get;}
    public Block Body {get;}
    public RepeatUntil(Expression condition, Block body)
    {
        Condition = condition;
        Body = body;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitRepeatUntil(this, arg);
    }
}