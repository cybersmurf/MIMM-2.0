using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MIMM.Frontend;
using MIMM.Frontend.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API base URL (from appsettings.json or environment variable)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] 
    ?? builder.Configuration["BACKEND_URL"]
    ?? "https://api.musicinmymind.app";

// Register services
builder.Services.AddMudServices();

// Auth state service (must be scoped for JSInterop)
builder.Services.AddScoped<IAuthStateService, AuthStateService>();

// Theme service (singleton for shared state across components)
builder.Services.AddSingleton<IThemeService, ThemeService>();

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
builder.Services.AddScoped<IAnalyticsApiService, AnalyticsApiService>();
builder.Services.AddScoped<IExportImportApiService, ExportImportApiService>();
builder.Services.AddScoped<IFriendsApiService, FriendsApiService>();
builder.Services.AddScoped<INotificationApiService, NotificationApiService>();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();

// Logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

await builder.Build().RunAsync();
