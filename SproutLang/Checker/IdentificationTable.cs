using SproutLang.AST;

namespace SproutLang.Checker;

public class IdentificationTable
{
    private List<IdEntry> Table { get; set; } = new List<IdEntry>();
    private int Level { get; set; } = 0;
    
    public void OpenScope()
    {
        Level++;
    }
    
    public void CloseScope()
    {
        Table.RemoveAll(entry => entry.level == Level);
        Level--;
    }

    public void Enter(string id, Declaration declaration)
    {
        IdEntry? entry = Find( id );

        if (entry != null && entry.level == Level)
        {
            Console.WriteLine(id + " declared twice");
        }
        else
        {
            Table.Add(new IdEntry(Level, id, declaration));
        }
    }
    
    public Declaration? Retrieve( String id )
    {
        return Find( id )?.declaration;
    }
    
    private IdEntry? Find( String id )
    {
        return Table.Find(i => i.id == id);
    }
}