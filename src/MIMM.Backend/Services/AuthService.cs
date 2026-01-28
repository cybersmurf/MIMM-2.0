using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using MIMM.Backend.Data;
using MIMM.Shared.Entities;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Services;

/// <summary>
/// Service for authentication operations (register, login, token refresh)
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register new user with email and password
    /// </summary>
    Task<(bool Success, string? ErrorMessage, AuthenticationResponse? Response)> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    Task<(bool Success, string? ErrorMessage, AuthenticationResponse? Response)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<(bool Success, string? ErrorMessage, AuthenticationResponse? Response)> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Verify and parse JWT token, return claims principal
    /// </summary>
    Task<(bool Success, ClaimsPrincipal? Principal, string? ErrorMessage)> VerifyTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of authentication service
/// </summary>
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext dbContext,
        ILogger<AuthService> logger,
        IConfiguration configuration
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Register new user: validate input, hash password, create user in DB
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, AuthenticationResponse? Response)> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Email and password are required", null);
        }

        if (request.Password.Length < 6)
        {
            return (false, "Password must be at least 6 characters long", null);
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check if user already exists (case-insensitive)
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return (false, "Email is already registered", null);
        }

        try
        {
            // Hash password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                DisplayName = request.DisplayName ?? request.Email.Split('@')[0],
                Language = request.Language ?? "en",
                CreatedAt = DateTime.UtcNow,
                EmailVerified = false
            };

            // Save to database
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);

            // Generate JWT tokens
            var (accessToken, accessTokenExpiration) = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Persist refresh token for newly registered user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7)
            );
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Return authentication response with tokens
            var authResponse = new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    DisplayName = user.DisplayName,
                    Language = user.Language,
                    EmailVerified = user.EmailVerified
                }
            };

            return (true, null, authResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            return (false, "An error occurred during registration", null);
        }
    }

    /// <summary>
    /// Login user: verify credentials and issue JWT tokens
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, AuthenticationResponse? Response)> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return (false, "Email and password are required", null);
        }

        // Find user by email
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return (false, "Invalid email or password", null);
        }

        // Check if user is soft-deleted
        if (user.DeletedAt.HasValue)
        {
            _logger.LogWarning("Login attempt with deleted user: {UserId}", user.Id);
            return (false, "Invalid email or password", null);
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for user: {UserId}", user.Id);
            return (false, "Invalid email or password", null);
        }

        try
        {
            // Generate tokens
            var (accessToken, accessTokenExpiration) = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Store refresh token in database
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7)
            );
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

            var response = new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    DisplayName = user.DisplayName ?? string.Empty,
                    Language = user.Language ?? "en",
                    EmailVerified = user.EmailVerified
                }
            };

            return (true, null, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {UserId}", user.Id);
            return (false, "An error occurred during login", null);
        }
    }

    /// <summary>
    /// Refresh access token using valid refresh token
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, AuthenticationResponse? Response)> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return (false, "Refresh token is required", null);
        }

        try
        {
            // Find user with matching refresh token
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(
                    u => u.RefreshToken == refreshToken && u.RefreshTokenExpiresAt > DateTime.UtcNow,
                    cancellationToken
                );

            if (user == null)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                return (false, "Invalid or expired refresh token", null);
            }

            // Generate new access token
            var (accessToken, accessTokenExpiration) = GenerateAccessToken(user);

            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7)
            );
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token refreshed for user: {UserId}", user.Id);

            var response = new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiresAt = accessTokenExpiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    DisplayName = user.DisplayName ?? string.Empty,
                    Language = user.Language ?? "en",
                    EmailVerified = user.EmailVerified
                }
            };

            return (true, null, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return (false, "An error occurred during token refresh", null);
        }
    }

    /// <summary>
    /// Verify JWT token and return claims principal
    /// </summary>
    public Task<(bool Success, ClaimsPrincipal? Principal, string? ErrorMessage)> VerifyTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult((false, default(ClaimsPrincipal?), (string?)"Token is required"));
        }

        try
        {
            // Use environment variables first (Docker production), fallback to appsettings
            var jwtKey = _configuration["JWT_SECRET_KEY"] ?? _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key not configured");
            var jwtIssuer = _configuration["JWT_ISSUER"] ?? _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer not configured");
            var jwtAudience = _configuration["JWT_AUDIENCE"] ?? _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience not configured");

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(10)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken)
            {
                return Task.FromResult((false, default(ClaimsPrincipal?), (string?)"Invalid token format"));
            }

            return Task.FromResult((true, (ClaimsPrincipal?)principal, (string?)null));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token verification failed");
            return Task.FromResult((false, default(ClaimsPrincipal?), (string?)"Token verification failed"));
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(
                u => u.Email == email.ToLowerInvariant() && u.DeletedAt == null,
                cancellationToken
            );
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Generate JWT access token for user
    /// </summary>
    private (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(User user)
    {
        // Use environment variables first (Docker production), fallback to appsettings
        var jwtKey = _configuration["JWT_SECRET_KEY"] ?? _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key not configured");
        var jwtIssuer = _configuration["JWT_ISSUER"] ?? _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer not configured");
        var jwtAudience = _configuration["JWT_AUDIENCE"] ?? _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience not configured");
        var expirationMinutes = _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 60);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.DisplayName ?? string.Empty),
            new Claim("language", user.Language ?? "en"),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.WriteToken(token);

        return (jwt, expiresAt);
    }

    /// <summary>
    /// Generate random refresh token (base64 encoded)
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }
        return Convert.ToBase64String(randomNumber);
    }
}
