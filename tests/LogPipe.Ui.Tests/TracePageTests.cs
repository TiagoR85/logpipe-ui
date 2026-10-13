using LogPipe.Ui.Components.Pages;
using LogPipe.Ui.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace LogPipe.Ui.Tests;

public sealed class TracePageTests : BunitContext
{
    private sealed class FakeTraceClient : ILogSearchClient
    {
        public Task<SearchResponse> SearchAsync(string? q, string? level, string? service,
            int page, int size, CancellationToken ct = default) =>
            Task.FromResult(new SearchResponse(0, [], new([], [])));

        public Task<TraceResponse> GetTraceAsync(string traceId, CancellationToken ct = default) =>
            Task.FromResult(new TraceResponse(traceId,
            [
                // The real search API returns entries ascending by timestamp (Task 8) —
                // the fake mirrors that contract; the component renders in received order.
                new SearchHit("e1", DateTimeOffset.UtcNow, "Information", "api", "before", traceId, null, null),
                new SearchHit("e2", DateTimeOffset.UtcNow.AddSeconds(1), "Information", "api", "after", traceId, null, null)
            ]));
    }

    [Fact]
    public async Task Renders_trace_entries_ascending()
    {
        Services.AddSingleton<ILogSearchClient>(new FakeTraceClient());
        var cut = Render<Trace>(parameters => parameters.Add(p => p.TraceId, "abc123"));
        await Task.Yield();
        cut.WaitForAssertion(() => Assert.Contains("before", cut.Markup));

        var rows = cut.FindAll("[data-testid=trace-row]");
        Assert.Equal(2, rows.Count);
        Assert.Contains("before", rows[0].TextContent);
        Assert.Contains("after", rows[1].TextContent);
    }
}
