using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIMM.Backend.Services;

namespace MIMM.Backend.Controllers;

[ApiController]
[Route("api/lastfm")]
public class LastFmController(ILastFmService lastFm, ILogger<LastFmController> logger, IConfiguration config) : ControllerBase
{
    private readonly ILastFmService _lastFm = lastFm;
    private readonly ILogger<LastFmController> _logger = logger;
    private readonly IConfiguration _config = config;

    [HttpGet("auth-url")]
    [Authorize]
    public async Task<ActionResult<object>> GetAuthUrl(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var requestBase = $"{Request.Scheme}://{Request.Host.Value}";
        var url = await _lastFm.GetAuthUrlAsync(userId, requestBase, cancellationToken);
        return Ok(new { url });
    }

    // Last.fm will redirect the user here with ?token=... and we include our signed state as ?s=...
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery(Name = "token")] string token, [FromQuery(Name = "s")] string state, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(state))
        {
            return BadRequest("Missing token or state");
        }

        var (success, username, error) = await _lastFm.HandleCallbackAsync(state, token, cancellationToken);
        var frontendBase = _config["App:FrontendUrl"] ?? "http://localhost:5000";
        var redirect = success
            ? $"{frontendBase}/dashboard?lastfm=connected&u={Uri.EscapeDataString(username ?? string.Empty)}"
            : $"{frontendBase}/dashboard?lastfm=error&msg={Uri.EscapeDataString(error ?? "unknown")}";
        return Redirect(redirect);
    }

    // Backwards-compatible route if configured callback uses legacy path
    [HttpGet("~/api/integrations/lastfm/callback")]
    [AllowAnonymous]
    public Task<IActionResult> LegacyCallback([FromQuery(Name = "token")] string token, [FromQuery(Name = "s")] string state, CancellationToken cancellationToken)
        => Callback(token, state, cancellationToken);
}
