namespace SproutLang.AST;

public class RepeatTimes: LoopStatement
{
    public int Times {get;}
    public Block Body {get;}
    
    public RepeatTimes(int times, Block body)
    {
        Times = times;
        Body = body;
    }

    public override object Visit(IAstVisitor v, object arg)
    {
        return v.VisitRepeatTimes(this, arg);
    }
}