using SproutLang.AST;

namespace SproutLang.Checker;

public class TypeResult
{
    // False if it's an r-value (e.g., a literal or binary operation result).
    public bool IsLValue { get; }
    public BaseType? Type { get; }

    public TypeResult(BaseType? type, bool isLValue)
    {
        Type = type;
        IsLValue = isLValue;
    } 
}