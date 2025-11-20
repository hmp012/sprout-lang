namespace SproutLang.AST;

public class RepeatTimes: LoopStatement
{
    public Expression Times {get;}
    public Block Body {get;}
    
    public RepeatTimes(Expression times, Block body)
    {
        Times = times;
        Body = body;
    }

    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitRepeatTimes(this, arg);
    }
}