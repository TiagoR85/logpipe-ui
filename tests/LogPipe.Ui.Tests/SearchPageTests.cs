using LogPipe.Ui.Components;
using LogPipe.Ui.Components.Pages;
using LogPipe.Ui.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LogPipe.Ui.Tests;

public sealed class SearchPageTests : BunitContext
{
    private sealed class FakeSearchClient : ILogSearchClient
    {
        public SearchResponse Response { get; set; } = new(0, [], new([], []));
        public string? LastQ { get; private set; }
        public string? LastLevel { get; private set; }
        public string? LastService { get; private set; }
        public int LastPage { get; private set; } = 1;

        public Task<SearchResponse> SearchAsync(string? q, string? level, string? service,
            int page, int size, CancellationToken ct = default)
        {
            (LastQ, LastLevel, LastService, LastPage) = (q, level, service, page);
            return Task.FromResult(Response);
        }

        public Task<TraceResponse> GetTraceAsync(string traceId, CancellationToken ct = default) =>
            Task.FromResult(new TraceResponse(traceId, []));
    }

    private FakeSearchClient Register(FakeSearchClient? client = null)
    {
        client ??= new FakeSearchClient();
        Services.AddSingleton<ILogSearchClient>(client);
        return client;
    }

    [Fact]
    public async Task Renders_hits_with_testids()
    {
        var fake = Register(new FakeSearchClient
        {
            Response = new SearchResponse(1,
                [new SearchHit("e1", DateTimeOffset.UtcNow, "Error", "api", "boom", null, null, null)],
                new SearchFacets([new FacetBucket("Error", 1)], [new FacetBucket("api", 1)]))
        });

        var cut = Render<Search>();
        await Task.Yield();
        cut.WaitForAssertion(() => Assert.Contains("boom", cut.Markup));

        Assert.Single(cut.FindAll("[data-testid=result-row]"));
        Assert.Contains("boom", cut.Markup);
        Assert.Contains("Error", cut.Markup);
        Assert.Contains(cut.FindAll("[data-testid=facet-level]"), e => e.TextContent.Contains("Error"));
        Assert.Contains(cut.FindAll("[data-testid=facet-service]"), e => e.TextContent.Contains("api"));
        Assert.Equal(1, fake.LastPage);
    }

    [Fact]
    public async Task Search_button_passes_filters_to_client()
    {
        var fake = Register();
        var cut = Render<Search>();

        cut.Find("[data-testid=level-filter]").Change("Error");
        cut.Find("[data-testid=search-input]").Input("timeout"); // @bind:event="oninput"
        cut.Find("[data-testid=search-button]").Click();
        await Task.Yield();

        Assert.Equal("timeout", fake.LastQ);
        Assert.Equal("Error", fake.LastLevel);
    }

    [Fact]
    public async Task Empty_result_shows_message()
    {
        Register();
        var cut = Render<Search>();
        await Task.Yield();
        cut.WaitForAssertion(() => Assert.Contains("No log entries", cut.Markup,
            StringComparison.OrdinalIgnoreCase));
    }
}
