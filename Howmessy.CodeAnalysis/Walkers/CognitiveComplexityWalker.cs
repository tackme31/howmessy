#nullable enable

namespace Howmessy.CodeAnalysis.Walkers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    using System;
    using System.Collections.Generic;

    internal class CognitiveComplexityWalker : CSharpSyntaxWalker
    {
        private readonly HashSet<SyntaxNode> _ignoreOperations = new HashSet<SyntaxNode>();
        private int _nesting = 0;
        public int Complexity { get; private set; } = 0;

        public CognitiveComplexityWalker() : base(SyntaxWalkerDepth.Node)
        {
        }

        public override void Visit(SyntaxNode? node)
        {
            if (node.IsKind(SyntaxKind.LocalFunctionStatement))
            {
                VisitWithNesting(node, base.Visit);
                return;
            }


            if (node is SwitchExpressionSyntax)
            {
                Complexity += 1 + _nesting;
                VisitWithNesting(node, base.Visit);
                return;
            }

            base.Visit(node);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            VisitWithNesting(node, base.VisitWhileStatement);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            VisitWithNesting(node, base.VisitSwitchStatement);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            VisitWithNesting(node, base.VisitDoStatement);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (node.Parent is ElseClauseSyntax)
            {
                base.VisitIfStatement(node);
            }
            else
            {
                Complexity += 1 + _nesting;
                VisitWithNesting(node, base.VisitIfStatement);
            }
        }

        public override void VisitElseClause(ElseClauseSyntax node)
        {
            Complexity++;
            base.VisitElseClause(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            VisitWithNesting(node, base.VisitForStatement);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            VisitWithNesting(node, base.VisitForEachStatement);
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            Complexity += 1 + _nesting;
            VisitWithNesting(node, base.VisitCatchClause);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            base.VisitGotoStatement(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsRecursiveCall(node))
            {
                Complexity++;
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            => VisitWithNesting(node, base.VisitParenthesizedLambdaExpression);

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            => VisitWithNesting(node, base.VisitSimpleLambdaExpression);

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Complexity += 1 + _nesting;
            VisitWithNesting(node, base.VisitConditionalExpression);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            var nodeKind = node.Kind();
            if (nodeKind != SyntaxKind.LogicalAndExpression && nodeKind != SyntaxKind.LogicalOrExpression)
            {
                base.VisitBinaryExpression(node);
                return;
            }

            if (_ignoreOperations.Contains(node))
            {
                base.VisitBinaryExpression(node);
                return;
            }

            var left = RemoveParentheses(node.Left);
            if (!left.IsKind(nodeKind))
            {
                Complexity++;
            }

            var right = RemoveParentheses(node.Right);
            if (right.IsKind(nodeKind))
            {
                _ = _ignoreOperations.Add(right);
            }

            base.VisitBinaryExpression(node);
        }

        private bool IsRecursiveCall(InvocationExpressionSyntax syntax)
        {
            return syntax.Expression is IdentifierNameSyntax identifierName && Inner(syntax);

            bool Inner(SyntaxNode? node)
            {
                switch (node?.Parent)
                {
                    case LocalFunctionStatementSyntax localFun when IsSameNameAndParameterCount(localFun.Identifier, localFun.ParameterList):
                        return true;
                    case MethodDeclarationSyntax method when IsSameNameAndParameterCount(method.Identifier, method.ParameterList):
                        return true;
                    case ClassDeclarationSyntax _:
                    case StructDeclarationSyntax _:
                    case NamespaceDeclarationSyntax _:
                    case CompilationUnitSyntax _:
                        return false;
                    default:
                        return Inner(node?.Parent);
                }
            }

            // This condition alone is not enough because
            // it is not possible to determine if the method is "overload" or recursive call.
            bool IsSameNameAndParameterCount(SyntaxToken token, ParameterListSyntax parameters)
                => token.ValueText == identifierName.Identifier.ValueText
                && parameters.Parameters.Count == syntax.ArgumentList.Arguments.Count;
        }

        private SyntaxNode RemoveParentheses(SyntaxNode node) => node switch
        {
            ParenthesizedExpressionSyntax expression => RemoveParentheses(expression.Expression),
            ParenthesizedPatternSyntax pattern => RemoveParentheses(pattern.Pattern),
            _ => node,
        };

        private void VisitWithNesting<T>(T node, Action<T> visit) where T : SyntaxNode
        {
            _nesting++;
            visit(node);
            _nesting--;
        }
    }
}
