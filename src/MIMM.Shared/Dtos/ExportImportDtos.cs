using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Dtos;

/// <summary>
/// DTO for exporting journal entries
/// </summary>
public record ExportEntryDto
{
    public required string SongTitle { get; init; }
    public string? ArtistName { get; init; }
    public string? AlbumName { get; init; }
    public string? SongId { get; init; }
    public string? CoverUrl { get; init; }
    public string Source { get; init; } = "manual";
    public double Valence { get; init; }
    public double Arousal { get; init; }
    public int? TensionLevel { get; init; }
    public string[] SomaticTags { get; init; } = [];
    public string? Notes { get; init; }
    public bool ScrobbledToLastFm { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// Response for import operation
/// </summary>
public record ImportResponse
{
    public int TotalProcessed { get; init; }
    public int SuccessfullyImported { get; init; }
    public int FailedCount { get; init; }
    public List<ImportError> Errors { get; init; } = [];
}

/// <summary>
/// Details about a single import error
/// </summary>
public record ImportError
{
    public int? LineNumber { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string? FailedData { get; init; }
}
