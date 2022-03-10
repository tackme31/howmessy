#nullable enable

namespace Howmessy.VSExtension;
using Howmessy.CodeAnalysis.Analyzers;
using Howmessy.Shared;
using Howmessy.VSExtension.Options;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Utilities;

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Howmessy.Shared.Logging;

[Export(typeof(ICodeLensCallbackListener))]
[ContentType("CSharp")]
public class CodeMetricsProvider : ICodeLensCallbackListener, ICodeMetricsProvider
{
    private readonly VisualStudioWorkspace workspace;

    [ImportingConstructor]
    public CodeMetricsProvider(VisualStudioWorkspace workspace) => this.workspace = workspace;

    public async Task<IGeneralOptions> GetGeneralOptions() => await GeneralOptions.GetLiveInstanceAsync().Caf();

    public async Task<IThresholdOptions> GetThresholdOptions() => await ThresholdOptions.GetLiveInstanceAsync().Caf();

    public int GetVisualStudioPid() => Process.GetCurrentProcess().Id;

    public async Task<CodeLensData> LoadCodeMetrics(
        Guid dataPointId,
        Guid projGuid,
        string filePath,
        int textStart,
        int textLen,
        CancellationToken ct)
    {
        try
        {
            var document = workspace.GetDocument(filePath, projGuid);
            var method = await GetMethod(document, textStart, textLen);

            var cyclomaticComplexity = CyclomaticComplexityAnalyzer.Analyze(method);
            var cognitiveComplexity = CognitiveComplexityAnalyzer.Analyze(method);
            var (physicalLoc, logicalLoc) = method?.ExpressionBody == null
                ? LinesOfCodeAnalyzer.Analyze(method?.Body, ignoreBlockBrackets: true)
                : LinesOfCodeAnalyzer.Analyze(method?.ExpressionBody?.Expression, ignoreBlockBrackets: false);
            var maintainabilityIndex = MaintainabilityIndexAnalyzer.Analyze(method, cyclomaticComplexity, logicalLoc);
            var data = new CodeLensData(null)
            {
                CyclomaticComplexity = cyclomaticComplexity,
                CognitiveComplexity = cognitiveComplexity,
                LinesOfCode = (physicalLoc, logicalLoc),
                MaintainabilityIndex = maintainabilityIndex,
            };

            return data;
        }
        catch (Exception ex)
        {
            LogVS(ex);
            return CodeLensData.Failure(ex.ToString());
        }
    }

    private async Task<BaseMethodDeclarationSyntax?> GetMethod(Document document, int textStart, int textLen)
    {
        var root = await document.GetSyntaxRootAsync();
        return root?.DescendantNodes()
            .OfType<BaseMethodDeclarationSyntax>()
            .FirstOrDefault(node => node.Span.Start == textStart && node.Span.Length == textLen);
    }
}
