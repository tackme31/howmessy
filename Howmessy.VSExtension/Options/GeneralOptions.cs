namespace Howmessy.VSExtension.Options
{
    using Howmessy.Shared;

    using Newtonsoft.Json;

    using System.ComponentModel;

    [JsonObject(MemberSerialization.OptIn)]
    public class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [Category("General")]
        [DisplayName("CodeLens metrics")]
        [Description("Specifies the metrics to be displayed on the CodeLens.")]
        [DefaultValue(MetricsType.CognitiveComplexity)]
        [TypeConverter(typeof(EnumConverter))]
        [JsonProperty]
        public MetricsType CodeLensMetrics { get; set; } = MetricsType.CognitiveComplexity;
    }
}
