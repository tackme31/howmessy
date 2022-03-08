namespace Howmessy.VSExtension.Options
{
    using Howmessy.Shared;

    using Newtonsoft.Json;

    using System.ComponentModel;

    [JsonObject(MemberSerialization.OptIn)]
    public class ThresholdOptions : BaseOptionModel<ThresholdOptions>, IThresholdOptions
    {
        [Category("Cognitive Complexity")]
        [DisplayName("mildly complex")]
        [Description("Specifies the threshold of mildly complex.")]
        [DefaultValue(10)]
        [JsonProperty]
        public int CognitiveComplexityThreshold1 { get; set; } = 8;

        [Category("Cognitive Complexity")]
        [DisplayName("very complex")]
        [Description("Specifies the threshold of very complex.")]
        [DefaultValue(15)]
        [JsonProperty]
        public int CognitiveComplexityThreshold2 { get; set; } = 15;

        [Category("Cyclomatic Complexity")]
        [DisplayName("mildly complex")]
        [Description("Specifies the threshold of mildly complex.")]
        [DefaultValue(10)]
        [JsonProperty]
        public int CyclomaticComplexityThreshold1 { get; set; } = 10;

        [Category("Cyclomatic Complexity")]
        [DisplayName("very complex")]
        [Description("Specifies the threshold of very complex.")]
        [DefaultValue(20)]
        [JsonProperty]
        public int CyclomaticComplexityThreshold2 { get; set; } = 20;

        [Category("Maintainability Index")]
        [DisplayName("mildly complex")]
        [Description("Specifies the threshold of mildly complex.")]
        [DefaultValue(0)]
        [JsonProperty]
        public int MaintainabilityIndexThreshold1 { get; set; } = 20;

        [Category("Maintainability Index")]
        [DisplayName("very complex")]
        [Description("Specifies the threshold of very complex.")]
        [DefaultValue(0)]
        [JsonProperty]
        public int MaintainabilityIndexThreshold2 { get; set; } = 10;

        [Category("Lines of Code")]
        [DisplayName("mildly complex")]
        [Description("Specifies the threshold of mildly complex.")]
        [DefaultValue(20)]
        [JsonProperty]
        public int LinesOfCodeThreshold1 { get; set; } = 20;

        [Category("Lines of Code")]
        [DisplayName("very complex")]
        [Description("Specifies the threshold of very complex.")]
        [DefaultValue(30)]
        [JsonProperty]
        public int LinesOfCodeThreshold2 { get; set; } = 30;
    }
}
