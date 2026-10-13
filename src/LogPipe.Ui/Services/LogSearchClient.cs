using System.Net;
using System.Net.Http.Json;

namespace LogPipe.Ui.Services;

public sealed class LogSearchClient(HttpClient http, OAuthTokenProvider tokens) : ILogSearchClient
{
    public async Task<SearchResponse> SearchAsync(string? q, string? level, string? service,
        int page, int size, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) query.Add($"q={Uri.EscapeDataString(q)}");
        if (!string.IsNullOrWhiteSpace(level)) query.Add($"level={Uri.EscapeDataString(level)}");
        if (!string.IsNullOrWhiteSpace(service)) query.Add($"service={Uri.EscapeDataString(service)}");
        query.Add($"page={page}");
        query.Add($"size={size}");

        return await GetAsync<SearchResponse>($"/v1/logs?{string.Join("&", query)}", ct);
    }

    public async Task<TraceResponse> GetTraceAsync(string traceId, CancellationToken ct = default) =>
        await GetAsync<TraceResponse>($"/v1/logs/trace/{Uri.EscapeDataString(traceId)}", ct);

    private async Task<T> GetAsync<T>(string path, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await tokens.GetTokenAsync(ct));

        using var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            string? detail = null;
            if (response.StatusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError)
            {
                try
                {
                    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
                    detail = problem?.Detail;
                }
                catch (System.Text.Json.JsonException)
                {
                    // non-ProblemDetails body — fall through to the status-code message
                }
            }
            throw new LogPipeApiException((int)response.StatusCode, detail);
        }

        return (await response.Content.ReadFromJsonAsync<T>(ct))
            ?? throw new LogPipeApiException((int)response.StatusCode, "empty response body");
    }

    private sealed record ProblemDetails(string? Detail);
}
