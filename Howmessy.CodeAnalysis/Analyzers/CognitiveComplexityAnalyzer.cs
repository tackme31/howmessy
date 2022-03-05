#nullable enable

namespace Howmessy.CodeAnalysis.Analyzers
{
    using Howmessy.CodeAnalysis.Walkers;

    using Microsoft.CodeAnalysis;

    public static class CognitiveComplexityAnalyzer
    {
        public static int Analyze(SyntaxNode? node)
        {
            if (node == null)
            {
                return 0;
            }

            var walker = new CognitiveComplexityWalker();
            walker.Visit(node);

            return walker.Complexity;
        }
    }
}
