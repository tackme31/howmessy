namespace Howmessy.CodeLensProvider.Options;

using Howmessy.Shared;

internal class GeneralOptions : IGeneralOptions
{
    public MetricsType CodeLensMetrics { get; set; } = MetricsType.CognitiveComplexity;

    public DisplayFormat DisplayFormat { get; set; } = DisplayFormat.Percentage;
    public string SimpleEnoughMessage { get; set; } = string.Empty;
    public string MildlyComplexMessage { get; set; } = string.Empty;
    public string VeryComplexMessage { get; set; } = string.Empty;
}
