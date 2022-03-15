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

    [Category("Message")]
    [DisplayName("simple enough")]
    [Description("Specifies the message displayed when 'simple enough'. ($PCT$ = percentage of the metric, $VAL$ = metric value)")]
    [DefaultValue("simple enough ($PCT$)")]
    [JsonProperty]
    public string SimpleEnoughMessage { get; set; } = "simple enough ($PCT$)";

    [Category("Message")]
    [DisplayName("mildly complex")]
    [Description("Specifies the message displayed when 'mildly complex'. ($PCT$ = percentage of the metric, $VAL$ = metric value)")]
    [DefaultValue("mildly complex ($PCT$)")]
    [JsonProperty]
    public string MildlyComplexMessage { get; set; } = "mildly complex ($PCT$)";

    [Category("Message")]
    [DisplayName("very complex")]
    [Description("Specifies the message displayed when 'very complex'. ($PCT$ = percentage of the metric, $VAL$ = metric value)")]
    [DefaultValue("very complex ($PCT$)")]
    [JsonProperty]
    public string VeryComplexMessage { get; set; } = "very complex ($PCT$)";
}
