namespace SproutLang.Scanner;

public class SourceFile
{
    public const char EOL = '\n';
    public const char EOT = (char)0;

    private FileStream _source;

    public SourceFile(string sourceFileName)
    {
        try
        {
            _source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"*** FILE NOT FOUND *** ({sourceFileName})");
            Environment.Exit(1);
        }
    }

    public char GetSource()
    {
        try
        {
            int c = _source.ReadByte();
            if (c < 0)
            {
                return EOT;
            }
            return (char)c;
        }
        catch (IOException)
        {    
            return EOT;
        }
    }
}