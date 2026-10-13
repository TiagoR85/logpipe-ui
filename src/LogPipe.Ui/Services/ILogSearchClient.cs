namespace LogPipe.Ui.Services;

public interface ILogSearchClient
{
    Task<SearchResponse> SearchAsync(string? q, string? level, string? service,
        int page, int size, CancellationToken ct = default);
    Task<TraceResponse> GetTraceAsync(string traceId, CancellationToken ct = default);
}
