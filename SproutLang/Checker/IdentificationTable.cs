using SproutLang.AST;
using Microsoft.Extensions.Logging;

namespace SproutLang.Checker;

public class IdentificationTable
{
    private readonly ILogger<IdentificationTable> _logger;
    private List<IdEntry> Table { get; set; } = new List<IdEntry>();
    private int Level { get; set; } = 0;
    
    public void OpenScope()
    {
        Level++;
    }
    
    public void CloseScope()
    {
        Table.RemoveAll(entry => entry.Level == Level);
        Level--;
    }

    public void Enter(string id, Declaration declaration)
    {
        IdEntry? entry = Find(id);

        if (entry != null && entry.Level == Level)
        {
            _logger.LogError("Identifier '{Id}' is already declared in the current scope at level {Level}", id, Level);
        }
        else
        {
            Table.Add(new IdEntry(Level, id, declaration));
        }
    }
    
    public Declaration? Retrieve( string id )
    {
        return Find( id )?.Declaration;
    }
    
    private IdEntry? Find( string id )
    {
        return Table.FindLast(i => i.Id == id);
    }
}