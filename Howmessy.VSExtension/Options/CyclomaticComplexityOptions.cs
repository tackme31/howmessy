namespace Howmessy.VSExtension.Options
{
    using Howmessy.Shared;

    using Newtonsoft.Json;

    using System.ComponentModel;

    [JsonObject(MemberSerialization.OptIn)]
    public class CyclomaticComplexityOptions : BaseOptionModel<CyclomaticComplexityOptions>, IMetricsOptions
    {
        [Category("Threshold")]
        [DisplayName("mildly complex")]
        [Description("Specifies the threshold at which the indicator color changes from green to yellow.")]
        [DefaultValue(10)]
        [JsonProperty]
        public int Threshold1 { get; set; } = 10;

        [Category("Threshold")]
        [DisplayName("very complex")]
        [Description("Specifies the threshold at which the indicator color changes from yellow to red.")]
        [DefaultValue(20)]
        [JsonProperty]
        public int Threshold2 { get; set; } = 20;
    }
}
