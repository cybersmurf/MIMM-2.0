using System.Net.Http.Json;

namespace MIMM.Frontend.Services;

public interface ILastFmApiService
{
    Task<string?> GetAuthUrlAsync(CancellationToken cancellationToken = default);
    Task<bool> ScrobbleAsync(string songTitle, string? artistName, string? albumName, DateTime? timestamp = null, CancellationToken cancellationToken = default);
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

    public async Task<bool> ScrobbleAsync(string songTitle, string? artistName, string? albumName, DateTime? timestamp = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ScrobbleRequest
            {
                SongTitle = songTitle,
                ArtistName = artistName,
                AlbumName = albumName,
                Timestamp = timestamp ?? DateTime.UtcNow
            };

            var resp = await _http.PostAsJsonAsync("/api/lastfm/scrobble", request, cancellationToken);
            
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to scrobble track to Last.fm: {Status}", resp.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scrobbling track to Last.fm");
            return false;
        }
    }

    private sealed record AuthUrlResponse(string Url);
    private sealed record ScrobbleRequest
    {
        public required string SongTitle { get; init; }
        public string? ArtistName { get; init; }
        public string? AlbumName { get; init; }
        public DateTime? Timestamp { get; init; }
    }
}
