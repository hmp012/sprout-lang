using SproutLang.TAM;

namespace SproutLang.AST;

public sealed class SubRoutineDeclar : Declaration
{
    public Identifier Name { get; }
    public List<Param> Params { get; }
    public Block Body { get; }
    public Address? Address { get; set; }

    public SubRoutineDeclar(Identifier name, List<Param> parameters, Block body)
    {
        Name = name;
        Params = parameters;
        Body = body;
    }
    public override object? Visit(IAstVisitor v, object? arg)
    {
        return v.VisitSubRoutineDecl(this, arg);
    }
}