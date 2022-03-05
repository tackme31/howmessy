#nullable enable

namespace Howmessy.CodeAnalysis.Walkers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal class CyclomaticComplexityWalker : CSharpSyntaxWalker
    {
        public int Complexity { get; private set; } = 1;

        public CyclomaticComplexityWalker() : base(SyntaxWalkerDepth.Node)
        {
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            Complexity++;

            base.VisitWhileStatement(node);
        }

        public override void VisitCaseSwitchLabel(CaseSwitchLabelSyntax node)
        {
            Complexity++;

            base.VisitCaseSwitchLabel(node);
        }

        public override void VisitCasePatternSwitchLabel(CasePatternSwitchLabelSyntax node)
        {
            Complexity++;

            base.VisitCasePatternSwitchLabel(node);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            Complexity++;

            base.VisitDoStatement(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            Complexity++;

            base.VisitIfStatement(node);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            Complexity++;

            base.VisitForStatement(node);
        }
    }
}
