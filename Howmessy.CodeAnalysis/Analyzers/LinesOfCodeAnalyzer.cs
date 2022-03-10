#nullable enable

namespace Howmessy.CodeAnalysis.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Linq;

public static class LinesOfCodeAnalyzer
{
    public static (int physical, int logical) Analyze(SyntaxNode? node, bool ignoreBlockBrackets = false)
    {
        if (node == null)
        {
            return (0, 0);
        }

        var trivias = node.DescendantTrivia().Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia));
        var removed = node.ReplaceTrivia(trivias, (t1, t2) => SyntaxFactory.CarriageReturn);

        var physical = node.ToString()
            .Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

        var logical = removed.ToString()
            .Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        var physicalLoc = physical.Length;
        var logicalLoc = logical.Length;
        if (node is BlockSyntax block && ignoreBlockBrackets)
        {
            var emptyMethod = physical.Length == 1 && block.Statements.Count == 0;
            if (emptyMethod)
            {
                physicalLoc = 0;
                logicalLoc = 0;
            }

            var openBracket = physical.Length > 1 && physical[0].Trim() == "{";
            if (openBracket)
            {
                physicalLoc--;
                logicalLoc--;
            }

            var closeBracket = physical.Length > 1 && physical[physical.Length - 1].Trim() == "}";
            if (closeBracket)
            {
                physicalLoc--;
                logicalLoc--;
            }
        }

        return (physicalLoc, logicalLoc);
    }
}
