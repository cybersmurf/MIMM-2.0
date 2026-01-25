using System.Net.Http.Json;
using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

public interface IMusicSearchApiService
{
    Task<MusicSearchResponse?> SearchAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
}

public class MusicSearchApiService : IMusicSearchApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MusicSearchApiService> _logger;

    public MusicSearchApiService(HttpClient httpClient, ILogger<MusicSearchApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<MusicSearchResponse?> SearchAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        try
        {
            var url = $"/api/music/search?query={Uri.EscapeDataString(query)}&limit={limit}";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Music search failed: {StatusCode}", response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MusicSearchResponse>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during music search");
            return null;
        }
    }
}
