using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

/// <summary>
/// Cached MusicBrainz recording metadata.
/// </summary>
public class MusicBrainzRecording
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [MaxLength(36)]
    public string? ArtistId { get; set; }

    [MaxLength(255)]
    public string ArtistName { get; set; } = "Unknown artist";

    [MaxLength(36)]
    public string? ReleaseId { get; set; }

    [MaxLength(255)]
    public string? ReleaseTitle { get; set; }

    [MaxLength(500)]
    public string? CoverUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
