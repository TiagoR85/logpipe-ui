using LogPipe.Ui.Components;
using LogPipe.Ui.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// OAuthClientOptions is bound from configuration; DI supplies it to the
// typed-client factory below, so no manual registration is needed.
builder.Services.AddSingleton(
    builder.Configuration.GetSection("OAuth").Get<OAuthClientOptions>()
        ?? throw new InvalidOperationException("Missing required configuration: OAuth. See .env.example."));

// Both the token provider and the search client share one HttpClient whose
// BaseAddress is derived from OAuth:TokenUrl — the einvoice-worker f3ee1f2
// lesson applied at the hub boundary.
builder.Services.AddHttpClient<OAuthTokenProvider>((sp, client) =>
{
    var options = sp.GetRequiredService<OAuthClientOptions>();
    client.BaseAddress = new Uri(options.TokenUrl, UriKind.Absolute);
});

// LogSearchClient reuses the same named client and the token provider.
builder.Services.AddSingleton<ILogSearchClient>(sp =>
    new LogSearchClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(OAuthTokenProvider)),
        sp.GetRequiredService<OAuthTokenProvider>()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
