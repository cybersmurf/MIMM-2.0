using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Dtos;

/// <summary>
/// Request DTO for creating a new journal entry
/// </summary>
public record CreateEntryRequest
{
    [Required(ErrorMessage = "Song title is required")]
    [MaxLength(500)]
    public required string SongTitle { get; init; }

    [MaxLength(500)]
    public string? ArtistName { get; init; }

    [MaxLength(500)]
    public string? AlbumName { get; init; }

    /// <summary>
    /// External song ID (from Last.fm, iTunes, Deezer, etc.)
    /// </summary>
    [MaxLength(500)]
    public string? SongId { get; init; }

    /// <summary>
    /// Album art URL
    /// </summary>
    [MaxLength(500)]
    [Url(ErrorMessage = "Cover URL must be a valid URL")]
    public string? CoverUrl { get; init; }

    /// <summary>
    /// Source of the entry (manual, lastfm, itunes, deezer, etc.)
    /// </summary>
    [MaxLength(50)]
    public string Source { get; init; } = "manual";

    /// <summary>
    /// Valence: musical positivity conveyed by a track (1.0 = happy/cheerful, -1.0 = sad/angry)
    /// </summary>
    [Range(-1.0, 1.0, ErrorMessage = "Valence must be between -1.0 and 1.0")]
    public double Valence { get; init; } = 0.0;

    /// <summary>
    /// Arousal: intensity/energy of the track (1.0 = very active/energetic, -1.0 = calm/peaceful)
    /// </summary>
    [Range(-1.0, 1.0, ErrorMessage = "Arousal must be between -1.0 and 1.0")]
    public double Arousal { get; init; } = 0.0;

    /// <summary>
    /// Physical tension level (0-100)
    /// </summary>
    [Range(0, 100, ErrorMessage = "Tension level must be between 0 and 100")]
    public int TensionLevel { get; init; } = 50;

    /// <summary>
    /// Somatic (bodily) sensations/tags (e.g., "happy", "anxious", "relaxed")
    /// </summary>
    public string[]? SomaticTags { get; init; }

    /// <summary>
    /// User notes/comments about the entry
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; init; }
}

/// <summary>
/// Request DTO for updating a journal entry
/// </summary>
public record UpdateEntryRequest
{
    [MaxLength(500)]
    public string? SongTitle { get; init; }

    [MaxLength(500)]
    public string? ArtistName { get; init; }

    [MaxLength(500)]
    public string? AlbumName { get; init; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Cover URL must be a valid URL")]
    public string? CoverUrl { get; init; }

    [Range(-1.0, 1.0, ErrorMessage = "Valence must be between -1.0 and 1.0")]
    public double? Valence { get; init; }

    [Range(-1.0, 1.0, ErrorMessage = "Arousal must be between -1.0 and 1.0")]
    public double? Arousal { get; init; }

    [Range(0, 100, ErrorMessage = "Tension level must be between 0 and 100")]
    public int? TensionLevel { get; init; }

    public string[]? SomaticTags { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }

    public bool? ScrobbledToLastFm { get; init; }
}

/// <summary>
/// Response DTO for journal entry
/// </summary>
public record EntryDto
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string SongTitle { get; init; }

    public string? ArtistName { get; init; }

    public string? AlbumName { get; init; }

    public string? SongId { get; init; }

    public string? CoverUrl { get; init; }

    public required string Source { get; init; }

    public double Valence { get; init; }

    public double Arousal { get; init; }

    public int TensionLevel { get; init; }

    public string[]? SomaticTags { get; init; }

    public string? Notes { get; init; }

    public bool ScrobbledToLastFm { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Pagination request parameters
/// </summary>
public record PaginationRequest
{
    /// <summary>
    /// Page number (1-indexed)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; init; } = 20;

    /// <summary>
    /// Sort by field: "created" (default), "title", "artist"
    /// </summary>
    [MaxLength(50)]
    public string SortBy { get; init; } = "created";

    /// <summary>
    /// Sort direction: "desc" (default) or "asc"
    /// </summary>
    [MaxLength(10)]
    public string SortDirection { get; init; } = "desc";
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public record PagedResponse<T>
{
    public required List<T> Items { get; init; }

    public required int PageNumber { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Entry search/filter request
/// </summary>
public record SearchEntriesRequest : PaginationRequest
{
    /// <summary>
    /// Search in song title or artist name
    /// </summary>
    [MaxLength(500)]
    public string? Query { get; init; }

    /// <summary>
    /// Filter by source
    /// </summary>
    [MaxLength(50)]
    public string? Source { get; init; }

    /// <summary>
    /// Filter by date range start
    /// </summary>
    public DateTime? DateFrom { get; init; }

    /// <summary>
    /// Filter by date range end
    /// </summary>
    public DateTime? DateTo { get; init; }

    /// <summary>
    /// Filter by minimum valence
    /// </summary>
    [Range(-1.0, 1.0)]
    public double? MinValence { get; init; }

    /// <summary>
    /// Filter by maximum valence
    /// </summary>
    [Range(-1.0, 1.0)]
    public double? MaxValence { get; init; }

    /// <summary>
    /// Filter by minimum arousal
    /// </summary>
    [Range(-1.0, 1.0)]
    public double? MinArousal { get; init; }

    /// <summary>
    /// Filter by maximum arousal
    /// </summary>
    [Range(-1.0, 1.0)]
    public double? MaxArousal { get; init; }
}
