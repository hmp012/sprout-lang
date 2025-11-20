namespace SproutLang.Scanner;

public class SourceFile : IDisposable
{
    public const char EOL = '\n';
    public const char EOT = (char)0;

    private FileStream _source;
    private bool _disposed = false;

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _source?.Dispose();
            }
            _disposed = true;
        }
    }

    ~SourceFile()
    {
        Dispose(false);
    }
}