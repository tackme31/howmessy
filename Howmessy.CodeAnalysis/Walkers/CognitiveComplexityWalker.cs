#nullable enable

namespace Howmessy.CodeAnalysis.Walkers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class CognitiveComplexityWalker : CSharpSyntaxWalker
    {
        private int _nesting = 0;
        public int Complexity { get; private set; } = 0;

        public CognitiveComplexityWalker() : base(SyntaxWalkerDepth.Node)
        {
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            _nesting++;

            base.VisitWhileStatement(node);

            _nesting--;

        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            _nesting++;

            base.VisitSwitchStatement(node);

            _nesting--;
        }

        public override void VisitWhenClause(WhenClauseSyntax node)
        {
            Complexity++;
            base.VisitWhenClause(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            _nesting++;

            base.VisitDoStatement(node);

            _nesting--;
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (node.Else == null)
            {
                Complexity += 1 + _nesting;
                _nesting++;
            }

            base.VisitIfStatement(node);

            if (node.Else == null)
            {
                _nesting--;
            }
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            _nesting++;

            base.VisitForStatement(node);

            _nesting--;
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            _nesting++;

            base.VisitForEachStatement(node);

            _nesting--;
        }

        public override void VisitUsingStatement(UsingStatementSyntax node)
        {
            Complexity += 1 + _nesting;
            _nesting++;

            base.VisitUsingStatement(node);

            _nesting--;
        }

        public override void VisitCatchClause(CatchClauseSyntax node)
        {
            Complexity += 1 + _nesting;
            _nesting++;

            base.VisitCatchClause(node);

            _nesting--;
        }

        public override void VisitCatchFilterClause(CatchFilterClauseSyntax node)
        {
            Complexity++;
            base.VisitCatchFilterClause(node);
        }

        public override void VisitGotoStatement(GotoStatementSyntax node)
        {
            Complexity++;

            base.VisitGotoStatement(node);
        }

        public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            _nesting++;

            base.VisitLocalFunctionStatement(node);

            _nesting--;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (IsRecursiveCall(node))
            {
                Complexity++;
            }

            base.VisitInvocationExpression(node);
        }

        public override void VisitInterpolation(InterpolationSyntax node)
        {
            _nesting++;

            base.VisitInterpolation(node);

            _nesting--;
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            _nesting++;

            base.VisitParenthesizedLambdaExpression(node);

            _nesting--;
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            Complexity++;

            base.VisitConditionalExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.OperatorToken.IsKind(SyntaxKind.AmpersandAmpersandToken) && IsNestedBinary(node) ||
                node.OperatorToken.IsKind(SyntaxKind.BarBarToken) && IsNestedBinary(node))
            {
                Complexity++;
            }

            base.VisitBinaryExpression(node);
        }

        private bool IsRecursiveCall(InvocationExpressionSyntax syntax)
        {
            return syntax.Expression is IdentifierNameSyntax identifierName && Inner(syntax);

            bool Inner(SyntaxNode node)
            {
                switch (node.Parent)
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
                        return Inner(node.Parent);
                }
            }

            // This condition alone is not enough because
            // it is not possible to determine if the method is "overload" or recursive call.
            bool IsSameNameAndParameterCount(SyntaxToken token, ParameterListSyntax parameters)
                => token.ValueText == identifierName.Identifier.ValueText
                && parameters.Parameters.Count == syntax.ArgumentList.Arguments.Count;
        }

        private bool IsNestedBinary(BinaryExpressionSyntax syntax)
        {
            return Inner(syntax);

            bool Inner(SyntaxNode node) => node.Parent switch
            {
                PrefixUnaryExpressionSyntax prefixUnary when prefixUnary.OperatorToken.IsKind(SyntaxKind.ExclamationToken) => Inner(prefixUnary),
                ParenthesizedExpressionSyntax parenthesized => Inner(parenthesized),
                BinaryExpressionSyntax binary => !binary.IsKind(syntax.Kind()),
                _ => false,
            };
        }
    }
}
