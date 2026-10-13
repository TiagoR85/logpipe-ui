using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace LogPipe.Ui.Services;

public sealed class OAuthTokenProvider(HttpClient http, OAuthClientOptions options)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (IsFresh()) return _token!;

        await _gate.WaitAsync(ct);
        try
        {
            if (IsFresh()) return _token!;

            using var response = await http.PostAsync(options.TokenUrl,
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = options.ClientId,
                    ["client_secret"] = options.ClientSecret,
                    ["scope"] = options.Scope
                }), ct);

            if (!response.IsSuccessStatusCode)
            {
                throw new LogPipeApiException((int)response.StatusCode,
                    $"token endpoint returned {response.StatusCode}");
            }

            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
                ?? throw new LogPipeApiException((int)response.StatusCode, "empty token response");
            _token = payload.AccessToken;
            _expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(payload.ExpiresIn - 60, 30));
            return _token;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsFresh() =>
        _token is not null && DateTimeOffset.UtcNow < _expiresAt;

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
