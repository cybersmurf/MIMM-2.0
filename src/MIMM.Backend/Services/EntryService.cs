using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MIMM.Backend.Data;
using MIMM.Shared.Entities;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Services;

/// <summary>
/// Service for journal entry operations (CRUD + search)
/// </summary>
public interface IEntryService
{
    /// <summary>
    /// Create new journal entry
    /// </summary>
    Task<(bool Success, string? ErrorMessage, EntryDto? Entry)> CreateAsync(
        Guid userId,
        CreateEntryRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get single entry by ID
    /// </summary>
    Task<EntryDto?> GetAsync(
        Guid userId,
        Guid entryId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get all entries for user with pagination
    /// </summary>
    Task<PagedResponse<EntryDto>> ListAsync(
        Guid userId,
        PaginationRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Search entries with filters
    /// </summary>
    Task<PagedResponse<EntryDto>> SearchAsync(
        Guid userId,
        SearchEntriesRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Update existing entry
    /// </summary>
    Task<(bool Success, string? ErrorMessage, EntryDto? Entry)> UpdateAsync(
        Guid userId,
        Guid entryId,
        UpdateEntryRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Delete entry (soft delete)
    /// </summary>
    Task<(bool Success, string? ErrorMessage)> DeleteAsync(
        Guid userId,
        Guid entryId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Get user's entries statistics
    /// </summary>
    Task<EntryStatisticsDto?> GetStatisticsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Statistics about user's journal entries
/// </summary>
public record EntryStatisticsDto
{
    public int TotalEntries { get; init; }

    public double AverageValence { get; init; }

    public double AverageArousal { get; init; }

    public int UniqueSongs { get; init; }

    public int UniqueArtists { get; init; }

    public DateTime? OldestEntryDate { get; init; }

    public DateTime? NewestEntryDate { get; init; }
}

/// <summary>
/// Implementation of entry service
/// </summary>
public class EntryService : IEntryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<EntryService> _logger;
    private readonly INotificationService _notificationService;

    public EntryService(
        ApplicationDbContext dbContext,
        ILogger<EntryService> logger,
        INotificationService notificationService
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Create new journal entry
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, EntryDto? Entry)> CreateAsync(
        Guid userId,
        CreateEntryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // Validate user exists
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

        if (user == null)
        {
            return (false, "User not found", null);
        }

        // Validate input
        if (string.IsNullOrWhiteSpace(request.SongTitle))
        {
            return (false, "Song title is required", null);
        }

        try
        {
            var entry = new JournalEntry
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SongTitle = request.SongTitle.Trim(),
                ArtistName = string.IsNullOrWhiteSpace(request.ArtistName) ? null : request.ArtistName.Trim(),
                AlbumName = string.IsNullOrWhiteSpace(request.AlbumName) ? "Unknown Album" : request.AlbumName.Trim(),
                SongId = request.SongId,
                CoverUrl = request.CoverUrl,
                Source = request.Source ?? "manual",
                Valence = (float)request.Valence,
                Arousal = (float)request.Arousal,
                TensionLevel = request.TensionLevel,
                SomaticTags = request.SomaticTags ?? [],
                Notes = request.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                ScrobbledToLastFm = false
            };

            _dbContext.Entries.Add(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Entry created: {EntryId} for user {UserId}", entry.Id, userId);

            await _notificationService.CreateAsync(new CreateNotificationRequest
            {
                UserId = userId,
                Type = "entry_created",
                Title = "New entry created",
                Message = $"Added {entry.SongTitle}",
                Link = $"/entries/{entry.Id}"
            }, cancellationToken);

            var dto = MapToDto(entry);
            return (true, null, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entry for user {UserId}", userId);
            return (false, "An error occurred while creating the entry", null);
        }
    }

    /// <summary>
    /// Get single entry by ID
    /// </summary>
    public async Task<EntryDto?> GetAsync(
        Guid userId,
        Guid entryId,
        CancellationToken cancellationToken = default
    )
    {
        var entry = await _dbContext.Entries
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.Id == entryId && e.UserId == userId && e.DeletedAt == null,
                cancellationToken
            );

        return entry == null ? null : MapToDto(entry);
    }

    /// <summary>
    /// Get all entries for user with pagination
    /// </summary>
    public async Task<PagedResponse<EntryDto>> ListAsync(
        Guid userId,
        PaginationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Entries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.DeletedAt == null);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.SongTitle)
                : query.OrderByDescending(e => e.SongTitle),
            "artist" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.ArtistName)
                : query.OrderByDescending(e => e.ArtistName),
            _ => request.SortDirection == "asc"
                ? query.OrderBy(e => e.CreatedAt)
                : query.OrderByDescending(e => e.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResponse<EntryDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Search entries with filters
    /// </summary>
    public async Task<PagedResponse<EntryDto>> SearchAsync(
        Guid userId,
        SearchEntriesRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Entries
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.DeletedAt == null);

        // Apply text search
        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var searchTerm = request.Query.ToLower();
            query = query.Where(e =>
                e.SongTitle.ToLower().Contains(searchTerm) ||
                (e.ArtistName != null && e.ArtistName.ToLower().Contains(searchTerm))
            );
        }

        // Apply source filter
        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            query = query.Where(e => e.Source == request.Source);
        }

        // Apply date range filter
        if (request.DateFrom.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            var endOfDay = request.DateTo.Value.AddDays(1);
            query = query.Where(e => e.CreatedAt < endOfDay);
        }

        // Apply mood filters
        if (request.MinValence.HasValue)
        {
            query = query.Where(e => e.Valence >= request.MinValence.Value);
        }

        if (request.MaxValence.HasValue)
        {
            query = query.Where(e => e.Valence <= request.MaxValence.Value);
        }

        if (request.MinArousal.HasValue)
        {
            query = query.Where(e => e.Arousal >= request.MinArousal.Value);
        }

        if (request.MaxArousal.HasValue)
        {
            query = query.Where(e => e.Arousal <= request.MaxArousal.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "title" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.SongTitle)
                : query.OrderByDescending(e => e.SongTitle),
            "artist" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.ArtistName)
                : query.OrderByDescending(e => e.ArtistName),
            _ => request.SortDirection == "asc"
                ? query.OrderBy(e => e.CreatedAt)
                : query.OrderByDescending(e => e.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResponse<EntryDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Update existing entry
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage, EntryDto? Entry)> UpdateAsync(
        Guid userId,
        Guid entryId,
        UpdateEntryRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var entry = await _dbContext.Entries
            .FirstOrDefaultAsync(
                e => e.Id == entryId && e.UserId == userId && e.DeletedAt == null,
                cancellationToken
            );

        if (entry == null)
        {
            return (false, "Entry not found", null);
        }

        try
        {
            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.SongTitle))
                entry.SongTitle = request.SongTitle.Trim();

            if (request.ArtistName != null)
                entry.ArtistName = string.IsNullOrWhiteSpace(request.ArtistName) ? null : request.ArtistName.Trim();

            if (request.AlbumName != null)
                entry.AlbumName = string.IsNullOrWhiteSpace(request.AlbumName) ? "Unknown Album" : request.AlbumName.Trim();

            if (request.CoverUrl != null)
                entry.CoverUrl = request.CoverUrl;

            if (request.SongId != null)
                entry.SongId = request.SongId;

            if (request.Source != null)
                entry.Source = request.Source;

            if (request.Valence.HasValue)
                entry.Valence = (float)request.Valence.Value;

            if (request.Arousal.HasValue)
                entry.Arousal = (float)request.Arousal.Value;

            if (request.TensionLevel.HasValue)
                entry.TensionLevel = request.TensionLevel.Value;

            if (request.SomaticTags != null)
                entry.SomaticTags = request.SomaticTags;

            if (request.Notes != null)
                entry.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

            if (request.ScrobbledToLastFm.HasValue)
                entry.ScrobbledToLastFm = request.ScrobbledToLastFm.Value;

            entry.UpdatedAt = DateTime.UtcNow;

            _dbContext.Entries.Update(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Entry updated: {EntryId} for user {UserId}", entryId, userId);

            var dto = MapToDto(entry);
            return (true, null, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entry {EntryId} for user {UserId}", entryId, userId);
            return (false, "An error occurred while updating the entry", null);
        }
    }

    /// <summary>
    /// Delete entry (soft delete)
    /// </summary>
    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(
        Guid userId,
        Guid entryId,
        CancellationToken cancellationToken = default
    )
    {
        var entry = await _dbContext.Entries
            .FirstOrDefaultAsync(
                e => e.Id == entryId && e.UserId == userId && e.DeletedAt == null,
                cancellationToken
            );

        if (entry == null)
        {
            return (false, "Entry not found");
        }

        try
        {
            entry.DeletedAt = DateTime.UtcNow;
            _dbContext.Entries.Update(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Entry deleted: {EntryId} for user {UserId}", entryId, userId);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entry {EntryId} for user {UserId}", entryId, userId);
            return (false, "An error occurred while deleting the entry");
        }
    }

    /// <summary>
    /// Get user's entries statistics
    /// </summary>
    public async Task<EntryStatisticsDto?> GetStatisticsAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        // Verify user exists
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

        if (user == null)
        {
            return null;
        }

        var entries = await _dbContext.Entries
            .Where(e => e.UserId == userId && e.DeletedAt == null)
            .ToListAsync(cancellationToken);

        if (!entries.Any())
        {
            return new EntryStatisticsDto
            {
                TotalEntries = 0,
                AverageValence = 0,
                AverageArousal = 0,
                UniqueSongs = 0,
                UniqueArtists = 0,
                OldestEntryDate = null,
                NewestEntryDate = null
            };
        }

        return new EntryStatisticsDto
        {
            TotalEntries = entries.Count,
            AverageValence = entries.Average(e => e.Valence),
            AverageArousal = entries.Average(e => e.Arousal),
            UniqueSongs = entries.Select(e => e.SongTitle).Distinct().Count(),
            UniqueArtists = entries
                .Where(e => !string.IsNullOrEmpty(e.ArtistName))
                .Select(e => e.ArtistName)
                .Distinct()
                .Count(),
            OldestEntryDate = entries.Min(e => e.CreatedAt),
            NewestEntryDate = entries.Max(e => e.CreatedAt)
        };
    }

    // ==================== PRIVATE HELPERS ====================

    /// <summary>
    /// Map JournalEntry entity to EntryDto
    /// </summary>
    private static EntryDto MapToDto(JournalEntry entry)
    {
        return new EntryDto
        {
            Id = entry.Id,
            UserId = entry.UserId,
            SongTitle = entry.SongTitle,
            ArtistName = entry.ArtistName,
            AlbumName = entry.AlbumName,
            SongId = entry.SongId,
            CoverUrl = entry.CoverUrl,
            Source = entry.Source,
            Valence = (double)entry.Valence,
            Arousal = (double)entry.Arousal,
            TensionLevel = entry.TensionLevel ?? 50,
            SomaticTags = entry.SomaticTags,
            Notes = entry.Notes,
            ScrobbledToLastFm = entry.ScrobbledToLastFm,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }
}
