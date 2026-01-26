using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;
using System.Security.Claims;

namespace MIMM.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IFriendService _friendService;
    private readonly ILogger<FriendsController> _logger;

    public FriendsController(IFriendService friendService, ILogger<FriendsController> logger)
    {
        _friendService = friendService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                          User.FindFirst("sub")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Get all friends for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<FriendsListResponse>> GetFriends(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _friendService.GetFriendsAsync(userId, page, pageSize, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching friends");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching friends");
        }
    }

    /// <summary>
    /// Add a new friend by email
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<FriendOperationResponse>> AddFriend(
        [FromBody] AddFriendRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            var result = await _friendService.AddFriendAsync(userId, request.FriendEmail, cancellationToken);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding friend");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error adding friend");
        }
    }

    /// <summary>
    /// Remove a friend
    /// </summary>
    [HttpDelete("{friendshipId}")]
    public async Task<ActionResult<FriendOperationResponse>> RemoveFriend(
        [FromRoute] Guid friendshipId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _friendService.RemoveFriendAsync(userId, friendshipId, cancellationToken);
            
            if (result.Success)
                return Ok(result);
            else
                return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing friend");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error removing friend");
        }
    }

    /// <summary>
    /// Get pending friend requests
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<FriendDto>>> GetPendingRequests(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var requests = await _friendService.GetPendingRequestsAsync(userId, cancellationToken);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching pending requests");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching pending requests");
        }
    }

    /// <summary>
    /// Respond to a friend request (accept, reject, block)
    /// </summary>
    [HttpPost("{friendshipId}/respond")]
    public async Task<ActionResult<FriendOperationResponse>> RespondFriendRequest(
        [FromRoute] Guid friendshipId,
        [FromBody] RespondFriendRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var userId = GetCurrentUserId();
            var result = await _friendService.RespondFriendRequestAsync(userId, friendshipId, request.Action, request.Reason, cancellationToken);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error responding to friend request");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error responding to friend request");
        }
    }
}
