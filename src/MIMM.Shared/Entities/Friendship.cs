using System.ComponentModel.DataAnnotations;

namespace MIMM.Shared.Entities;

/// <summary>
/// Represents a friendship relationship between two users (one-way).
/// A friendship is directional: User A can follow User B.
/// </summary>
public class Friendship
{
    public Guid Id { get; set; }

    /// <summary>
    /// The user initiating/maintaining the friendship
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The friend being followed
    /// </summary>
    public Guid FriendUserId { get; set; }

    /// <summary>
    /// Friend request status: Pending, Accepted, Rejected, Blocked
    /// </summary>
    [MaxLength(20)]
    public string Status { get; set; } = "Accepted"; // "Pending", "Accepted", "Rejected", "Blocked"

    /// <summary>
    /// When the friendship was requested
    /// </summary>
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the friendship was accepted (if applicable)
    /// </summary>
    public DateTime? AcceptedAt { get; set; }

    /// <summary>
    /// When the friendship was created/established
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional: reason for rejection/blocking
    /// </summary>
    [MaxLength(255)]
    public string? Reason { get; set; }

    /// <summary>
    /// Can friends see this user's entries?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the friendship was removed
    /// </summary>
    public DateTime? DeletedAt { get; set; } // Soft delete

    // Navigation properties
    public User User { get; set; } = null!;
    public User FriendUser { get; set; } = null!;
}
