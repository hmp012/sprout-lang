# SproutLang Programming Guide

Welcome to SproutLang! This guide will help you learn how to write programs in this unique programming language.

## Table of Contents
1. [Introduction](#introduction)
2. [Basic Syntax](#basic-syntax)
3. [Data Types](#data-types)
4. [Variables](#variables)
5. [Arrays](#arrays)
6. [Operators](#operators)
7. [Control Flow](#control-flow)
8. [Input/Output](#inputoutput)
9. [Functions (Subroutines)](#functions-subroutines)
10. [Comments](#comments)
11. [Complete Examples](#complete-examples)
12. [TAM Encoding Templates](#tam-encoding-templates)
13. [Compilation Process](#compilation-process)

---

## Introduction

SproutLang is a statically-typed imperative programming language that compiles to Triangle Abstract Machine (TAM) code. It features:
- **Static typing** with type checking at compile time
- **Simple syntax** with intuitive keywords
- **Arrays** for data structure support
- **Control structures** including conditionals and loops
- **Subroutines** for code reusability
- **TAM compilation** for efficient execution

The language is designed for learning compiler construction and understanding how high-level constructs map to abstract machine instructions.

---

## Basic Syntax

SproutLang programs consist of statements that end with semicolons (`;`). Code blocks are enclosed in curly braces `{ }`.

```sprout
create int x = 5;
vomit x;
```

---

## Data Types

SproutLang supports three primitive data types:

### Integer (`int`)
Whole numbers, positive or negative.
```sprout
create int age = 25;
create int temperature = -10;
```

### Boolean (`bool`)
True or false values.
```sprout
create bool isActive = true;
create bool hasPermission = false;
```

### Character (`char`)
Single characters enclosed in single quotes.
```sprout
create char grade = 'A';
create char initial = 'J';
```

---

## Variables

### Variable Declaration

Variables are declared using the `create` keyword, followed by the type, name, and optionally an initial value:

```sprout
create int x;              # Declare without initialization
create int y = 10;         # Declare with initialization
create bool flag = true;   # Boolean variable
create char letter = 'x';  # Character variable
```

### Variable Assignment

After declaration, you can assign values using the `=` operator:

```sprout
create int x;
x = 42;
x = x + 10;
```

---

## Arrays

Arrays are collections of elements of the same type with a fixed size.

### Array Declaration

Arrays are declared with the syntax `[type, size]`:

```sprout
create [int, 10] numbers;      # Array of 10 integers
create [char, 5] letters;      # Array of 5 characters
create [bool, 3] flags;        # Array of 3 booleans
```

### Array Access and Assignment

Access array elements using square brackets with an index (0-based):

```sprout
create [int, 10] arr;
arr[0] = 30;           # Set first element to 30
arr[1] = arr[0] + 5;   # Set second element to 35
vomit arr[0];          # Output first element
```

**Example:**
```sprout
create [int, 5] scores;
scores[0] = 85;
scores[1] = 90;
scores[2] = 78;
scores[3] = 92;
scores[4] = 88;
vomit scores[2];       # Outputs: 78
```

---

## Operators

### Arithmetic Operators
- `+` Addition
- `-` Subtraction
- `*` Multiplication
- `/` Division

```sprout
create int a = 10;
create int b = 3;
vomit a + b;    # Outputs: 13
vomit a - b;    # Outputs: 7
vomit a * b;    # Outputs: 30
vomit a / b;    # Outputs: 3 (integer division)
```

### Comparison Operators
- `==` Equal to
- `!=` Not equal to
- `<` Less than
- `>` Greater than

```sprout
create int x = 5;
create int y = 10;
create bool result = x < y;    # true
create bool same = x == y;     # false
```

### Logical Operators
- `&&` Logical AND
- `||` Logical OR
- `!` Logical NOT

```sprout
create bool a = true;
create bool b = false;
create bool result1 = a && b;   # false
create bool result2 = a || b;   # true
create bool result3 = !a;       # false
```

### Unary Operators
```sprout
create int x = 5;
create int negative = -x;       # -5
create bool flag = true;
create bool notFlag = !flag;    # false
```

---

## Control Flow

### Conditional Statements (If)

Use `si` (if), `o` (or/else if), and `sino` (else) for conditional execution:

```sprout
create int x = 10;

# Simple if
si (x < 10) {
    vomit x;
}

# If-else
si (x < 10) {
    vomit x;
} sino {
    vomit 0;
}

# If-else if-else
si (x < 5) {
    vomit 1;
} o (x < 10) {
    vomit 2;
} sino {
    vomit 3;
}
```

**Single-line statements:**
```sprout
si (x < 10) vomit x;
si (x < 10) vomit x; sino vomit 0;
```

### Loops

#### Repeat-While Loop

Use `repeat` to create a while loop:

```sprout
create int counter = 0;
repeat (counter < 5) {
    vomit counter;
    counter = counter + 1;
}
# Outputs: 0, 1, 2, 3, 4
```

#### Repeat-Until Loop

Use `repeat ... until` for a loop that executes until a condition is true:

```sprout
create int x = 0;
repeat {
    vomit x;
    x = x + 1;
} until (x == 5);
# Outputs: 0, 1, 2, 3, 4
```

#### Repeat-Times Loop

Use `repeat ... times` to execute a block a specific number of times:

```sprout
repeat 5 times {
    vomit 42;
}
# Outputs 42 five times
```

### Loop Control

- `bloom` - Continue to next iteration (like `continue`)
- `sprout` - Exit the loop (like `break`)

```sprout
create int i = 0;
repeat (i < 10) {
    i = i + 1;
    si (i == 5) bloom;      # Skip when i is 5
    si (i == 8) sprout;     # Exit when i is 8
    vomit i;
}
# Outputs: 1, 2, 3, 4, 6, 7
```

---

## Input/Output

### Output (`vomit`)

Use `vomit` to output values to the console:

```sprout
vomit 42;                    # Output a number
vomit 42 + 8;                # Output an expression (50)

create int x = 100;
vomit x;                     # Output a variable

create char letter = 'A';
vomit letter;                # Output a character
```

### Input (`listenCarefully`)

Use `listenCarefully` to read input from the user:

```sprout
create int userInput;
listenCarefully userInput;   # Read an integer from user
vomit userInput;             # Output what was entered
```

**Example with input:**
```sprout
create int age;
listenCarefully age;
si (age >= 18) {
    vomit 1;  # Adult
} sino {
    vomit 0;  # Minor
}
```

---

## Functions (Subroutines)

Functions (called subroutines) allow you to define reusable blocks of code.

### Function Declaration

```sprout
sprout functionName(int param1, bool param2) {
    # Function body
}
```

### Calling Functions

```sprout
functionName(42, true);
```

**Example:**
```sprout
sprout greet(int times) {
    create int i = 0;
    repeat (i < times) {
        vomit 72;  # ASCII for 'H'
        i = i + 1;
    }
}

# Call the function
greet(3);
```

---

## Comments

Use `#` for single-line comments:

```sprout
# This is a comment
create int x = 5;  # This is also a comment

# Comments can span multiple lines
# by using # at the start of each line
create int y = 10;
```

---

## Complete Examples

### Example 1: Simple Calculator
```sprout
create int a = 10;
create int b = 5;

vomit a + b;    # 15
vomit a - b;    # 5
vomit a * b;    # 50
vomit a / b;    # 2
```

### Example 2: Counting Loop
```sprout
create int number = 0;
repeat (number < 10) {
    vomit number;
    number = number + 1;
}
```

### Example 3: Conditional Logic
```sprout
create int score = 85;

si (score >= 90) {
    vomit 65;  # 'A'
} o (score >= 80) {
    vomit 66;  # 'B'
} o (score >= 70) {
    vomit 67;  # 'C'
} sino {
    vomit 70;  # 'F'
}
```

### Example 4: Array Sum
```sprout
create [int, 5] numbers;
numbers[0] = 10;
numbers[1] = 20;
numbers[2] = 30;
numbers[3] = 40;
numbers[4] = 50;

create int sum = 0;
create int i = 0;
repeat (i < 5) {
    sum = sum + numbers[i];
    i = i + 1;
}
vomit sum;  # Outputs: 150
```

### Example 5: Finding Maximum
```sprout
create int a = 15;
create int b = 23;
create int max;

si (a > b) {
    max = a;
} sino {
    max = b;
}
vomit max;  # Outputs: 23
```

### Example 6: Interactive Program
```sprout
# Read a number from user
create int userNumber;
listenCarefully userNumber;

# Check if even or odd
create int remainder = userNumber / 2 * 2;
si (remainder == userNumber) {
    vomit 1;  # Even
} sino {
    vomit 0;  # Odd
}
```

### Example 7: Nested Loops
```sprout
create int i = 0;
repeat (i < 3) {
    create int j = 0;
    repeat (j < 3) {
        vomit i * 3 + j;
        j = j + 1;
    }
    i = i + 1;
}
# Outputs: 0, 1, 2, 3, 4, 5, 6, 7, 8
```

### Example 8: Complex Conditions
```sprout
create int age = 25;
create bool hasLicense = true;

si (age >= 18 && hasLicense) {
    vomit 1;  # Can drive
} sino {
    vomit 0;  # Cannot drive
}
```

### Example 9: Repeat-Until Example
```sprout
create int countdown = 5;
repeat {
    vomit countdown;
    countdown = countdown - 1;
} until (countdown == 0);
# Outputs: 5, 4, 3, 2, 1
```

### Example 10: Array Search
```sprout
create [int, 5] data;
data[0] = 12;
data[1] = 45;
data[2] = 78;
data[3] = 23;
data[4] = 56;

create int target = 78;
create int found = 0;
create int i = 0;

repeat (i < 5) {
    si (data[i] == target) {
        found = 1;
        sprout;  # Exit loop when found
    }
    i = i + 1;
}

vomit found;  # Outputs: 1 (true)
```

---

## Quick Reference

### Keywords
- `create` - Declare a variable
- `vomit` - Output to console
- `listenCarefully` - Read input
- `si` - If statement
- `o` - Else if
- `sino` - Else
- `repeat` - Loop
- `until` - Repeat-until loop condition
- `times` - Repeat a fixed number of times
- `sprout` - Break from loop
- `bloom` - Continue to next iteration

### Types
- `int` - Integer
- `bool` - Boolean (true/false)
- `char` - Character
- `[type, size]` - Array

### Operators
- Arithmetic: `+`, `-`, `*`, `/`
- Comparison: `==`, `!=`, `<`, `>`
- Logical: `&&`, `||`, `!`
- Assignment: `=`

---

## Tips and Best Practices

1. **Always declare variables before use** with the `create` keyword
2. **Use meaningful variable names** to make code readable
3. **Comment your code** to explain complex logic
4. **Initialize arrays before accessing** to avoid undefined behavior
5. **Check array bounds** to prevent index out of range errors
6. **Use blocks `{ }`** for multiple statements in control structures
7. **Test incrementally** - write small pieces and test often

---

## Common Patterns

### Swap Two Variables
```sprout
create int a = 5;
create int b = 10;
create int temp;

temp = a;
a = b;
b = temp;
```

### Accumulator Pattern
```sprout
create int sum = 0;
create int i = 1;
repeat (i <= 10) {
    sum = sum + i;
    i = i + 1;
}
vomit sum;  # Sum of 1 to 10
```

### Flag Pattern
```sprout
create bool found = false;
create int i = 0;
repeat (i < 10 && !found) {
    si (someCondition) {
        found = true;
    }
    i = i + 1;
}
```

---

Happy coding in SproutLang! 🌱

---

## TAM Encoding Templates

SproutLang compiles to Triangle Abstract Machine (TAM) instructions. This section describes how high-level constructs map to low-level TAM code using formal semantic function notation.

### TAM Architecture Overview

The TAM is a stack-based abstract machine with:
- **Code Store (CS)**: Stores the compiled program instructions
- **Data Store (DS)**: Runtime stack for data and control information
- **Registers**:
  - `CB` - Code Base register (start of code)
  - `CP` - Code Pointer (program counter)
  - `CT` - Code Top (end of code)
  - `PB` - Primitive Base (start of primitive routines)
  - `SB` - Stack Base (start of global variables)
  - `ST` - Stack Top (top of runtime stack)
  - `HT` - Heap Top (for dynamic allocation)
  - `LB` - Local Base (current stack frame)
  - `L1-L6` - Display registers for static scoping

### Instruction Format

TAM instructions have the format: `OPCODE n r d`
- **OPCODE**: The operation to perform
- **n**: Length operand (number of words)
- **r**: Register operand (which register to use)
- **d**: Displacement/offset operand

---

## Encoding Rules (Semantic Functions)

### Program Execution

```
Run[[Program]] = 
    Execute[[Block]]
    HALT
```

### Block Execution

```
Execute[[{ Declaration* Statement* }]] = 
    JUMP s
    Elaborate[[Declaration*]]
    s: PUSH varsize              (PUSH not generated if varsize = 0)
    Execute[[Statement*]]
    POP 0 0 varsize             (POP deallocates local variables)
```

**Example:**
```sprout
{
    create int x = 5;
    vomit x;
}
```
**Generates:**
```
JUMP s
s: PUSH 0 0 1
   LOADL 1 0 5
   STORE 1 LB 0
   LOAD 1 LB 0
   CALL 0 PB putint
   POP 0 0 1
```

---

### Declaration Elaboration

```
Elaborate[[Declaration*]] = 
    Elaborate[[Declaration₁]]
    Elaborate[[Declaration₂]]
    …
    Elaborate[[Declarationₙ]]
```

#### Variable Declaration

```
Elaborate[[create Type Identifier;]] = 
    (allocate 1 word, no initialization)

Elaborate[[create Type Identifier = Expression;]] = 
    Evaluate[[Expression]]
    STORE 1 d[r] offset         (where d[r] is varoffset[varreg])
```

**Example:**
```sprout
create int x = 42;
```
**Generates:**
```
LOADL 1 0 42
STORE 1 SB 0
```

#### Array Declaration

```
Elaborate[[create [Type, size] Identifier;]] = 
    PUSH 0 0 size               (allocate size words)
```

**Example:**
```sprout
create [int, 10] arr;
```
**Generates:**
```
PUSH 0 0 10
```

#### Subroutine Declaration

```
Elaborate[[sprout Identifier(Params) Block]] = 
    JUMP after
    label:
        Elaborate[[Params]]
        Execute[[Block]]
        RETURN (1) paramsize
    after:
```

**Example:**
```sprout
sprout func(int a, int b) {
    vomit a + b;
}
```
**Generates:**
```
JUMP after_func
func_label:
    LOAD 1 LB -2             # Load parameter a
    LOAD 1 LB -1             # Load parameter b
    CALL 0 PB add
    CALL 0 PB putint
    RETURN (1) 2
after_func:
```

---

### Statement Execution

```
Execute[[Statement*]] = 
    Execute[[Statement₁]]
    Execute[[Statement₂]]
    …
    Execute[[Statementₙ]]
```

#### Expression Statement

```
Execute[[Expression;]] = 
    Evaluate[[Expression]]
    POP 0 0 1                   (discard result if not used)
```

#### Assignment Statement

```
Execute[[Identifier = Expression;]] = 
    Evaluate[[Expression]]
    STORE 1 d[r] varoffset      (where d[r] = varoffset[varreg])
```

**Example:**
```sprout
x = 10;
```
**Generates:**
```
LOADL 1 0 10
STORE 1 SB 0
```

#### Array Assignment

```
Execute[[Identifier[IndexExpr] = Expression;]] = 
    Evaluate[[IndexExpr]]
    LOAD 1 d[r] arraybase
    Evaluate[[Expression]]
    STOREI 1                    (indirect store)
```

**Example:**
```sprout
arr[2] = 5;
```
**Generates:**
```
LOADL 1 0 2              # Index
LOAD 1 SB 0              # Array base
LOADL 1 0 5              # Value
STOREI 1                 # arr[2] = 5
```

#### If Statement

```
Execute[[si (Expression) Statement1 sino Statement2]] = 
    Evaluate[[Expression]]
    JUMPIF (0) e
    Execute[[Statement1]]
    JUMP d
    e: Execute[[Statement2]]
    d:
```

**Example:**
```sprout
si (x < 10) vomit 1; sino vomit 0;
```
**Generates:**
```
LOAD 1 SB 0              # Load x
LOADL 1 0 10
CALL 0 PB lt
JUMPIF (0) else_branch
LOADL 1 0 1
CALL 0 PB putint
JUMP end_if
else_branch:
LOADL 1 0 0
CALL 0 PB putint
end_if:
```

#### If Statement (No Else)

```
Execute[[si (Expression) Statement]] = 
    Evaluate[[Expression]]
    JUMPIF (0) d
    Execute[[Statement]]
    d:
```

#### If-ElseIf-Else Statement

```
Execute[[si (E1) S1 o (E2) S2 ... sino Sn]] = 
    Evaluate[[E1]]
    JUMPIF (0) test2
    Execute[[S1]]
    JUMP end
    test2: Evaluate[[E2]]
    JUMPIF (0) test3
    Execute[[S2]]
    JUMP end
    ...
    testN: Execute[[Sn]]
    end:
```

#### While Loop

```
Execute[[repeat (Expression) Statement]] = 
    r: Evaluate[[Expression]]
    JUMPIF (0) d
    Execute[[Statement]]
    JUMP r
    d:
```

**Example:**
```sprout
repeat (x < 10) { x = x + 1; }
```
**Generates:**
```
loop_start:
    LOAD 1 SB 0              # Load x
    LOADL 1 0 10
    CALL 0 PB lt
    JUMPIF (0) loop_end
    LOAD 1 SB 0              # x
    LOADL 1 0 1              # 1
    CALL 0 PB add            # x + 1
    STORE 1 SB 0             # x = 
    JUMP loop_start
loop_end:
```

#### Repeat-Until Loop

```
Execute[[repeat Statement until (Expression);]] = 
    r: Execute[[Statement]]
    Evaluate[[Expression]]
    JUMPIF (0) r                (jump back if false)
```

**Example:**
```sprout
repeat { x = x + 1; } until (x == 10);
```
**Generates:**
```
loop_start:
    LOAD 1 SB 0              # Load x
    LOADL 1 0 1
    CALL 0 PB add
    STORE 1 SB 0             # x = x + 1
    LOAD 1 SB 0              # Load x
    LOADL 1 0 10
    CALL 0 PB eq             # x == 10
    JUMPIF (0) loop_start    # Continue if false
```

#### Repeat-Times Loop

```
Execute[[repeat Expression times Statement]] = 
    Evaluate[[Expression]]      (push counter)
    loop: LOAD 1 ST -1          (duplicate counter)
    LOADL 1 0 0
    CALL 0 PB gt                (counter > 0?)
    JUMPIF (0) end
    Execute[[Statement]]
    LOAD 1 ST -1                (load counter)
    LOADL 1 0 1
    CALL 0 PB sub               (counter - 1)
    STORE 1 ST -1               (store counter)
    JUMP loop
    end: POP 0 0 1              (remove counter)
```

**Example:**
```sprout
repeat 5 times { vomit 42; }
```
**Generates:**
```
LOADL 1 0 5              # Counter = 5
loop_start:
    LOAD 1 ST -1         # Check counter
    LOADL 1 0 0
    CALL 0 PB gt
    JUMPIF (0) loop_end
    LOADL 1 0 42         # Body
    CALL 0 PB putint
    LOAD 1 ST -1         # Decrement
    LOADL 1 0 1
    CALL 0 PB sub
    STORE 1 ST -1
    JUMP loop_start
loop_end:
    POP 0 0 1            # Clean up counter
```

#### Break Statement

```
Execute[[sprout;]] = 
    JUMP loop_end_addr          (jump to end of enclosing loop)
```

#### Continue Statement

```
Execute[[bloom;]] = 
    JUMP loop_start_addr        (jump to start of enclosing loop)
```

#### Output Statement

```
Execute[[vomit Expression;]] = 
    Evaluate[[Expression]]
    CALL 0 PB putint            (or putchar for char type)
    CALL 0 PB puteol            (optional: print newline)
```

**Example:**
```sprout
vomit x + 5;
```
**Generates:**
```
LOAD 1 SB 0              # Load x
LOADL 1 0 5
CALL 0 PB add
CALL 0 PB putint
```

#### Input Statement

```
Execute[[listenCarefully Identifier;]] = 
    CALL 0 PB getint            (reads value onto stack)
    STORE 1 d[r] varoffset
```

**Example:**
```sprout
listenCarefully x;
```
**Generates:**
```
CALL 0 PB getint
STORE 1 SB 0
```

---

### Expression Evaluation

```
Evaluate[[IntegerLiteral]] = 
    LOADL 1 0 literal

Evaluate[[true]] = 
    LOADL 1 0 1

Evaluate[[false]] = 
    LOADL 1 0 0

Evaluate[[CharLiteral]] = 
    LOADL 1 0 ascii_value

Evaluate[[Identifier]] = 
    LOAD 1 d[r] varoffset

Evaluate[[Identifier[Expression]]] = 
    Evaluate[[Expression]]       (index)
    LOAD 1 d[r] arraybase
    LOADI 1                     (indirect load)
```

#### Binary Operations

```
Evaluate[[Expression₁ Operator Expression₂]] = 
    Evaluate[[Expression₁]]
    Evaluate[[Expression₂]]
    CALL 0 PB operator_disp
```

**Operators:**
- `+` → `add_disp`
- `-` → `sub_disp`
- `*` → `mul_disp`
- `/` → `div_disp`
- `<` → `lt_disp`
- `>` → `gt_disp`
- `==` → `eq_disp`
- `!=` → `ne_disp`
- `&&` → `and_disp`
- `||` → `or_disp`

**Example:**
```sprout
a + b * c
```
**Generates:**
```
LOAD 1 SB 0              # Load a
LOAD 1 SB 1              # Load b
LOAD 1 SB 2              # Load c
CALL 0 PB mul            # b * c
CALL 0 PB add            # a + (b * c)
```

#### Unary Operations

```
Evaluate[[- Expression]] = 
    Evaluate[[Expression]]
    CALL 0 PB neg

Evaluate[[+ Expression]] = 
    Evaluate[[Expression]]       (unary + is identity)

Evaluate[[! Expression]] = 
    Evaluate[[Expression]]
    CALL 0 PB not
```

**Example:**
```sprout
-x
```
**Generates:**
```
LOAD 1 SB 0
CALL 0 PB neg
```

#### Function Call

```
Evaluate[[Identifier(ExpressionList)]] = 
    Evaluate[[ExpressionList]]
    CALL 0 CB funcaddr

Evaluate[[Expression₁, Expression₂, ..., Expressionₙ]] = 
    Evaluate[[Expression₁]]
    Evaluate[[Expression₂]]
    …
    Evaluate[[Expressionₙ]]
```

**Example:**
```sprout
func(10, 20)
```
**Generates:**
```
LOADL 1 0 10             # Arg 1
LOADL 1 0 20             # Arg 2
CALL 0 CB func_addr      # Call function
```

---

### Addressing Modes

#### Variable Address Calculation

```
varoffset[varreg] where:
    varreg = SB    (global variables, level 0)
    varreg = LB    (local variables, current level)
    varreg = LB+k  (variables k levels up, using display registers)
```

#### Display Register Selection

```
DisplayRegister(currentLevel, entityLevel) =
    if entityLevel = 0 then SB
    else if currentLevel - entityLevel ≤ 6 then LB + (currentLevel - entityLevel)
    else error (too many nesting levels)
```

---

### Primitive Routine Displacements

Common primitive routines available at PB (Primitive Base):

| Operation | Displacement | Description |
|-----------|-------------|-------------|
| `putint` | `put_disp` | Output integer |
| `getint` | `get_disp` | Input integer |
| `putchar` | `putc_disp` | Output character |
| `getchar` | `getc_disp` | Input character |
| `puteol` | `puteol_disp` | Output newline |
| `add` | `add_disp` | Integer addition |
| `sub` | `sub_disp` | Integer subtraction |
| `mul` | `mul_disp` | Integer multiplication |
| `div` | `div_disp` | Integer division |
| `mod` | `mod_disp` | Modulo operation |
| `neg` | `neg_disp` | Unary negation |
| `lt` | `lt_disp` | Less than comparison |
| `gt` | `gt_disp` | Greater than comparison |
| `le` | `le_disp` | Less than or equal |
| `ge` | `ge_disp` | Greater than or equal |
| `eq` | `eq_disp` | Equality comparison |
| `ne` | `ne_disp` | Not equal comparison |
| `and` | `and_disp` | Logical AND |
| `or` | `or_disp` | Logical OR |
| `not` | `not_disp` | Logical NOT |

---

### Complete Example

**Source Program:**
```sprout
create int x = 5;
create int y = 10;
vomit x + y;
```

**Generated TAM Code:**
```
JUMP start
start:
    PUSH 0 0 2           # Allocate space for x and y
    LOADL 1 0 5          # Load 5
    STORE 1 SB 0         # Store to x
    LOADL 1 0 10         # Load 10
    STORE 1 SB 1         # Store to y
    LOAD 1 SB 0          # Load x
    LOAD 1 SB 1          # Load y
    CALL 0 PB add        # x + y
    CALL 0 PB putint     # Output result
    HALT                 # End program
```

---

## Compilation Process

### Overview

The SproutLang compiler follows a multi-phase architecture:

```
Source Code (.txt)
    ↓
[Scanner] → Tokens
    ↓
[Parser] → Abstract Syntax Tree (AST)
    ↓
[Checker] → Type-checked AST + Symbol Table
    ↓
[Encoder] → TAM Instructions
    ↓
TAM Binary (.tam)
    ↓
[TAM Interpreter] → Program Execution
```

### Phase 1: Scanning (Lexical Analysis)

**Input**: Source code text
**Output**: Stream of tokens
**File**: `Scanner/Scanner.cs`

The scanner reads the source character by character and groups them into tokens:
- Keywords: `create`, `vomit`, `si`, `repeat`, etc.
- Identifiers: variable and function names
- Literals: numbers, characters, booleans
- Operators: `+`, `-`, `*`, `/`, `==`, `<`, etc.
- Delimiters: `;`, `,`, `(`, `)`, `{`, `}`, `[`, `]`

**Example**:
```
Input:  create int x = 42;
Output: [CREATE] [INT] [IDENTIFIER:"x"] [ASSIGN] [INTLITERAL:42] [SEMICOLON]
```

### Phase 2: Parsing (Syntax Analysis)

**Input**: Token stream
**Output**: Abstract Syntax Tree (AST)
**File**: `Parser/Parser.cs`

The parser analyzes the syntactic structure and builds a tree representation:
- Checks that tokens follow grammar rules
- Creates AST nodes for each language construct
- Reports syntax errors with line/column information

**Example AST for** `create int x = 42;`:
```
Program
└── Block
    └── VarDecl
        ├── Type: SimpleType(int)
        ├── Identifier: "x"
        └── Expression: IntLiteral(42)
```

### Phase 3: Semantic Analysis (Type Checking)

**Input**: AST
**Output**: Type-checked AST with symbol table
**File**: `Checker/Checker.cs`

The checker performs:
- **Identification**: Resolves variable/function references
- **Type Checking**: Ensures type compatibility
- **Scope Analysis**: Manages nested scopes
- **Error Detection**: Reports semantic errors

**Symbol Table**: Maintains information about:
- Variable declarations and types
- Function signatures
- Scope levels
- Memory addresses

**Type Checking Rules**:
- Arithmetic operators require integer operands
- Logical operators require boolean operands
- Assignment requires type compatibility
- Array indices must be integers
- Function arguments must match parameter types

**Example Checks**:
```sprout
create int x = 5;
create bool y = x;      # ERROR: Type mismatch (int → bool)

create int a = 10;
create int b = a + 5;   # OK: int + int → int

create bool flag = true;
si (flag) { }           # OK: condition is bool
si (a) { }              # ERROR: condition must be bool, got int
```

### Phase 4: Code Generation

**Input**: Type-checked AST
**Output**: TAM instructions
**File**: `TAM/Encoder.cs`

The encoder traverses the AST and generates TAM code:
- **Expression Encoding**: Generates code to evaluate expressions
- **Statement Encoding**: Generates control flow instructions
- **Address Calculation**: Computes variable offsets
- **Label Resolution**: Resolves jump targets
- **Optimization**: Basic peephole optimizations (future work)

**Key Techniques**:
1. **Stack-Based Evaluation**: Expressions evaluated on runtime stack
2. **Static Scoping**: Display registers for nested scopes
3. **Backpatching**: Resolve forward jumps for control structures
4. **Calling Convention**: Standard parameter passing and return

### Phase 5: Execution

**Input**: TAM binary
**Output**: Program output
**File**: `TAM/Machine.cs`

The TAM interpreter:
- Loads compiled program into code store
- Initializes registers and stack
- Fetches and executes instructions
- Manages runtime stack
- Handles primitive operations

**Runtime Stack Layout**:
```
High Address
    |  Heap (grows down)  |
    |---------------------|
    |  Stack (grows up)   |
    |  Local Variables    |
    |  Parameters         |
    |  Return Address     |
    |  Dynamic Link       |
    |  Static Link        |
    |---------------------|
    |  Global Variables   |
Low Address
```

### Running the Compiler

#### Compile Only
```bash
dotnet run <source_file.txt>
```

#### Compile and View AST
```bash
dotnet run --view-ast <source_file.txt>
```

#### Run with Debugging
```bash
dotnet run --debug <source_file.txt>
```

### Error Handling

#### Lexical Errors
```
Error at line 5, column 10: Unrecognized character '@'
```

#### Syntax Errors
```
Parse Error at line 3: Expected ';' after statement
```

#### Semantic Errors
```
Type Error at line 7: Cannot assign bool to int variable 'x'
Undefined variable 'y' at line 10
```

#### Runtime Errors
```
Runtime Error: Division by zero at instruction 42
Stack overflow at instruction 108
```

### Optimization Considerations

While the current compiler performs minimal optimization, potential improvements include:

1. **Constant Folding**: Evaluate constant expressions at compile time
   ```
   create int x = 5 + 3;  →  create int x = 8;
   ```

2. **Dead Code Elimination**: Remove unreachable code
   ```
   si (false) { vomit 42; }  →  # removed
   ```

3. **Common Subexpression Elimination**: Reuse computed values
   ```
   a = b + c;
   d = b + c;  →  a = b + c; d = a;
   ```

4. **Register Allocation**: Better use of TAM registers

5. **Strength Reduction**: Replace expensive operations
   ```
   x * 2  →  x + x
   ```

### Language Extension Ideas

Potential enhancements to SproutLang:

- **Strings**: String type and operations
- **Records/Structs**: User-defined composite types
- **Pointers**: Direct memory manipulation
- **Functions with Return Values**: Current subroutines are void
- **Recursion**: Full recursive function support
- **Dynamic Arrays**: Runtime-sized arrays
- **File I/O**: Read/write files
- **Standard Library**: Common utility functions

---

## Debugging Tips

### Understanding Compilation Errors

1. **Read the error message carefully** - It tells you what went wrong
2. **Check the line number** - Error location is usually accurate
3. **Look at surrounding code** - Error might be earlier than reported
4. **Verify types** - Many errors are type mismatches
5. **Check declaration** - Variables must be declared before use

### Common Mistakes

#### Forgetting Type in Declaration
```sprout
create x = 5;           # ERROR: Missing type
create int x = 5;       # CORRECT
```

#### Type Mismatch
```sprout
create int x = true;    # ERROR: bool cannot be assigned to int
create bool x = true;   # CORRECT
```

#### Undeclared Variable
```sprout
x = 5;                  # ERROR: x not declared
create int x;
x = 5;                  # CORRECT
```

#### Missing Semicolon
```sprout
create int x = 5        # ERROR: Missing semicolon
vomit x;

create int x = 5;       # CORRECT
vomit x;
```

#### Wrong Condition Type
```sprout
create int x = 5;
si (x) { }              # ERROR: Condition must be bool
si (x > 0) { }          # CORRECT
```

### Testing Strategy

1. **Start Small**: Test with minimal programs first
2. **Add Incrementally**: Add one feature at a time
3. **Test Each Construct**: Verify each language feature works
4. **Check Edge Cases**: Test boundary conditions
5. **Verify Output**: Ensure output matches expectations

---

Happy coding in SproutLang! 🌱

