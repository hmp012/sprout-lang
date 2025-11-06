/*
 * SproutLang TAM Encoder
 * 
 * Code generator for the Triangle Abstract Machine (TAM)
 * Based on the Triangle compiler encoder pattern
 */

using SproutLang.AST;

namespace SproutLang.TAM;

public class Encoder : IAstVisitor
{
    private int _nextAdr = Machine.CB;
    private int _currentLevel;

    /// <summary>
    /// Emit a TAM instruction to the code store
    /// </summary>
    private void Emit(int op, int n, int r, int d)
    {
        if (n > 255)
        {
            Console.WriteLine("Operand too long");
            n = 255;
        }

        var instr = new Instruction
        {
            Op = op,
            N = n,
            R = r,
            D = d
        };

        if (_nextAdr >= Machine.PB)
            Console.WriteLine("Program too large");
        else
            Machine.Code[_nextAdr++] = instr;
    }

    /// <summary>
    /// Patch a previously emitted instruction's displacement field
    /// Used for backpatching jumps
    /// </summary>
    private void Patch(int adr, int d)
    {
        Machine.Code[adr].D = d;
    }

    /// <summary>
    /// Calculate the appropriate display register for accessing a variable
    /// based on the current scope level and the entity's declaration level
    /// </summary>
    private int DisplayRegister(int currentLevel, int entityLevel)
    {
        if (entityLevel == 0)
            return Machine.SBr;
        else if (currentLevel - entityLevel <= 6)
            return Machine.LBr + currentLevel - entityLevel;
        else
        {
            Console.WriteLine("Accessing across too many levels");
            return Machine.L6r;
        }
    }

    /// <summary>
    /// Save the generated TAM program to a binary file
    /// </summary>
    public void SaveTargetProgram(string fileName)
    {
        try
        {
            using var output = new BinaryWriter(File.Open(fileName, FileMode.Create));
            
            for (int i = Machine.CB; i < _nextAdr; i++)
                Machine.Code[i].Write(output);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Trouble writing {fileName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Main entry point to encode a program
    /// </summary>
    public void Encode(AST.Program p)
    {
        p.Visit(this, null);
    }

    public object VisitProgram(AST.Program program, object? arg)
    {
        _currentLevel = 0;

        program.Block.Visit(this, new Address());

        Emit(Machine.HALTop, 0, 0, 0);

        return null!;
    }

    public object? VisitBlock(Block block, object? arg)
    {
        // TODO: Handle declarations when added
        // For now, just process statements
        
        block.Statements.ForEach(statement => statement.Visit(this, null));

        return 0; // size of declarations
    }

    public object VisitArgList(ArgList argList, object? arg)
    {
        // Evaluate each argument expression and push onto stack
        foreach (var expr in argList.Arguments)
            expr.Visit(this, true); // true = value needed

        return null!;
    }

    public object? VisitStatement(Statement statement, object? arg)
    {
        // Generic statement - shouldn't be called directly
        return null;
    }

    public object? VisitIfStatement(IfStatement ifStatement, object? arg)
    {
        // Evaluate condition of first branch
        ifStatement.First.Condition.Visit(this, true);

        // Jump to else part if condition is false (0)
        int jump1Adr = _nextAdr;
        Emit(Machine.JUMPIFop, 0, Machine.CBr, 0);

        // Then branch
        ifStatement.First.Block.Visit(this, null);

        // Jump over else part
        int jump2Adr = _nextAdr;
        Emit(Machine.JUMPop, 0, Machine.CBr, 0);

        // Patch first jump to point here (else part)
        Patch(jump1Adr, _nextAdr);

        // Handle else-if branches
        foreach (var elseIfBranch in ifStatement.ElseIfBranches)
        {
            elseIfBranch.Condition.Visit(this, true);
            int elseIfJump = _nextAdr;
            Emit(Machine.JUMPIFop, 0, Machine.CBr, 0);
            elseIfBranch.Block.Visit(this, null);
            int skipJump = _nextAdr;
            Emit(Machine.JUMPop, 0, Machine.CBr, 0);
            Patch(elseIfJump, _nextAdr);
        }

        // Else block (if exists)
        if (ifStatement.ElseBlock != null)
            ifStatement.ElseBlock.Visit(this, null);

        // Patch second jump to point here (after if statement)
        Patch(jump2Adr, _nextAdr);

        return null;
    }

    public object? VisitIfBranch(IfBranch ifBranch, object? arg)
    {
        ifBranch.Block.Visit(this, arg);
        return null;
    }

    public object? VisitRepeatTimes(RepeatTimes repeatTimes, object? arg)
    {
        // Evaluate the count expression
        repeatTimes.Times.Visit(this, true);

        // Start of loop
        int startAdr = _nextAdr;

        // Duplicate counter on stack
        Emit(Machine.LOADop, 1, Machine.STr, -1);

        // Check if counter > 0
        Emit(Machine.LOADLop, 1, 0, 0);
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.GtDisplacement);

        // Jump out of loop if counter <= 0
        int jumpAdr = _nextAdr;
        Emit(Machine.JUMPIFop, 0, Machine.CBr, 0);

        // Execute loop body
        repeatTimes.Body.Visit(this, null);

        // Decrement counter
        Emit(Machine.LOADop, 1, Machine.STr, -1);
        Emit(Machine.LOADLop, 1, 0, 1);
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.SubDisplacement);
        Emit(Machine.STOREop, 1, Machine.STr, -1);

        // Jump back to start
        Emit(Machine.JUMPop, 0, Machine.CBr, startAdr);

        // Patch exit jump
        Patch(jumpAdr, _nextAdr);

        // Pop counter off stack
        Emit(Machine.POPop, 0, 0, 1);

        return null;
    }

    public object? VisitRepeatUntil(RepeatUntil repeatUntil, object? arg)
    {
        // Start of loop
        int startAdr = _nextAdr;

        // Execute loop body
        repeatUntil.Body.Visit(this, null);

        // Evaluate condition
        repeatUntil.Condition.Visit(this, true);

        // Jump back if condition is false (0)
        Emit(Machine.JUMPIFop, 0, Machine.CBr, startAdr);

        return null;
    }

    public object? VisitVarAssignment(VarAssignment varAssignment, object? arg)
    {
        // Get the variable's address (TODO: from declaration)
        // For now, create a dummy address
        var adr = new Address(0, 0);

        // Evaluate the expression
        varAssignment.Expr.Visit(this, true);

        // Store to variable
        int register = DisplayRegister(_currentLevel, adr.Level);
        Emit(Machine.STOREop, 1, register, adr.Displacement);

        return null;
    }

    public object? VisitArrayAssignment(ArrayAssignment arrayAssignment, object? arg)
    {
        // Load array base address (TODO: from declaration)
        // For now, create a dummy address
        var adr = new Address(0, 0);
        int register = DisplayRegister(_currentLevel, adr.Level);
        Emit(Machine.LOADAop, 1, register, adr.Displacement);

        // Evaluate index
        arrayAssignment.Index.Visit(this, true);

        // Evaluate value to store - but ArrayAssignment doesn't have Value property yet
        // TODO: Need to check if this should be part of a statement or expression

        // Store indirect
        Emit(Machine.STOREIop, 1, 0, 0);

        return null;
    }

    public object? VisitBoolLiteral(BoolLiteral boolLiteral, object? arg)
    {
        return boolLiteral.Value ? Machine.TrueRep : Machine.FalseRep;
    }

    public object? VisitCharLiteral(CharLiteral charLiteral, object? arg)
    {
        return (int)charLiteral.Value;
    }

    public object VisitIdentifier(Identifier identifier, object? arg)
    {
        return null!;
    }

    public object? VisitIntLiteral(IntLiteral intLiteral, object? arg)
    {
        return intLiteral.Value;
    }

    public object VisitOperator(Operator @operator, object? arg)
    {
        return @operator.Spelling;
    }

    public object? VisitParam(Param param, object? arg)
    {
        // Parameters handled in function declaration
        return null;
    }

    public object VisitSimpleType(SimpleType simpleType, object? arg)
    {
        return null!;
    }

    public object? VisitVomitStatement(VomitStatement vomitStatement, object? arg)
    {
        // Evaluate expression to output
        vomitStatement.Expression.Visit(this, true);

        // Call appropriate output primitive based on type
        // For now, assume integer output
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.PutintDisplacement);
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.PuteolDisplacement);

        return null;
    }

    public object? VisitListenStatement(ListenStatement listenStatement, object? arg)
    {
        // Call getint primitive
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.GetintDisplacement);

        // Store to the target variable (TODO: get address from declaration)
        var adr = new Address(0, 0);
        int register = DisplayRegister(_currentLevel, adr.Level);
        Emit(Machine.STOREop, 1, register, adr.Displacement);

        return null;
    }

    public object? VisitSubRoutineDecl(SubRoutineDeclar subRoutineDeclar, object? arg)
    {
        // TODO: Implement function/subroutine code generation
        // Similar to Java FunctionDeclaration visitor
        return null;
    }

    public object VisitCallStatement(CallStatement callStatement, object? arg)
    {
        // Evaluate arguments
        callStatement.Call.Visit(this, false); // Don't need return value for statement

        return null!;
    }

    public object? VisitDeclaration(Declaration declaration, object? arg)
    {
        // Generic declaration
        return null;
    }

    public object? VisitVarDecl(VarDecl varDecl, object? arg)
    {
        // Assign address to variable
        // TODO: Track variable addresses
        return null;
    }

    public object? VisitExpression(Expression expression, object? arg)
    {
        // Generic expression - shouldn't be called directly
        return null;
    }

    public object VisitBinaryExpr(BinaryExpr binaryExpr, object? arg)
    {
        bool valueNeeded = arg is bool b ? b : true;

        string op = (string)binaryExpr.op.Visit(this, null)!;

        // Evaluate left operand
        binaryExpr.left.Visit(this, valueNeeded);

        // Evaluate right operand
        binaryExpr.right.Visit(this, valueNeeded);

        if (valueNeeded)
        {
            // Call appropriate TAM primitive based on operator
            switch (op)
            {
                case "+":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.AddDisplacement);
                    break;
                case "-":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.SubDisplacement);
                    break;
                case "*":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.MultDisplacement);
                    break;
                case "/":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.DivDisplacement);
                    break;
                case "%":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.ModDisplacement);
                    break;
                case "<":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.LtDisplacement);
                    break;
                case "<=":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.LeDisplacement);
                    break;
                case ">":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.GtDisplacement);
                    break;
                case ">=":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.GeDisplacement);
                    break;
                case "==":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.EqDisplacement);
                    break;
                case "!=":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.NeDisplacement);
                    break;
                case "&&":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.AndDisplacement);
                    break;
                case "||":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.OrDisplacement);
                    break;
            }
        }

        return null!;
    }

    public object VisitUnaryExpr(UnaryExpr unaryExpr, object? arg)
    {
        bool valueNeeded = arg is bool b ? b : true;

        string op = (string)unaryExpr.Operator.Visit(this, null)!;

        // Evaluate operand
        unaryExpr.Operand.Visit(this, valueNeeded);

        if (valueNeeded)
        {
            switch (op)
            {
                case "-":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.NegDisplacement);
                    break;
                case "!":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.NotDisplacement);
                    break;
            }
        }

        return null!;
    }

    public object VisitIntLiteralExpression(IntLiteralExpression intLiteralExpression, object? arg)
    {
        bool valueNeeded = arg is bool b ? b : true;

        int lit = (int)intLiteralExpression.Literal.Visit(this, null)!;

        if (valueNeeded)
            Emit(Machine.LOADLop, 1, 0, lit);

        return null!;
    }

    public object VisitBoolLiteralExpression(BoolLiteralExpression boolLiteralExpression, object? arg)
    {
        bool valueNeeded = arg is bool b ? b : true;

        int lit = (int)boolLiteralExpression.Literal.Visit(this, null)!;

        if (valueNeeded)
            Emit(Machine.LOADLop, 1, 0, lit);

        return null!;
    }

    public object VisitCharLiteralExpression(CharLiteralExpression charLiteralExpression, object? arg)
    {
        bool valueNeeded = arg is bool b ? b : true;

        int lit = (int)charLiteralExpression.Literal.Visit(this, null)!;

        if (valueNeeded)
            Emit(Machine.LOADLop, 1, 0, lit);

        return null!;
    }

    public object VisitVarExpression(VarExpression varExpression, object? arg)
    {
        bool valueNeeded = arg is bool b ? b : true;

        // TODO: Get variable address from declaration
        // For now, return a dummy address
        var adr = new Address(0, 0);

        if (valueNeeded)
        {
            int register = DisplayRegister(_currentLevel, adr.Level);
            Emit(Machine.LOADop, 1, register, adr.Displacement);
        }

        return adr;
    }

    public object VisitCallExpr(CallExpr callExpr, object? arg)
    {
        bool valueNeeded = arg is bool b ? b : true;

        // Evaluate arguments
        callExpr.Arguments.Visit(this, null);

        // TODO: Get function address and call it
        // For now, just a placeholder

        if (!valueNeeded)
            Emit(Machine.POPop, 0, 0, 1);

        return null!;
    }
}
