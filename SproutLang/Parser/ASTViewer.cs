namespace SproutLang.Tools;
using SproutLang.AST;

public static class ASTPrinter
{
    public static void Print(AST ast, string indent = "", bool isLast = true)
    {
        Console.Write(indent);
        Console.Write(isLast ? "└── " : "├── ");

        switch (ast)
        {
            case Block block:
                Console.WriteLine("Block");
                for (int i = 0; i < block.Statements.Count; i++)
                    Print(block.Statements[i], indent + (isLast ? "    " : "│   "), i == block.Statements.Count - 1);
                break;

            case VarDecl decl:
                Console.WriteLine($"VarDecl {decl.Name.Spelling}");
                Print(decl.Type, indent + (isLast ? "    " : "│   "), true);
                break;

            case VarAssignment assign:
                Console.WriteLine($"VarAssignment {assign.Name.Spelling}");
                Print(assign.Expr, indent + (isLast ? "    " : "│   "), true);
                break;

            case ArrayAssigment assign:
                Console.WriteLine($"ArrayAssignment {assign.Name.Spelling}");
                Print(assign.Index, indent + (isLast ? "    " : "│   "), true);
                break;

            case BinaryExpr bin:
                Console.WriteLine($"BinaryExpr {bin.op}");
                Print(bin.left, indent + (isLast ? "    " : "│   "), false);
                Print(bin.right, indent + (isLast ? "    " : "│   "), true);
                break;

            case UnaryExpr un:
                Console.WriteLine($"UnaryExpr {un.Operator}");
                Print(un.Operand, indent + (isLast ? "    " : "│   "), true);
                break;

            case VarExpression v:
                Console.WriteLine($"VarRef {v.Name.Spelling}");
                break;

            case Identifier id:
                Console.WriteLine($"Identifier \"{id.Spelling}\"");
                break;

            case SimpleType st:
                Console.WriteLine($"SimpleType {st.Kind}");
                break;

            default:
                Console.WriteLine(ast.GetType().Name);
                break;
        }
    }
}
