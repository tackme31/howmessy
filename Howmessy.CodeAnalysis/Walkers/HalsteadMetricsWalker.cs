#nullable enable

namespace Howmessy.CodeAnalysis.Walkers;

using Howmessy.CodeAnalysis.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using System.Collections.Generic;
using System.Linq;

internal class HalsteadMetricsWalker : CSharpSyntaxWalker
{
    public HalsteadMetrics Metrics { get; private set; }

    public HalsteadMetricsWalker() : base(SyntaxWalkerDepth.Node) => Metrics = new HalsteadMetrics(0, 0, 0, 0);

		public override void Visit(SyntaxNode? node)
    {
        if (node == null)
        {
            return;
        }

        var tokens = node.DescendantTokens().ToList();
        var dictionary = ParseTokens(tokens, HalsteadConst.Operands);
        var dictionary2 = ParseTokens(tokens, HalsteadConst.Operators);
        var metrics = new HalsteadMetrics(
            numOperands: dictionary.Values.Sum(x => x.Count),
            numUniqueOperands: dictionary.Values.SelectMany(x => x).Distinct().Count(),
            numOperators: dictionary2.Values.Sum(x => x.Count),
            numUniqueOperators: dictionary2.Values.SelectMany(x => x).Distinct().Count());
        Metrics = metrics;
    }

    private static IDictionary<SyntaxKind, IList<string>> ParseTokens(IEnumerable<SyntaxToken> tokens, IEnumerable<SyntaxKind> filter)
    {
        var dictionary = new Dictionary<SyntaxKind, IList<string>>();
        foreach (var token in tokens)
        {
            var kind = token.Kind();
            if (!filter.Any(x => x == kind))
            {
                continue;
            }

            var valueText = token.ValueText;
            if (dictionary.TryGetValue(kind, out var list))
            {
                list.Add(valueText);
            }
            else
            {
                dictionary[kind] = new List<string>() {
                    valueText
                };
            }
        }

        return dictionary;
    }
}
