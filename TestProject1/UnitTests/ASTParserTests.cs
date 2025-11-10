using SproutLang.AST;
using SproutLang.Parser;
using SproutLang.Scanner;
using SproutLang.Tools;
using Xunit.Abstractions;

namespace TestProject1.UnitTests;

public class ASTParserTests
{

    private readonly ITestOutputHelper _testOutputHelper;
    public ASTParserTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private SproutLang.AST.Program AssertParses(string code)
    {
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, code);
        try
        {
            var sourceFile = new SourceFile(tempFile);
            var scanner = new Scanner(sourceFile);
            var parser = new ASTParser(scanner);
            return parser.ParseProgram();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
        public void Parse_VarDecl_ShouldBuildVarDeclNode()
        {
            string code = "create int x;";
            var program = AssertParses(code);

            Assert.Single(program.Block.Statements);
            var decl = Assert.IsType<VarDecl>(program.Block.Statements[0]);
            Assert.Equal("x", decl.Name.Spelling);
            Assert.IsType<SimpleType>(decl.Type);
            Assert.Equal(BaseType.Int, ((SimpleType)decl.Type).Kind);
        }

        [Fact]
        public void Parse_Assignment_ShouldBuildVarAssignment()
        {
            string code = "x = 42;";
            var program = AssertParses(code);

            var stmt = Assert.IsType<VarAssignment>(program.Block.Statements[0]);
            Assert.Equal("x", stmt.Name.Spelling);

            var literal = Assert.IsType<IntLiteralExpression>(stmt.Expr);
            Assert.Equal(42, literal.Literal.Value);
        }

        [Fact]
        public void Parse_IfStatement_ShouldContainConditionAndBlocks()
        {
            string code = "si (x > 0) { vomit x; } sino { vomit 0; }";
            var program = AssertParses(code);

            var ifStmt = Assert.IsType<IfStatement>(program.Block.Statements[0]);

            Assert.IsType<BinaryExpr>(ifStmt.First.Condition);
            Assert.NotNull(ifStmt.First.Block);

            Assert.NotNull(ifStmt.ElseBlock);
        }

        [Fact]
        public void Parse_RepeatTimes_ShouldContainTimesAndBody()
        {
            string code = "repeat 3 times { vomit 1; }";
            var program = AssertParses(code);

            var loop = Assert.IsType<RepeatTimes>(program.Block.Statements[0]);
            Assert.Equal(3, loop.Times.Literal.Value);
            Assert.Single(loop.Body.Statements);
        }

        [Fact]
        public void Parse_RepeatUntil_ShouldContainConditionAndBody()
        {
            string code = "repeat until (x < 10) { x = x + 1; }";
            var program = AssertParses(code);

            var loop = Assert.IsType<RepeatUntil>(program.Block.Statements[0]);
            Assert.IsType<BinaryExpr>(loop.Condition);
            Assert.Single(loop.Body.Statements);
        }

        [Fact]
        public void Parse_Vomit_ShouldContainExpression()
        {
            string code = "vomit x;";
            var program = AssertParses(code);

            var stmt = Assert.IsType<VomitStatement>(program.Block.Statements[0]);
            Assert.IsType<VarExpression>(stmt.Expression);
        }

        [Fact]
        public void Parse_Listen_ShouldContainIdentifier()
        {
            string code = "listenCarefully x;";
            var program = AssertParses(code);

            var stmt = Assert.IsType<ListenStatement>(program.Block.Statements[0]);
            Assert.Equal("x", stmt.Identifier.Spelling);
        }

        [Fact]
        public void Parse_Subroutine_ShouldContainParametersAndBody()
        {
            string code = "sprout foo(int x, char y) { vomit x; }";
            var program = AssertParses(code);

            var sub = Assert.IsType<SubRoutineDeclar>(program.Block.Statements[0]);
            Assert.Equal("foo", sub.Name.Spelling);
            Assert.Equal(2, sub.Params.Count);
            Assert.Single(sub.Body.Statements);
        }

        [Fact]
        public void Parse_FunctionCall_ShouldBuildCallStatement()
        {
            string code = "bloom print(x, 1);";
            var program = AssertParses(code);

            var call = Assert.IsType<CallStatement>(program.Block.Statements[0]);
            Assert.Equal("print", call.Call.Callee.Spelling);
            Assert.Equal(2, call.Call.Arguments.Arguments.Count);
        }
        
        [Fact]
        public void ASTPrinter_ShouldPrintSimpleVarDecl()
        {
            string code = "create int x; si (x > 0) { vomit x; } sino { vomit 0; }";
            var program = AssertParses(code);
            
            using var sw = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(sw);

            ASTPrinter.Print(program.Block);

            Console.SetOut(originalOut);
            string output = sw.ToString();
            _testOutputHelper.WriteLine(output);

            Assert.Contains("Block", output);
            Assert.Contains("VarDecl x", output);
            Assert.Contains("SimpleType Int", output);
        }
        
        [Fact]
        public void BinaryExpr_ShouldHaveCorrectStructure()
        {
            string code = "create bool result; result = 5==7;";
            
            var program = AssertParses(code);
            
            Assert.IsType<VarAssignment>(program.Block.Statements[1]);
        }
        
        [Fact]
        public void BinaryExpressionWithIncorrectLType_ShouldThrow()
        {
            string code = @"
                create int a;
                create bool b;
                5 = a;
            ";
            
            var exception = Assert.Throws<ParserException>(() => AssertParses(code));
            Assert.Contains("Expected Identifier, but found IntLiteral", exception.Message);
        }
            
        [Fact]
        public void Declare_And_Assign_Char_Variable()
        {
            string code = @"
                create char letter;
                letter = 'A';
            ";
            
            var program = AssertParses(code);
            
            var decl = Assert.IsType<VarDecl>(program.Block.Statements[0]);
            Assert.Equal("letter", decl.Name.Spelling);
            Assert.IsType<SimpleType>(decl.Type);
            Assert.Equal(BaseType.Char, ((SimpleType)decl.Type).Kind);
            
            var assignment = Assert.IsType<VarAssignment>(program.Block.Statements[1]);
            Assert.Equal("letter", assignment.Name.Spelling);
            
            var charLiteralExpr = Assert.IsType<CharLiteralExpression>(assignment.Expr);
            Assert.Equal('A', charLiteralExpr.Literal.Value);
        }
        
        [Fact]
        public void Declare_And_Assign_Bool_Variable()
        {
            string code = @"
                create bool isActive;
                isActive = true;
            ";
            
            var program = AssertParses(code);
            
            var decl = Assert.IsType<VarDecl>(program.Block.Statements[0]);
            Assert.Equal("isActive", decl.Name.Spelling);
            Assert.IsType<SimpleType>(decl.Type);
            Assert.Equal(BaseType.Bool, ((SimpleType)decl.Type).Kind);
            
            var assignment = Assert.IsType<VarAssignment>(program.Block.Statements[1]);
            Assert.Equal("isActive", assignment.Name.Spelling);
            
            var boolLiteralExpr = Assert.IsType<BoolLiteralExpression>(assignment.Expr);
            Assert.True(boolLiteralExpr.Literal.Value);
        }
    
        [Fact]
        public void ArrayAssignment()   
        {
            string code = "arr[2] = 10;";
            var program = AssertParses(code);

            var stmt = Assert.IsType<ArrayAssignment>(program.Block.Statements[0]);
            Assert.Equal("arr", stmt.Name.Spelling);

            var indexLiteral = Assert.IsType<IntLiteralExpression>(stmt.Index);
            Assert.Equal(2, indexLiteral.Literal.Value);

            var valueLiteral = Assert.IsType<IntLiteralExpression>(stmt.Expr);
            Assert.Equal(10, valueLiteral.Literal.Value);
        }
        
        [Fact]
        public void Parse_ArrayExpression_AsRhsOfAssignment_ShouldBuildArrayExpression()
        {
            string code = @"
                          create int x;
                          x = arr[2];
                          ";
            var program = AssertParses(code);

            var assign = Assert.IsType<VarAssignment>(program.Block.Statements[1]);
            var arrExpr = Assert.IsType<ArrayExpression>(assign.Expr);

            Assert.Equal("arr", arrExpr.Name.Spelling);
            Assert.Equal(2, arrExpr.Index.Literal.Value);
        }
        

        [Fact]
        public void Parse_ArrayExpression_WithNonIntIndex_ShouldThrow()
        {
            // Adjust the expected message if your parser emits a different one.
            string code = "vomit arr[true];";
            var ex = Assert.Throws<ParserException>(() => AssertParses(code));
            Assert.Contains("Expected IntLiteral", ex.Message);
        }

        [Fact]
        public void Parse_ArrayExpression_MissingClosingBracket_ShouldThrow()
        {
            string code = "vomit arr[3;";
            var ex = Assert.Throws<ParserException>(() => AssertParses(code));
            Assert.Contains("Expected RightBracket", ex.Message);
        }
}
