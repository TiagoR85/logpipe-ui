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

// Search and token are different origins: /v1/logs must never resolve against
// OAuth:TokenUrl (ingest), or the UI would query the ingest API instead of search.
string searchBaseUrl = builder.Configuration["Search:BaseUrl"]
    ?? throw new InvalidOperationException("Missing required configuration: Search:BaseUrl. See .env.example.");

// The token provider gets its own client (BaseAddress = token origin).
builder.Services.AddHttpClient<OAuthTokenProvider>((sp, client) =>
{
    var options = sp.GetRequiredService<OAuthClientOptions>();
    client.BaseAddress = new Uri(options.TokenUrl, UriKind.Absolute);
});

// Separate named client for search: sharing the token client would resolve
// /v1/logs against the ingest origin (OAuth:TokenUrl) instead of search.
builder.Services.AddHttpClient("Search", client =>
{
    client.BaseAddress = new Uri(searchBaseUrl, UriKind.Absolute);
});

builder.Services.AddSingleton<ILogSearchClient>(sp =>
    new LogSearchClient(
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("Search"),
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
