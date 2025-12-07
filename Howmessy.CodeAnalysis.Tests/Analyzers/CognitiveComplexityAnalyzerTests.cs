using System.Linq;
using FluentAssertions;
using Howmessy.CodeAnalysis.Analyzers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;

namespace Howmessy.CodeAnalysis.Tests.Analyzers;

public class CognitiveComplexityAnalyzerTests
{
    // Basic Tests
    [Fact]
    public void Analyze_NullNode_ReturnsZero()
    {
        // Arrange & Act
        var result = CognitiveComplexityAnalyzer.Analyze(null);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Analyze_EmptyMethod_ReturnsZero()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Analyze_SimpleMethodWithoutComplexity_ReturnsZero()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        var x = 1;
        var y = 2;
        var z = x + y;
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(0);
    }

    // Control Structures - If Statements
    [Fact]
    public void Analyze_SingleIfStatement_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method(bool condition)
    {
        if (condition)
        {
            Console.WriteLine(""true"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_IfElseStatement_ReturnsTwo()
    {
        // Arrange
        var code = @"
class Test
{
    void Method(bool condition)
    {
        if (condition)
        {
            Console.WriteLine(""true"");
        }
        else
        {
            Console.WriteLine(""false"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void Analyze_IfElseIfElseStatement_ReturnsThree()
    {
        // Arrange - else if does not count nesting
        var code = @"
class Test
{
    void Method(int value)
    {
        if (value == 1)
        {
            Console.WriteLine(""one"");
        }
        else if (value == 2)
        {
            Console.WriteLine(""two"");
        }
        else
        {
            Console.WriteLine(""other"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    // Control Structures - Loops
    [Fact]
    public void Analyze_WhileLoop_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        while (true)
        {
            Console.WriteLine(""loop"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_DoWhileLoop_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        do
        {
            Console.WriteLine(""test"");
        } while (true);
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_ForLoop_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(i);
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_ForEachLoop_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method(int[] items)
    {
        foreach (var item in items)
        {
            Console.WriteLine(item);
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    // Control Structures - Switch
    [Fact]
    public void Analyze_SwitchStatement_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method(int value)
    {
        switch (value)
        {
            case 1:
                Console.WriteLine(""one"");
                break;
            case 2:
                Console.WriteLine(""two"");
                break;
            default:
                Console.WriteLine(""other"");
                break;
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_SwitchExpression_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    string Method(int value)
    {
        return value switch
        {
            1 => ""one"",
            2 => ""two"",
            _ => ""other""
        };
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    // Nesting Tests
    [Fact]
    public void Analyze_NestedIfStatements_IncludesNestingPenalty()
    {
        // Arrange - outer if: +1, inner if: +1 (base) + 1 (nesting) = 3 total
        var code = @"
class Test
{
    void Method(bool a, bool b)
    {
        if (a)
        {
            if (b)
            {
                Console.WriteLine(""both true"");
            }
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void Analyze_NestedLoops_IncludesNestingPenalty()
    {
        // Arrange - outer for: +1, inner for: +1 (base) + 1 (nesting) = 3 total
        var code = @"
class Test
{
    void Method()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Console.WriteLine($""{i},{j}"");
            }
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void Analyze_DeeplyNestedStructures_IncludesNestingPenalty()
    {
        // Arrange - test nesting level 3+
        // if: +1, for: +1+1=+2 (nesting), while: +1+2=+3 (nesting), total = 6
        var code = @"
class Test
{
    void Method(bool condition)
    {
        if (condition)
        {
            for (int i = 0; i < 10; i++)
            {
                while (true)
                {
                    Console.WriteLine(""deep"");
                    break;
                }
            }
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(6);
    }

    [Fact]
    public void Analyze_MixedNesting_IncludesNestingPenalty()
    {
        // Arrange - if inside loop: for +1, if: +1 (base) + 1 (nesting) = 3 total
        var code = @"
class Test
{
    void Method(int[] items)
    {
        foreach (var item in items)
        {
            if (item > 0)
            {
                Console.WriteLine(item);
            }
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    // Exception Handling
    [Fact]
    public void Analyze_TryCatchBlock_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        try
        {
            Console.WriteLine(""test"");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_MultipleCatchClauses_CountsEachCatch()
    {
        // Arrange - each catch adds +1
        var code = @"
class Test
{
    void Method()
    {
        try
        {
            Console.WriteLine(""test"");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(""arg"");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(""invalid"");
        }
        catch (Exception ex)
        {
            Console.WriteLine(""general"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void Analyze_NestedTryCatch_IncludesNestingPenalty()
    {
        // Arrange - outer catch: +1, inner catch: +1 (base) + 1 (nesting) = 3 total
        var code = @"
class Test
{
    void Method()
    {
        try
        {
            Console.WriteLine(""outer"");
        }
        catch (Exception outer)
        {
            try
            {
                Console.WriteLine(""inner"");
            }
            catch (Exception inner)
            {
                Console.WriteLine(""nested"");
            }
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    // Logical Operators
    [Fact]
    public void Analyze_LogicalAndExpression_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method(bool a, bool b)
    {
        if (a && b)
        {
            Console.WriteLine(""both true"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2); // if: +1, &&: +1
    }

    [Fact]
    public void Analyze_LogicalOrExpression_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method(bool a, bool b)
    {
        if (a || b)
        {
            Console.WriteLine(""at least one true"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2); // if: +1, ||: +1
    }

    [Fact]
    public void Analyze_MultipleAndExpressions_CountsOnlyFirst()
    {
        // Arrange - a && b && c should be if: +1, first &&: +1 = 2 total
        var code = @"
class Test
{
    void Method(bool a, bool b, bool c)
    {
        if (a && b && c)
        {
            Console.WriteLine(""all true"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void Analyze_MultipleOrExpressions_CountsOnlyFirst()
    {
        // Arrange - a || b || c should be if: +1, first ||: +1 = 2 total
        var code = @"
class Test
{
    void Method(bool a, bool b, bool c)
    {
        if (a || b || c)
        {
            Console.WriteLine(""at least one true"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void Analyze_MixedAndOrExpressions_CountsEachChange()
    {
        // Arrange - a && b || c should be if: +1, &&: +1, ||: +1 = 3 total
        var code = @"
class Test
{
    void Method(bool a, bool b, bool c)
    {
        if (a && b || c)
        {
            Console.WriteLine(""complex"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void Analyze_ParenthesizedLogicalExpressions_HandlesCorrectly()
    {
        // Arrange - (a && b) || (c && d): if: +1, first &&: +1, ||: +1, second &&: +1 = 4 total
        var code = @"
class Test
{
    void Method(bool a, bool b, bool c, bool d)
    {
        if ((a && b) || (c && d))
        {
            Console.WriteLine(""complex"");
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(4);
    }

    // Conditional (Ternary) Operator
    [Fact]
    public void Analyze_TernaryOperator_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    int Method(bool condition)
    {
        return condition ? 1 : 0;
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_NestedTernaryOperator_IncludesNestingPenalty()
    {
        // Arrange - outer ternary: +1, inner ternary: +1 (base) + 1 (nesting) = 3 total
        var code = @"
class Test
{
    int Method(bool a, bool b)
    {
        return a ? (b ? 1 : 2) : 3;
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    // Lambda Expressions
    [Fact]
    public void Analyze_SimpleLambdaWithIfStatement_IncludesNestingPenalty()
    {
        // Arrange - lambda increases nesting but doesn't add complexity itself
        // if inside lambda: +1 (base) + 1 (nesting) = 2 total
        var code = @"
class Test
{
    void Method(int[] items)
    {
        var filtered = items.Where(x =>
        {
            if (x > 0)
            {
                return true;
            }
            return false;
        });
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public void Analyze_ParenthesizedLambdaWithIfStatement_IncludesNestingPenalty()
    {
        // Arrange - if inside lambda: +1 (base) + 1 (nesting) = 2 total
        var code = @"
class Test
{
    void Method(int[] items)
    {
        var filtered = items.Where((x) =>
        {
            if (x > 0)
            {
                return true;
            }
            return false;
        });
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2);
    }

    // Local Functions
    [Fact]
    public void Analyze_LocalFunctionWithIfStatement_IncludesNestingPenalty()
    {
        // Arrange - if inside local function: +1 (base) + 1 (nesting) = 2 total
        var code = @"
class Test
{
    void Method()
    {
        void LocalFunc(bool condition)
        {
            if (condition)
            {
                Console.WriteLine(""true"");
            }
        }

        LocalFunc(true);
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2);
    }

    // Recursive Calls
    [Fact]
    public void Analyze_RecursiveMethodCall_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    int Factorial(int n)
    {
        if (n <= 1)
        {
            return 1;
        }
        return n * Factorial(n - 1);
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(2); // if: +1, recursion: +1
    }

    [Fact]
    public void Analyze_NonRecursiveMethodCall_ReturnsZero()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        OtherMethod();
    }

    void OtherMethod()
    {
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void Analyze_RecursiveLocalFunctionCall_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        int LocalFactorial(int n)
        {
            if (n <= 1)
            {
                return 1;
            }
            return n * LocalFactorial(n - 1);
        }

        var result = LocalFactorial(5);
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3); // if inside local: +1 (base) + 1 (nesting), recursion: +1
    }

    // Goto Statement
    [Fact]
    public void Analyze_GotoStatement_ReturnsOne()
    {
        // Arrange
        var code = @"
class Test
{
    void Method()
    {
        goto end;
        Console.WriteLine(""skip"");
        end:
        Console.WriteLine(""end"");
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void Analyze_NestedGotoStatement_IncludesNestingPenalty()
    {
        // Arrange - goto inside if: +1 (if) + 1 (goto base) + 1 (nesting) = 3 total
        var code = @"
class Test
{
    void Method(bool condition)
    {
        if (condition)
        {
            goto end;
        }
        Console.WriteLine(""middle"");
        end:
        Console.WriteLine(""end"");
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(3);
    }

    // Complex Real-World Scenarios
    [Fact]
    public void Analyze_ComplexMethodWithMultipleStructures_CalculatesCorrectly()
    {
        // Arrange - combination of if, loops, and logical operators
        // if: +1, &&: +1, for: +1+1=+2 (nesting), if inside for: +1+2=+3 (nesting), total = 7
        var code = @"
class Test
{
    void Method(bool condition, int[] items)
    {
        if (condition && items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] > 0)
                {
                    Console.WriteLine(items[i]);
                }
            }
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(7);
    }

    [Fact]
    public void Analyze_RealWorldExample_MatchesExpectedComplexity()
    {
        // Arrange - real code example with known complexity
        // Complexity breakdown:
        // - if (items == null || items.Length == 0): +1 (if), +1 (||) = +2
        // - foreach: +1
        // - if (item <= 0): +1 (if base) + 1 (nesting) = +2
        // - else if (item > 100): +1
        // - else: +1
        // Total: 2 + 1 + 2 + 1 + 1 = 7
        var code = @"
class Test
{
    void ProcessItems(int[] items)
    {
        if (items == null || items.Length == 0)
        {
            return;
        }

        foreach (var item in items)
        {
            if (item <= 0)
            {
                Console.WriteLine(""Invalid"");
            }
            else if (item > 100)
            {
                Console.WriteLine(""Too large"");
            }
            else
            {
                Console.WriteLine($""Valid: {item}"");
            }
        }
    }
}";
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = tree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var result = CognitiveComplexityAnalyzer.Analyze(method);

        // Assert
        result.Should().Be(7);
    }
}
