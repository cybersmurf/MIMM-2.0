using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using MIMM.Backend.Data;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;
using Xunit;

namespace MIMM.Tests.Unit.Services;

public class MusicSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_ReturnsMusicBrainzTracksAndCaches_OnSuccess()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "recordings": [
                        {
                          "id": "mbid-123",
                          "title": "Song",
                          "artist-credit": [{ "name": "Artist", "artist": { "id": "artist-1", "name": "Artist" } }],
                          "releases": [{ "id": "release-1", "title": "Album", "cover-art-archive": { "front": true }, "date": "1991-09-10" }]
                        }
                      ]
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });

        var httpClient = new HttpClient(handler.Object);
        var logger = Mock.Of<ILogger<MusicSearchService>>();
        await using var dbContext = CreateDbContext();
        var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
        var service = new MusicSearchService(httpClient, dbContext, logger, cache);

        // Act
        var result = await service.SearchAsync("Song");

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Song");
        result[0].Artist.Should().Be("Artist");
        result[0].Album.Should().Be("Album");
        result[0].CoverUrl.Should().Contain("release-1");
        result[0].Source.Should().Be("musicbrainz");
        result[0].ExternalId.Should().Be("mbid-123");

        (await dbContext.MusicBrainzRecordings.CountAsync()).Should().Be(1);
        (await dbContext.MusicBrainzArtists.CountAsync()).Should().Be(1);
        (await dbContext.MusicBrainzReleases.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SearchAsync_InvalidQuery_ReturnsEmpty()
    {
        var logger = Mock.Of<ILogger<MusicSearchService>>();
        await using var dbContext = CreateDbContext();
        var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
        var service = new MusicSearchService(new HttpClient(new Mock<HttpMessageHandler>().Object), dbContext, logger, cache);

        var result = await service.SearchAsync(" ");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_FailedRequest_ReturnsEmpty()
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadGateway));

        var httpClient = new HttpClient(handler.Object);
        var logger = Mock.Of<ILogger<MusicSearchService>>();
        await using var dbContext = CreateDbContext();
        var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
        var service = new MusicSearchService(httpClient, dbContext, logger, cache);

        var result = await service.SearchAsync("Song");

        result.Should().BeEmpty();
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
