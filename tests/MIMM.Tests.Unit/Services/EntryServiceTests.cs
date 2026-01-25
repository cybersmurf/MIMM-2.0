using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MIMM.Backend.Services;
using MIMM.Backend.Data;
using MIMM.Shared.Entities;
using MIMM.Shared.Dtos;
using Xunit;

namespace MIMM.Tests.Unit.Services;

public class EntryServiceTests : IAsyncLifetime
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<EntryService>> _mockLogger;
    private EntryService? _entryService;
    private Guid _testUserId;

    public EntryServiceTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"EntryServiceTestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _mockLogger = new Mock<ILogger<EntryService>>();
        _testUserId = Guid.NewGuid();
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
        
        // Create test user
        var testUser = new User
        {
            Id = _testUserId,
            Email = "testuser@example.com",
            PasswordHash = "hash",
            DisplayName = "Test User",
            Language = "en"
        };
        
        _dbContext.Users.Add(testUser);
        await _dbContext.SaveChangesAsync();
        
        _entryService = new EntryService(_dbContext, _mockLogger.Object);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    #region Create Tests

    [Fact]
    public async Task CreateAsync_WithValidInput_ShouldCreateEntry()
    {
        // Arrange
        var request = new CreateEntryRequest
        {
            SongTitle = "Bohemian Rhapsody",
            ArtistName = "Queen",
            AlbumName = "A Night at the Opera",
            SongId = "lastfm:123",
            CoverUrl = "https://example.com/cover.jpg",
            Source = "lastfm",
            Valence = 0.5f,
            Arousal = 0.8f,
            TensionLevel = 45,
            SomaticTags = ["euphoric", "energetic"],
            Notes = "Amazing song!"
        };

        // Act
        var result = await _entryService!.CreateAsync(_testUserId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Entry.Should().NotBeNull();
        result.Entry!.SongTitle.Should().Be("Bohemian Rhapsody");
        result.Entry.ArtistName.Should().Be("Queen");
        result.Entry.Valence.Should().Be(0.5f);

        // Verify in database
        var dbEntry = await _dbContext.Entries.FirstOrDefaultAsync(e => e.SongTitle == "Bohemian Rhapsody");
        dbEntry.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WithMinimalInput_ShouldCreateEntry()
    {
        // Arrange
        var request = new CreateEntryRequest
        {
            SongTitle = "Simple Song"
        };

        // Act
        var result = await _entryService!.CreateAsync(_testUserId, request);

        // Assert
        result.Success.Should().BeTrue();
        result.Entry.Should().NotBeNull();
        result.Entry!.SongTitle.Should().Be("Simple Song");
        result.Entry.Source.Should().Be("manual");
        result.Entry.Valence.Should().Be(0.0f);
        result.Entry.Arousal.Should().Be(0.0f);
    }

    [Fact]
    public async Task CreateAsync_WithMissingTitle_ShouldReturnError()
    {
        // Arrange
        var request = new CreateEntryRequest
        {
            SongTitle = ""
        };

        // Act
        var result = await _entryService!.CreateAsync(_testUserId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Song title is required");
    }

    [Fact]
    public async Task CreateAsync_WithNonexistentUser_ShouldReturnError()
    {
        // Arrange
        var invalidUserId = Guid.NewGuid();
        var request = new CreateEntryRequest { SongTitle = "Test" };

        // Act
        var result = await _entryService!.CreateAsync(invalidUserId, request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("User not found");
    }

    #endregion

    #region Get Tests

    [Fact]
    public async Task GetAsync_WithValidId_ShouldReturnEntry()
    {
        // Arrange
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            SongTitle = "Test Song",
            ArtistName = "Test Artist",
            AlbumName = "Test Album",
            Source = "manual",
            Valence = 0.6f,
            Arousal = 0.4f
        };
        
        _dbContext.Entries.Add(entry);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _entryService!.GetAsync(_testUserId, entry.Id);

        // Assert
        result.Should().NotBeNull();
        result!.SongTitle.Should().Be("Test Song");
        result.ArtistName.Should().Be("Test Artist");
    }

    [Fact]
    public async Task GetAsync_WithNonexistentId_ShouldReturnNull()
    {
        // Act
        var result = await _entryService!.GetAsync(_testUserId, Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithDeletedEntry_ShouldReturnNull()
    {
        // Arrange
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            SongTitle = "Deleted Song",
            AlbumName = "Unknown",
            Source = "manual",
            DeletedAt = DateTime.UtcNow
        };
        
        _dbContext.Entries.Add(entry);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _entryService!.GetAsync(_testUserId, entry.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region List Tests

    [Fact]
    public async Task ListAsync_WithMultipleEntries_ShouldReturnPaginated()
    {
        // Arrange
        var entries = Enumerable.Range(1, 25)
            .Select(i => new JournalEntry
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                SongTitle = $"Song {i}",
                ArtistName = $"Artist {i}",
                AlbumName = $"Album {i}",
                Source = "manual",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            })
            .ToList();

        _dbContext.Entries.AddRange(entries);
        await _dbContext.SaveChangesAsync();

        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _entryService!.ListAsync(_testUserId, request);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_WithPageTwo_ShouldReturnCorrectPage()
    {
        // Arrange
        var entries = Enumerable.Range(1, 15)
            .Select(i => new JournalEntry
            {
                Id = Guid.NewGuid(),
                UserId = _testUserId,
                SongTitle = $"Song {i}",
                AlbumName = "Album",
                Source = "manual"
            })
            .ToList();

        _dbContext.Entries.AddRange(entries);
        await _dbContext.SaveChangesAsync();

        var request = new PaginationRequest { PageNumber = 2, PageSize = 5 };

        // Act
        var result = await _entryService!.ListAsync(_testUserId, request);

        // Assert
        result.Items.Should().HaveCount(5);
        result.PageNumber.Should().Be(2);
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public async Task ListAsync_WithSorting_ShouldReturnSorted()
    {
        // Arrange
        _dbContext.Entries.AddRange(
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Zebra", AlbumName = "A", Source = "manual" },
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Apple", AlbumName = "A", Source = "manual" },
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Mango", AlbumName = "A", Source = "manual" }
        );
        await _dbContext.SaveChangesAsync();

        var request = new PaginationRequest { SortBy = "title", SortDirection = "asc" };

        // Act
        var result = await _entryService!.ListAsync(_testUserId, request);

        // Assert
        result.Items[0].SongTitle.Should().Be("Apple");
        result.Items[1].SongTitle.Should().Be("Mango");
        result.Items[2].SongTitle.Should().Be("Zebra");
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_WithQueryFilter_ShouldReturnMatches()
    {
        // Arrange
        _dbContext.Entries.AddRange(
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Bohemian Rhapsody", ArtistName = "Queen", AlbumName = "A Night at the Opera", Source = "manual" },
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Another Song", ArtistName = "Artist", AlbumName = "Album", Source = "manual" }
        );
        await _dbContext.SaveChangesAsync();

        var request = new SearchEntriesRequest { Query = "Bohemian" };

        // Act
        var result = await _entryService!.SearchAsync(_testUserId, request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].SongTitle.Should().Be("Bohemian Rhapsody");
    }

    [Fact]
    public async Task SearchAsync_WithMoodFilter_ShouldReturnMatches()
    {
        // Arrange
        _dbContext.Entries.AddRange(
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Happy", AlbumName = "A", Source = "manual", Valence = 0.8f },
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Sad", AlbumName = "A", Source = "manual", Valence = -0.8f }
        );
        await _dbContext.SaveChangesAsync();

        var request = new SearchEntriesRequest { MinValence = 0.5 };

        // Act
        var result = await _entryService!.SearchAsync(_testUserId, request);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].SongTitle.Should().Be("Happy");
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_WithValidInput_ShouldUpdateEntry()
    {
        // Arrange
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            SongTitle = "Original Title",
            AlbumName = "Album",
            Source = "manual",
            Valence = 0.5f
        };
        
        _dbContext.Entries.Add(entry);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateEntryRequest
        {
            SongTitle = "Updated Title",
            Valence = 0.8
        };

        // Act
        var result = await _entryService!.UpdateAsync(_testUserId, entry.Id, updateRequest);

        // Assert
        result.Success.Should().BeTrue();
        result.Entry.Should().NotBeNull();
        result.Entry!.SongTitle.Should().Be("Updated Title");
        result.Entry.Valence.Should().Be(0.8f);
    }

    [Fact]
    public async Task UpdateAsync_WithNonexistentEntry_ShouldReturnError()
    {
        // Arrange
        var request = new UpdateEntryRequest { SongTitle = "New Title" };

        // Act
        var result = await _entryService!.UpdateAsync(_testUserId, Guid.NewGuid(), request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Entry not found");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeleteEntry()
    {
        // Arrange
        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            UserId = _testUserId,
            SongTitle = "To Delete",
            AlbumName = "Album",
            Source = "manual",
            SomaticTags = ["test"]
        };
        
        _dbContext.Entries.Add(entry);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _entryService!.DeleteAsync(_testUserId, entry.Id);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentEntry_ShouldReturnError()
    {
        // Act
        var result = await _entryService!.DeleteAsync(_testUserId, Guid.NewGuid());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Entry not found");
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_WithEntries_ShouldReturnStats()
    {
        // Arrange
        _dbContext.Entries.AddRange(
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Song 1", ArtistName = "Artist A", AlbumName = "Album A", Source = "manual", Valence = 0.6f, Arousal = 0.7f },
            new JournalEntry { Id = Guid.NewGuid(), UserId = _testUserId, SongTitle = "Song 2", ArtistName = "Artist B", AlbumName = "Album B", Source = "manual", Valence = 0.4f, Arousal = 0.5f }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _entryService!.GetStatisticsAsync(_testUserId);

        // Assert
        result.Should().NotBeNull();
        result!.TotalEntries.Should().Be(2);
        result.AverageValence.Should().Be(0.5f);
        result.AverageArousal.Should().Be(0.6f);
        result.UniqueSongs.Should().Be(2);
        result.UniqueArtists.Should().Be(2);
    }

    [Fact]
    public async Task GetStatisticsAsync_WithNoEntries_ShouldReturnZeroStats()
    {
        // Act
        var result = await _entryService!.GetStatisticsAsync(_testUserId);

        // Assert
        result.Should().NotBeNull();
        result!.TotalEntries.Should().Be(0);
        result.UniqueSongs.Should().Be(0);
    }

    #endregion
}
