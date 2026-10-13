using LogPipe.Ui.Components;
using LogPipe.Ui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LogPipe.Ui.Tests;

/// <summary>Shared no-op ILogSearchClient for tests that render the whole app
/// (the router renders the layout, which renders the Search page at "/").</summary>
public sealed class FakeSearchClient : ILogSearchClient
{
    public Task<SearchResponse> SearchAsync(string? q, string? level, string? service,
        int page, int size, CancellationToken ct = default) =>
        Task.FromResult(new SearchResponse(0, [], new([], [])));

    public Task<TraceResponse> GetTraceAsync(string traceId, CancellationToken ct = default) =>
        Task.FromResult(new TraceResponse(traceId, []));
}

public sealed class SmokeTests : BunitContext
{
    [Fact]
    public void App_renders()
    {
        Services.AddSingleton<ILogSearchClient>(new FakeSearchClient());
        var cut = Render<Routes>();
        Assert.Contains("logpipe", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }
}
