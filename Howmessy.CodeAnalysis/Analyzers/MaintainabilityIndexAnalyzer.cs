#nullable enable

namespace Howmessy.CodeAnalysis.Analyzers;

using Howmessy.CodeAnalysis.Walkers;

using Microsoft.CodeAnalysis;

using System;

public static class MaintainabilityIndexAnalyzer
{
    public static double Analyze(SyntaxNode? node, int cyclomaticComplexity, int linesOfCode)
    {
        if (node == null)
        {
            return 0;
        }

        if (linesOfCode == 0)
        {
            return 100;
        }

        var hmWalker = new HalsteadMetricsWalker();
        hmWalker.Visit(node);
        var hm = hmWalker.Metrics;
        if (hm.Volume == 0)
        {
            return 100;
        }

        var mi = (171
                - (5.2 * Math.Log(hm.Volume))
                - (0.23 * cyclomaticComplexity)
                - (16.2 * Math.Log(linesOfCode))
            ) * 100 / 171;

        return Math.Max(0.0, mi);
    }
}
