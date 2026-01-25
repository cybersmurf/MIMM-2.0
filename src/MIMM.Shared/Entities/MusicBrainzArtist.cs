using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

/// <summary>
/// Cached MusicBrainz artist metadata to reduce repeated lookups.
/// </summary>
public class MusicBrainzArtist
{
    [Key]
    [MaxLength(36)]
    public string Id { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
