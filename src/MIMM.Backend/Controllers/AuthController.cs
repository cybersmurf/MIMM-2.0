using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <response code="201">User registered successfully</response>
    /// <response code="400">Invalid request or user already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (success, errorMessage, response) = await _authService.RegisterAsync(request, cancellationToken);

            if (!success)
            {
                return BadRequest(new { error = errorMessage });
            }

            _logger.LogInformation("User registered: {Email}", request.Email);
            // Return 200 with authentication payload (tokens + user)
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return StatusCode(500, new { error = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <response code="200">Login successful</response>
    /// <response code="401">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (success, errorMessage, response) = await _authService.LoginAsync(request, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                return Unauthorized(new { error = errorMessage });
            }

            _logger.LogInformation("User logged in: {Email}", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return StatusCode(500, new { error = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (success, errorMessage, response) = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

            if (!success)
            {
                _logger.LogWarning("Failed refresh token attempt");
                return Unauthorized(new { error = errorMessage });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Verify access token validity
    /// </summary>
    /// <param name="token">JWT access token (from Authorization header)</param>
    /// <response code="200">Token is valid</response>
    /// <response code="401">Token is invalid or expired</response>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyToken(
        [FromHeader(Name = "Authorization")] string? token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { error = "Invalid token format" });
            }

            var jwtToken = token["Bearer ".Length..].Trim();
            var (isValid, errorMessage, _) = await _authService.VerifyTokenAsync(jwtToken, cancellationToken);

            if (!isValid)
            {
                return Unauthorized(new { error = errorMessage });
            }

            return Ok(new { valid = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token verification");
            return StatusCode(500, new { error = "An error occurred during token verification" });
        }
    }

    /// <summary>
    /// Get current authenticated user profile
    /// </summary>
    /// <response code="200">Returns user profile</response>
    /// <response code="401">Unauthorized</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user ID in token" });
            }

            var user = await _authService.GetUserByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                Language = user.Language ?? "en",
                EmailVerified = user.EmailVerified
            };

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return StatusCode(500, new { error = "An error occurred while retrieving user profile" });
        }
    }

    /// <summary>
    /// Logout (client should discard tokens)
    /// </summary>
    /// <response code="200">Logout successful</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        // In a stateless JWT architecture, logout is handled client-side by discarding tokens
        // If you want to implement token revocation, you would need to maintain a blacklist
        _logger.LogInformation("User logged out");
        return Ok(new { message = "Logout successful. Please discard your tokens." });
    }
}
