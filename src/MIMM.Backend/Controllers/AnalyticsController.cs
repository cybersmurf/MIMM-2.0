using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger) : ControllerBase
{
    private readonly IAnalyticsService _analyticsService = analyticsService;
    private readonly ILogger<AnalyticsController> _logger = logger;

    /// <summary>
    /// Get mood trends for the current user
    /// </summary>
    /// <param name="daysLookback">Number of days to analyze (default: 30)</param>
    [HttpGet("mood-trends")]
    public async Task<ActionResult<MoodTrendDto>> GetMoodTrends([FromQuery] int daysLookback = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized("Invalid user context");
            }

            if (daysLookback is < 1 or > 365)
            {
                return BadRequest("daysLookback must be between 1 and 365");
            }

            var trend = await _analyticsService.GetMoodTrendAsync(userGuid, daysLookback, cancellationToken);
            return Ok(trend);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mood trends for user");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get music statistics for the current user
    /// </summary>
    [HttpGet("music-stats")]
    public async Task<ActionResult<MusicStatisticsDto>> GetMusicStatistics(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized("Invalid user context");
            }

            var stats = await _analyticsService.GetMusicStatisticsAsync(userGuid, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting music statistics for user");
            return StatusCode(500, new { message = "Internal server error", details = ex.Message });
        }
    }
}
