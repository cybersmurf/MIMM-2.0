using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

public class JournalEntry
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    // Song information
    [Required]
    [MaxLength(255)]
    public string SongTitle { get; set; } = null!;
    
    [MaxLength(255)]
    public string? ArtistName { get; set; }
    
    [MaxLength(255)]
    public string AlbumName { get; set; } = "Unknown Album";
    
    [MaxLength(100)]
    public string? SongId { get; set; } // iTunes/Deezer/MusicBrainz ID

    [MaxLength(100)]
    public string? SpotifyId { get; set; } // Spotify track ID
    
    [MaxLength(500)]
    public string? SpotifyUri { get; set; } // Spotify URI (spotify:track:...)
    
    [MaxLength(500)]
    public string? CoverUrl { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Source { get; set; } = "manual"; // "itunes", "deezer", "musicbrainz", "manual", "lastfm"
    
    // Mood (Russell Circumplex Model)
    [Range(-1.0, 1.0)]
    public float Valence { get; set; } // -1.0 (negative) to 1.0 (positive)
    
    [Range(-1.0, 1.0)]
    public float Arousal { get; set; } // -1.0 (low energy) to 1.0 (high energy)
    
    // Body sensations
    [Range(0, 100)]
    public int TensionLevel { get; set; } // 0-100 scale
    
    public string[] SomaticTags { get; set; } = Array.Empty<string>(); // ["headache", "butterflies", ...]
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; } // Soft delete flag
    
    public bool ScrobbledToLastFm { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}
