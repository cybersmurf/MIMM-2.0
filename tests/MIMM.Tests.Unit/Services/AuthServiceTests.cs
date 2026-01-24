using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MIMM.Backend.Services;
using MIMM.Backend.Data;
using MIMM.Shared.Entities;
using MIMM.Shared.Dtos;
using Xunit;

namespace MIMM.Tests.Unit.Services;

public class AuthServiceTests : IAsyncLifetime
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private AuthService? _authService;

    public AuthServiceTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuthServiceTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<AuthService>>();

        // Create a simple configuration dictionary for testing
        var configDict = new Dictionary<string, string?>
        {
            { "Jwt:Key", "this-is-a-very-long-secret-key-for-jwt-256bits" },
            { "Jwt:Issuer", "https://mimm.local" },
            { "Jwt:Audience", "mimm-frontend" },
            { "Jwt:RefreshTokenExpirationDays", "7" },
            { "Jwt:AccessTokenExpirationMinutes", "60" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
        _authService = new AuthService(_dbContext, _mockLogger.Object, _configuration);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_WithValidInput_ShouldCreateUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "SecurePassword123",
            DisplayName = "Test User",
            Language = "en"
        };

        // Act
        var result = await _authService!.RegisterAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.User.Should().NotBeNull();
        result.User!.Email.Should().Be("test@example.com");
        result.User.DisplayName.Should().Be("Test User");

        // Verify user was persisted to database
        var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        dbUser.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldReturnError()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com",
            PasswordHash = "hash",
            DisplayName = "Existing"
        };
        
        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "SecurePassword123",
            DisplayName = "Duplicate"
        };

        // Act
        var result = await _authService!.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already registered");
    }

    [Fact]
    public async Task RegisterAsync_WithWeakPassword_ShouldReturnError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "weak"
        };

        // Act
        var result = await _authService!.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("at least 6 characters");
    }

    [Fact]
    public async Task RegisterAsync_WithMissingEmail_ShouldReturnError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "",
            Password = "ValidPassword123"
        };

        // Act
        var result = await _authService!.RegisterAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("required");
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            DisplayName = "Test",
            Language = "en"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "CorrectPassword123"
        };

        // Act
        var result = await _authService!.LoginAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Response.Should().NotBeNull();
        result.Response!.AccessToken.Should().NotBeNullOrEmpty();
        result.Response.RefreshToken.Should().NotBeNullOrEmpty();
        result.Response.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "AnyPassword123"
        };

        // Act
        var result = await _authService!.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnError()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = hashedPassword,
            DisplayName = "Test"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123"
        };

        // Act
        var result = await _authService!.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithDeletedUser_ShouldReturnError()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "deleted@example.com",
            PasswordHash = hashedPassword,
            DisplayName = "Deleted",
            DeletedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "deleted@example.com",
            Password = "CorrectPassword123"
        };

        // Act
        var result = await _authService!.LoginAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var validRefreshToken = "valid-refresh-token-value";

        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            DisplayName = "Test",
            Language = "en",
            RefreshToken = validRefreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService!.RefreshTokenAsync(validRefreshToken);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Response.Should().NotBeNull();
        result.Response!.AccessToken.Should().NotBeNullOrEmpty();
        result.Response.RefreshToken.Should().NotBeNullOrEmpty();
        result.Response.RefreshToken.Should().NotBe(validRefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnError()
    {
        // Arrange
        var expiredRefreshToken = "expired-token";

        // Act
        var result = await _authService!.RefreshTokenAsync(expiredRefreshToken);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_WithEmptyToken_ShouldReturnError()
    {
        // Act
        var result = await _authService!.RefreshTokenAsync("");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Refresh token is required");
    }

    #endregion

    #region GetUser Tests

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            DisplayName = "Test"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService!.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            DisplayName = "Test"
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _authService!.GetUserByEmailAsync("test@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonexistentId_ShouldReturnNull()
    {
        // Act
        var result = await _authService!.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region VerifyToken Tests

    [Fact]
    public async Task VerifyTokenAsync_WithValidToken_ShouldReturnPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            Language = "en",
            PasswordHash = "hash"
        };

        // Generate valid token manually
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;

        var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(jwtKey)
        );
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            securityKey,
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256
        );

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName)
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = tokenHandler.WriteToken(token);

        // Act
        var result = await _authService!.VerifyTokenAsync(jwt);

        // Assert
        result.Success.Should().BeTrue();
        result.Principal.Should().NotBeNull();
        result.ErrorMessage.Should().BeNull();
        result.Principal!.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@example.com");
    }

    [Fact]
    public async Task VerifyTokenAsync_WithInvalidToken_ShouldReturnError()
    {
        // Arrange
        var invalidToken = "invalid.token.format";

        // Act
        var result = await _authService!.VerifyTokenAsync(invalidToken);

        // Assert
        result.Success.Should().BeFalse();
        result.Principal.Should().BeNull();
        result.ErrorMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task VerifyTokenAsync_WithEmptyToken_ShouldReturnError()
    {
        // Act
        var result = await _authService!.VerifyTokenAsync("");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Token is required");
    }

    #endregion
}
