namespace Howmessy.VSExtension.Options
{
    using Howmessy.Shared;

    using Newtonsoft.Json;

    using System.ComponentModel;

    [JsonObject(MemberSerialization.OptIn)]
    public class CognitiveComplexityOptions : BaseOptionModel<CognitiveComplexityOptions>, IMetricsOptions
    {
        [Category("Threshold")]
        [DisplayName("mildly complex")]
        [Description("Specifies the threshold at which the indicator color changes from green to yellow.")]
        [DefaultValue(10)]
        [JsonProperty]
        public int Threshold1 { get; set; } = 8;

        [Category("Threshold")]
        [DisplayName("very complex")]
        [Description("Specifies the threshold at which the indicator color changes from yellow to red.")]
        [DefaultValue(15)]
        [JsonProperty]
        public int Threshold2 { get; set; } = 15;
    }
}
