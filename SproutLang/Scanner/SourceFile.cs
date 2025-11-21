namespace SproutLang.Scanner;

public class SourceFile : IDisposable
{
    public const char EOL = '\n';
    public const char EOT = (char)0;

    private StreamReader _reader;
    private bool _disposed = false;

    public SourceFile(string sourceFileName)
    {
        try
        {
            // Use StreamReader with UTF-8 encoding to properly handle text files
            _reader = new StreamReader(sourceFileName, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
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
            int c = _reader.Read();
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
                _reader?.Dispose();
            }
            _disposed = true;
        }
    }

    ~SourceFile()
    {
        Dispose(false);
    }
}