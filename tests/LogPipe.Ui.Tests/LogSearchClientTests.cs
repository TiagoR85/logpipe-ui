using LogPipe.Ui.Services;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace LogPipe.Ui.Tests;

public sealed class LogSearchClientTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Dispose();

    private (LogSearchClient Client, OAuthTokenProvider Tokens) Client()
    {
        var tokens = new OAuthTokenProvider(
            new HttpClient { BaseAddress = new Uri(_server.Url!) },
            new OAuthClientOptions("/connect/token", "logpipe-ui", "secret", "logs:read"));
        var http = new HttpClient { BaseAddress = new Uri(_server.Url!) };
        return (new LogSearchClient(http, tokens), tokens);
    }

    [Fact]
    public async Task Search_sends_bearer_and_parses_response()
    {
        _server.Given(Request.Create().WithPath("/connect/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBody("""{"access_token":"tok","expires_in":3600}""")
                .WithHeader("Content-Type", "application/json"));
        _server.Given(Request.Create().WithPath("/v1/logs").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBodyAsJson(new
                {
                    total = 1,
                    hits = new[]
                    {
                        new
                        {
                            id = "e1", timestamp = "2026-10-12T10:00:00Z", level = "Error",
                            service = "api", message = "boom", traceId = (string?)null,
                            spanId = (string?)null, attributes = (Dictionary<string, string>?)null
                        }
                    },
                    facets = new
                    {
                        levels = new[] { new { value = "Error", count = 1 } },
                        services = new[] { new { value = "api", count = 1 } }
                    }
                }));

        var (client, _) = Client();
        var result = await client.SearchAsync(q: "bo", level: "Error", service: "api", page: 1, size: 20);

        Assert.Equal(1, result.Total);
        Assert.Equal("boom", Assert.Single(result.Hits).Message);
        Assert.Equal("Error", Assert.Single(result.Facets.Levels).Value);

        var searchRequest = _server.LogEntries.Single(e => e.RequestMessage.Path == "/v1/logs");
        Assert.Equal("Bearer tok", searchRequest.RequestMessage.Headers["Authorization"]?.First());
        Assert.Contains("q=bo", searchRequest.RequestMessage.Url);
        Assert.Contains("level=Error", searchRequest.RequestMessage.Url);
    }

    [Fact]
    public async Task Non_success_throws_with_problem_detail()
    {
        _server.Given(Request.Create().WithPath("/connect/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBody("""{"access_token":"tok","expires_in":3600}""")
                .WithHeader("Content-Type", "application/json"));
        _server.Given(Request.Create().WithPath("/v1/logs").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(400)
                .WithBodyAsJson(new { detail = "unknown level 'Bogus'" }));

        var (client, _) = Client();
        var ex = await Assert.ThrowsAsync<LogPipeApiException>(
            () => client.SearchAsync(null, "Bogus", null, 1, 20));
        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("unknown level", ex.Message);
    }

    [Fact]
    public async Task Get_trace_returns_entries()
    {
        _server.Given(Request.Create().WithPath("/connect/token").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBody("""{"access_token":"tok","expires_in":3600}""")
                .WithHeader("Content-Type", "application/json"));
        _server.Given(Request.Create().WithPath("/v1/logs/trace/abc123").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithBodyAsJson(new { traceId = "abc123", entries = Array.Empty<object>() }));

        var (client, _) = Client();
        var result = await client.GetTraceAsync("abc123");
        Assert.Equal("abc123", result.TraceId);
        Assert.Empty(result.Entries);
    }
}
