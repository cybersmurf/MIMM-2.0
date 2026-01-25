using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Controllers;

[ApiController]
[Route("api/music")]
[Produces("application/json")]
public class MusicSearchController : ControllerBase
{
    private readonly IMusicSearchService _musicSearchService;
    private readonly ILogger<MusicSearchController> _logger;

    public MusicSearchController(IMusicSearchService musicSearchService, ILogger<MusicSearchController> logger)
    {
        _musicSearchService = musicSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Search for songs by title/artist via external catalog (iTunes API)
    /// </summary>
    /// <param name="query">Search term (required)</param>
    /// <param name="limit">Max results (1-25, default 10)</param>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MusicSearchResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MusicSearchResponse>> Search(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new { error = "Query is required" });
        }

        try
        {
            var items = await _musicSearchService.SearchAsync(query, limit, cancellationToken);
            return Ok(new MusicSearchResponse { Items = items.ToList(), Query = query });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Music search failed for query {Query}", query);
            return StatusCode(500, new { error = "An error occurred while performing music search" });
        }
    }
}
