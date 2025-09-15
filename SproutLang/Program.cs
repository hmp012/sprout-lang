// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

Console.WriteLine("=== Scanner Test ===\n");

// Test 1: Basic functionality with test file
ScannerTest.TestScannerWithFile();

// Test 2: Test individual token types
ScannerTest.TestTokenTypes();

// Test 3: Test error handling
ScannerTest.TestErrorHandling();

// Test 4: Test keyword recognition
ScannerTest.TestKeywords();

Console.WriteLine("\n=== All tests completed ===");
