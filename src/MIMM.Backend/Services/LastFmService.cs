using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MIMM.Backend.Data;
using MIMM.Shared.Entities;

namespace MIMM.Backend.Services;

public sealed class LastFmService(ApplicationDbContext db, IConfiguration config, IHttpClientFactory httpFactory, ILogger<LastFmService> logger) : ILastFmService
{
    private readonly ApplicationDbContext _db = db;
    private readonly IConfiguration _config = config;
    private readonly IHttpClientFactory _httpFactory = httpFactory;
    private readonly ILogger<LastFmService> _logger = logger;

    public Task<string> GetAuthUrlAsync(Guid userId, string requestBaseUrl, CancellationToken cancellationToken = default)
    {
        var apiKey = _config["LastFm:ApiKey"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("LastFm:ApiKey is not configured");

        var state = CreateState(userId);
        // Prefer configured callback if provided, else build from request base
        var configuredCb = _config["LastFm:CallbackUrl"];
        var rawCb = !string.IsNullOrWhiteSpace(configuredCb)
            ? configuredCb!
            : new Uri(new Uri(AppendSlash(requestBaseUrl)), "api/lastfm/callback").ToString();
        var cb = rawCb.Contains('?')
            ? $"{rawCb}&s={Uri.EscapeDataString(state)}"
            : $"{rawCb}?s={Uri.EscapeDataString(state)}";
        var url = $"https://www.last.fm/api/auth/?api_key={Uri.EscapeDataString(apiKey)}&cb={Uri.EscapeDataString(cb)}";
        return Task.FromResult(url);
    }

    public async Task<(bool Success, string? Username, string? Error)> HandleCallbackAsync(string state, string token, CancellationToken cancellationToken = default)
    {
        var userId = ValidateState(state);
        if (userId == Guid.Empty)
            return (false, null, "Invalid state");

        var apiKey = _config["LastFm:ApiKey"] ?? string.Empty;
        var secret = _config["LastFm:SharedSecret"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secret))
            return (false, null, "Last.fm credentials not configured");

        try
        {
            var client = _httpFactory.CreateClient("lastfm");
            // Build auth.getSession signed request
            // params: api_key, method=auth.getSession, token, format=json
            var apiSig = BuildApiSig(new Dictionary<string, string>
            {
                ["api_key"] = apiKey,
                ["method"] = "auth.getSession",
                ["token"] = token
            }, secret);

            var url = $"?method=auth.getSession&api_key={Uri.EscapeDataString(apiKey)}&token={Uri.EscapeDataString(token)}&api_sig={apiSig}&format=json";
            var resp = await client.GetAsync(url, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Last.fm getSession failed: {Status}", resp.StatusCode);
                return (false, null, $"Last.fm error: {resp.StatusCode}");
            }

            using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("session", out var sessionEl))
                return (false, null, "Invalid response from Last.fm");

            var key = sessionEl.GetProperty("key").GetString();
            var name = sessionEl.GetProperty("name").GetString();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name))
                return (false, null, "Missing session data");

            // Upsert token for user
            var existing = await _db.LastFmTokens.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            if (existing is null)
            {
                existing = new LastFmToken
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    SessionKey = key,
                    LastFmUsername = name,
                    ExpiresAt = null
                };
                _db.LastFmTokens.Add(existing);
            }
            else
            {
                existing.SessionKey = key;
                existing.LastFmUsername = name;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return (true, name, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Last.fm callback processing failed");
            return (false, null, ex.Message);
        }
    }

    private static string AppendSlash(string baseUrl) => baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";

    private string CreateState(Guid userId)
    {
        var key = _config["LastFm:StateSecret"] ?? _config["Jwt:Key"] ?? throw new InvalidOperationException("State secret missing");
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var payload = $"{userId:N}|{ts}";
        var sig = ComputeHmac(payload, key);
        return $"{userId:N}.{ts}.{sig}";
    }

    private Guid ValidateState(string state)
    {
        try
        {
            var key = _config["LastFm:StateSecret"] ?? _config["Jwt:Key"] ?? string.Empty;
            var parts = state.Split('.', 3);
            if (parts.Length != 3) return Guid.Empty;
            var uidStr = parts[0];
            var tsStr = parts[1];
            var sig = parts[2];
            if (!Guid.TryParseExact(uidStr, "N", out var uid)) return Guid.Empty;
            if (!long.TryParse(tsStr, out var ts)) return Guid.Empty;
            var payload = $"{uidStr}|{tsStr}";
            var expect = ComputeHmac(payload, key);
            if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expect), Encoding.UTF8.GetBytes(sig))) return Guid.Empty;
            var age = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ts;
            if (age < 0 || age > 600) return Guid.Empty; // 10 min
            return uid;
        }
        catch
        {
            return Guid.Empty;
        }
    }

    private static string ComputeHmac(string data, string key)
    {
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var bytes = h.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string BuildApiSig(IDictionary<string, string> parameters, string secret)
    {
        // Sort by key asc and concatenate key+value, then append secret, MD5
        var sb = new StringBuilder();
        foreach (var kv in parameters.OrderBy(k => k.Key, StringComparer.Ordinal))
        {
            sb.Append(kv.Key);
            sb.Append(kv.Value);
        }
        sb.Append(secret);
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<(bool Success, string? Error)> ScrobbleAsync(
        Guid userId,
        string songTitle,
        string? artistName,
        string? albumName,
        DateTime? timestamp,
        CancellationToken cancellationToken = default)
    {
        // Get user's Last.fm session key
        var token = await _db.LastFmTokens.FirstOrDefaultAsync(
            x => x.UserId == userId && x.SessionKey != null,
            cancellationToken);

        if (token?.SessionKey == null)
        {
            return (false, "User has not connected Last.fm");
        }

        var apiKey = _config["LastFm:ApiKey"] ?? string.Empty;
        var secret = _config["LastFm:SharedSecret"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(secret))
        {
            return (false, "Last.fm credentials not configured");
        }

        try
        {
            var client = _httpFactory.CreateClient("lastfm");
            
            // Build track.scrobble signed request
            // https://www.last.fm/api/show/track.scrobble
            var parameters = new Dictionary<string, string>
            {
                ["api_key"] = apiKey,
                ["artist"] = artistName ?? "Unknown Artist",
                ["track"] = songTitle,
                ["method"] = "track.scrobble",
                ["sk"] = token.SessionKey,
                ["timestamp"] = ((timestamp ?? DateTime.UtcNow).Ticks / 10000000 - 62135596800).ToString() // Unix timestamp
            };

            if (!string.IsNullOrWhiteSpace(albumName))
            {
                parameters["album"] = albumName;
            }

            var apiSig = BuildApiSig(parameters, secret);
            parameters["api_sig"] = apiSig;

            // POST request with form data
            using var content = new FormUrlEncodedContent(parameters);
            var response = await client.PostAsync("", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Last.fm scrobble failed: {Status}", response.StatusCode);
                return (false, $"Last.fm error: {response.StatusCode}");
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            // Check for error in response
            if (doc.RootElement.TryGetProperty("error", out var errorEl))
            {
                var errorCode = errorEl.GetInt32();
                var errorMsg = doc.RootElement.TryGetProperty("message", out var msgEl) 
                    ? msgEl.GetString() 
                    : $"Last.fm error {errorCode}";
                
                _logger.LogWarning("Last.fm scrobble error {Code}: {Message}", errorCode, errorMsg);
                return (false, errorMsg);
            }

            if (doc.RootElement.TryGetProperty("scrobbles", out var scrobblesEl))
            {
                _logger.LogInformation("Track scrobbled to Last.fm: {Song} by {Artist}", songTitle, artistName);
                return (true, null);
            }

            return (false, "Invalid response from Last.fm");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Last.fm scrobble failed for track {Song}", songTitle);
            return (false, ex.Message);
        }
    }
}
