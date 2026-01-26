using System.Net.Http.Json;
using MIMM.Shared.Dtos;

namespace MIMM.Frontend.Services;

public interface IAnalyticsApiService
{
    Task<MoodTrendDto> GetMoodTrendsAsync(int daysLookback = 30, CancellationToken cancellationToken = default);
    Task<MusicStatisticsDto> GetMusicStatisticsAsync(CancellationToken cancellationToken = default);
    Task<YearlyReportDto?> GetYearlyReportAsync(int year, CancellationToken cancellationToken = default);
    Task<MoodCorrelationDto?> GetMoodByArtistAsync(CancellationToken cancellationToken = default);
}

public class AnalyticsApiService(HttpClient httpClient) : IAnalyticsApiService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<MoodTrendDto> GetMoodTrendsAsync(int daysLookback = 30, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/analytics/mood-trends?daysLookback={daysLookback}", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MoodTrendDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize mood trends");
    }

    public async Task<MusicStatisticsDto> GetMusicStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/analytics/music-stats", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MusicStatisticsDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize music statistics");
    }

    public async Task<YearlyReportDto?> GetYearlyReportAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/analytics/yearly-report/{year}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<YearlyReportDto>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching yearly report: {ex.Message}");
            return null;
        }
    }

    public async Task<MoodCorrelationDto?> GetMoodByArtistAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/analytics/mood-by-artist", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<MoodCorrelationDto>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching mood by artist: {ex.Message}");
            return null;
        }
    }
}
