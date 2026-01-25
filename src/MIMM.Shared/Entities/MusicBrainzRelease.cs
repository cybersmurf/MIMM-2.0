using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

/// <summary>
/// Cached MusicBrainz release (album) metadata.
/// </summary>
public class MusicBrainzRelease
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    [MaxLength(100)]
    public string? ReleaseDate { get; set; }

    [MaxLength(500)]
    public string? CoverArtUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
