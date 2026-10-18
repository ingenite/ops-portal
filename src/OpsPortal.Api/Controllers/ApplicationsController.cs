using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using OpsPortal.Api.Contracts;
using OpsPortal.Api.Data;
using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Synchronization;

namespace OpsPortal.Api.Controllers;

[ApiController]
[Route("api/applications")]
[ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
public sealed class ApplicationsController : ControllerBase
{
    private readonly ApplicationQuery _applicationQuery;
    private readonly ApplicationSyncService _applicationSyncService;
    private readonly IOptions<AtlassianOptions> _options;
    private readonly ApplicationSyncCoordinator _coordinator;

    public ApplicationsController(ApplicationQuery applicationQuery,
        ApplicationSyncService applicationSyncService,
        IOptions<AtlassianOptions> options,
        ApplicationSyncCoordinator coordinator)
    {
        _applicationQuery = applicationQuery;
        _applicationSyncService = applicationSyncService;
        _options = options;
        _coordinator = coordinator;
    }

    [HttpGet]
    [Authorize(Policy = "CanRead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ApplicationListRow>>> GetAll(
        CancellationToken cancellationToken)
    {
        var applications = await _applicationQuery.GetAllAsync(
            _options.Value.WorkspaceId,
            cancellationToken);

        return Ok(applications);
    }

    [HttpPost("sync")]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> SyncApplications(
        CancellationToken cancellationToken)
    {

        var entered = await _coordinator.TryEnterAsync(cancellationToken);
        if (!entered)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Application sync already in progress",
                detail: "Try again after the current sync finishes."
            );
        }

        try
        {
            await _applicationSyncService.SyncAsync(cancellationToken);
            return NoContent();
        }
        catch (HttpRequestException)
        {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Problem contacting external provider",
                detail: "Unable to synchronize applications from the provider API."
            );
        }
        finally
        {
            _coordinator.Exit();
        }
    }
}