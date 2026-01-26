namespace MIMM.Shared.Dtos;

/// <summary>
/// Represents a friend in the friends list view
/// </summary>
public record FriendDto
{
    public Guid Id { get; init; }
    public Guid FriendUserId { get; init; }
    public string? FriendDisplayName { get; init; }
    public string? FriendEmail { get; init; }
    public DateTime ConnectedSince { get; init; }
    public string Status { get; init; } = "Accepted";
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Response for friend operations (add, remove, etc)
/// </summary>
public record FriendOperationResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public FriendDto? Data { get; init; }
}

/// <summary>
/// Request to add a friend (by email)
/// </summary>
public record AddFriendRequest
{
    public required string FriendEmail { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Request to respond to a friend request
/// </summary>
public record RespondFriendRequestRequest
{
    public Guid FriendshipId { get; init; }
    public required string Action { get; init; } // "accept", "reject", "block"
    public string? Reason { get; init; }
}

/// <summary>
/// List of friends with pagination
/// </summary>
public record FriendsListResponse
{
    public required List<FriendDto> Friends { get; init; }
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
}
