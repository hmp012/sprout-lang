

using System;
using System.Collections.Generic;
using System.IO;
using SproutLang.Scanner;

public class ScannerTest
{
    public static void TestScannerWithFile()
    {
        Console.WriteLine("Test 1: Scanning test_input.sprout");
        Console.WriteLine("=====================================");
        
        try
        {
            var scanner = new Scanner("/Users/dkhumape/Documents/7th Semester/CMC/SproutLang/test_input.sprout");
            var tokens = new List<Token>();
            
            Token token;
            do
            {
                token = scanner.Scan();
                tokens.Add(token);
                Console.WriteLine($"{token.Kind,-20} | {token.Spelling}");
            } while (token.Kind != TokenKind.EOT);
            
            Console.WriteLine($"\nTotal tokens scanned: {tokens.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine();
    }
    
    public static void TestTokenTypes()
    {
        Console.WriteLine("Test 2: Testing different token types");
        Console.WriteLine("====================================");
        
        // Create a temporary test file with various token types
        var testContent = @"123 'a' ""hello"" identifier + - * / == != < > && || ! = , ; ( ) [ ] { }";
        File.WriteAllText("temp_test.sprout", testContent);
        
        try
        {
            var scanner = new Scanner("temp_test.sprout");
            var expectedTypes = new[]
            {
                TokenKind.IntLiteral,
                TokenKind.CharLiteral,
                TokenKind.StringLiteral,
                TokenKind.Identifier,
                TokenKind.Plus,
                TokenKind.Minus,
                TokenKind.Multiply,
                TokenKind.Divide,
                TokenKind.Equals,
                TokenKind.NotEquals,
                TokenKind.LessThan,
                TokenKind.GreaterThan,
                TokenKind.And,
                TokenKind.Or,
                TokenKind.Not,
                TokenKind.Assign,
                TokenKind.Comma,
                TokenKind.Semicolon,
                TokenKind.LParenthesis,
                TokenKind.RParenthesis,
                TokenKind.LBracket,
                TokenKind.RBracket,
                TokenKind.LBrace,
                TokenKind.RBrace,
                TokenKind.EOT
            };
            
            int index = 0;
            Token token;
            do
            {
                token = scanner.Scan();
                
                if (index < expectedTypes.Length)
                {
                    bool correct = token.Kind == expectedTypes[index];
                    string status = correct ? "✓" : "✗";
                    Console.WriteLine($"{status} Expected: {expectedTypes[index],-15} Got: {token.Kind,-15} Spelling: '{token.Spelling}'");
                    
                    if (!correct)
                    {
                        Console.WriteLine($"  ERROR: Expected {expectedTypes[index]} but got {token.Kind}");
                    }
                }
                else
                {
                    Console.WriteLine($"! Unexpected token: {token}");
                }
                
                index++;
            } while (token.Kind != TokenKind.EOT);
            
            // Clean up
            File.Delete("temp_test.sprout");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine();
    }
    
    public static void TestErrorHandling()
    {
        Console.WriteLine("Test 3: Testing error handling");
        Console.WriteLine("==============================");
        
        // Test with invalid characters
        var testContent = "@#$%^&*()_+";
        File.WriteAllText("error_test.sprout", testContent);
        
        try
        {
            var scanner = new Scanner("error_test.sprout");
            
            Token token;
            do
            {
                token = scanner.Scan();
                if (token.Kind == TokenKind.Error)
                {
                    Console.WriteLine($"✓ Correctly identified error token: '{token.Spelling}'");
                }
                else if (token.Kind != TokenKind.EOT)
                {
                    Console.WriteLine($"  Valid token found: {token}");
                }
            } while (token.Kind != TokenKind.EOT);
            
            // Clean up
            File.Delete("error_test.sprout");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine();
    }
    
    // Additional helper method to test keyword recognition
    public static void TestKeywords()
    {
        Console.WriteLine("Test 4: Testing keyword recognition");
        Console.WriteLine("===================================");
        
        var keywords = new[] { "int", "bool", "char", "string", "create", "vomit", "listenCarefully", 
                              "si", "o", "sino", "repeat", "until", "sprout", "bloom", "times" };
        
        var testContent = string.Join(" ", keywords);
        File.WriteAllText("keyword_test.sprout", testContent);
        
        try
        {
            var scanner = new Scanner("keyword_test.sprout");
            
            Token token;
            do
            {
                token = scanner.Scan();
                if (token.Kind != TokenKind.EOT)
                {
                    bool isKeyword = token.IsKeyword() || 
                                   token.Kind == TokenKind.Int || token.Kind == TokenKind.Bool || 
                                   token.Kind == TokenKind.Char || token.Kind == TokenKind.String;
                    string status = isKeyword ? "✓" : "✗";
                    Console.WriteLine($"{status} {token.Kind,-20} | '{token.Spelling}'");
                }
            } while (token.Kind != TokenKind.EOT);
            
            // Clean up
            File.Delete("keyword_test.sprout");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
