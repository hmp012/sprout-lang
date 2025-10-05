namespace SproutLang.AST;

public class CallStatement: Statement
{
    public CallExpr Call { get; }

    public CallStatement(CallExpr call)
    {
        Call = call;
    }
}