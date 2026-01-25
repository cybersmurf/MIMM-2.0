namespace MIMM.Shared.Dtos;

/// <summary>
/// Request DTO for user registration
/// </summary>
public record RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? DisplayName { get; init; }
    public string Language { get; init; } = "en";
}

/// <summary>
/// Request DTO for user login
/// </summary>
public record LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

/// <summary>
/// Request DTO for refreshing access token
/// </summary>
public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}

/// <summary>
/// Response DTO for successful authentication
/// </summary>
public record AuthenticationResponse
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public required UserDto User { get; init; }
    public DateTime AccessTokenExpiresAt { get; init; }
}

/// <summary>
/// Minimal user DTO for auth responses
/// </summary>
public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required string Language { get; init; }
    public required bool EmailVerified { get; init; }
}

/// <summary>
/// Music track result used by music search endpoints
/// </summary>
public record MusicTrackDto
{
    public required string Title { get; init; }
    public string? Artist { get; init; }
    public string? Album { get; init; }
    public string? CoverUrl { get; init; }
    public string? Source { get; init; }
    public string? ExternalId { get; init; }
}

/// <summary>
/// Response wrapper for music search
/// </summary>
public record MusicSearchResponse
{
    public required string Query { get; init; }
    public required List<MusicTrackDto> Items { get; init; }
}
