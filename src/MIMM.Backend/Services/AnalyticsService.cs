using Microsoft.EntityFrameworkCore;
using MIMM.Backend.Data;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Services;

public interface IAnalyticsService
{
    Task<MoodTrendDto> GetMoodTrendAsync(Guid userId, int daysLookback = 30, CancellationToken cancellationToken = default);
    Task<MusicStatisticsDto> GetMusicStatisticsAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Analytics service for mood trends and music statistics
/// </summary>
public sealed class AnalyticsService(ApplicationDbContext db, ILogger<AnalyticsService> logger) : IAnalyticsService
{
    private readonly ApplicationDbContext _db = db;
    private readonly ILogger<AnalyticsService> _logger = logger;

    public async Task<MoodTrendDto> GetMoodTrendAsync(Guid userId, int daysLookback = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysLookback);
        
        var entries = await _db.Entries
            .Where(e => e.UserId == userId && e.CreatedAt >= cutoffDate)
            .OrderBy(e => e.CreatedAt)
            .Select(e => new { e.CreatedAt, e.Valence, e.Arousal })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return new MoodTrendDto
            {
                DaysAnalyzed = daysLookback,
                TotalEntries = 0,
                AverageValence = 0,
                AverageArousal = 0,
                DailyAverages = [],
                MoodDistribution = new MoodDistributionDto()
            };
        }

        // Calculate daily averages
        var dailyAverages = entries
            .GroupBy(e => e.CreatedAt.Date)
            .Select(g => new DailyMoodDto
            {
                Date = g.Key,
                AverageValence = g.Average(x => x.Valence),
                AverageArousal = g.Average(x => x.Arousal),
                EntryCount = g.Count()
            })
            .ToList();

        // Calculate mood distribution
        var moodDistribution = CalculateMoodDistribution(entries.Select(e => (e.Valence, e.Arousal)).ToList());

        var trend = new MoodTrendDto
        {
            DaysAnalyzed = daysLookback,
            TotalEntries = entries.Count,
            AverageValence = entries.Average(e => e.Valence),
            AverageArousal = entries.Average(e => e.Arousal),
            DailyAverages = dailyAverages,
            MoodDistribution = moodDistribution
        };

        _logger.LogInformation("Calculated mood trends for user {UserId}: {EntryCount} entries over {Days} days", 
            userId, entries.Count, daysLookback);

        return trend;
    }

    public async Task<MusicStatisticsDto> GetMusicStatisticsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var entries = await _db.Entries
            .Where(e => e.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return new MusicStatisticsDto
            {
                TotalEntriesLogged = 0,
                TotalScrobbled = 0,
                TopArtists = [],
                TopSongs = [],
                EntriesBySource = new Dictionary<string, int>()
            };
        }

        var topArtists = entries
            .Where(e => !string.IsNullOrWhiteSpace(e.ArtistName))
            .GroupBy(e => e.ArtistName ?? string.Empty)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new ArtistStatDto { Name = g.Key, Count = g.Count() })
            .ToList();

        var topSongs = entries
            .Where(e => !string.IsNullOrWhiteSpace(e.SongTitle))
            .GroupBy(e => new { e.SongTitle, e.ArtistName })
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => new SongStatDto 
            { 
                Title = g.Key.SongTitle, 
                Artist = g.Key.ArtistName, 
                Count = g.Count() 
            })
            .ToList();

        var entriesBySource = entries
            .GroupBy(e => e.Source ?? "unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        var stats = new MusicStatisticsDto
        {
            TotalEntriesLogged = entries.Count,
            TotalScrobbled = entries.Count(e => e.ScrobbledToLastFm),
            ScrobbleRate = entries.Count == 0 ? 0 : (double)entries.Count(e => e.ScrobbledToLastFm) / entries.Count,
            TopArtists = topArtists,
            TopSongs = topSongs,
            EntriesBySource = entriesBySource
        };

        _logger.LogInformation("Calculated music statistics for user {UserId}: {Total} entries, {Scrobbled} scrobbled",
            userId, stats.TotalEntriesLogged, stats.TotalScrobbled);

        return stats;
    }

    private static MoodDistributionDto CalculateMoodDistribution(List<(float valence, float arousal)> moods)
    {
        if (moods.Count == 0)
            return new MoodDistributionDto();

        var happy = moods.Count(m => m.valence > 0.3 && m.arousal > 0.3);
        var calm = moods.Count(m => m.valence > 0.3 && m.arousal <= 0.3);
        var angry = moods.Count(m => m.valence <= 0.3 && m.arousal > 0.3);
        var sad = moods.Count(m => m.valence <= 0.3 && m.arousal <= 0.3);

        var total = moods.Count;

        return new MoodDistributionDto
        {
            HappyPercent = (double)happy / total * 100,
            CalmPercent = (double)calm / total * 100,
            AngryPercent = (double)angry / total * 100,
            SadPercent = (double)sad / total * 100
        };
    }
}
