using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using OpsPortal.Api.Contracts;
using OpsPortal.Api.Reporting;

namespace OpsPortal.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly WorkItemReportQuery _reportQuery;

    public ReportsController(WorkItemReportQuery reportQuery)
    {
        _reportQuery = reportQuery;
    }

    [HttpGet("workItemStatus")]
    [Authorize(Policy = "CanRead")]
    public async Task<ActionResult<IReadOnlyList<WorkItemsByStatusReportRow>>> GetStatusCounts()
    {
        return Ok(await _reportQuery.GetStatusCountsAsync());
    }
}