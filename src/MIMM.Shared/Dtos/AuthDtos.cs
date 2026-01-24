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
