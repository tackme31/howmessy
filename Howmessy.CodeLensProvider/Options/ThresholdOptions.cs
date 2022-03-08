namespace Howmessy.CodeLensProvider.Options
{
    using Howmessy.Shared;

    internal class ThresholdOptions : IThresholdOptions
    {
        public int CognitiveComplexityThreshold1 { get; set; }

        public int CognitiveComplexityThreshold2 { get; set; }

        public int CyclomaticComplexityThreshold1 { get; set; }

        public int CyclomaticComplexityThreshold2 { get; set; }

        public int MaintainabilityIndexThreshold1 { get; set; }

        public int MaintainabilityIndexThreshold2 { get; set; }

        public int LinesOfCodeThreshold1 { get; set; }

        public int LinesOfCodeThreshold2 { get; set; }
    }
}
