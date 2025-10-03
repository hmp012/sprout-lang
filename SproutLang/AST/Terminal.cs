namespace SproutLang.AST;

public abstract class Terminal : AST
{
    public string Spelling { get; }

    protected Terminal(string spelling)
    {
        Spelling = spelling;
    }
}