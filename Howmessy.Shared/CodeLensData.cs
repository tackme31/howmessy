#nullable enable

namespace Howmessy.Shared; 
public class CodeLensData {
    public int CyclomaticComplexity { get; set; }
    public int CognitiveComplexity { get; set; }
    public (int Physical, int Logical) LinesOfCode { get; set; }
    public double MaintainabilityIndex { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsFailure => ErrorMessage != null;

    public CodeLensData(string? errorMessage) => ErrorMessage = errorMessage;

    public static CodeLensData Success() => new(null);

    public static CodeLensData Failure(string message) => new(message);
}

public static class CodeLensDataExtension
{
    public static int GetMetricValue(this CodeLensData data, MetricsType type) => type switch
    {
        MetricsType.CognitiveComplexity => data.CognitiveComplexity,
        MetricsType.CyclomaticComplexity => data.CyclomaticComplexity,
        MetricsType.MaintainabilityIndex => (int)data.MaintainabilityIndex,
        MetricsType.LinesOfCode => data.LinesOfCode.Logical,
        _ => throw new System.ArgumentOutOfRangeException(),
    };
}
