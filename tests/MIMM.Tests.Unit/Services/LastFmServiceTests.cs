using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using MIMM.Backend.Services;
using MIMM.Backend.Data;
using MIMM.Shared.Entities;
using Xunit;

namespace MIMM.Tests.Unit.Services;

public class LastFmServiceTests : IAsyncLifetime
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<LastFmService>> _mockLogger;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly IConfiguration _configuration;
    private LastFmService? _lastFmService;
    private readonly User _testUser;
    private readonly LastFmToken _testToken;

    public LastFmServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"LastFmServiceTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<LastFmService>>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();

        var configDict = new Dictionary<string, string?>
        {
            { "LastFm:ApiKey", "test-api-key" },
            { "LastFm:SharedSecret", "test-shared-secret" },
            { "LastFm:StateSecret", "test-state-secret" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        _testToken = new LastFmToken
        {
            UserId = _testUser.Id,
            SessionKey = "test-session-key",
            LastFmUsername = "testlastfmuser",
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
        await _dbContext.Users.AddAsync(_testUser);
        await _dbContext.LastFmTokens.AddAsync(_testToken);
        await _dbContext.SaveChangesAsync();

        _lastFmService = new LastFmService(_dbContext, _configuration, _mockHttpClientFactory.Object, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task ScrobbleAsync_WithoutSessionKey_ReturnsFalse()
    {
        // Arrange
        var userWithoutToken = new User
        {
            Id = Guid.NewGuid(),
            Email = "notoken@example.com",
            PasswordHash = "hash"
        };

        await _dbContext.Users.AddAsync(userWithoutToken);
        await _dbContext.SaveChangesAsync();

        // Act
        var (success, error) = await _lastFmService!.ScrobbleAsync(
            userWithoutToken.Id,
            "Test Song",
            "Test Artist",
            "Test Album",
            DateTime.UtcNow);

        // Assert
        success.Should().BeFalse();
        error.Should().Contain("not connected");
    }

    [Fact]
    public async Task ScrobbleAsync_WithValidSessionKey_CallsHttpClient()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("""{"lfm": {"status": "ok"}}""")
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient("lastfm")).Returns(httpClient);

        // Act
        var (success, error) = await _lastFmService!.ScrobbleAsync(
            _testUser.Id,
            "Test Song",
            "Test Artist",
            "Test Album",
            DateTime.UtcNow);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNullOrEmpty();
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
