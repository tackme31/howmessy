namespace Howmessy.VSExtension.Options
{
    using Howmessy.Shared;

    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;

    [JsonObject(MemberSerialization.OptIn)]
    public class MaintainabilityIndexOptions : BaseOptionModel<MaintainabilityIndexOptions>, IMetricsOptions
    {
        [Category("Threshold")]
        [DisplayName("Threshold 1")]
        [Description("Specifies the threshold at which the indicator color changes from green to yellow.")]
        [DefaultValue(0)]
        [JsonProperty]
        public int Threshold1 { get; set; } = 20;

        [Category("Threshold")]
        [DisplayName("Threshold 2")]
        [Description("Specifies the threshold at which the indicator color changes from yellow to red.")]
        [DefaultValue(0)]
        [JsonProperty]
        public int Threshold2 { get; set; } = 10;
    }
}
