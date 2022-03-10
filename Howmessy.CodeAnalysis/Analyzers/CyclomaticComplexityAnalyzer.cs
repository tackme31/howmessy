#nullable enable

namespace Howmessy.CodeAnalysis.Analyzers;

using Howmessy.CodeAnalysis.Walkers;

using Microsoft.CodeAnalysis;

public static class CyclomaticComplexityAnalyzer
{
    public static int Analyze(SyntaxNode? node)
    {
        if (node == null)
        {
            return 0;
        }

        var walker = new CyclomaticComplexityWalker();
        walker.Visit(node);

        return walker.Complexity;
    }
}
