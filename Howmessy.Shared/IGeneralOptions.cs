namespace Howmessy.Shared;

public interface IGeneralOptions
{
    MetricsType CodeLensMetrics { get; }

    DisplayFormat DisplayFormat { get; }

    string SimpleEnoughMessage { get; set; }

    string MildlyComplexMessage { get; set; }

    string VeryComplexMessage { get; set; }
}
