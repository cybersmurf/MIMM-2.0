using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MIMM.Backend.Services;
using MIMM.Shared.Dtos;

namespace MIMM.Backend.Controllers;

/// <summary>
/// Controller for importing/exporting journal entries
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExportImportController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly IImportService _importService;
    private readonly ILogger<ExportImportController> _logger;

    public ExportImportController(
        IExportService exportService,
        IImportService importService,
        ILogger<ExportImportController> logger)
    {
        _exportService = exportService;
        _importService = importService;
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
    /// Export entries as CSV file
    /// </summary>
    /// <response code="200">CSV file with entries</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportAsCsv(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Exporting entries as CSV for user {UserId}", userId);

            var csvData = await _exportService.ExportAsCsvAsync(userId, cancellationToken);

            return File(
                csvData,
                "text/csv",
                $"entries_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv"
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid user ID in token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting entries as CSV");
            return BadRequest("Error exporting entries");
        }
    }

    /// <summary>
    /// Export entries as JSON file
    /// </summary>
    /// <response code="200">JSON file with entries</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportAsJson(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Exporting entries as JSON for user {UserId}", userId);

            var jsonData = await _exportService.ExportAsJsonAsync(userId, cancellationToken);

            return File(
                System.Text.Encoding.UTF8.GetBytes(jsonData),
                "application/json",
                $"entries_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json"
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid user ID in token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting entries as JSON");
            return BadRequest("Error exporting entries");
        }
    }

    /// <summary>
    /// Import entries from CSV file
    /// </summary>
    /// <param name="file">CSV file to import</param>
    /// <param name="continueOnError">If true, skip invalid entries instead of failing entire import</param>
    /// <response code="200">Import results with success count and errors</response>
    /// <response code="400">Invalid file format or other error</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("csv/import")]
    [ProducesResponseType(typeof(ImportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ImportResponse>> ImportFromCsv(
        IFormFile? file,
        [FromQuery] bool continueOnError = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ImportResponse
                {
                    Errors = [new ImportError { ErrorMessage = "File is required" }]
                });
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ImportResponse
                {
                    Errors = [new ImportError { ErrorMessage = "File must be a CSV file" }]
                });
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Importing entries from CSV for user {UserId}", userId);

            var result = await _importService.ImportFromCsvAsync(userId, file, continueOnError, cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid user ID in token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing entries from CSV");
            return BadRequest(new ImportResponse
            {
                Errors = [new ImportError { ErrorMessage = "Error importing CSV file" }]
            });
        }
    }

    /// <summary>
    /// Import entries from JSON file
    /// </summary>
    /// <param name="file">JSON file to import</param>
    /// <param name="continueOnError">If true, skip invalid entries instead of failing entire import</param>
    /// <response code="200">Import results with success count and errors</response>
    /// <response code="400">Invalid file format or other error</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("json/import")]
    [ProducesResponseType(typeof(ImportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ImportResponse>> ImportFromJson(
        IFormFile? file,
        [FromQuery] bool continueOnError = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ImportResponse
                {
                    Errors = [new ImportError { ErrorMessage = "File is required" }]
                });
            }

            if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ImportResponse
                {
                    Errors = [new ImportError { ErrorMessage = "File must be a JSON file" }]
                });
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("Importing entries from JSON for user {UserId}", userId);

            var result = await _importService.ImportFromJsonAsync(userId, file, continueOnError, cancellationToken);

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid user ID in token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing entries from JSON");
            return BadRequest(new ImportResponse
            {
                Errors = [new ImportError { ErrorMessage = "Error importing JSON file" }]
            });
        }
    }
}
