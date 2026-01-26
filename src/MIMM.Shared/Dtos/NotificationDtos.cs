using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Dtos;

public record NotificationDto
{
    public required Guid Id { get; init; }
    public required string Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? Link { get; init; }
    public bool IsRead { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}

public record CreateNotificationRequest
{
    [Required]
    public Guid UserId { get; init; }

    [MaxLength(50)]
    public string Type { get; init; } = "info";

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Link { get; init; }
}
