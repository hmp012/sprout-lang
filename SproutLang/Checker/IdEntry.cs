using SproutLang.AST;

namespace SproutLang.Checker;

public class IdEntry
{
    public int level { get; set; }
    public string id { get; set; }
    public Declaration declaration { get; set; }
    
    public IdEntry(int level, string id, Declaration declaration)
    {
        this.level = level;
        this.id = id;
        this.declaration = declaration;
    }
    
    public override string ToString()
    {
        return $"IdEntry(level: {level}, id: {id}, declaration: {declaration})";
    }
}