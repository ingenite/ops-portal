using Microsoft.AspNetCore.Mvc;
using OpsPortal.Api.Contracts;
using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Synchronization;

namespace OpsPortal.Api.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController : ControllerBase {
    private readonly IAtlassianAssetsClient _assetsClient;
    private readonly ApplicationSyncService _applicationSyncService;

    public ApplicationsController(IAtlassianAssetsClient assetsClient,
        ApplicationSyncService applicationSyncService) {
        _assetsClient = assetsClient;
        _applicationSyncService = applicationSyncService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationListRow>>> GetAll(
        CancellationToken cancellationToken) {
        try {
            var response = await _assetsClient.GetApplicationsAsync(cancellationToken);

            var applications = response.Values
                .Select(AtlassianApplicationMapper.Map)
                .ToList();

            return Ok(applications);
        } catch (HttpRequestException) {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Problem contacting external provider",
                detail: "Unable to retrieve Applications from provider API"
            );
        }
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncApplications(
        CancellationToken cancellationToken) {
        try {
            await _applicationSyncService.SyncAsync(cancellationToken);
            return NoContent();
        } catch (HttpRequestException) {
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Problem contacting external provider",
                detail: "Unable to synchronize applications from the provider API."
            );
        }
    }
}
