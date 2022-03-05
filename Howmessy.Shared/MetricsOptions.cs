namespace Howmessy.Shared
{
    public interface IMetricsOptions
    {
        int Threshold1 { get; set; }

        int Threshold2 { get; set; }
    }

    public class MetricsOptions : IMetricsOptions
    {
        public int Threshold1 { get; set; }

        public int Threshold2 { get; set; }
    }
}
