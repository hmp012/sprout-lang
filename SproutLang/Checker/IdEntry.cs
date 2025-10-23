using SproutLang.AST;

namespace SproutLang.Checker;

public class IdEntry(int level, string id, Declaration declaration)
{
    public int Level { get; set; } = level;
    public string Id { get; set; } = id;
    public Declaration Declaration { get; set; } = declaration;

    public override string ToString()
    {
        return $"IdEntry(level: {Level}, id: {Id}, declaration: {Declaration})";
    }
}