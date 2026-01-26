using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MIMM.Backend.Data;
using MIMM.Shared.Dtos;
using MIMM.Shared.Entities;

namespace MIMM.Backend.Services;

public interface IFriendService
{
    Task<FriendsListResponse> GetFriendsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<FriendOperationResponse> AddFriendAsync(Guid userId, string friendEmail, CancellationToken cancellationToken = default);
    Task<FriendOperationResponse> RemoveFriendAsync(Guid userId, Guid friendshipId, CancellationToken cancellationToken = default);
    Task<FriendOperationResponse> RespondFriendRequestAsync(Guid userId, Guid friendshipId, string action, string? reason = null, CancellationToken cancellationToken = default);
    Task<List<FriendDto>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class FriendService : IFriendService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FriendService> _logger;

    public FriendService(ApplicationDbContext dbContext, ILogger<FriendService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all accepted friends for a user
    /// </summary>
    public async Task<FriendsListResponse> GetFriendsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching friends for user {UserId}, page {Page}, size {PageSize}", userId, page, pageSize);

        var query = _dbContext.Friendships
            .Where(f => (f.UserId == userId || f.FriendUserId == userId) && f.Status == "Accepted" && f.DeletedAt == null)
            .OrderByDescending(f => f.CreatedAt);

        var total = await query.CountAsync(cancellationToken);
        var skip = (page - 1) * pageSize;

        var friendships = await query
            .Skip(skip)
            .Take(pageSize)
            .Include(f => f.User)
            .Include(f => f.FriendUser)
            .ToListAsync(cancellationToken);

        var friends = friendships.Select(f =>
        {
            var friendUser = f.UserId == userId ? f.FriendUser : f.User;
            return new FriendDto
            {
                Id = f.Id,
                FriendUserId = friendUser.Id,
                FriendDisplayName = friendUser.DisplayName ?? friendUser.Email,
                FriendEmail = friendUser.Email,
                ConnectedSince = f.CreatedAt,
                Status = f.Status,
                IsActive = f.IsActive
            };
        }).ToList();

        _logger.LogInformation("Found {FriendsCount} friends for user {UserId}", friends.Count, userId);

        return new FriendsListResponse
        {
            Friends = friends,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Add a friend by email (sends friend request)
    /// </summary>
    public async Task<FriendOperationResponse> AddFriendAsync(Guid userId, string friendEmail, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} attempting to add friend with email {FriendEmail}", userId, friendEmail);

        // Validate user exists
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return new FriendOperationResponse { Success = false, Message = "User not found" };
        }

        // Find friend by email
        var friendUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == friendEmail && u.DeletedAt == null, cancellationToken);
        if (friendUser == null)
        {
            _logger.LogWarning("Friend user with email {FriendEmail} not found", friendEmail);
            return new FriendOperationResponse { Success = false, Message = "Friend user not found" };
        }

        // Can't add yourself
        if (friendUser.Id == userId)
        {
            _logger.LogWarning("User {UserId} attempted to add themselves", userId);
            return new FriendOperationResponse { Success = false, Message = "Cannot add yourself as a friend" };
        }

        // Check if friendship already exists
        var existingFriendship = await _dbContext.Friendships
            .FirstOrDefaultAsync(f =>
                (f.UserId == userId && f.FriendUserId == friendUser.Id) ||
                (f.UserId == friendUser.Id && f.FriendUserId == userId),
                cancellationToken);

        if (existingFriendship != null && existingFriendship.DeletedAt == null)
        {
            _logger.LogWarning("Friendship already exists between {UserId} and {FriendUserId}", userId, friendUser.Id);
            return new FriendOperationResponse { Success = false, Message = "Friendship already exists" };
        }

        // Create or restore friendship
        if (existingFriendship != null && existingFriendship.DeletedAt != null)
        {
            // Restore deleted friendship
            existingFriendship.DeletedAt = null;
            existingFriendship.Status = "Accepted";
            existingFriendship.IsActive = true;
        }
        else
        {
            // Create new friendship (bidirectional - auto-accept for now)
            var friendship = new Friendship
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FriendUserId = friendUser.Id,
                Status = "Accepted",
                CreatedAt = DateTime.UtcNow,
                AcceptedAt = DateTime.UtcNow,
                IsActive = true
            };
            _dbContext.Friendships.Add(friendship);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Friend {FriendUserId} added successfully for user {UserId}", friendUser.Id, userId);

        return new FriendOperationResponse
        {
            Success = true,
            Message = "Friend added successfully",
            Data = new FriendDto
            {
                Id = friendUser.Id,
                FriendUserId = friendUser.Id,
                FriendDisplayName = friendUser.DisplayName ?? friendUser.Email,
                FriendEmail = friendUser.Email,
                ConnectedSince = DateTime.UtcNow,
                Status = "Accepted",
                IsActive = true
            }
        };
    }

    /// <summary>
    /// Remove a friend (soft delete)
    /// </summary>
    public async Task<FriendOperationResponse> RemoveFriendAsync(Guid userId, Guid friendshipId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} attempting to remove friendship {FriendshipId}", userId, friendshipId);

        var friendship = await _dbContext.Friendships
            .FirstOrDefaultAsync(f => f.Id == friendshipId && (f.UserId == userId || f.FriendUserId == userId) && f.DeletedAt == null,
                cancellationToken);

        if (friendship == null)
        {
            _logger.LogWarning("Friendship {FriendshipId} not found for user {UserId}", friendshipId, userId);
            return new FriendOperationResponse { Success = false, Message = "Friendship not found" };
        }

        friendship.DeletedAt = DateTime.UtcNow;
        friendship.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Friendship {FriendshipId} removed for user {UserId}", friendshipId, userId);

        return new FriendOperationResponse { Success = true, Message = "Friend removed successfully" };
    }

    /// <summary>
    /// Respond to a friend request (accept, reject, block)
    /// </summary>
    public async Task<FriendOperationResponse> RespondFriendRequestAsync(
        Guid userId, Guid friendshipId, string action, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("User {UserId} responding to friendship {FriendshipId} with action {Action}", userId, friendshipId, action);

        var friendship = await _dbContext.Friendships
            .FirstOrDefaultAsync(f => f.Id == friendshipId && f.FriendUserId == userId && f.Status == "Pending" && f.DeletedAt == null,
                cancellationToken);

        if (friendship == null)
        {
            _logger.LogWarning("Pending friendship {FriendshipId} not found for user {UserId}", friendshipId, userId);
            return new FriendOperationResponse { Success = false, Message = "Friend request not found" };
        }

        switch (action.ToLower())
        {
            case "accept":
                friendship.Status = "Accepted";
                friendship.AcceptedAt = DateTime.UtcNow;
                break;
            case "reject":
                friendship.Status = "Rejected";
                friendship.Reason = reason;
                friendship.DeletedAt = DateTime.UtcNow;
                break;
            case "block":
                friendship.Status = "Blocked";
                friendship.Reason = reason;
                break;
            default:
                return new FriendOperationResponse { Success = false, Message = "Invalid action" };
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Friendship {FriendshipId} response processed: {Action}", friendshipId, action);

        return new FriendOperationResponse { Success = true, Message = $"Friend request {action}ed successfully" };
    }

    /// <summary>
    /// Get pending friend requests for a user
    /// </summary>
    public async Task<List<FriendDto>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching pending friend requests for user {UserId}", userId);

        var pendingRequests = await _dbContext.Friendships
            .Where(f => f.FriendUserId == userId && f.Status == "Pending" && f.DeletedAt == null)
            .Include(f => f.User)
            .ToListAsync(cancellationToken);

        var requests = pendingRequests.Select(f => new FriendDto
        {
            Id = f.Id,
            FriendUserId = f.User.Id,
            FriendDisplayName = f.User.DisplayName ?? f.User.Email,
            FriendEmail = f.User.Email,
            ConnectedSince = f.RequestedAt,
            Status = f.Status,
            IsActive = f.IsActive
        }).ToList();

        _logger.LogInformation("Found {RequestCount} pending friend requests for user {UserId}", requests.Count, userId);

        return requests;
    }
}
