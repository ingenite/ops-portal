using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using OpsPortal.Api.Contracts;
using OpsPortal.Api.Data;
using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Models;
using OpsPortal.Api.Storage;

namespace OpsPortal.Api.Controllers;

[ApiController]
[Route("api/work-items")]
[ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
public sealed class WorkItemsController : ControllerBase
{
    private readonly IWorkItemStore _workItemStore;
    private readonly ApplicationQuery _applicationQuery;
    private readonly IOptions<AtlassianOptions> _options;

    public WorkItemsController(IWorkItemStore workItemStore,
        ApplicationQuery applicationQuery,
        IOptions<AtlassianOptions> options)
    {
        _workItemStore = workItemStore;
        _applicationQuery = applicationQuery;
        _options = options;
    }

    [HttpGet]
    [Authorize(Policy = "CanRead")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<WorkItem>>> GetAll()
    {
        return Ok(await _workItemStore.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "CanRead")]
    [ProducesResponseType<WorkItem>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkItem>> GetById(Guid id)
    {
        var workItem = await _workItemStore.GetByIdAsync(id);

        return workItem is null
            ? NotFound()
            : Ok(workItem);
    }

    [HttpPost]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType<WorkItem>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkItem>> Create(
        CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ApplicationId.HasValue)
        {
            var applicationExists = await _applicationQuery.ApplicationExists(
                request.ApplicationId.Value,
                _options.Value.WorkspaceId,
                cancellationToken);

            if (!applicationExists)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Application selection",
                    Detail = "The selected Application does not exist in the configured workspace.",
                    Status = StatusCodes.Status400BadRequest
                });
            }
        }

        var workItem = await _workItemStore.CreateAsync(
            request.Title!,
            request.ApplicationId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = workItem.Id },
            workItem);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType<WorkItem>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkItem>> Update(Guid id, UpdateWorkItemRequest request)
    {
        if (request.Title is null && request.Status is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid update request",
                Detail = "Provide at least one supported field to update.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var workItem = await _workItemStore.UpdateAsync(id, request.Title, request.Status);

        return workItem is null
            ? NotFound()
            : Ok(workItem);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanWrite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _workItemStore.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}