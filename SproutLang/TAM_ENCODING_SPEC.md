# SproutLang TAM Encoding Specification

This document describes the translation from SproutLang abstract syntax to Triangle Abstract Machine (TAM) instructions.

## Program Execution

```
Run[[Program]] = 
    Execute[[Block]] 
    HALT
```

## Block Execution

```
Execute[[Block]] = 
    JUMP s
    Elaborate[[Declarations]]
s:  PUSH varsize               (PUSH not generated if varsize = 0)
    Execute[[Statements]]
```

## Declaration Elaboration

### Multiple Declarations
```
Elaborate[[Declaration*]] = 
    Elaborate[[Declaration₁]]
    Elaborate[[Declaration₂]]
    ...
    Elaborate[[Declarationₙ]]
```

### Variable Declaration
```
Elaborate[[var Identifier : Type;]] = 
    (allocates space in current frame at displacement)
    (SimpleType: 1 word)
    (ArrayType: size words)
```

### Function/Subroutine Declaration
```
Elaborate[[func Identifier(Params) Block]] = 
    JUMP over
    Elaborate[[Params]]
entry:
    Execute[[Block]]
    RETURN paramsize 0 3
over:
    (function address stored as closure: 2 words)
```

### Parameter Elaboration
```
Elaborate[[Param]] = 
    (allocate 1 word per parameter at current displacement)
```

## Statement Execution

### Multiple Statements
```
Execute[[Statement*]] = 
    Execute[[Statement₁]]
    Execute[[Statement₂]]
    ...
    Execute[[Statementₙ]]
```

### Expression Statement
```
Execute[[Expression;]] = 
    Evaluate[[Expression]]
    POP 1
```

### If Statement (without else-if)
```
Execute[[if Expression then Statements₁ else Statements₂ fi;]] = 
    Evaluate[[Expression]]
    JUMPIF (0) e
    Execute[[Statements₁]]
    JUMP d
e:  Execute[[Statements₂]]
d:
```

### If Statement (with else-if branches)
```
Execute[[if Expression₁ then Statements₁ elsif Expression₂ then Statements₂ else Statements₃ fi;]] = 
    Evaluate[[Expression₁]]
    JUMPIF (0) e1
    Execute[[Statements₁]]
    JUMP d
e1: Evaluate[[Expression₂]]
    JUMPIF (0) e2
    Execute[[Statements₂]]
    JUMP d
e2: Execute[[Statements₃]]
d:
```

### Repeat-Times Loop (for loop with count)
```
Execute[[repeat Expression times Statements od;]] = 
    Evaluate[[Expression]]      (push counter)
r:  LOAD 1 [ST] -1            (duplicate counter)
    LOADL 1 0 0               (push 0)
    CALL gt                   (counter > 0?)
    JUMPIF (0) d              (exit if counter <= 0)
    Execute[[Statements]]
    LOAD 1 [ST] -1            (load counter)
    LOADL 1 0 1               (push 1)
    CALL sub                  (counter - 1)
    STORE 1 [ST] -1           (store counter)
    JUMP r
d:  POP 0 0 1                 (remove counter)
```

### Repeat-Until Loop
```
Execute[[repeat Statements until Expression od;]] = 
r:  Execute[[Statements]]
    Evaluate[[Expression]]
    JUMPIF (0) r              (loop back if condition false)
```

### Vomit Statement (output)
```
Execute[[vomit Expression;]] = 
    Evaluate[[Expression]]
    CALL putint               (for int expressions)
    CALL puteol

Execute[[vomit Expression;]] = 
    Evaluate[[Expression]]
    CALL put                  (for char expressions)
    CALL puteol
```

### Listen Statement (input)
```
Execute[[listen Identifier;]] = 
    CALL getint
    STORE 1 varoffset[varreg]
```

### Call Statement
```
Execute[[Identifier(ExpressionList);]] = 
    Evaluate[[ExpressionList]]
    CALL (funcreg) funcadr[CB]
    (no POP needed for procedures)
```

## Assignment

### Variable Assignment
```
Evaluate[[Identifier := Expression]] = 
    Evaluate[[Expression]]
    STORE 1 varoffset[varreg]
    LOAD 1 varoffset[varreg]
```

### Array Assignment
```
Evaluate[[Identifier[Index] := Expression]] = 
    Evaluate[[Expression]]         (value to store)
    LOADA 0 varoffset[varreg]     (base address)
    Evaluate[[Index]]              (index)
    CALL add                       (compute address)
    STOREI 1 0 0                   (store indirectly)
```

## Expression Evaluation

### Binary Expressions
```
Evaluate[[Expression₁ Operator Expression₂]] = 
    Evaluate[[Expression₁]]
    Evaluate[[Expression₂]]
    CALL operator

where operator ∈ {
    +   → add
    -   → sub
    *   → mult
    /   → div
    %   → mod
    <   → lt
    <=  → le
    >   → gt
    >=  → ge
    ==  → eq
    !=  → ne
    &&  → and
    ||  → or
}
```

### Unary Expressions
```
Evaluate[[- Expression]] = 
    Evaluate[[Expression]]
    CALL neg

Evaluate[[! Expression]] = 
    Evaluate[[Expression]]
    CALL not
```

### Identity (no operation needed)
```
Evaluate[[+ Expression]] = 
    Evaluate[[Expression]]
```

### Integer Literal
```
Evaluate[[IntegerLiteral]] = 
    LOADL 1 0 literal
```

### Boolean Literal
```
Evaluate[[true]] = 
    LOADL 1 0 1

Evaluate[[false]] = 
    LOADL 1 0 0
```

### Character Literal
```
Evaluate[[CharLiteral]] = 
    LOADL 1 0 ascii_value
```

### Variable Reference
```
Evaluate[[Identifier]] = 
    LOAD 1 varoffset[varreg]
```

### Array Access
```
Evaluate[[Identifier[Index]]] = 
    LOADA 0 varoffset[varreg]     (base address)
    Evaluate[[Index]]              (index)
    CALL add                       (compute address)
    LOADI 1 0 0                    (load indirectly)
```

### Function Call Expression
```
Evaluate[[Identifier(ExpressionList)]] = 
    Evaluate[[ExpressionList]]
    CALL (funcreg) funcadr[CB]
    (result remains on stack)
```

### Argument List Evaluation
```
Evaluate[[Expression ( , Expression )*]] = 
    Evaluate[[Expression₁]]
    Evaluate[[Expression₂]]
    ...
    Evaluate[[Expressionₙ]]
```

## Display Register Calculation

The display register for variable access is calculated based on the scope level:

```
DisplayRegister(currentLevel, entityLevel) = 
    if entityLevel = 0 then SB
    else if currentLevel - entityLevel ≤ 6 then LB + (currentLevel - entityLevel)
    else error (too many nesting levels)
```

## Address Allocation

- **Simple types** (int, bool, char): 1 word
- **Array types**: size words (where size is the array dimension)
- **Closures** (function references): 2 words (static link + code address)
- **Link data**: 3 words (dynamic link, static link, return address)

## Notes

1. All evaluations that produce values leave the result on the stack top.
2. The `arg` parameter in visitor methods is used to pass context:
   - `Address` for declarations (tracks allocation)
   - `bool` for expressions (indicates if value is needed)
3. Backpatching is used for forward jumps (if statements, loops, function declarations).
4. The encoder maintains `_currentLevel` to track lexical nesting depth.
5. Variables and parameters are accessed via display registers based on their declaration level.

