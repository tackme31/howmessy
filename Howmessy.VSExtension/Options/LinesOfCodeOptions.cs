namespace Howmessy.VSExtension.Options
{
    using Howmessy.Shared;

    using Newtonsoft.Json;

    using System;
    using System.ComponentModel;

    [JsonObject(MemberSerialization.OptIn)]
    public class LinesOfCodeOptions : BaseOptionModel<LinesOfCodeOptions>, IMetricsOptions
    {
        [Category("Threshold")]
        [DisplayName("mildly complex")]
        [Description("Specifies the threshold at which the indicator color changes from green to yellow.")]
        [DefaultValue(20)]
        [JsonProperty]
        public int Threshold1 { get; set; } = 20;

        [Category("Threshold")]
        [DisplayName("very complex")]
        [Description("Specifies the threshold at which the indicator color changes from yellow to red.")]
        [DefaultValue(30)]
        [JsonProperty]
        public int Threshold2 { get; set; } = 30;
    }
}
