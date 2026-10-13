namespace LogPipe.Ui.Services;

public sealed class LogPipeApiException(int statusCode, string? detail)
    : Exception(detail ?? $"logpipe API returned {statusCode}.")
{
    public int StatusCode { get; } = statusCode;
}
