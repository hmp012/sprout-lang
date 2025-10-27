namespace SproutLang.Checker;

public class TypeResult
{
    // False if it's an r-value (e.g., a literal or binary operation result).
    public bool IsLValue { get; }

    public TypeResult(bool isLValue)
    {
        IsLValue = isLValue;
    } 
}