#nullable enable

namespace Howmessy.CodeLensProvider;

using Howmessy.CodeLensProvider.Options;
using Howmessy.Shared;

using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.CodeLens;
using Microsoft.VisualStudio.Language.CodeLens.Remoting;
using Microsoft.VisualStudio.Threading;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using static Howmessy.Shared.Logging;

enum ComplexityLevel
{
    SimpleEnough, MildlyComplex, VeryComplex
}

public class CodeLensDataPoint : IAsyncCodeLensDataPoint, IDisposable
{
    public readonly Guid id = Guid.NewGuid();
    private readonly ManualResetEventSlim dataLoaded = new(initialState: false);
    private readonly ICodeLensCallbackService callbackService;
    private VisualStudioConnectionHandler? visualStudioConnection;
    private volatile CodeLensData? data;

    public CodeLensDescriptor Descriptor { get; }

    public event AsyncEventHandler? InvalidatedAsync;

    public CodeLensDataPoint(ICodeLensCallbackService callbackService, CodeLensDescriptor descriptor)
    {
        this.callbackService = callbackService;
        Descriptor = descriptor;
    }

    public void Dispose()
    {
        visualStudioConnection?.Dispose();
        dataLoaded.Dispose();
    }

    public async Task ConnectToVisualStudio(int vspid) =>
        visualStudioConnection = await VisualStudioConnectionHandler.Create(owner: this, vspid).Caf();

    public async Task<CodeLensDataPointDescriptor> GetDataAsync(CodeLensDescriptorContext context, CancellationToken ct)
    {
        try
        {
            data = await LoadCodeMetrics(context, ct).Caf();
            dataLoaded.Set();

            var generalOptions = await GetGeneralOptions();
            var thresholdOptions = await GetThresholdOptions();
            var (threshold1, threshold2) = GetThreshold(thresholdOptions, generalOptions.CodeLensMetrics);

            var metricValue = data.GetMetricValue(generalOptions.CodeLensMetrics);
            var percentage = CalculatePercentage(generalOptions.CodeLensMetrics, threshold2, metricValue);
            var complexityLevel = DetermineComplexityLevel(generalOptions.CodeLensMetrics, threshold1, threshold2, metricValue);

            var message = complexityLevel switch
            {
                ComplexityLevel.SimpleEnough => generalOptions.SimpleEnoughMessage,
                ComplexityLevel.MildlyComplex => generalOptions.MildlyComplexMessage,
                ComplexityLevel.VeryComplex => generalOptions.VeryComplexMessage,
                _ => throw new InvalidOperationException(),
            };

            return new CodeLensDataPointDescriptor
            {
                Description = message.Replace("$PCT$", $"{percentage}%").Replace("$VAL$", $"{metricValue}"),
                TooltipText = $"See other metrics",
                ImageId = GetImageId(complexityLevel),
            };
        }
        catch (Exception ex)
        {
            LogCL(ex);
            throw;
        }
    }

    public async Task<CodeLensDetailsDescriptor> GetDetailsAsync(CodeLensDescriptorContext context, CancellationToken ct)
    {
        try
        {
            // When opening the details pane, the data point is re-created leaving `data` uninitialized. VS will
            // then call `GetDataAsync()` and `GetDetailsAsync()` concurrently.
            if (!dataLoaded.Wait(timeout: TimeSpan.FromSeconds(.5), ct))
            {
                data = await LoadCodeMetrics(context, ct).Caf();
            }

            if (data!.IsFailure)
            {
                throw new InvalidOperationException($"Getting CodeLens details for {context.FullName()} failed: {data.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            LogCL(ex);
            throw;
        }

        return new CodeLensDetailsDescriptor
        {
            Headers = CreateHeaders(),
            Entries = new List<CodeLensDetailEntryDescriptor>
            {
                await CreateEntry(MetricsType.CognitiveComplexity, "Cognitive Complexity", data.CognitiveComplexity),
                await CreateEntry(MetricsType.CyclomaticComplexity, "Cyclomatic Complexity", data.CyclomaticComplexity),
                await CreateEntry(MetricsType.MaintainabilityIndex, "Maintainability Index", (int)data.MaintainabilityIndex),
                await CreateEntry(MetricsType.LinesOfCode, "Lines of Code", data.LinesOfCode.Logical),
            },
        };
    }

    private static List<CodeLensDetailHeaderDescriptor> CreateHeaders() => new()
    {
        new CodeLensDetailHeaderDescriptor()
        {
            UniqueName = "Color",
            DisplayName = "  ",
            Width = 25,
        },
        new CodeLensDetailHeaderDescriptor()
        {
            UniqueName = "Metrics",
            DisplayName = "Metrics",
            Width = 150,
        },
        new CodeLensDetailHeaderDescriptor()
        {
            UniqueName = "Percentage",
            DisplayName = "Percentage",
            Width = 75,
        },
        new CodeLensDetailHeaderDescriptor()
        {
            UniqueName = "Value",
            DisplayName = "Value",
            Width = 50,
        },
        new CodeLensDetailHeaderDescriptor()
        {
            UniqueName = "SimpeEnough",
            DisplayName = "simple enough",
            Width = 90,
        },
        new CodeLensDetailHeaderDescriptor()
        {
            UniqueName = "MildlyComplex",
            DisplayName = "mildly complex",
            Width = 90,
        },
        new CodeLensDetailHeaderDescriptor()
        {
            UniqueName = "VeryComplex",
            DisplayName = "very complex",
            Width = 90,
        },
    };

    private async Task<CodeLensDetailEntryDescriptor> CreateEntry(MetricsType type, string name, int value)
    {
        var options = await GetThresholdOptions();
        var (threshold1, threshold2) = GetThreshold(options, type);
        var percentage = CalculatePercentage(type, threshold2, value);
        var complexityType = DetermineComplexityLevel(type, threshold1, threshold2, value);


        var (simple, mildly, very) = type switch
        {
            MetricsType.CognitiveComplexity => ($"0 - {threshold1}", $"{threshold1 + 1} - {threshold2}", $"{threshold2 + 1}+"),
            MetricsType.CyclomaticComplexity => ($"1 - {threshold1}", $"{threshold1 + 1} - {threshold2}", $"{threshold2 + 1}+"),
            MetricsType.MaintainabilityIndex => ($"100 - {threshold1}", $"{threshold1 - 1} - {threshold2}", $"{threshold2 - 1} - 0"),
            MetricsType.LinesOfCode => ($"0 - {threshold1}", $"{threshold1 + 1} - {threshold2}", $"{threshold2 + 1}+"),
            _ => throw new ArgumentOutOfRangeException(),
        };

        return CreateEntry(complexityType, name, percentage, value, simple, mildly, very);
    }

    private (int threshold1, int threshold2) GetThreshold(IThresholdOptions options, MetricsType type) => type switch
    {
        MetricsType.CognitiveComplexity => (options.CognitiveComplexityThreshold1, options.CognitiveComplexityThreshold2),
        MetricsType.CyclomaticComplexity => (options.CyclomaticComplexityThreshold1, options.CyclomaticComplexityThreshold2),
        MetricsType.MaintainabilityIndex => (options.MaintainabilityIndexThreshold1, options.MaintainabilityIndexThreshold2),
        MetricsType.LinesOfCode => (options.LinesOfCodeThreshold1, options.LinesOfCodeThreshold2),
        _ => throw new NotSupportedException(),
    };

    private int CalculatePercentage(MetricsType type, int threshold2, int value) => type switch
    {
        MetricsType.CognitiveComplexity  => (int)(value / (double)threshold2 * 100),
        MetricsType.LinesOfCode          => (int)(value / (double)threshold2 * 100),
        MetricsType.CyclomaticComplexity => (int)((value - 1) / (double)(threshold2 - 1) * 100),
        MetricsType.MaintainabilityIndex => (int)((100 - value) / (double)(100 - threshold2) * 100),
        _ => throw new ArgumentOutOfRangeException(),
    };

    private ComplexityLevel DetermineComplexityLevel(MetricsType type, int threshold1, int threshold2, int value) => type switch
    {
        MetricsType.CognitiveComplexity or MetricsType.CyclomaticComplexity or MetricsType.LinesOfCode
            => value <= threshold1
            ? ComplexityLevel.SimpleEnough
            : value <= threshold2
            ? ComplexityLevel.MildlyComplex
            : ComplexityLevel.VeryComplex,
        MetricsType.MaintainabilityIndex
            => value >= threshold1
            ? ComplexityLevel.SimpleEnough
            : value >= threshold2
            ? ComplexityLevel.MildlyComplex
            : ComplexityLevel.VeryComplex,
        _ => throw new ArgumentOutOfRangeException(),
    };

    private CodeLensDetailEntryDescriptor CreateEntry(ComplexityLevel complexityType, string name, int percentage, int value, string simple, string mildly, string very)
        => new()
        {
            Fields = new List<CodeLensDetailEntryField>
            {
                new CodeLensDetailEntryField()
                {
                    ImageId = GetImageId(complexityType),
                },
                new CodeLensDetailEntryField()
                {
                    Text = name,
                },
                new CodeLensDetailEntryField()
                {
                    Text = $"{percentage}%",
                },
                new CodeLensDetailEntryField()
                {
                    Text = value.ToString(),
                },
                new CodeLensDetailEntryField()
                {
                    Text = simple,
                },
                new CodeLensDetailEntryField()
                {
                    Text = mildly,
                },
                new CodeLensDetailEntryField()
                {
                    Text = very,
                },
            }
        };

    private ImageId GetImageId(ComplexityLevel complexityType) => complexityType switch
    {
        ComplexityLevel.SimpleEnough => new ImageId(Guid.Parse("f64bd60c-175b-481f-95b7-b126a5ebc53f"), 1),
        ComplexityLevel.MildlyComplex => new ImageId(Guid.Parse("f64bd60c-175b-481f-95b7-b126a5ebc53f"), 2),
        ComplexityLevel.VeryComplex => new ImageId(Guid.Parse("f64bd60c-175b-481f-95b7-b126a5ebc53f"), 3),
        _ => throw new ArgumentOutOfRangeException(),
    };

    // Called from VS via JSON RPC.
    public void Refresh() => InvalidatedAsync?.InvokeAsync(this, EventArgs.Empty);

    private async Task<CodeLensData> LoadCodeMetrics(CodeLensDescriptorContext ctx, CancellationToken ct)
        => await callbackService
            .InvokeAsync<CodeLensData>(
                this,
                nameof(ICodeMetricsProvider.LoadCodeMetrics),
                new object[] {
                    id,
                    Descriptor.ProjectGuid,
                    Descriptor.FilePath,
                    ctx.ApplicableSpan != null
                        ? ctx.ApplicableSpan.Value.Start
                        : throw new InvalidOperationException($"No ApplicableSpan given for {ctx.FullName()}."),
                    ctx.ApplicableSpan!.Value.Length
                },
                ct).Caf();

    private async Task<IGeneralOptions> GetGeneralOptions()
        => await callbackService.InvokeAsync<GeneralOptions>(
            this,
            nameof(ICodeMetricsProvider.GetGeneralOptions)
            ).Caf();

    private async Task<IThresholdOptions> GetThresholdOptions()
        => await callbackService.InvokeAsync<ThresholdOptions>(
            this,
            nameof(ICodeMetricsProvider.GetThresholdOptions)
            ).Caf();
}
