using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

public class Notification
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public User? User { get; set; }

    [MaxLength(50)]
    public string Type { get; set; } = "info";

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Link { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }
}
