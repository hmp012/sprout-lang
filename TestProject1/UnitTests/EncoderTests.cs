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
        vomit x;";

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
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[currentInstr].D);
    }

    [Fact]
    public void TestVarDecl_Char_GeneratesCorrectInstructionsAndSave()
    {
        var source = @"
        create char x;
        x = 'A';
        vomit x;";
        
        var (encoder, program, errors) = EncodeProgram(source);
        
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
        Assert.Equal(Machine.JUMPop, Machine.Code[Machine.CB].Op);

        // Find the jump target and verify PUSH instruction
        int jumpTarget = Machine.Code[Machine.CB].D;
        Assert.Equal(Machine.PUSHop, Machine.Code[jumpTarget].Op);
        Assert.Equal(1, Machine.Code[jumpTarget].D); // 1 word for the variable
        
        int currentInstr = jumpTarget + 1; // Skip PUSH

        // Assignment: LOADL 'A' (ASCII value 65)
        Assert.Equal(Machine.LOADLop, Machine.Code[currentInstr].Op);
        Assert.Equal((int)'A', Machine.Code[currentInstr].D);
        currentInstr++;

        // Assignment: STORE to x
        Assert.Equal(Machine.STOREop, Machine.Code[currentInstr].Op);
        currentInstr++;

        // Vomit: LOAD x
        Assert.Equal(Machine.LOADop, Machine.Code[currentInstr].Op);
        currentInstr++;

        // Vomit: CALL put (for char output)
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PutDisplacement, Machine.Code[currentInstr].D);
        currentInstr++;

        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[currentInstr].D);
        var outputPath = Path.Combine(Path.GetTempPath(), "test_varDeclChar.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for char variable saved to: {outputPath}");
    }
    
    [Fact]
    public void TestVarDecl_Bool_GeneratesCorrectInstructionsAndSave()
    {
        var source = @"
        create bool x;
        x = false;
        vomit x;";
        
        var (encoder, program, errors) = EncodeProgram(source);
        
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
        // Check that instructions were generated correctly
        Assert.Equal(Machine.JUMPop, Machine.Code[Machine.CB].Op);

        // Find the jump target and verify PUSH instruction
        int jumpTarget = Machine.Code[Machine.CB].D;
        Assert.Equal(Machine.PUSHop, Machine.Code[jumpTarget].Op);
        Assert.Equal(1, Machine.Code[jumpTarget].D); // 1 word for the variable

        int currentInstr = jumpTarget + 1; // Skip PUSH

        // Assignment: LOADL false (0)
        Assert.Equal(Machine.LOADLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.FalseRep, Machine.Code[currentInstr].D);
        currentInstr++;

        // Assignment: STORE to x
        Assert.Equal(Machine.STOREop, Machine.Code[currentInstr].Op);
        currentInstr++;

        // Vomit: LOAD x
        Assert.Equal(Machine.LOADop, Machine.Code[currentInstr].Op);
        currentInstr++;

        // Vomit: CALL putint (for bool output as 0/1)
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PutintDisplacement, Machine.Code[currentInstr].D);
        currentInstr++;

        // Vomit: CALL puteol (newline)
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[currentInstr].D);
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_varDeclBool.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for bool variable saved to: {outputPath}");
    }
    
    
    [Fact]
    public void TestFunctionDeclaration_AllocatesCorrectAddress()
    {
        // Arrange: Create a program with a function declaration
        string source = @"
            sprout add(int a, int b) {
                vomit a + b;
            }
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        // Find the function declaration
        var funcDecl = program.Block.Statements
            .OfType<SubRoutineDeclar>()
            .FirstOrDefault();
        
        Assert.NotNull(funcDecl);
        Assert.NotNull(funcDecl.Address);
        Assert.Equal(0, funcDecl.Address.Level);
        
        // Function address should point to the code after the initial JUMP
        Assert.True(funcDecl.Address.Displacement > 0);
        
        _testOutputHelper.WriteLine($"Function 'add' allocated at address: Level={funcDecl.Address.Level}, Displacement={funcDecl.Address.Displacement}");
    }
    
    [Fact]
    public void TestFunctionDeclaration_GeneratesCorrectInstructions()
    {
        // Arrange: Create a program with a function declaration
        string source = @"
            sprout add(int a, int b) {
                vomit a + b;
            }
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        // First instruction should be JUMP (to skip function body)
        Assert.Equal(Machine.JUMPop, Machine.Code[Machine.CB].Op);
        
        // Find the function declaration
        var funcDecl = program.Block.Statements
            .OfType<SubRoutineDeclar>()
            .FirstOrDefault();
        
        Assert.NotNull(funcDecl);
        Assert.NotNull(funcDecl.Address);
        
        // Function body starts at the address stored in the declaration
        int funcStart = funcDecl.Address.Displacement;
        
        _testOutputHelper.WriteLine($"Function body starts at instruction {funcStart}");
        _testOutputHelper.WriteLine("Generated TAM instructions:");
        for (int i = Machine.CB; i < funcStart + 15; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
        string outputPath = Path.Combine(Path.GetTempPath(), "test_funDecl.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for function called saved to: {outputPath}");
        
        // The function body should contain instructions for: LOAD a, LOAD b, CALL add, CALL putint, CALL puteol, RETURN
        // We'll just verify there's a RETURN instruction
        bool foundReturn = false;
        for (int i = funcStart; i < funcStart + 20; i++)
        {
            if (Machine.Code[i].Op == Machine.RETURNop)
            {
                foundReturn = true;
                // RETURN should have n=2 (number of parameters)
                Assert.Equal(2, Machine.Code[i].D);
                _testOutputHelper.WriteLine($"Found RETURN at instruction {i}");
                break;
            }
        }
        Assert.True(foundReturn, "Function should end with RETURN instruction");
    }
    
    [Fact]
    public void TestFunctionCall_GeneratesCorrectInstructions()
    {
        // Arrange: Create a program with a function declaration and call
        string source = @"
            create int x;
            sprout add(int a) {
                x = a;
            }
            add(10);
            vomit x;
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        _testOutputHelper.WriteLine("Generated TAM instructions:");
        for (int i = Machine.CB; i < Machine.CB + 30; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
        // Find the user function declaration
           var funcDecl = program.Block.Statements.OfType<SubRoutineDeclar>().FirstOrDefault();
           Assert.NotNull(funcDecl);
           Assert.NotNull(funcDecl.Address);

           // Find first user CALL (R != Machine.PBr indicates non-primitive)
           int callIdx = FindInstructionIndex(instr => instr.Op == Machine.CALLop && instr.R != Machine.PBr, Machine.CB, Machine.CB + 200);
           Assert.True(callIdx >= 0, "User CALL not found");

           // Argument should be loaded immediately before CALL (LOADL for literal 10)
           Assert.True(callIdx - 1 >= 0);
           Assert.Equal(Machine.LOADLop, Machine.Code[callIdx - 1].Op);
           Assert.Equal(10, Machine.Code[callIdx - 1].D);

           // CALL instruction fields:
           // - N field encodes the display register used (for top-level functions this is Machine.SBr)
           // - R should be Machine.CBr (code base) and D should be the function displacement
           Assert.Equal(Machine.CBr, Machine.Code[callIdx].R);
           Assert.Equal(funcDecl.Address.Displacement, Machine.Code[callIdx].D);

           // For top-level functions, display register is Machine.SBr
           Assert.Equal(Machine.SBr, Machine.Code[callIdx].N);

           // Because the call is used as a statement, a POP should follow to drop the dummy return value
           Assert.True(callIdx + 1 < Machine.PB, "No instruction following CALL to check for POP");
           Assert.Equal(Machine.POPop, Machine.Code[callIdx + 1].Op);
        string outputPath = Path.Combine(Path.GetTempPath(), "test_funDecl.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for function called saved to: {outputPath}");
    }
    
    private List<int> FindAllInstructionIndices(Func<dynamic, bool> predicate, int start = Machine.CB, int? end = null)
    {
        var list = new List<int>();
        int to = end ?? Machine.PB;
        for (int i = start; i < to; i++)
        {
            var instr = Machine.Code[i];
            if (instr != null && predicate(instr))
                list.Add(i);
        }
        return list;
    }
    private int FindInstructionIndex(Func<dynamic, bool> predicate, int start = Machine.CB, int? end = null)
    {
        int to = end ?? Machine.PB;
        for (int i = start; i < to; i++)
        {
            var instr = Machine.Code[i];
            if (instr != null && predicate(instr))
                return i;
        }
        return -1;
    }
    
    [Fact]
    public void TestFunctionCall_WithVariables_GeneratesCorrectInstructions()
    {
        // Arrange: Create a program with variables passed to function
        string source = @"
            sprout add(int a, int b) {
                vomit a + b;
            }
            
            create int x;
            create int y;
            x = 5;
            y = 10;
            add(x, y);
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        _testOutputHelper.WriteLine("Generated TAM instructions:");
        for (int i = Machine.CB; i < Machine.CB + 40; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
        // Find the CALL instruction
        bool foundCall = false;
        for (int i = Machine.CB; i < Machine.CB + 40; i++)
        {
            if (Machine.Code[i].Op == Machine.CALLop && Machine.Code[i].R != Machine.PBr)
            {
                foundCall = true;
                _testOutputHelper.WriteLine($"Found function CALL at instruction {i}");
                
                // Before the CALL, there should be LOAD instructions for the variables
                Assert.Equal(Machine.LOADop, Machine.Code[i - 2].Op);
                Assert.Equal(Machine.LOADop, Machine.Code[i - 1].Op);
                
                break;
            }
        }
        var outputPath = Path.Combine(Path.GetTempPath(), "test_funCallVars.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for function call with variables saved to: {outputPath}");
        Assert.True(foundCall, "Should find a CALL instruction for the function");
    }
    
    [Fact]
    public void TestFunctionWithNoParameters_GeneratesCorrectCode()
    {
        // Arrange: Create a function with no parameters
        string source = @"
            sprout sayHello() {
                vomit 42;
            }
       
            sayHello();
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        var funcDecl = program.Block.Statements
            .OfType<SubRoutineDeclar>()
            .FirstOrDefault();
        
        Assert.NotNull(funcDecl);
        Assert.Empty(funcDecl.Params);
        
        // Find RETURN instruction - should have n=0 (no parameters)
        int funcStart = funcDecl.Address!.Displacement;
        bool foundReturn = false;
        for (int i = funcStart; i < funcStart + 20; i++)
        {
            if (Machine.Code[i].Op == Machine.RETURNop)
            {
                foundReturn = true;
                Assert.Equal(1, Machine.Code[i].N); //there is a dummy 
                Assert.Equal(0, Machine.Code[i].D);
                break;
            }
        }
        var outputPath = Path.Combine(Path.GetTempPath(), "test_funNoParams.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for function with no parameters saved to: {outputPath}");
        
        Assert.True(foundReturn);
    }
    
    [Fact]
    public void TestNestedFunctionCalls_GeneratesCorrectCode()
    {
        // Arrange: One function calling another
        string source = @"
            sprout innerFunc(int x) {
                vomit x;
            }
            
            sprout outerFunc(int y) {
                innerFunc(y);
            }
            
            outerFunc(42);
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        var funcDecls = program.Block.Statements
            .OfType<SubRoutineDeclar>()
            .ToList();
        
        Assert.Equal(2, funcDecls.Count);
        Assert.NotNull(funcDecls[0].Address);
        Assert.NotNull(funcDecls[1].Address);
        
        var outputPath = Path.GetTempPath();
        encoder.SaveTargetProgram(Path.Combine(outputPath, "test_nestedFuncCalls.tam"));
        _testOutputHelper.WriteLine($"TAM code for nested function calls saved to: {outputPath}");
        
        _testOutputHelper.WriteLine($"innerFunc at displacement {funcDecls[0].Address.Displacement}");
        _testOutputHelper.WriteLine($"outerFunc at displacement {funcDecls[1].Address.Displacement}");
    }
    
    [Fact]
    public void TestFunctionWithLocalVariables_AllocatesCorrectSpace()
    {
        // Arrange: Function with local variables
        string source = @"
            sprout test(int param) {
                create int local;
                local = param ;
                local = local + 10;
                vomit local;
            }
            
            test(5);
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_funLocalVars.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for function with local variables saved to: {outputPath}");
        
        var funcDecl = program.Block.Statements
            .OfType<SubRoutineDeclar>()
            .FirstOrDefault();
        
        Assert.NotNull(funcDecl);
        int funcStart = funcDecl.Address!.Displacement;
        
        _testOutputHelper.WriteLine("Generated TAM instructions for function:");
        for (int i = funcStart; i < funcStart + 25; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
             // Verify parameter address (negative offset)
        var param = funcDecl.Params[0];
        Assert.NotNull(param.Address);
        Assert.Equal(1, param.Address.Level);
        Assert.Equal(-1, param.Address.Displacement);

        // Verify local variable address allocation
        var localVar = funcDecl.Body.Statements
            .OfType<VarDecl>()
            .FirstOrDefault();
        Assert.NotNull(localVar);
        Assert.NotNull(localVar.Address);
        Assert.Equal(1, localVar.Address.Level);
        Assert.Equal(3, localVar.Address.Displacement); // After link data (3 words)

        // Function body should start with JUMP over declarations
        Assert.Equal(Machine.JUMPop, Machine.Code[funcStart].Op);
        int jumpTarget = Machine.Code[funcStart].D;

        // Should have PUSH for local variable
        Assert.Equal(Machine.PUSHop, Machine.Code[jumpTarget].Op);
        Assert.Equal(1, Machine.Code[jumpTarget].D); // 1 word for local

        // Verify assignment: local = param
        // Should have LOAD param, then STORE local
        int currentInstr = jumpTarget + 1;
        Assert.Equal(Machine.LOADop, Machine.Code[currentInstr].Op);
        Assert.Equal(-1, Machine.Code[currentInstr].D); // Load param
        currentInstr++;
        Assert.Equal(Machine.STOREop, Machine.Code[currentInstr].Op);
        Assert.Equal(3, Machine.Code[currentInstr].D); // Store to local

        // Verify: local = local + 10
        // LOAD local, LOADL 10, CALL add, STORE local
        currentInstr++;
        Assert.Equal(Machine.LOADop, Machine.Code[currentInstr].Op);
        Assert.Equal(3, Machine.Code[currentInstr].D); // Load local
        currentInstr++;
        Assert.Equal(Machine.LOADLop, Machine.Code[currentInstr].Op);
        Assert.Equal(10, Machine.Code[currentInstr].D); // Load literal 10
        currentInstr++;
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.AddDisplacement, Machine.Code[currentInstr].D); // Add
        currentInstr++;
        Assert.Equal(Machine.STOREop, Machine.Code[currentInstr].Op);
        Assert.Equal(3, Machine.Code[currentInstr].D); // Store to local

        // Verify vomit local
        currentInstr++;
        Assert.Equal(Machine.LOADop, Machine.Code[currentInstr].Op);
        Assert.Equal(3, Machine.Code[currentInstr].D); // Load local
        currentInstr++;
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PutintDisplacement, Machine.Code[currentInstr].D);
        currentInstr++;
        Assert.Equal(Machine.CALLop, Machine.Code[currentInstr].Op);
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[currentInstr].D);

        // Verify RETURN instruction
        currentInstr++;
        Assert.Equal(Machine.LOADLop, Machine.Code[currentInstr].Op); // Dummy return value
        currentInstr++;
        Assert.Equal(Machine.RETURNop, Machine.Code[currentInstr].Op);
        Assert.Equal(1, Machine.Code[currentInstr].N); // Return 1 word
        Assert.Equal(1, Machine.Code[currentInstr].D); // Pop 1 parameter

        // Verify function call: test(5)
        int callIdx = FindInstructionIndex(instr => 
            instr.Op == Machine.CALLop && instr.R != Machine.PBr, 
            jumpTarget + 1, 
            Machine.CB + 50);
        
        Assert.True(callIdx > 0, "Function call not found");
        Assert.Equal(Machine.LOADLop, Machine.Code[callIdx - 1].Op);
        Assert.Equal(5, Machine.Code[callIdx - 1].D); // Argument value
        Assert.Equal(Machine.CBr, Machine.Code[callIdx].R);
        Assert.Equal(funcStart, Machine.Code[callIdx].D);
    }
    
    [Fact]
    public void TestMultipleFunctions_AllocatesDifferentAddresses()
    {
        // Arrange: Multiple function declarations
        string source = @"
            sprout func1(int a) {
                vomit a;
            }
            
            sprout func2(int b) {
                vomit b;
            }
            
            sprout func3(int c) {
                vomit c;
            }
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        var funcDecls = program.Block.Statements
            .OfType<SubRoutineDeclar>()
            .ToList();
        
        Assert.Equal(3, funcDecls.Count);
        
        // All functions should have different addresses
        var addresses = funcDecls.Select(f => f.Address!.Displacement).ToList();
        Assert.Equal(3, addresses.Distinct().Count());
        
        _testOutputHelper.WriteLine($"func1 at displacement {addresses[0]}");
        _testOutputHelper.WriteLine($"func2 at displacement {addresses[1]}");
        _testOutputHelper.WriteLine($"func3 at displacement {addresses[2]}");
        
        // Addresses should be in increasing order
        Assert.True(addresses[0] < addresses[1]);
        Assert.True(addresses[1] < addresses[2]);
    }
    
    [Fact]
    public void TestFunctionParameters_AllocateCorrectAddresses()
    {
        // Arrange: Function with multiple parameters
        string source = @"
            sprout calculate(int a, int b, int c) {
                vomit a + b + c;
            }
            
            calculate(1, 2, 3);
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        _testOutputHelper.WriteLine("Generated TAM instructions:");
        for (int i = Machine.CB; i < Machine.CB + 30; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }

        // Find the function declaration
        var funcDecl = program.Block.Statements.OfType<SubRoutineDeclar>().FirstOrDefault();
        Assert.NotNull(funcDecl);
        Assert.NotNull(funcDecl.Address);

        // Verify parameter address allocation (negative offsets in TAM)
        Assert.Equal(3, funcDecl.Params.Count);

        var paramA = funcDecl.Params[0];
        var paramB = funcDecl.Params[1];
        var paramC = funcDecl.Params[2];

        Assert.NotNull(paramA.Address);
        Assert.NotNull(paramB.Address);
        Assert.NotNull(paramC.Address);

        // All parameters at level 1 (inside function)
        Assert.Equal(1, paramA.Address.Level);
        Assert.Equal(1, paramB.Address.Level);
        Assert.Equal(1, paramC.Address.Level);

        // Parameters have negative displacements: -3, -2, -1
        Assert.Equal(-3, paramA.Address.Displacement);
        Assert.Equal(-2, paramB.Address.Displacement);
        Assert.Equal(-1, paramC.Address.Displacement);

        _testOutputHelper.WriteLine($"Parameter 'a' at Level={paramA.Address.Level}, Displacement={paramA.Address.Displacement}");
        _testOutputHelper.WriteLine($"Parameter 'b' at Level={paramB.Address.Level}, Displacement={paramB.Address.Displacement}");
        _testOutputHelper.WriteLine($"Parameter 'c' at Level={paramC.Address.Level}, Displacement={paramC.Address.Displacement}");
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_funParamAddrs.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for function parameters saved to: {outputPath}");
    }
    
    [Fact]
    public void TestFunctionWithMixedTypes_EncodesCorrectly()
    {
        // Arrange: Function with different parameter types
        string source = @"
            sprout mixed(int a, char b, bool c) {
                vomit a;
                vomit b;
                vomit c;
            }
            
            mixed(42, 'x', true);
        ";
        
        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        
        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        // Verify arguments are loaded correctly
        _testOutputHelper.WriteLine("Generated TAM instructions:");
        for (int i = Machine.CB; i < Machine.CB + 40; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
        // Find the function declaration
        var funcDecl = program.Block.Statements.OfType<SubRoutineDeclar>().FirstOrDefault();
        Assert.NotNull(funcDecl);
        Assert.NotNull(funcDecl.Address);

        // Verify parameter address allocation (negative offsets in TAM)
        Assert.Equal(3, funcDecl.Params.Count);
        
        var paramA = funcDecl.Params[0];
        var paramB = funcDecl.Params[1];
        var paramC = funcDecl.Params[2];
        
        Assert.NotNull(paramA.Address);
        Assert.NotNull(paramB.Address);
        Assert.NotNull(paramC.Address);
        
        Assert.Equal(1, paramA.Address.Level);
        Assert.Equal(-3, paramA.Address.Displacement);
        
        Assert.Equal(1, paramB.Address.Level);
        Assert.Equal(-2, paramB.Address.Displacement);
        
        Assert.Equal(1, paramC.Address.Level);
        Assert.Equal(-1, paramC.Address.Displacement);

        int funcStart = funcDecl.Address.Displacement;

        // First vomit: int parameter 'a' (displacement -3)
        int loadA = FindInstructionIndex(instr =>
            instr.Op == Machine.LOADop &&
            instr.R == Machine.LBr &&
            instr.D == -3,
            funcStart, funcStart + 30);
        Assert.Equal(Machine.CALLop, Machine.Code[loadA + 1].Op);
        Assert.Equal(Machine.PutintDisplacement, Machine.Code[loadA + 1].D);
        Assert.Equal(Machine.CALLop, Machine.Code[loadA + 2].Op);
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[loadA + 2].D);

        // Second vomit: char parameter 'b' (displacement -2)
        int loadB = FindInstructionIndex(instr =>
            instr.Op == Machine.LOADop &&
            instr.R == Machine.LBr &&
            instr.D == -2,
            funcStart, funcStart + 30);
        Assert.Equal(Machine.CALLop, Machine.Code[loadB + 1].Op);
        Assert.Equal(Machine.PutDisplacement, Machine.Code[loadB + 1].D);
        Assert.Equal(Machine.CALLop, Machine.Code[loadB + 2].Op);
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[loadB + 2].D);

        // Third vomit: bool parameter 'c' (displacement -1)
        int loadC = FindInstructionIndex(instr =>
            instr.Op == Machine.LOADop &&
            instr.R == Machine.LBr &&
            instr.D == -1,
            funcStart, funcStart + 30);
        Assert.Equal(Machine.CALLop, Machine.Code[loadC + 1].Op);
        Assert.Equal(Machine.PutintDisplacement, Machine.Code[loadC + 1].D);
        Assert.Equal(Machine.CALLop, Machine.Code[loadC + 2].Op);
        Assert.Equal(Machine.PuteolDisplacement, Machine.Code[loadC + 2].D);
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_funMixedTypes.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for function with mixed types saved to : {outputPath}");
        // Should find three LOADL instructions for the arguments
        
    }

    [Fact]
    public void TestLoopStatement_GeneratesCorrectInstructions()
    {
        // Arrange: simple while loop
        string source = @"
        create int x;
        x = 0;
        repeat 5 times {
            x = x + 1;
            vomit x;
        }";
        
        // Act: encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        _testOutputHelper.WriteLine("Generated TAM instructions for repeat times:");
        for (int i = Machine.CB; i < Machine.CB + 30; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
        // Find the repeat times loop structure
        // Should have: LOADL 5 (counter), then loop with LOAD to check counter, compare, JUMPIF(0) to exit
        
        // Find LOADL 5 for the counter initialization
        int loadCounterIdx = FindInstructionIndex(instr => 
            instr.Op == Machine.LOADLop && instr.D == 5, 
            Machine.CB, Machine.CB + 30);
        Assert.True(loadCounterIdx > 0, "Counter initialization (LOADL 5) not found");
        
        // After counter init, should have loop start with LOAD to check counter
        int loopStartIdx = loadCounterIdx + 1;
        Assert.Equal(Machine.LOADop, Machine.Code[loopStartIdx].Op);
        Assert.Equal(Machine.STr, Machine.Code[loopStartIdx].R);
        Assert.Equal(-1, Machine.Code[loopStartIdx].D); // Load counter from stack
        
        // Should have comparison with 0
        Assert.Equal(Machine.LOADLop, Machine.Code[loopStartIdx + 1].Op);
        Assert.Equal(0, Machine.Code[loopStartIdx + 1].D);
        
        // Should have CALL gt (greater than)
        Assert.Equal(Machine.CALLop, Machine.Code[loopStartIdx + 2].Op);
        Assert.Equal(Machine.GtDisplacement, Machine.Code[loopStartIdx + 2].D);
        
        // Should have JUMPIF(0) to exit loop
        int jumpIfIdx = loopStartIdx + 3;
        Assert.Equal(Machine.JUMPIFop, Machine.Code[jumpIfIdx].Op);
        Assert.Equal(0, Machine.Code[jumpIfIdx].N); // Jump if false
        
        // Should have decrement logic: LOAD counter, LOADL 1, CALL sub, STORE counter
        var decrementIndices = FindAllInstructionIndices(instr => 
            instr.Op == Machine.CALLop && instr.D == Machine.SubDisplacement,
            loopStartIdx, loopStartIdx + 30);
        Assert.NotEmpty(decrementIndices);
        
        // After decrement, should have JUMP back to loop start
        int jumpBackIdx = FindInstructionIndex(instr => 
            instr.Op == Machine.JUMPop && instr.D == loopStartIdx,
            loopStartIdx, loopStartIdx + 30);
        Assert.True(jumpBackIdx > 0, "JUMP back to loop start not found");
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_repeatTimes.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for repeat times loop saved to: {outputPath}");
    }
    
    [Fact]
    public void TestRepeatUntilLoop_GeneratesCorrectInstructions()
    {
        // Arrange: repeat until loop
        string source = @"
        create int x;
        x = 0;
        repeat until (x == 3) {
            x = x + 1;
        }
        vomit x;
       ";
        
        // Act: encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        _testOutputHelper.WriteLine("Generated TAM instructions for repeat until:");
        for (int i = Machine.CB; i < Machine.CB + 30; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_repeatUntil.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for repeat until loop saved to: {outputPath}");

        
        // Find the repeat until loop structure
        // Should have: loop body first, then condition check, then JUMPIF(0) back to loop start
        
        // Find the assignment x = 0
        int assignZeroIdx = FindInstructionIndex(instr => 
            instr.Op == Machine.LOADLop && instr.D == 0,
            Machine.CB, Machine.CB + 10);
        Assert.True(assignZeroIdx > 0, "Initial assignment not found");
        
        // Loop should start after the initial setup
        // Find the loop body - should have increment (x = x + 1)
        // This will have: LOAD x, LOADL 1, CALL add, STORE x
        
        var addIndices = FindAllInstructionIndices(instr => 
            instr.Op == Machine.CALLop && instr.D == Machine.AddDisplacement,
            Machine.CB, Machine.CB + 30);
        Assert.NotEmpty(addIndices);
        int loopBodyIdx = addIndices[0]; // First add is in the loop
        
        // After loop body, should have condition check: x == 3
        // This will be: LOAD x, LOADL 3, CALL eq
        var eqIndices = FindAllInstructionIndices(instr => 
            instr.Op == Machine.CALLop && instr.D == Machine.EqDisplacement,
            loopBodyIdx, loopBodyIdx + 20);
        Assert.NotEmpty(eqIndices);
        int conditionCheckIdx = eqIndices[0];
        
        // After condition check, should have JUMPIF(0) back to loop start
        int jumpBackIdx = conditionCheckIdx + 1;
        Assert.Equal(Machine.JUMPIFop, Machine.Code[jumpBackIdx].Op);
        Assert.Equal(0, Machine.Code[jumpBackIdx].N); // Jump if false (condition not met)
        
        // The jump should go back to before the loop body
        Assert.True(Machine.Code[jumpBackIdx].D < jumpBackIdx, "JUMPIF should jump backwards");
        
    }
    
    [Fact]
    public void TestRepeatTimesWithVariable_GeneratesCorrectCode()
    {
        // Arrange: repeat times with variable count
        string source = @"
        create int count;
        create int sum;
        count = 3;
        sum = 0;
        repeat count times {
            sum = sum + 10;
        }
        vomit sum;";
        
        // Act: encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        _testOutputHelper.WriteLine("Generated TAM instructions for repeat times with variable:");
        for (int i = Machine.CB; i < Machine.CB + 35; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }
        
        // Should have LOAD count (not LOADL) to get the loop counter
        var loadIndices = FindAllInstructionIndices(instr => 
            instr.Op == Machine.LOADop && instr.R == Machine.SBr && instr.D == 0,
            Machine.CB, Machine.CB + 35);
        Assert.NotEmpty(loadIndices);
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_repeatTimesVariable.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for repeat times with variable saved to: {outputPath}");
    }

    [Fact]
    public void TestListenCarefully_Char_GeneratesCorrectInstructions()
    {
        // Arrange: repeat times with variable count
        string source = @"
        create char count;
        listenCarefully count;
        vomit count;";
        
        // Act: encode the program
        var (encoder, program, errors) = EncodeProgram(source);
        Assert.Empty(errors);
        Assert.NotNull(encoder);
        
        
        _testOutputHelper.WriteLine("Generated TAM instructions for listenCarefully (char):");
        for (int i = Machine.CB; i < Machine.CB + 15; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }

        // Find the variable declaration
        var varDecl = program.Block.Statements
            .OfType<VarDecl>()
            .FirstOrDefault();

        Assert.NotNull(varDecl);
        Assert.NotNull(varDecl.Address);

        // Find LOADA instruction (load address for input)
        int loadaIdx = FindInstructionIndex(instr =>
                instr.Op == Machine.LOADAop && instr.D == varDecl.Address.Displacement,
            Machine.CB, Machine.CB + 15);
        Assert.True(loadaIdx > 0, "LOADA instruction not found");

        // Should have CALL get (for char input)
        Assert.Equal(Machine.CALLop, Machine.Code[loadaIdx + 1].Op);
        Assert.Equal(Machine.GetDisplacement, Machine.Code[loadaIdx + 1].D);
        Assert.Equal(1, Machine.Code[loadaIdx + 1].N);
        
        var outputPath = Path.Combine(Path.GetTempPath(), "test_listenCarefully_char.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for listenCarefully char saved to: {outputPath}");
    }
    
    [Fact]
    public void TestListenCarefully_Int_GeneratesCorrectInstructions()
    {
        // Arrange: Create a program with int input
        string source = @"
        create int num;
        listenCarefully num;
        vomit num;";

        // Act: Encode the program
        var (encoder, program, errors) = EncodeProgram(source);

        // Assert: No errors
        Assert.Empty(errors);
        Assert.NotNull(encoder);

        _testOutputHelper.WriteLine("Generated TAM instructions for listenCarefully (int):");
        for (int i = Machine.CB; i < Machine.CB + 15; i++)
        {
            var instr = Machine.Code[i];
            if (instr.Op != 0 || instr.D != 0 || instr.N != 0 || instr.R != 0)
            {
                _testOutputHelper.WriteLine($"{i}: Op={instr.Op}, N={instr.N}, R={instr.R}, D={instr.D}");
            }
        }

        // Find the variable declaration
        var varDecl = program.Block.Statements
            .OfType<VarDecl>()
            .FirstOrDefault();

        Assert.NotNull(varDecl);
        Assert.NotNull(varDecl.Address);

        // Find LOADA instruction (load address for input)
        int loadaIdx = FindInstructionIndex(instr =>
                instr.Op == Machine.LOADAop && instr.D == varDecl.Address.Displacement,
            Machine.CB, Machine.CB + 15);
        Assert.True(loadaIdx > 0, "LOADA instruction not found");

        // Should have CALL getint (for int input)
        Assert.Equal(Machine.CALLop, Machine.Code[loadaIdx + 1].Op);
        Assert.Equal(Machine.GetintDisplacement, Machine.Code[loadaIdx + 1].D);
        Assert.Equal(1, Machine.Code[loadaIdx + 1].N); // 1 word for result

        var outputPath = Path.Combine(Path.GetTempPath(), "test_listenCarefully_int.tam");
        encoder.SaveTargetProgram(outputPath);
        _testOutputHelper.WriteLine($"TAM code for listenCarefully (int) saved to: {outputPath}");
    }
    
    
}


