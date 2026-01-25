using Refit;

namespace MIMM.Backend.Services;

/// <summary>
/// Spotify Web API client interface using Refit
/// </summary>
public interface ISpotifyHttpClient
{
    /// <summary>
    /// Get authorization URL for OAuth flow
    /// </summary>
    [Get("/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope={scopes}&state={state}")]
    Task<HttpResponseMessage> GetAuthUrlAsync(
        string clientId,
        string redirectUri,
        string scopes,
        string state
    );

    /// <summary>
    /// Exchange authorization code for access token
    /// </summary>
    [Post("/api/token")]
    Task<SpotifyTokenResponse> RequestTokenAsync(
        [Body(BodySerializationMethod.UrlEncoded)] SpotifyTokenRequest request
    );

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [Get("/v1/me")]
    Task<SpotifyUserDto> GetCurrentUserAsync(
        [Header("Authorization")] string authorization
    );

    /// <summary>
    /// Search for tracks
    /// </summary>
    [Get("/v1/search?type=track&limit={limit}")]
    Task<SpotifySearchResponse> SearchTracksAsync(
        [Query] string q,
        [Header("Authorization")] string authorization,
        int limit = 20
    );

    /// <summary>
    /// Get track audio features for mood analysis
    /// </summary>
    [Get("/v1/audio-features/{trackId}")]
    Task<SpotifyAudioFeaturesDto> GetAudioFeaturesAsync(
        string trackId,
        [Header("Authorization")] string authorization
    );

    /// <summary>
    /// Get recommendations based on seed tracks
    /// </summary>
    [Get("/v1/recommendations?limit={limit}")]
    Task<SpotifyRecommendationsResponse> GetRecommendationsAsync(
        [Query] string seed_tracks,
        [Query] string seed_artists,
        [Query] double target_energy,
        [Query] double target_valence,
        [Header("Authorization")] string authorization,
        int limit = 20
    );

    /// <summary>
    /// Create or get user's MIMM playlist
    /// </summary>
    [Post("/v1/users/{userId}/playlists")]
    Task<SpotifyPlaylistDto> CreatePlaylistAsync(
        string userId,
        [Body] CreatePlaylistRequest request,
        [Header("Authorization")] string authorization
    );

    /// <summary>
    /// Add tracks to playlist
    /// </summary>
    [Post("/v1/playlists/{playlistId}/tracks")]
    Task<HttpResponseMessage> AddTracksToPlaylistAsync(
        string playlistId,
        [Body] AddTracksRequest request,
        [Header("Authorization")] string authorization
    );
}

/// <summary>
/// OAuth token request payload
/// </summary>
public class SpotifyTokenRequest
{
    [AliasAs("grant_type")]
    public string GrantType { get; set; } = "authorization_code";

    [AliasAs("code")]
    public string Code { get; set; } = string.Empty;

    [AliasAs("redirect_uri")]
    public string RedirectUri { get; set; } = string.Empty;

    [AliasAs("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [AliasAs("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// OAuth token response
/// </summary>
public class SpotifyTokenResponse
{
    [AliasAs("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [AliasAs("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [AliasAs("expires_in")]
    public int ExpiresIn { get; set; }

    [AliasAs("refresh_token")]
    public string? RefreshToken { get; set; }
}

/// <summary>
/// Spotify user profile
/// </summary>
public class SpotifyUserDto
{
    [AliasAs("id")]
    public string Id { get; set; } = string.Empty;

    [AliasAs("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [AliasAs("external_urls")]
    public Dictionary<string, string>? ExternalUrls { get; set; }

    [AliasAs("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Track search response
/// </summary>
public class SpotifySearchResponse
{
    [AliasAs("tracks")]
    public TracksContainer? Tracks { get; set; }

    public class TracksContainer
    {
        [AliasAs("items")]
        public List<SpotifyTrackDto> Items { get; set; } = [];
    }
}

/// <summary>
/// Spotify track object
/// </summary>
public class SpotifyTrackDto
{
    [AliasAs("id")]
    public string Id { get; set; } = string.Empty;

    [AliasAs("name")]
    public string Name { get; set; } = string.Empty;

    [AliasAs("uri")]
    public string Uri { get; set; } = string.Empty;

    [AliasAs("artists")]
    public List<SpotifyArtistDto> Artists { get; set; } = [];

    [AliasAs("album")]
    public SpotifyAlbumDto? Album { get; set; }

    [AliasAs("explicit")]
    public bool Explicit { get; set; }
}

/// <summary>
/// Spotify artist object
/// </summary>
public class SpotifyArtistDto
{
    [AliasAs("id")]
    public string Id { get; set; } = string.Empty;

    [AliasAs("name")]
    public string Name { get; set; } = string.Empty;

    [AliasAs("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Spotify album object
/// </summary>
public class SpotifyAlbumDto
{
    [AliasAs("id")]
    public string Id { get; set; } = string.Empty;

    [AliasAs("name")]
    public string Name { get; set; } = string.Empty;

    [AliasAs("images")]
    public List<SpotifyImageDto> Images { get; set; } = [];
}

/// <summary>
/// Spotify image object
/// </summary>
public class SpotifyImageDto
{
    [AliasAs("url")]
    public string Url { get; set; } = string.Empty;

    [AliasAs("height")]
    public int? Height { get; set; }

    [AliasAs("width")]
    public int? Width { get; set; }
}

/// <summary>
/// Audio features for mood analysis
/// </summary>
public class SpotifyAudioFeaturesDto
{
    [AliasAs("id")]
    public string Id { get; set; } = string.Empty;

    [AliasAs("energy")]
    public double Energy { get; set; }

    [AliasAs("valence")]
    public double Valence { get; set; }

    [AliasAs("tempo")]
    public double Tempo { get; set; }

    [AliasAs("danceability")]
    public double Danceability { get; set; }

    [AliasAs("acousticness")]
    public double Acousticness { get; set; }

    [AliasAs("speechiness")]
    public double Speechiness { get; set; }
}

/// <summary>
/// Playlist recommendations response
/// </summary>
public class SpotifyRecommendationsResponse
{
    [AliasAs("tracks")]
    public List<SpotifyTrackDto> Tracks { get; set; } = [];
}

/// <summary>
/// Spotify playlist object
/// </summary>
public class SpotifyPlaylistDto
{
    [AliasAs("id")]
    public string Id { get; set; } = string.Empty;

    [AliasAs("name")]
    public string Name { get; set; } = string.Empty;

    [AliasAs("external_urls")]
    public Dictionary<string, string>? ExternalUrls { get; set; }

    [AliasAs("uri")]
    public string Uri { get; set; } = string.Empty;
}

/// <summary>
/// Request to create playlist
/// </summary>
public class CreatePlaylistRequest
{
    public string Name { get; set; } = "MIMM Playlist";
    public string Description { get; set; } = "Playlist synced from Music & Mood Journal";
    public bool Public { get; set; } = false;
}

/// <summary>
/// Request to add tracks to playlist
/// </summary>
public class AddTracksRequest
{
    [AliasAs("uris")]
    public List<string> Uris { get; set; } = [];
}
