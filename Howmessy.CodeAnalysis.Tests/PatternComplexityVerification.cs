using Xunit;
using Xunit.Abstractions;

namespace Howmessy.CodeAnalysis.Tests;

/// <summary>
/// This test outputs the actual complexity values for pattern matching features
/// Run this to see what the current implementation returns
/// </summary>
public class PatternComplexityVerification
{
    private readonly ITestOutputHelper _output;

    public PatternComplexityVerification(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OutputCurrentPatternComplexityValues()
    {
        using var sw = new System.IO.StringWriter();
        var originalOut = System.Console.Out;
        System.Console.SetOut(sw);

        PatternTestHelper.VerifyPatternComplexity();

        System.Console.SetOut(originalOut);
        _output.WriteLine(sw.ToString());
    }
}
