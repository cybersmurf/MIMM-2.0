using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MIMM.Frontend;
using MIMM.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API base URL (HTTP in dev to avoid self-signed cert prompts)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5001";

// Register services
builder.Services.AddMudServices();

// Auth state service (must be scoped for JSInterop)
builder.Services.AddScoped<IAuthStateService, AuthStateService>();

// HTTP client with auth handler
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var authHandler = sp.GetRequiredService<AuthorizationMessageHandler>();
    authHandler.InnerHandler = new HttpClientHandler();
    
    var httpClient = new HttpClient(authHandler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
    
    return httpClient;
});

// API services
builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<IEntryApiService, EntryApiService>();
builder.Services.AddScoped<IMusicSearchApiService, MusicSearchApiService>();
builder.Services.AddScoped<ILastFmApiService, LastFmApiService>();

// Logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

await builder.Build().RunAsync();
