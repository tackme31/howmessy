namespace Howmessy.VSExtension.Options;

using Howmessy.Shared;

using Newtonsoft.Json;

using System.ComponentModel;

[JsonObject(MemberSerialization.OptIn)]
public class GeneralOptions : BaseOptionModel<GeneralOptions>, IGeneralOptions
{
    [Category("General")]
    [DisplayName("CodeLens metrics")]
    [Description("Specifies the metrics to be displayed in the CodeLens.")]
    [DefaultValue(MetricsType.CognitiveComplexity)]
    [TypeConverter(typeof(EnumConverter))]
    [JsonProperty]
    public MetricsType CodeLensMetrics { get; set; } = MetricsType.CognitiveComplexity;

    [Category("General")]
    [DisplayName("Display format")]
    [Description("Specifies the format in which to display in the CodeLens.")]
    [DefaultValue(DisplayFormat.Percentage)]
    [TypeConverter(typeof(EnumConverter))]
    [JsonProperty]
    public DisplayFormat DisplayFormat { get; set; } = DisplayFormat.Percentage;
}
