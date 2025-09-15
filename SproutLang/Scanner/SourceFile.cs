namespace SproutLang.Scanner;

using System;
using System.IO;

public class SourceFile
{
    public const char EOL = '\n';
    public const char EOT = '\0';

    private FileStream source;

    public SourceFile(string sourceFileName)
    {
        try
        {
            source = new FileStream(sourceFileName, FileMode.Open, FileAccess.Read);
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
            int c = source.ReadByte();
            if (c < 0)
                return EOT;
            else
                return (char)c;
        }
        catch (IOException)
        {
            return EOT;
        }
    }
}
