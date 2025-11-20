/*
 * SproutLang TAM Encoder
 *
 * Code generator for the Triangle Abstract Machine (TAM)
 * Based on the Triangle compiler encoder pattern
 */

using SproutLang.AST;
using SproutLang.Checker;
using Microsoft.Extensions.Logging;

namespace SproutLang.TAM;

public class Encoder : IAstVisitor
{
    private int _nextAdr = Machine.CB;
    private int _currentLevel;
    private readonly ILogger _logger;

    public Encoder(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Emit a TAM instruction to the code store
    /// </summary>
    private void Emit(int op, int n, int r, int d)
    {
        if (n > 255)
        {
            _logger.LogWarning("Operand too long: {Operand}. Truncating to 255.", n);
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
            _logger.LogError("Program too large. Next address {NextAddress} exceeds program base {ProgramBase}.",
                _nextAdr, Machine.PB);
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
            _logger.LogError("Accessing across too many levels. Current: {CurrentLevel}, Entity: {EntityLevel}",
                currentLevel, entityLevel);
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
            _logger.LogError(ex, "Trouble writing to file {FileName}", fileName);
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
        int before = _nextAdr;
        Emit(Machine.JUMPop, 0, Machine.CB, 0);

        // Ensure we have a start address and that its level reflects the current level
        var startAddress = (arg as Address) ?? new Address();
        startAddress.Level = _currentLevel;

        // First pass: handle declarations (they are represented as statements in out AST)
        object addr = startAddress;
        foreach (var stmt in block.Statements)
        {
            if (stmt is Declaration)
            {
                // Declaration visitors should return an Address when given an Address
                addr = stmt.Visit(this, addr) ?? addr;
            }
        }

        // Compute total size from start displacement
        int sizeOfDeclarations = (addr as Address)!.Displacement - startAddress.Displacement;

        // Patch the initial jump to here (start of executable code)
        Patch(before, _nextAdr);

        // Allocate space for local declarations if needed
        if (sizeOfDeclarations > 0)
            Emit(Machine.PUSHop, 0, 0, sizeOfDeclarations);

        // Second pass: emit code for non-declaration statements
        foreach (var statement in block.Statements)
        {
            if (statement is not Declaration)
            {
                statement.Visit(this, null);
            }
        }

        return sizeOfDeclarations;
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
        // Evaluate the count expression and push onto stack
        repeatTimes.Times.Visit(this, true);

        // Start of loop - counter is now at a fixed position on the stack
        int startAdr = _nextAdr;

        // Load counter to check it (counter stays on stack)
        Emit(Machine.LOADop, 1, Machine.STr, -1);

        // Check if counter > 0
        Emit(Machine.LOADLop, 1, 0, 0);
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.GtDisplacement);

        // Jump out of loop if counter <= 0 (comparison result is 0)
        int jumpAdr = _nextAdr;
        Emit(Machine.JUMPIFop, 0, Machine.CBr, 0);

        // Execute loop body
        repeatTimes.Body.Visit(this, null);

        // Decrement counter properly
        // Current stack: [counter]
        Emit(Machine.LOADop, 1, Machine.STr, -1);     // Stack: [counter, counter]
        Emit(Machine.LOADLop, 1, 0, 1);               // Stack: [counter, counter, 1]
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.SubDisplacement);  // Stack: [counter, counter-1]
        
        // Now we have [old_counter, new_counter] and we need just [new_counter]
        // STORE pops the value and stores it, so:
        // Store new_counter to the position of old_counter (which is at offset -2 from top)
        Emit(Machine.STOREop, 1, Machine.STr, -2);    // Pops new_counter and stores to old_counter position
                                                       // Stack is now: [new_counter] (the one we just stored)

        // Jump back to start
        Emit(Machine.JUMPop, 0, Machine.CBr, startAdr);

        // Patch exit jump to here
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
        // Get the declaration reference that was set by the checker
        var decl = varAssignment.Declaration;
        Address? adr = null;

        if (decl is VarDecl varDecl)
        {
            adr = varDecl.Address;
        }
        else if (decl is Param param)
        {
            adr = param.Address;
        }

        if (adr == null)
        {
            _logger.LogError("Variable {VariableName} not found or has no address", varAssignment.Name.Spelling);
            adr = new Address(0, 0);
        }

        // Evaluate the expression
        varAssignment.Expr.Visit(this, true);

        // Store to variable
        int register = DisplayRegister(_currentLevel, adr.Level);
        Emit(Machine.STOREop, 1, register, adr.Displacement);

        return null;
    }

    public object? VisitArrayAssignment(ArrayAssignment arrayAssignment, object? arg)
    {
        var decl = arrayAssignment.Declaration;
        Address? adr = null;

        if (decl is VarDecl varDecl)
        {
            adr = varDecl.Address;
        }

        if (adr == null)
        {
            _logger.LogError("Array {ArrayName} not found or has no address", arrayAssignment.Name.Spelling);
            adr = new Address(0, 0);
        }

        // Evaluate the value expression FIRST to push it onto the stack.
        arrayAssignment.Expr.Visit(this, true);

        // Now, calculate the target address.
        // Load the base address of the array.
        int register = DisplayRegister(_currentLevel, adr.Level);
        Emit(Machine.LOADAop, 0, register, adr.Displacement);

        // Evaluate the index expression.
        arrayAssignment.Index.Visit(this, true);

        // Add index to base address.
        Emit(Machine.CALLop, 0, Machine.PBr, Machine.AddDisplacement);

        // Store indirectly. The stack is now [..., value, address].
        // STOREI(1) will pop address, then pop value, and store.
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
        var currentAddress = (Address)arg!;
        param.Address = new Address(currentAddress);

        return new Address(currentAddress, 1);
    }

    public object VisitSimpleType(SimpleType simpleType, object? arg)
    {
        return null!;
    }

    public object? VisitArrayType(ArrayType n, object? arg)
    {
        var size = n.Size;

        // If used in a declaration context, allocate space for the entire array
        if (arg is Address address)
        {
            return new Address(address, size);
        }

        return null;
    }

    public object? VisitVomitStatement(VomitStatement vomitStatement, object? arg)
    {
        // Evaluate expression to output
        vomitStatement.Expression.Visit(this, true);

        // Call appropriate output primitive based on expression type
        if (vomitStatement.Expression is IntLiteralExpression ||
            (vomitStatement.Expression is VarExpression varExpr && 
             (varExpr.Declaration is VarDecl { Type: SimpleType type } && 
             type.Kind.Equals(BaseType.Int) ||
             varExpr.Declaration is Param { Type: SimpleType paramType } && paramType.Kind.Equals(BaseType.Int))))
        {
            Emit(Machine.CALLop, 0, Machine.PBr, Machine.PutintDisplacement);
        }
        else if (vomitStatement.Expression is CharLiteralExpression ||
                 (vomitStatement.Expression is VarExpression varExpr2 &&
                  ((varExpr2.Declaration is VarDecl { Type: SimpleType type2 } && type2.Kind.Equals(BaseType.Char)) ||
                   (varExpr2.Declaration is Param { Type: SimpleType paramType2 } && paramType2.Kind.Equals(BaseType.Char)))))
        {
            Emit(Machine.CALLop, 0, Machine.PBr, Machine.PutDisplacement);
        }
        else if (vomitStatement.Expression is BoolLiteralExpression ||
                 (vomitStatement.Expression is VarExpression varExpr3 &&
                  ((varExpr3.Declaration is VarDecl { Type: SimpleType type3 } && type3.Kind.Equals(BaseType.Bool)) ||
                   (varExpr3.Declaration is Param { Type: SimpleType paramType3 } && paramType3.Kind.Equals(BaseType.Bool)))))
        {
            // Print boolean as 0/1
            Emit(Machine.CALLop, 0, Machine.PBr, Machine.PutintDisplacement);
        }
        else
        {
            // Default to integer output for binary/unary expressions
            Emit(Machine.CALLop, 0, Machine.PBr, Machine.PutintDisplacement);
        }

        // Output newline
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
        // Get current address allocation position
        var currentAddress = (Address)arg!;
        
        // Store the entry point address for this function
        subRoutineDeclar.Address = new Address(_currentLevel, _nextAdr);
        
        // Enter new scope level for function body
        _currentLevel++;
        
        int jumpAddr = _nextAdr;
        Emit(Machine.JUMPop, 0, Machine.CBr, 0);
        // Create address for parameters starting at offset 0
        var paramAddress = new Address(currentAddress);
        
        // Count parameter size
        int paramSize = 0;
        foreach (var param in subRoutineDeclar.Params)
        {
            paramSize++;
        }
        
        // Allocate addresses for each parameter at negative offsets
        // (parameters are below the local base in TAM)
        int offset = -paramSize;
        foreach (var param in subRoutineDeclar.Params)
        {
            param.Address = new Address(_currentLevel, offset);
            offset++;
        }
        
        // Process function body with local variables starting after link data
        var localAddress = new Address(_currentLevel, Machine.LinkDataSize);
        var body = subRoutineDeclar.Body;
        
        // First pass: handle declarations in the function body
        object addr = localAddress;
        foreach (var stmt in body.Statements)
        {
            if (stmt is Declaration)
            {
                addr = stmt.Visit(this, addr) ?? addr;
            }
        }
        
        // Compute size of local variables
        int localVarSize = (addr as Address)!.Displacement - localAddress.Displacement;
        
        Patch(jumpAddr, _nextAdr);

        // Allocate space for local variables if needed
        if (localVarSize > 0)
            Emit(Machine.PUSHop, 0, 0, localVarSize);
        
        // Second pass: emit code for non-declaration statements
        foreach (var stmt in body.Statements)
        {
            if (stmt is not Declaration)
            {
                stmt.Visit(this, null);
            }
        }
        
        // Push a dummy return value (0) since our functions are procedures
        // TAM requires at least 1 word on the stack before RETURN
        Emit(Machine.LOADLop, 1, 0, 0);
        
        // Emit RETURN instruction
        // Returns 1 word (the return value), pops paramSize words of arguments
        Emit(Machine.RETURNop, 1, 0, paramSize);
        
        // Exit scope
        _currentLevel--;
        
        // Function declarations don't take up stack space
        // Return the same address (no space allocated)
        return arg;
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
        var currentAddress = (Address)arg!;
        varDecl.Address = currentAddress;

        var size = 1; // Default size for simple types

        if (varDecl.Type is ArrayType arrayType)
        {
                size = arrayType.Size;
        }

        return new Address(currentAddress, size);
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
                case "<":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.LtDisplacement);
                    break;
                case ">":
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.GtDisplacement);
                    break;
                case "==":
                    Emit(Machine.LOADLop, 0, 0, 1);
                    Emit(Machine.CALLop, 0, Machine.PBr, Machine.EqDisplacement);
                    break;
                case "!=":
                    Emit(Machine.LOADLop, 0, 0, 1);
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

        var decl = varExpression.Declaration;

        Address? adr = null;

        if (decl is VarDecl varDecl)
        {
            adr = varDecl.Address;
        }
        else if (decl is Param param)
        {
            adr = param.Address;
        }

        if (adr == null)
        {
            _logger.LogError("Variable {VariableName} not found or has no address", varExpression.Name.Spelling);
            adr = new Address(0, 0);
        }

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
        
        // Evaluate arguments (pushes them onto the stack)
        callExpr.Arguments.Visit(this, null);
        
        // Get the function's address
        var decl = callExpr.Declaration;
        if (decl is not SubRoutineDeclar subRoutineDecl)
        {
            _logger.LogError("Function {functionName} not found", callExpr.Callee.Spelling);
            return null!;
        }
        
        Address adr = subRoutineDecl.Address!;
        int register = DisplayRegister(_currentLevel, adr.Level);
        
        // Emit CALL instruction
        Emit(Machine.CALLop, register, Machine.CBr, adr.Displacement);
        
        // Pop return value if not needed
        if (!valueNeeded)
            Emit(Machine.POPop, 0, 0, 1);
        
        return null!;
    }

    public object VisitArrayExpression(ArrayExpression n, object? arg)
    {
        bool valueNeeded = arg is not bool b || b;

        // Get the array declaration reference that was set by the checker
        var decl = n.Declaration;
        Address? adr = null;

        if (decl is VarDecl varDecl)
        {
            adr = varDecl.Address;
        }

        if (adr == null)
        {
            _logger.LogError("Array {ArrayName} not found or has no address", n.Name.Spelling);
            adr = new Address(0, 0);
        }

        if (valueNeeded)
        {
            // Load the base address of the array
            int register = DisplayRegister(_currentLevel, adr.Level);
            Emit(Machine.LOADAop, 0, register, adr.Displacement);

            // Evaluate the index expression
            n.Index.Visit(this, true);

            // Add index to base address
            Emit(Machine.CALLop, 0, Machine.PBr, Machine.AddDisplacement);

            // Load value indirectly from computed address
            Emit(Machine.LOADIop, 1, 0, 0);
        }

        return null!;
    }
}