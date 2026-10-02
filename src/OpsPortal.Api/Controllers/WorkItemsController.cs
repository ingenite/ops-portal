using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpsPortal.Api.Contracts;
using OpsPortal.Api.Models;
using OpsPortal.Api.Storage;

namespace OpsPortal.Api.Controllers;

[ApiController]
[Route("api/work-items")]
public sealed class WorkItemsController : ControllerBase {
    private readonly IWorkItemStore _workItemStore;

    public WorkItemsController(IWorkItemStore workItemStore) {
        _workItemStore = workItemStore;
    }

    [HttpGet]
    [Authorize(Policy = "CanRead")]
    public async Task<ActionResult<IReadOnlyCollection<WorkItem>>> GetAll() {
        return Ok(await _workItemStore.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkItem>> GetById(Guid id) {
        var workItem = await _workItemStore.GetByIdAsync(id);

        return workItem is null
            ? NotFound()
            : Ok(workItem);
    }

    [HttpPost]
    public async Task<ActionResult<WorkItem>> Create(CreateWorkItemRequest request) {
        var workItem = await _workItemStore.CreateAsync(request.Title!);

        return CreatedAtAction(
            nameof(GetById),
            new { id = workItem.Id },
            workItem);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<WorkItem>> Update(Guid id, UpdateWorkItemRequest request) {
        if (request.Title is null && request.Status is null) {
            return BadRequest(new ProblemDetails {
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
    public async Task<IActionResult> Delete(Guid id) {
        return await _workItemStore.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
}