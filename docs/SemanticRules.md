# Semantic rules

This document outlines the semantic rules for a programming language. These rules define how the constructs of the language behave and interact with each other.

## Variable Declaration and Scope
- Variables must be declared before they are used.
- Variables must have a type when they are declared.
- Variables have a scope that is defined by the block in which they are declared.
- Variables declared in an inner block can shadow variables with the same name in an outer block.
- Variables must be initialized/assignmed before they are used.

## Functions 
- Functions must be declared before they are called.
- Parameters of a function are local to that function.
- Function do not return anything.

## Shared
- A name defined for a function cannot be used as a variable and vice versa.
