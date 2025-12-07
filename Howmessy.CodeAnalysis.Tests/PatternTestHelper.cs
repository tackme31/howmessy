using System;
using System.Linq;
using Howmessy.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Howmessy.CodeAnalysis.Tests;

/// <summary>
/// Helper to verify actual complexity values for pattern matching tests
/// </summary>
public static class PatternTestHelper
{
    public static void VerifyPatternComplexity()
    {
        Console.WriteLine("=== Pattern Matching Complexity Verification ===\n");

        // Relational Pattern
        VerifyCode("Relational Pattern (> 90)", @"
class Test
{
    string GetGrade(int score)
    {
        return score switch
        {
            > 90 => ""A"",
            > 80 => ""B"",
            > 70 => ""C"",
            > 60 => ""D"",
            _ => ""F""
        };
    }
}");

        // And Pattern
        VerifyCode("And Pattern (is int and > 0)", @"
class Test
{
    void Method(object obj)
    {
        if (obj is int and > 0)
        {
            Console.WriteLine(""Positive integer"");
        }
    }
}");

        // Or Pattern
        VerifyCode("Or Pattern (is string or char)", @"
class Test
{
    void Method(object obj)
    {
        if (obj is string or char)
        {
            Console.WriteLine(""Text type"");
        }
    }
}");

        // Not Pattern
        VerifyCode("Not Pattern (is not null)", @"
class Test
{
    void Method(object obj)
    {
        if (obj is not null)
        {
            Console.WriteLine(""Not null"");
        }
    }
}");

        // List Pattern
        VerifyCode("List Pattern ([1, 2, 3])", @"
class Test
{
    void Method(int[] numbers)
    {
        if (numbers is [1, 2, 3])
        {
            Console.WriteLine(""Matched"");
        }
    }
}");

        // Mixed And/Or Patterns
        VerifyCode("Mixed And/Or Pattern", @"
class Test
{
    void Method(int value)
    {
        if (value is (> 0 and < 50) or (> 100 and < 200))
        {
            Console.WriteLine(""Valid range"");
        }
    }
}");

        // Multiple And Patterns
        VerifyCode("Multiple And Patterns", @"
class Test
{
    void Method(int value)
    {
        if (value is > 0 and < 100 and not 50)
        {
            Console.WriteLine(""Valid"");
        }
    }
}");

        // Multiple Or Patterns
        VerifyCode("Multiple Or Patterns", @"
class Test
{
    void Method(object obj)
    {
        if (obj is int or long or short)
        {
            Console.WriteLine(""Integer type"");
        }
    }
}");
    }

    private static void VerifyCode(string description, string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        var result = CognitiveComplexityAnalyzer.Analyze(method);

        Console.WriteLine($"{description}:");
        Console.WriteLine($"  Complexity: {result}");
        Console.WriteLine();
    }
}
