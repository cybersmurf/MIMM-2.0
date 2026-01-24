using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

public class LastFmToken
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string SessionKey { get; set; } = null!; // Encrypted in storage
    
    [Required]
    [MaxLength(100)]
    public string LastFmUsername { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}
