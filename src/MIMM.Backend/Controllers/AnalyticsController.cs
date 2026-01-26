using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Controllers;

/// <summary>
/// Analytics endpoints for mood trends and music statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger) : ControllerBase
{
    private readonly IAnalyticsService _analyticsService = analyticsService;
    private readonly ILogger<AnalyticsController> _logger = logger;

    /// <summary>
    /// Get mood trends for the current user over specified period
    /// </summary>
    /// <param name="daysLookback">Number of days to analyze (default: 30, max: 365)</param>
    /// <returns>Mood trend data including daily averages and distribution</returns>
    /// <response code="200">Returns mood trend data</response>
    /// <response code="400">Invalid daysLookback parameter</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("mood-trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MoodTrendDto>> GetMoodTrends(
        [FromQuery] int daysLookback = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized(new { error = "Invalid user context" });
            }

            if (daysLookback is < 1 or > 365)
            {
                _logger.LogWarning("Invalid daysLookback parameter: {DaysLookback}", daysLookback);
                return BadRequest(new { error = "daysLookback must be between 1 and 365" });
            }

            _logger.LogInformation("Fetching mood trends for user {UserId}, period: {Days} days", userGuid, daysLookback);
            var trend = await _analyticsService.GetMoodTrendAsync(userGuid, daysLookback, cancellationToken);

            return Ok(trend);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request cancelled for mood trends");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Request timeout" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mood trends for user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get music statistics for the current user
    /// </summary>
    /// <returns>Music statistics including top artists, songs, and scrobble rate</returns>
    /// <response code="200">Returns music statistics</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("music-stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MusicStatisticsDto>> GetMusicStatistics(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized(new { error = "Invalid user context" });
            }

            _logger.LogInformation("Fetching music statistics for user {UserId}", userGuid);
            var stats = await _analyticsService.GetMusicStatisticsAsync(userGuid, cancellationToken);

            return Ok(stats);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request cancelled for music statistics");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Request timeout" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting music statistics for user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get yearly report for the current user
    /// </summary>
    /// <param name="year">Year to report on</param>
    /// <returns>Yearly report with monthly breakdown, top artists/songs, mood distribution</returns>
    /// <response code="200">Returns yearly report</response>
    /// <response code="400">Invalid year parameter</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("yearly-report/{year:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<YearlyReportDto>> GetYearlyReport(
        int year,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized(new { error = "Invalid user context" });
            }

            if (year < 2020 || year > DateTime.Now.Year)
            {
                return BadRequest(new { error = $"Year must be between 2020 and {DateTime.Now.Year}" });
            }

            _logger.LogInformation("Fetching yearly report for user {UserId}, year {Year}", userGuid, year);
            var report = await _analyticsService.GetYearlyReportAsync(userGuid, year, cancellationToken);

            return Ok(report);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request cancelled for yearly report");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Request timeout" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting yearly report for user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get mood correlation with artists and sources
    /// </summary>
    /// <returns>Mood correlation data for artists and sources</returns>
    /// <response code="200">Returns mood correlation data</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("mood-by-artist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MoodCorrelationDto>> GetMoodByArtist(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                _logger.LogWarning("Invalid or missing user ID in JWT claims");
                return Unauthorized(new { error = "Invalid user context" });
            }

            _logger.LogInformation("Fetching mood by artist for user {UserId}", userGuid);
            var correlation = await _analyticsService.GetMoodByArtistAsync(userGuid, cancellationToken);

            return Ok(correlation);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request cancelled for mood by artist");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "Request timeout" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mood by artist for user");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error" });
        }
    }
}
