#nullable enable

namespace Howmessy.CodeLensProvider
{
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

    enum IndicatorColor
    {
        Green, Yellow, Red
    }

    public class CodeLensDataPoint : IAsyncCodeLensDataPoint, IDisposable
    {
        public readonly Guid id = Guid.NewGuid();
        private readonly ManualResetEventSlim dataLoaded = new ManualResetEventSlim(initialState: false);
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

                var type = await GetCodeLensMetrics();
                var options = await GetMetricsOptions(type);
                var metrics = data.GetMetrics(type);

                var percentage = CalculatePercentage(type, options, metrics);
                var color = DetermineIndicatorColor(type, options, metrics);
                var description = color switch
                {
                    IndicatorColor.Green => $"simple enough ({percentage}%)",
                    IndicatorColor.Yellow => $"mildly complex ({percentage}%)",
                    IndicatorColor.Red => $"very complex ({percentage}%)",
                    _ => throw new ArgumentOutOfRangeException(),
                };

                return new CodeLensDataPointDescriptor
                {
                    Description = description,
                    TooltipText = $"See other metrics",
                    ImageId = GetImageId(color),
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

        private static List<CodeLensDetailHeaderDescriptor> CreateHeaders() => new List<CodeLensDetailHeaderDescriptor>()
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
                UniqueName = "Threshold1",
                DisplayName = "simple enough",
                Width = 90,
            },
            new CodeLensDetailHeaderDescriptor()
            {
                UniqueName = "Threshold2",
                DisplayName = "mildly complex",
                Width = 90,
            },
                        new CodeLensDetailHeaderDescriptor()
            {
                UniqueName = "Threshold2",
                DisplayName = "very complex",
                Width = 90,
            },
        };

        private async Task<CodeLensDetailEntryDescriptor> CreateEntry(MetricsType type, string name, int value)
        {
            var options = await GetMetricsOptions(type);
            var percentage = CalculatePercentage(type, options, value);
            var color = DetermineIndicatorColor(type, options, value);
            var threshold1 = options.Threshold1;
            var threshold2 = options.Threshold2;
            var (simple, mildly, very) = type switch
            {
                MetricsType.CognitiveComplexity => ($"0 - {threshold1}", $"{threshold1 + 1} - {threshold2}", $"{threshold2 + 1}+"),
                MetricsType.CyclomaticComplexity => ($"1 - {threshold1}", $"{threshold1 + 1} - {threshold2}", $"{threshold2 + 1}+"),
                MetricsType.MaintainabilityIndex => ($"100 - {threshold1}", $"{threshold1 - 1} - {threshold2}", $"{threshold2 - 1} - 0"),
                MetricsType.LinesOfCode => ($"0 - {threshold1}", $"{threshold1 + 1} - {threshold2}", $"{threshold2 + 1}+"),
                _ => throw new ArgumentOutOfRangeException(),
            };

            return CreateEntry(color, name, value, percentage, simple, mildly, very);
        }

        private int CalculatePercentage(MetricsType type, IMetricsOptions options, int value) => type switch
        {
            MetricsType.CognitiveComplexity  => (int)(value / (double)options.Threshold2 * 100),
            MetricsType.LinesOfCode          => (int)(value / (double)options.Threshold2 * 100),
            MetricsType.CyclomaticComplexity => (int)((value - 1) / (double)(options.Threshold2 - 1) * 100),
            MetricsType.MaintainabilityIndex => (int)((100 - value) / (double)(100 - options.Threshold2) * 100),
            _ => throw new ArgumentOutOfRangeException(),
        };

        private IndicatorColor DetermineIndicatorColor(MetricsType type, IMetricsOptions options, int value)
        {
            switch (type)
            {
                case MetricsType.CognitiveComplexity:
                case MetricsType.CyclomaticComplexity:
                case MetricsType.LinesOfCode:
                    return value <= options.Threshold1
                        ? IndicatorColor.Green
                        : value <= options.Threshold2
                        ? IndicatorColor.Yellow
                        : IndicatorColor.Red;
                case MetricsType.MaintainabilityIndex:
                    return value >= options.Threshold2
                        ? IndicatorColor.Green
                        : value >= options.Threshold1
                        ? IndicatorColor.Yellow
                        : IndicatorColor.Red;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private CodeLensDetailEntryDescriptor CreateEntry(IndicatorColor color, string name, int value, int percentage, string simple, string mildly, string very)
            => new CodeLensDetailEntryDescriptor
            {
                Fields = new List<CodeLensDetailEntryField>
                {
                    new CodeLensDetailEntryField()
                    {
                        ImageId = GetImageId(color),
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

        private ImageId GetImageId(IndicatorColor color) => color switch
        {
            IndicatorColor.Green => new ImageId(Guid.Parse("a1fa08e5-519b-4810-bdb0-89f586af37e9"), 13),
            IndicatorColor.Yellow => new ImageId(Guid.Parse("a1fa08e5-519b-4810-bdb0-89f586af37e9"), 2),
            IndicatorColor.Red => new ImageId(Guid.Parse("a1fa08e5-519b-4810-bdb0-89f586af37e9"), 4),
            _ => throw new ArgumentOutOfRangeException(),
        };

        // Called from VS via JSON RPC.
        public void Refresh() => _ = InvalidatedAsync?.InvokeAsync(this, EventArgs.Empty);

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

        private async Task<MetricsType> GetCodeLensMetrics()
            => await callbackService.InvokeAsync<MetricsType>(
                this,
                nameof(ICodeMetricsProvider.GetCodeLensMetrics)
                ).Caf();

        private async Task<IMetricsOptions> GetMetricsOptions(MetricsType type)
            => await callbackService.InvokeAsync<MetricsOptions>(
                this,
                nameof(ICodeMetricsProvider.GetMetricsOptions),
                new object[] { type }
                ).Caf();
    }
}
