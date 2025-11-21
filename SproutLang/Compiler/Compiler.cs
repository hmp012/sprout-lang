using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SproutLang.Parser;
using SproutLang.Scanner;
using SproutLang.TAM;
using SproutLang.Checker;

namespace SproutLang.Compiler;

public class Compiler
{
    private readonly ILogger _logger;

    public Compiler(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Compile a SproutLang source file to TAM bytecode
    /// </summary>
    /// <param name="sourcePath">Path to the source file</param>
    /// <returns>True if compilation succeeded, false otherwise</returns>
    public bool Compile(string sourcePath)
    {
        try
        {
            // Convert to absolute path
            sourcePath = Path.GetFullPath(sourcePath);
            _logger.LogInformation("Compiling: {SourcePath}", sourcePath);
            
            // Verify file exists
            if (!File.Exists(sourcePath))
            {
                _logger.LogError("Source file not found: {SourcePath}", sourcePath);
                return false;
            }
            
            // Step 1: Create source file reader
            using var sourceFile = new SourceFile(sourcePath);
            
            // Step 2: Create scanner (lexical analysis)
            _logger.LogInformation("Creating scanner...");
            var scanner = new Scanner.Scanner(sourceFile);
            
            // Step 3: Create parser (syntax analysis)
            _logger.LogInformation("Creating parser...");
            var parser = new ASTParser(scanner);

            // Step 4: Create checker (semantic analysis)
            var checker = new Checker.Checker(_logger);

            // Step 5: Create encoder (code generation)
            var encoder = new Encoder(_logger);

            // Parse the program to build AST
            _logger.LogInformation("Parsing...");
            var program = parser.ParseProgram();

            // Check the program for semantic errors
            _logger.LogInformation("Type checking...");
            checker.Check(program);

            // Encode the program to TAM instructions
            _logger.LogInformation("Generating code...");
            encoder.Encode(program);

            // Determine output file name
            string targetPath = GetTargetPath(sourcePath);

            // Save the compiled program
            _logger.LogInformation("Saving to: {TargetPath}", targetPath);
            encoder.SaveTargetProgram(targetPath);

            _logger.LogInformation("Compilation successful!");
            return true;
        }
        catch (ParserException ex)
        {
            _logger.LogError(ex, "Parser error: {Message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Compilation error: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Determine the output file path based on the source file path
    /// </summary>
    private string GetTargetPath(string sourcePath)
    {
        if (sourcePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        {
            return Path.ChangeExtension(sourcePath, ".tam");
        }
        else
        {
            return sourcePath + ".tam";
        }
    }

    /// <summary>
    /// Compile a source file with console logging
    /// </summary>
    public static bool CompileFile(string sourcePath)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<Compiler>();
        var compiler = new Compiler(logger);

        return compiler.Compile(sourcePath);
    }
}