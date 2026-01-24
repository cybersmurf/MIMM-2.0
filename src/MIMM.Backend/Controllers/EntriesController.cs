using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Controllers;

/// <summary>
/// Controller for managing journal entries
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EntriesController : ControllerBase
{
    private readonly IEntryService _entryService;
    private readonly ILogger<EntriesController> _logger;

    public EntriesController(
        IEntryService entryService,
        ILogger<EntriesController> logger)
    {
        _entryService = entryService;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's ID from JWT claims
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }

        return userId;
    }

    /// <summary>
    /// Get paginated list of entries for current user
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field: created, title, artist (default: created)</param>
    /// <param name="sortDirection">Sort direction: asc, desc (default: desc)</param>
    /// <response code="200">Returns paginated entries</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<EntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<EntryDto>>> GetEntries(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "created",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Clamp page size to prevent abuse
            pageSize = Math.Clamp(pageSize, 1, 100);

            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var result = await _entryService.ListAsync(userId, request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entries");
            return StatusCode(500, new { error = "An error occurred while retrieving entries" });
        }
    }

    /// <summary>
    /// Get single entry by ID
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <response code="200">Returns the entry</response>
    /// <response code="404">Entry not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EntryDto>> GetEntry(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var entry = await _entryService.GetAsync(userId, id, cancellationToken);

            if (entry == null)
            {
                return NotFound(new { error = "Entry not found" });
            }

            return Ok(entry);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving entry {EntryId}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the entry" });
        }
    }

    /// <summary>
    /// Create new journal entry
    /// </summary>
    /// <param name="request">Entry creation request</param>
    /// <response code="201">Entry created successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(EntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EntryDto>> CreateEntry(
        [FromBody] CreateEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, errorMessage, entry) = await _entryService.CreateAsync(userId, request, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = errorMessage });
            }

            return CreatedAtAction(
                nameof(GetEntry),
                new { id = entry!.Id },
                entry);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating entry");
            return StatusCode(500, new { error = "An error occurred while creating the entry" });
        }
    }

    /// <summary>
    /// Update existing entry
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <param name="request">Entry update request</param>
    /// <response code="200">Entry updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Entry not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EntryDto>> UpdateEntry(
        Guid id,
        [FromBody] UpdateEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, errorMessage, entry) = await _entryService.UpdateAsync(userId, id, request, cancellationToken);

            if (!success)
            {
                if (errorMessage?.Contains("not found") == true)
                {
                    return NotFound(new { error = errorMessage });
                }
                return BadRequest(new { error = errorMessage });
            }

            return Ok(entry);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entry {EntryId}", id);
            return StatusCode(500, new { error = "An error occurred while updating the entry" });
        }
    }

    /// <summary>
    /// Delete entry (soft delete)
    /// </summary>
    /// <param name="id">Entry ID</param>
    /// <response code="204">Entry deleted successfully</response>
    /// <response code="404">Entry not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteEntry(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var (success, errorMessage) = await _entryService.DeleteAsync(userId, id, cancellationToken);

            if (!success)
            {
                return NotFound(new { error = errorMessage });
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entry {EntryId}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the entry" });
        }
    }

    /// <summary>
    /// Search entries with filters
    /// </summary>
    /// <param name="query">Text search in song title, artist, album, notes</param>
    /// <param name="source">Filter by source (lastfm, itunes, deezer, manual)</param>
    /// <param name="fromDate">Start date filter (ISO 8601)</param>
    /// <param name="toDate">End date filter (ISO 8601)</param>
    /// <param name="minValence">Minimum valence (-1.0 to 1.0)</param>
    /// <param name="maxValence">Maximum valence (-1.0 to 1.0)</param>
    /// <param name="minArousal">Minimum arousal (-1.0 to 1.0)</param>
    /// <param name="maxArousal">Maximum arousal (-1.0 to 1.0)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
    /// <param name="sortBy">Sort field: created, title, artist (default: created)</param>
    /// <param name="sortDirection">Sort direction: asc, desc (default: desc)</param>
    /// <response code="200">Returns filtered entries</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponse<EntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<EntryDto>>> SearchEntries(
        [FromQuery] string? query = null,
        [FromQuery] string? source = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] double? minValence = null,
        [FromQuery] double? maxValence = null,
        [FromQuery] double? minArousal = null,
        [FromQuery] double? maxArousal = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sortBy = "created",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Clamp page size to prevent abuse
            pageSize = Math.Clamp(pageSize, 1, 100);

            var request = new SearchEntriesRequest
            {
                Query = query,
                Source = source,
                DateFrom = fromDate,
                DateTo = toDate,
                MinValence = minValence,
                MaxValence = maxValence,
                MinArousal = minArousal,
                MaxArousal = maxArousal,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var result = await _entryService.SearchAsync(userId, request, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching entries");
            return StatusCode(500, new { error = "An error occurred while searching entries" });
        }
    }

    /// <summary>
    /// Get statistics for current user's entries
    /// </summary>
    /// <response code="200">Returns entry statistics</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(EntryStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EntryStatisticsDto>> GetStatistics(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var statistics = await _entryService.GetStatisticsAsync(userId, cancellationToken);

            if (statistics == null)
            {
                return Ok(new EntryStatisticsDto
                {
                    TotalEntries = 0,
                    UniqueSongs = 0,
                    UniqueArtists = 0,
                    AverageValence = 0,
                    AverageArousal = 0
                });
            }

            return Ok(statistics);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { error = "An error occurred while retrieving statistics" });
        }
    }
}
