using Microsoft.EntityFrameworkCore;
using MIMM.Backend.Data;
using MIMM.Shared.Entities;

namespace MIMM.Backend.Services;

public interface ISpotifyService
{
    /// <summary>
    /// Get Spotify authorization URL for OAuth flow
    /// </summary>
    Task<string> GetAuthUrlAsync(Guid userId, string requestBaseUrl);

    /// <summary>
    /// Handle Spotify OAuth callback
    /// </summary>
    Task<(bool Success, string? AccessToken, string? Error)> HandleCallbackAsync(
        Guid userId,
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sync user's music entries to Spotify playlist
    /// </summary>
    Task<(bool Success, string? PlaylistUri, string? Error)> SyncPlaylistAsync(
        Guid userId,
        string playlistName = "MIMM Playlist",
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get track recommendations based on mood
    /// </summary>
    Task<(bool Success, List<SpotifyTrackDto>? Tracks, string? Error)> GetMoodRecommendationsAsync(
        Guid userId,
        double valence,
        double arousal,
        int limit = 20,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Spotify integration service
/// </summary>
public sealed class SpotifyService(
    ISpotifyHttpClient spotifyClient,
    ApplicationDbContext db,
    IConfiguration configuration,
    ILogger<SpotifyService> logger
) : ISpotifyService
{
    private readonly ISpotifyHttpClient _spotifyClient = spotifyClient;
    private readonly ApplicationDbContext _db = db;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<SpotifyService> _logger = logger;

    private string ClientId => _configuration["Spotify:ClientId"] ?? string.Empty;
    private string ClientSecret => _configuration["Spotify:ClientSecret"] ?? string.Empty;

    public async Task<string> GetAuthUrlAsync(Guid userId, string requestBaseUrl)
    {
        var redirectUri = $"{requestBaseUrl.TrimEnd('/')}/api/auth/spotify/callback";
        var scopes = "playlist-modify-public playlist-modify-private";
        var state = $"{userId}:{Guid.NewGuid()}";

        // Store state in session or database for verification
        var user = await _db.Users.FindAsync(userId);
        if (user != null)
        {
            user.SpotifyState = state;
            user.SpotifyStateExpiresAt = DateTime.UtcNow.AddMinutes(10);
            await _db.SaveChangesAsync();
        }

        var authUrl = $"https://accounts.spotify.com/authorize?client_id={ClientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopes)}&state={state}";
        
        _logger.LogInformation("Generated Spotify auth URL for user {UserId}", userId);
        return authUrl;
    }

    public async Task<(bool Success, string? AccessToken, string? Error)> HandleCallbackAsync(
        Guid userId,
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken: cancellationToken);
            if (user == null)
                return (false, null, "User not found");

            // Verify state hasn't expired
            if (user.SpotifyStateExpiresAt < DateTime.UtcNow)
                return (false, null, "State expired");

            // Request access token
            var tokenRequest = new SpotifyTokenRequest
            {
                Code = code,
                RedirectUri = redirectUri,
                ClientId = ClientId,
                ClientSecret = ClientSecret
            };

            var tokenResponse = await _spotifyClient.RequestTokenAsync(tokenRequest);

            // Store token
            user.SpotifyAccessToken = tokenResponse.AccessToken;
            user.SpotifyRefreshToken = tokenResponse.RefreshToken;
            user.SpotifyTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            user.SpotifyConnectedAt = DateTime.UtcNow;
            user.SpotifyState = null;

            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Spotify connected for user {UserId}", userId);
            return (true, tokenResponse.AccessToken, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Spotify callback for user {UserId}", userId);
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? PlaylistUri, string? Error)> SyncPlaylistAsync(
        Guid userId,
        string playlistName = "MIMM Playlist",
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return (false, null, "User not found");

            if (string.IsNullOrWhiteSpace(user.SpotifyAccessToken))
                return (false, null, "Spotify not connected");

            if (user.SpotifyTokenExpiresAt < DateTime.UtcNow)
                return (false, null, "Spotify token expired");

            var auth = $"Bearer {user.SpotifyAccessToken}";

            // Get user's Spotify profile
            var spotifyUser = await _spotifyClient.GetCurrentUserAsync(auth);

            // Create playlist
            var createPlaylistRequest = new CreatePlaylistRequest
            {
                Name = playlistName,
                Description = $"Music entries synced from MIMM on {DateTime.UtcNow:G}"
            };
            var playlist = await _spotifyClient.CreatePlaylistAsync(
                spotifyUser.Id,
                createPlaylistRequest,
                auth
            );

            // Get user's entries with Spotify URIs
            var entries = await _db.Entries
                .Where(e => e.UserId == userId && !string.IsNullOrWhiteSpace(e.SpotifyUri))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (entries.Count > 0)
            {
                var trackUris = entries.Select(e => e.SpotifyUri!).ToList();
                var addTracksRequest = new AddTracksRequest { Uris = trackUris };

                await _spotifyClient.AddTracksToPlaylistAsync(
                    playlist.Id,
                    addTracksRequest,
                    auth
                );

                _logger.LogInformation("Synced {TrackCount} tracks to Spotify playlist {PlaylistId} for user {UserId}",
                    trackUris.Count, playlist.Id, userId);
            }

            return (true, playlist.Uri, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Spotify playlist for user {UserId}", userId);
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, List<SpotifyTrackDto>? Tracks, string? Error)> GetMoodRecommendationsAsync(
        Guid userId,
        double valence,
        double arousal,
        int limit = 20,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                return (false, null, "User not found");

            if (string.IsNullOrWhiteSpace(user.SpotifyAccessToken))
                return (false, null, "Spotify not connected");

            var auth = $"Bearer {user.SpotifyAccessToken}";

            // Get user's top tracks as seeds
            var topEntries = await _db.Entries
                .Where(e => e.UserId == userId && !string.IsNullOrWhiteSpace(e.SpotifyId))
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (topEntries.Count == 0)
                return (false, null, "No tracks found for recommendations");

            var seedTracks = string.Join(",", topEntries.Select(e => e.SpotifyId).Take(3));
            var seedArtists = topEntries.Count > 3
                ? string.Join(",", topEntries.Skip(3).Select(e => e.ArtistName).Take(2).Distinct())
                : string.Empty;

            // Map mood to Spotify parameters
            var targetEnergy = arousal;  // High arousal = high energy
            var targetValence = valence; // High valence = happy, positive

            var recommendations = await _spotifyClient.GetRecommendationsAsync(
                seedTracks,
                seedArtists,
                targetEnergy,
                targetValence,
                auth,
                limit
            );

            _logger.LogInformation("Retrieved {TrackCount} recommendations for user {UserId} with valence={Valence}, arousal={Arousal}",
                recommendations.Tracks.Count, userId, valence, arousal);

            return (true, recommendations.Tracks, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Spotify recommendations for user {UserId}", userId);
            return (false, null, ex.Message);
        }
    }
}
