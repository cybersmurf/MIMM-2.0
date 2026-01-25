using System.Net.Http.Json;

namespace MIMM.Frontend.Services;

public interface ILastFmApiService
{
    Task<string?> GetAuthUrlAsync(CancellationToken cancellationToken = default);
}

public sealed class LastFmApiService(HttpClient http, ILogger<LastFmApiService> logger) : ILastFmApiService
{
    private readonly HttpClient _http = http;
    private readonly ILogger<LastFmApiService> _logger = logger;

    public async Task<string?> GetAuthUrlAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var resp = await _http.GetAsync("/api/lastfm/auth-url", cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get Last.fm auth URL: {Status}", resp.StatusCode);
                return null;
            }
            var json = await resp.Content.ReadFromJsonAsync<AuthUrlResponse>(cancellationToken: cancellationToken);
            return json?.Url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Last.fm auth URL");
            return null;
        }
    }

    private sealed record AuthUrlResponse(string Url);
}
