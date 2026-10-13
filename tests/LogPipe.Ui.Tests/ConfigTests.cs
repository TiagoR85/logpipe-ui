using System.Text.Json;

namespace LogPipe.Ui.Tests;

public sealed class ConfigTests
{
    private static JsonElement LoadAppSettings()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "src", "LogPipe.Ui", "appsettings.json");
            if (File.Exists(candidate))
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(candidate));
                return doc.RootElement.Clone();
            }
            dir = dir.Parent;
        }
        throw new FileNotFoundException(
            "src/LogPipe.Ui/appsettings.json not found walking up from " + AppContext.BaseDirectory);
    }

    [Fact]
    public void Search_base_url_is_configured_separately_from_oauth()
    {
        var cfg = LoadAppSettings();
        var baseUrl = cfg.GetProperty("Search").GetProperty("BaseUrl").GetString();

        Assert.Equal("http://localhost:8091", baseUrl);
    }

    [Fact]
    public void Search_base_url_differs_from_token_origin()
    {
        var cfg = LoadAppSettings();
        var tokenUrl = cfg.GetProperty("OAuth").GetProperty("TokenUrl").GetString()!;
        var baseUrl = cfg.GetProperty("Search").GetProperty("BaseUrl").GetString()!;

        Assert.NotEqual(
            new Uri(tokenUrl).GetLeftPart(UriPartial.Authority),
            new Uri(baseUrl).GetLeftPart(UriPartial.Authority));
    }
}
