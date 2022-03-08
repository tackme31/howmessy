#nullable enable

namespace Howmessy.Shared {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICodeMetricsProvider {
        int GetVisualStudioPid();

        Task<IGeneralOptions> GetGeneralOptions();

        Task<IMetricsOptions> GetMetricsOptions(MetricsType type);

        Task<CodeLensData> LoadCodeMetrics(Guid dataPointId, Guid projGuid, string filePath, int textStart, int textLen, CancellationToken ct);
    }
}
