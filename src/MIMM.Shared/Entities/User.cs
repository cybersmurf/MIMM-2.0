using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

public class User
{
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = null!;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;
    
    [MaxLength(100)]
    public string? DisplayName { get; set; }
    
    [MaxLength(5)]
    public string Language { get; set; } = "en"; // "en" or "cs"
    
    [MaxLength(50)]
    public string? TimeZone { get; set; }
    
    public bool EmailVerified { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; } // Soft delete
    
    // Refresh token for token rotation
    [MaxLength(500)]
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiresAt { get; set; }
    
    // Navigation properties
    public ICollection<JournalEntry> Entries { get; set; } = new List<JournalEntry>();
    public LastFmToken? LastFmToken { get; set; }
}
