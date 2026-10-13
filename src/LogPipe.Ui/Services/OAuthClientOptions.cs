namespace LogPipe.Ui.Services;

public sealed record OAuthClientOptions(
    string TokenUrl,
    string ClientId,
    string ClientSecret,
    string Scope);
