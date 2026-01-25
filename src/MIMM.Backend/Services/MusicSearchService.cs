using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MIMM.Backend.Data;
using MIMM.Shared.Dtos;
using MIMM.Shared.Entities;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;

namespace MIMM.Backend.Services;

public interface IMusicSearchService
{
    Task<IReadOnlyList<MusicTrackDto>> SearchAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Music search against MusicBrainz recordings API with iTunes fallback. Caches MusicBrainz metadata.
/// </summary>
public class MusicSearchService : IMusicSearchService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MusicSearchService> _logger;
    private readonly IMemoryCache _cache;
    private static DateTime _lastMbCallUtc = DateTime.UnixEpoch;

    public MusicSearchService(HttpClient httpClient, ApplicationDbContext dbContext, ILogger<MusicSearchService> logger, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IReadOnlyList<MusicTrackDto>> SearchAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Array.Empty<MusicTrackDto>();
        }

        var safeLimit = Math.Clamp(limit, 1, 25);

        // Cache key per query+limit
        var cacheKey = $"music_search::{query.Trim().ToLowerInvariant()}::{safeLimit}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<MusicTrackDto>? cached) && cached is { Count: > 0 })
        {
            return cached;
        }

        var musicBrainzResults = await SearchMusicBrainzAsync(query, safeLimit, cancellationToken);

        if (musicBrainzResults.Count > 0)
        {
            await CacheMusicBrainzAsync(musicBrainzResults.Select(r => r.CachePayload), cancellationToken);
            var tracks = Deduplicate(musicBrainzResults.Select(r => r.Track).ToList());
            _cache.Set(cacheKey, tracks, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
                Size = 1
            });
            return tracks;
        }

        var deezerResults = await SearchDeezerFallbackAsync(query, safeLimit, cancellationToken);
        if (deezerResults.Count > 0)
        {
            var tracks = Deduplicate(deezerResults);
            _cache.Set(cacheKey, tracks, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8),
                Size = 1
            });
            return tracks;
        }

        var fallback = Deduplicate(await SearchItunesFallbackAsync(query, safeLimit, cancellationToken));
        if (fallback.Count > 0)
        {
            _cache.Set(cacheKey, fallback, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6),
                Size = 1
            });
        }
        return fallback;
    }

    private async Task<List<MusicBrainzRecordingResult>> SearchMusicBrainzAsync(string query, int limit, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://musicbrainz.org/ws/2/recording?query={Uri.EscapeDataString(query)}&fmt=json&limit={limit}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("MIMM/2.0 (+https://github.com/cybersmurf/MIMM-2.0)");

            // Simple rate limiting for MusicBrainz (1 req/sec recommended)
            await EnforceMusicBrainzRateLimitAsync(cancellationToken);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("MusicBrainz search failed: {StatusCode}", response.StatusCode);
                return [];
            }

            var payload = await response.Content.ReadFromJsonAsync<MusicBrainzRecordingResponse>(cancellationToken: cancellationToken);
            if (payload?.Recordings == null || payload.Recordings.Count == 0)
            {
                return [];
            }

            var mapped = new List<MusicBrainzRecordingResult>();

            foreach (var recording in payload.Recordings)
            {
                var result = MapMusicBrainzRecording(recording);
                if (result != null)
                {
                    mapped.Add(result);
                }
            }

            return mapped;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MusicBrainz search request failed for query {Query}", query);
            return [];
        }
    }

    private static Task EnforceMusicBrainzRateLimitAsync(CancellationToken cancellationToken)
    {
        const int minDelayMs = 1100;
        var now = DateTime.UtcNow;
        var since = (int)(now - _lastMbCallUtc).TotalMilliseconds;
        if (since < minDelayMs)
        {
            var delay = minDelayMs - since;
            return Task.Delay(delay, cancellationToken).ContinueWith(_ => { _lastMbCallUtc = DateTime.UtcNow; }, cancellationToken);
        }
        _lastMbCallUtc = now;
        return Task.CompletedTask;
    }

    private async Task CacheMusicBrainzAsync(IEnumerable<MusicBrainzCachePayload> payloads, CancellationToken cancellationToken)
    {
        var cacheItems = payloads.ToList();

        if (cacheItems.Count == 0)
        {
            return;
        }

        var artistIds = cacheItems
            .Select(p => p.ArtistId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var existingArtistIds = await _dbContext.MusicBrainzArtists
            .Where(a => artistIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var newArtists = artistIds
            .Where(id => id != null && !existingArtistIds.Contains(id))
            .Select(id => new MusicBrainzArtist
            {
                Id = id!,
                Name = cacheItems.First(p => p.ArtistId == id).ArtistName
            })
            .ToList();

        if (newArtists.Count > 0)
        {
            await _dbContext.MusicBrainzArtists.AddRangeAsync(newArtists, cancellationToken);
        }

        var releaseIds = cacheItems
            .Select(p => p.ReleaseId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();

        var existingReleaseIds = await _dbContext.MusicBrainzReleases
            .Where(r => releaseIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var newReleases = releaseIds
            .Where(id => id != null && !existingReleaseIds.Contains(id))
            .Select(id =>
            {
                var payload = cacheItems.First(p => p.ReleaseId == id);
                return new MusicBrainzRelease
                {
                    Id = id!,
                    Title = payload.AlbumTitle ?? "Unknown album",
                    CoverArtUrl = payload.CoverUrl,
                    ReleaseDate = payload.ReleaseDate
                };
            })
            .ToList();

        if (newReleases.Count > 0)
        {
            await _dbContext.MusicBrainzReleases.AddRangeAsync(newReleases, cancellationToken);
        }

        var recordingIds = cacheItems.Select(p => p.RecordingId).Distinct().ToList();

        var existingRecordingIds = await _dbContext.MusicBrainzRecordings
            .Where(r => recordingIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var newRecordings = cacheItems
            .Where(p => !existingRecordingIds.Contains(p.RecordingId))
            .Select(p => new MusicBrainzRecording
            {
                Id = p.RecordingId,
                Title = p.Title,
                ArtistId = p.ArtistId,
                ArtistName = p.ArtistName,
                ReleaseId = p.ReleaseId,
                ReleaseTitle = p.AlbumTitle,
                CoverUrl = p.CoverUrl
            })
            .ToList();

        if (newRecordings.Count > 0)
        {
            await _dbContext.MusicBrainzRecordings.AddRangeAsync(newRecordings, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static MusicBrainzRecordingResult? MapMusicBrainzRecording(MusicBrainzRecordingItem recording)
    {
        if (string.IsNullOrWhiteSpace(recording.Id) || string.IsNullOrWhiteSpace(recording.Title))
        {
            return null;
        }

        var artistCredit = recording.ArtistCredit?.FirstOrDefault();
        var artistName = artistCredit?.Name ?? artistCredit?.Artist?.Name ?? "Unknown artist";
        var artistId = artistCredit?.Artist?.Id;

        var release = recording.Releases?.FirstOrDefault();
        var albumTitle = release?.Title ?? "Unknown album";
        var releaseId = release?.Id;
        string? coverUrl = null;

        if (release?.CoverArtArchive?.Front == true && !string.IsNullOrWhiteSpace(release.Id))
        {
            coverUrl = $"https://coverartarchive.org/release/{release.Id}/front-250";
        }

        var track = new MusicTrackDto
        {
            Title = recording.Title,
            Artist = artistName,
            Album = albumTitle,
            CoverUrl = coverUrl,
            Source = "musicbrainz",
            ExternalId = recording.Id
        };

        var cachePayload = new MusicBrainzCachePayload(
            RecordingId: recording.Id,
            Title: recording.Title,
            ArtistId: artistId,
            ArtistName: artistName,
            ReleaseId: releaseId,
            AlbumTitle: albumTitle,
            ReleaseDate: release?.Date,
            CoverUrl: coverUrl
        );

        return new MusicBrainzRecordingResult(track, cachePayload);
    }

    private async Task<List<MusicTrackDto>> SearchDeezerFallbackAsync(string query, int limit, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://api.deezer.com/search?q={Uri.EscapeDataString(query)}&limit={limit}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Deezer search failed: {StatusCode}", response.StatusCode);
                return [];
            }

            var payload = await response.Content.ReadFromJsonAsync<DeezerResponse>(cancellationToken: cancellationToken);
            if (payload?.Data == null || payload.Data.Count == 0)
            {
                return [];
            }

            return payload.Data
                .Select(MapDeezerTrack)
                .Where(t => t is not null)
                .Cast<MusicTrackDto>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Deezer fallback search failed for query {Query}", query);
            return [];
        }
    }

    private static MusicTrackDto? MapDeezerTrack(DeezerTrack track)
    {
        if (string.IsNullOrWhiteSpace(track.Title))
        {
            return null;
        }

        return new MusicTrackDto
        {
            Title = track.Title,
            Artist = track.Artist?.Name ?? string.Empty,
            Album = track.Album?.Title ?? string.Empty,
            CoverUrl = track.Album?.CoverMedium,
            Source = "deezer",
            ExternalId = track.Id?.ToString() ?? track.Title
        };
    }

    private async Task<List<MusicTrackDto>> SearchItunesFallbackAsync(string query, int limit, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://itunes.apple.com/search?media=music&entity=song&limit={limit}&term={Uri.EscapeDataString(query)}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("iTunes music search failed: {StatusCode}", response.StatusCode);
                return new List<MusicTrackDto>();
            }

            var payload = await response.Content.ReadFromJsonAsync<ItunesResponse>(cancellationToken: cancellationToken);
            if (payload?.Results == null || payload.Results.Count == 0)
            {
                return new List<MusicTrackDto>();
            }

            return payload.Results
                .Select(MapItunesTrack)
                .Where(t => t is not null)
                .Cast<MusicTrackDto>()
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "iTunes fallback search failed for query {Query}", query);
            return new List<MusicTrackDto>();
        }
    }

    private static List<MusicTrackDto> Deduplicate(List<MusicTrackDto> items)
        {
            if (items.Count <= 1) return items;

            string Key(MusicTrackDto t)
            {
                var title = NormalizeText(t.Title);
                var artist = NormalizeText(t.Artist);
                return $"{title}__{artist}";
            }

            return items
                .GroupBy(Key)
                .Select(g => g.First())
                .ToList();
        }

        private static string NormalizeText(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var t = s.Trim().ToLowerInvariant();
            // Remove bracketed suffices like (Remastered 2011), [Mono]
            t = Regex.Replace(t, "[\\(\\[\\{].*?[\\)\\]\\}]", " ");
            // Remove common " - 2011 Remaster" / " - Remastered 2011" patterns
            t = Regex.Replace(t, @"\s*[-–]\s*(\d{4}\s*)?remaster(ed)?(\s*\d{4})?$", " ");
            // Collapse whitespace
            t = Regex.Replace(t, @"\s+", " ").Trim();
            return t;
        }

        private static MusicTrackDto? MapItunesTrack(ItunesTrack track)
    {
        if (string.IsNullOrWhiteSpace(track.TrackName))
        {
            return null;
        }

        return new MusicTrackDto
        {
            Title = track.TrackName,
            Artist = track.ArtistName ?? string.Empty,
            Album = track.CollectionName ?? string.Empty,
            CoverUrl = track.ArtworkUrl100,
            Source = "itunes",
            ExternalId = track.TrackId?.ToString() ?? track.TrackViewUrl ?? track.TrackName
        };
    }

    private sealed class ItunesResponse
    {
        public int ResultCount { get; set; }
        public List<ItunesTrack> Results { get; set; } = [];
    }

    private sealed class DeezerResponse
    {
        public List<DeezerTrack> Data { get; set; } = [];
    }

    private sealed class DeezerTrack
    {
        public long? Id { get; set; }
        public string? Title { get; set; }
        public DeezerArtist? Artist { get; set; }
        public DeezerAlbum? Album { get; set; }
    }

    private sealed class DeezerArtist
    {
        public string? Name { get; set; }
    }

    private sealed class DeezerAlbum
    {
        public string? Title { get; set; }
        public string? CoverMedium { get; set; }
    }

    private sealed class ItunesTrack
    {
        public string? TrackName { get; set; }
        public string? ArtistName { get; set; }
        public string? CollectionName { get; set; }
        public string? ArtworkUrl100 { get; set; }
        public long? TrackId { get; set; }
        public string? TrackViewUrl { get; set; }
    }

    private sealed class MusicBrainzRecordingResponse
    {
        public List<MusicBrainzRecordingItem> Recordings { get; set; } = [];
    }

    private sealed class MusicBrainzRecordingItem
    {
        public string? Id { get; set; }
        public string? Title { get; set; }

        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("artist-credit")]
        public List<ArtistCreditItem>? ArtistCredit { get; set; }
        public List<MusicBrainzReleaseItem>? Releases { get; set; }
    }

    private sealed class ArtistCreditItem
    {
        public string? Name { get; set; }
        public MusicBrainzArtistItem? Artist { get; set; }
    }

    private sealed class MusicBrainzArtistItem
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

    private sealed class MusicBrainzReleaseItem
    {
        public string? Id { get; set; }
        public string? Title { get; set; }

        // ReSharper disable once IdentifierTypo
        [JsonPropertyName("cover-art-archive")]
        public CoverArtArchive? CoverArtArchive { get; set; }

        public string? Date { get; set; }
    }

    private sealed class CoverArtArchive
    {
        public bool Front { get; set; }
    }

    private sealed record MusicBrainzCachePayload(
        string RecordingId,
        string Title,
        string? ArtistId,
        string ArtistName,
        string? ReleaseId,
        string? AlbumTitle,
        string? ReleaseDate,
        string? CoverUrl
    );

    private sealed record MusicBrainzRecordingResult(MusicTrackDto Track, MusicBrainzCachePayload CachePayload);
}
