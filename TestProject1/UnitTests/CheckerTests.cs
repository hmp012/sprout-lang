using Microsoft.Extensions.Logging;
using SproutLang.Checker;
using SproutLang.Parser;
using SproutLang.Scanner;
using Xunit;

namespace TestProject1.UnitTests;

public class CheckerTests
{
    // A simple logger implementation for testing that captures error messages.
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

    // Helper method to run the checker on a code snippet and return errors.
    private List<string> CheckCode(string source)
    {

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, source);
        try
        {
            var logger = new TestLogger();
            var sourceFile = new SourceFile(tempFile);
            var scanner = new Scanner(sourceFile);
            var parser = new ASTParser(scanner);
            var program = parser.ParseProgram();
            if (logger.ErrorMessages.Any())
            {
                // Stop if there are parsing errors, as the AST might be invalid.
                return logger.ErrorMessages;
            }

            var checker = new Checker(logger);
            checker.Check(program);
            return logger.ErrorMessages;
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

    }

    [Fact]
    public void Check_ValidProgram_ShouldProduceNoErrors()
    {
        // Arrange
        var code = @"
            create int x;
            x = 10;
        ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Check_AssignmentToLiteral_ShouldLogError()
    {
        // Arrange
        var code = @"
            create int x;
            vomit(1 = x);
        ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Left operand of assignment must be an l-value.", errors[0]);
    }

    [Fact]
    public void Check_UndeclaredVariable_ShouldLogError()
    {
        // Arrange
        var code = "x = 10;";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Contains("Identifier 'x' is not declared in the current scope", errors[0]);
        Assert.Contains("Variable 'x' not declared", errors[1]);
    }

    [Fact]
    public void Check_AssignmentToExpression_ShouldLogError()
    {
        // Arrange
        var code = @"
            create int x;
            create int y;
            vomit((x + y) = 5);
        ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Left operand of assignment must be an l-value.", errors[0]);
    }

    [Fact]
    public void Check_DuplicateVariableDeclaration_ShouldLogError()
    {
        // Arrange
        var code = @"
            create int x;
            create int x;
        ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Identifier 'x' is already declared in the current scope", errors[0]);
    }

    [Fact]
    public void Check_VariableUsedBeforeDeclaration_ShouldLogError()
    {
        // Arrange
        var code = @"
            vomit x;
            create int x;
        ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Contains("Identifier 'x' is not declared in the current scope", errors[0]);
        Assert.Contains("Variable 'x' not declared", errors[1]);
    }   

    [Fact]
    public void Check_Same_VariableNameInDifferentScopes_ShouldProduceNoErrors()
    {
        // Arrange
        var code = @"
           create int x;
           si (x > 0) {
               create int x;
               x = 5;
           }
        ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Check_SubroutineParameterDeclaration_ShouldProduceNoErrors()
    {
        // Arrange
        var code = @"
            sprout foo(int a, char b) {
                vomit a;
                vomit b;
            }
        ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void Check_SubroutineParameterNameConflict_ShouldLogError()
    {
        // Arrange
        var code = @"
            sprout foo(int a, char a) {
                vomit a;
               }
         ";

        // Act
        var errors = CheckCode(code);

        // Assert
        Assert.Single(errors);
        Assert.Contains("Identifier 'a' is already declared in the current scope", errors[0]);
    }

    [Fact]
    public void Check_BinnaryExpressionWithCorrect_LeftType_ShouldBeCorrect()
    {
        var code =
            @"
            create int a;
            create int b;
            a = b + 2;
            ";
        var errors = CheckCode(code);
        Assert.Empty(errors);
    }
    
    [Fact]
    public void Check_BinaryExpressionWithIncompatibleTypes_ShouldLogError()
    {
        var code =
            @"
            create int a;
            create char b;
            a = 'l';
            ";
        var errors = CheckCode(code);
        Assert.Single(errors);
        Assert.Contains("Type mismatch in assignment to 'a'.", errors[0]);
    }
    
    [Fact]
    public void Check_UnaryExpressionWitValidOperator_ShouldNotLogError()
    {
        var code = @"
        create int x;
        create int y;
        x = 5;
        y = -x;
    ";
        var errors = CheckCode(code);
        Assert.Empty(errors);
    }
    
    [Fact]
    public void Check_UnaryExpressionWithInvalidOperator_ShouldLogError()
    {
        var code = @"
        create bool x;
        create int y;
        x = true;
        y = -x;";
        
        var errors = CheckCode(code);
        Assert.Single(errors);
        Assert.Contains("Type mismatch in assignment to 'y'. Expected Int, got Bool.", errors[0]);

    }
    
    [Fact]
    public void Check_BinaryExpressionDifferentOperators_ShouldNotLogError()
    {
        var code = @"
        create int a;
        create int b;
        create int result;
        create bool boolResult;
        
        a = 10;
        b = 5;
        
        result = a + b; 
        result = a - b; 
        result = a * b; 
        result = a / b; 
        
        boolResult = a == b;  
        boolResult = a != b;  
        boolResult = a < b;   
        boolResult = a > b;   
        
        boolResult = (a > 0) && (b > 0); 
        boolResult = (a > 0) || (b > 0);  
        ";
        var errors = CheckCode(code);
        Assert.Empty(errors);
    }

    [Fact]
    public void Check_BinaryExpressionWithIncompatibleOperands_ShouldLogError()
    {
        var code = @"
        create int a;
        create bool b;
        create int result;
        create bool boolResult;
        
        a = 10;
        b = true;
        
        result = a + b;
        result = a - b;
        result = a * b;
        result = a / b;
        
        boolResult = a == b;
        boolResult = a != b;
        
        boolResult = a && b;
        boolResult = a || b;
        ";
        var errors = CheckCode(code);
        Assert.NotEmpty(errors);
        Assert.True(errors.Count > 0);
    }

    [Fact]
    public void Check_ArithmeticOperatorWithBoolOperands_ShouldLogError()
    {
        var code = @"
        create bool a;
        create bool b;
        create int result;
        
        a = true;
        b = false;
        result = a + b;
        ";
        var errors = CheckCode(code);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Type mismatch") || e.Contains("type"));
    }

    [Fact]
    public void Check_LogicalOperatorWithIntOperands_ShouldLogError()
    {
        var code = @"
        create int a;
        create int b;
        create bool result;
        
        a = 5;
        b = 10;
        result = a && b;
        ";
        var errors = CheckCode(code);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Type mismatch") || e.Contains("type"));
    }

    [Fact]
    public void Check_ComparisonBetweenIntAndChar_ShouldLogError()
    {
        var code = @"
        create int a;
        create char b;
        create bool result;
        
        a = 10;
        b = 'x';
        result = a == b;
        ";
        var errors = CheckCode(code);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("Type mismatch") || e.Contains("type"));
    }
    

     [Fact]
     public void Check_ArrayDeclarationAndUsage_ShouldProduceNoErrors()
     {
         // Arrange
         var code = @"
             create [int, 10] arr;
             arr[0] = 5;
             vomit arr[0];
         ";

         // Act
         var errors = CheckCode(code);

         // Assert
         Assert.Empty(errors);
     }
     
     [Fact]
     public void Check_ArrayDeclarationAndUsage_ShouldProduceErrors()
     {
         // Arrange
         var code = @"
             create [int, 10] arr;
             arr[0] = 'A';
         ";

         // Act
         var errors = CheckCode(code);

         // Assert
         Assert.Single(errors);
     }
     
     [Fact]
     public void Check_ArrayAssignmentSizes_ShouldProduceErrors()
     {
         // Arrange
         var code = @"
             create [int, 10] arr;
             arr[11] = 5;
         ";

         // Act
         var errors = CheckCode(code);

         // Assert
         Assert.Single(errors);
     }
     
}
