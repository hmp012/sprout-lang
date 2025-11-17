using SproutLang.AST;
using SproutLang.TAM;
using SproutLang.Scanner;
using SproutLang.Parser;
using SproutLang.Checker;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace TestProject1.UnitTests;

public class EncoderTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public EncoderTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private class TestLogger : ILogger
    {
        public readonly List<string> ErrorMessages = new();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            // Print all logs to the console
            Console.WriteLine($"[{logLevel.ToString().ToUpper()}] {message}");

            if (logLevel == LogLevel.Error)
            {
                ErrorMessages.Add(message);
            }
        }

        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();
    }

    // Helper method to run the full compiler pipeline and return the encoder
    private (Encoder encoder, Program program, List<string> errors) EncodeProgram(string source)
    {
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, source);
        try
        {
            var logger = new TestLogger();
            var sourceFile = new SourceFile(tempFile);
            var scanner = new SproutLang.Scanner.Scanner(sourceFile);
            var parser = new ASTParser(scanner);
            var program = parser.ParseProgram();
            
            if (logger.ErrorMessages.Any())
            {
                // Stop if there are parsing errors
                return (null!, program, logger.ErrorMessages);
            }

            var checker = new Checker(logger);
            checker.Check(program);
            
            if (logger.ErrorMessages.Any())
            {
                // Stop if there are checking errors
                return (null!, program, logger.ErrorMessages);
            }

            var encoder = new Encoder(logger);
            encoder.Encode(program);
            
            return (encoder, program, logger.ErrorMessages);
        }
        finally
        {
            // Try to delete the temp file, but ignore if it's still locked
            try
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
            catch (IOException)
            {
                // File is still in use, ignore
            }
        }
    }

    [Fact]
    public void TestVarDecl_AllocatesCorrectAddress()
    {
        // Arrange: Create a program with a single variable declaration
        string source = @"
            create int x;
        ";
        
        // Act: Run through the full compiler pipeline
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        // Find the variable declaration in the AST
        var varDecl = program.Block.Statements
            .OfType<VarDecl>()
            .FirstOrDefault();
        
        Assert.NotNull(varDecl);
        Assert.NotNull(varDecl.Address);
        Assert.Equal(0, varDecl.Address.Level);
        Assert.Equal(0, varDecl.Address.Displacement);
    }
    
    [Fact]
    public void TestVarDecl_GeneratesCorrectInstructions()
    {
        // Arrange: Create a program with a variable declaration
        string source = @"
           create int x;
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        // Check that instructions were generated correctly
        // First instruction should be JUMP (to skip over declarations)
        Assert.Equal(Machine.JUMPop, Machine.Code[Machine.CB].Op);
        
        // After the jump, there should be a PUSH to allocate space for the variable
        int jumpTarget = Machine.Code[Machine.CB].D;
        Assert.Equal(Machine.PUSHop, Machine.Code[jumpTarget].Op);
        Assert.Equal(1, Machine.Code[jumpTarget].D); // 1 word for the variable
        
        // Last instruction should be HALT
        // Find the last non-zero instruction
        int lastInstr = Machine.CB;
        for (int i = Machine.CB; i < Machine.PB; i++)
        {
            if (Machine.Code[i].Op != 0 || Machine.Code[i].D != 0)
                lastInstr = i;
            else
                break;
        }
        Assert.Equal(Machine.HALTop, Machine.Code[lastInstr].Op);
    }
    
    [Fact]
    public void TestMultipleVarDecls_AllocatesSequentialAddresses()
    {
        // Arrange: Create a program with multiple variable declarations
        string source = @"
                create int x;
                create int y;
                create int z;
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        // Find all variable declarations
        var varDecls = program.Block.Statements
            .OfType<VarDecl>()
            .ToList();
        
        Assert.Equal(3, varDecls.Count);
        
        var varX = varDecls[0];
        var varY = varDecls[1];
        var varZ = varDecls[2];
        
        // Check addresses are allocated sequentially
        Assert.NotNull(varX.Address);
        Assert.NotNull(varY.Address);
        Assert.NotNull(varZ.Address);
        
        Assert.Equal(0, varX.Address.Displacement);
        Assert.Equal(1, varY.Address.Displacement);
        Assert.Equal(2, varZ.Address.Displacement);
        
        // All should be at the same level (0)
        Assert.Equal(0, varX.Address.Level);
        Assert.Equal(0, varY.Address.Level);
        Assert.Equal(0, varZ.Address.Level);
        
        // PUSH instruction should allocate space for all 3 variables
        int jumpTarget = Machine.Code[Machine.CB].D;
        Assert.Equal(Machine.PUSHop, Machine.Code[jumpTarget].Op);
        Assert.Equal(3, Machine.Code[jumpTarget].D); // 3 words total
    }
    
    [Fact]
    public void TestVarDecl_GeneratesCorrectInstructionsAndSave()
    {
        // Arrange: Create a program with a variable declaration
        string source = @"
        create int x;
        x = 10;
        vomit x;
    ";

        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);

        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        _testOutputHelper.WriteLine("Generated TAM instructions:");
        for (int i = Machine.CB; i < Machine.CB + 10; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        // Save the generated TAM code to a file
        string outputPath = Path.Combine(Path.GetTempPath(), "test_varDecl2.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code saved to: {outputPath}");

        // Check that instructions were generated correctly
        Assert.Equal(Machine.JUMPop, Machine.Code[Machine.CB].Op);
    
        // Find the vomit statement instructions
        // After PUSH, there should be: LOADL 10, STORE (for assignment), then LOAD, CALL putint, CALL puteol
        int jumpTarget = Machine.Code[Machine.CB].D;
        int currentInstr = jumpTarget + 1; // Skip PUSH
    
        // Assignment: LOADL 10
        Assert.Equal(Machine.LOADLop, Machine.Code[currentInstr].Op);
        Assert.Equal(10, Machine.Code[currentInstr].D);
        currentInstr++;
    
        // Assignment: STORE to x
        Assert.Equal(Machine.STOREop, Machine.Code[currentInstr].Op);
        currentInstr++;
    
        // Vomit: LOAD x
        Assert.Equal(Machine.LOADop, Machine.Code[currentInstr].Op);
        currentInstr++;
    
        // Vomit: CALL putint
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PutintDisplacement, Machine.Code[currentInstr].D);
        currentInstr++;
    
        // Vomit: CALL puteol (newline)
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[currentInstr].D);    }
}
