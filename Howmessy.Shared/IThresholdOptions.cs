namespace Howmessy.Shared
{
    public interface IThresholdOptions
    {
        int CognitiveComplexityThreshold1 { get; }
        int CognitiveComplexityThreshold2 { get; }
        int CyclomaticComplexityThreshold1 { get; }
        int CyclomaticComplexityThreshold2 { get; }
        int MaintainabilityIndexThreshold1 { get; }
        int MaintainabilityIndexThreshold2 { get; }
        int LinesOfCodeThreshold1 { get; }
        int LinesOfCodeThreshold2 { get; }
    }
}
