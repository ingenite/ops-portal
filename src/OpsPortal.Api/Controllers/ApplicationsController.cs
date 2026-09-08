using Microsoft.AspNetCore.Mvc;
using OpsPortal.Api.Contracts;
using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController : ControllerBase {
    private readonly IAtlassianAssetsClient _assetsClient;

    public ApplicationsController(IAtlassianAssetsClient assetsClient) {
        _assetsClient = assetsClient;
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
}
