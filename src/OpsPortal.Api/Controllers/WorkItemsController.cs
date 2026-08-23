using Microsoft.AspNetCore.Mvc;
using OpsPortal.Api.Contracts;
using OpsPortal.Api.Models;
using OpsPortal.Api.Storage;

namespace OpsPortal.Api.Controllers;

[ApiController]
[Route("api/work-items")]
public sealed class WorkItemsController : ControllerBase
{
    private readonly IWorkItemStore _workItemStore;

    public WorkItemsController(IWorkItemStore workItemStore)
    {
        _workItemStore = workItemStore;
    }

    [HttpGet]
    public ActionResult<IReadOnlyCollection<WorkItem>> GetAll()
    {
        return Ok(_workItemStore.GetAll());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<WorkItem> GetById(Guid id)
    {
        var workItem = _workItemStore.GetById(id);

        return workItem is null
            ? NotFound()
            : Ok(workItem);
    }

    [HttpPost]
    public ActionResult<WorkItem> Create(CreateWorkItemRequest request)
    {
        var workItem = _workItemStore.Create(request.Title!);

        return CreatedAtAction(
            nameof(GetById),
            new { id = workItem.Id },
            workItem);
    }

    [HttpPatch("{id:guid}")]
    public ActionResult<WorkItem> Update(Guid id, UpdateWorkItemRequest request)
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

        var workItem = _workItemStore.Update(id, request.Title, request.Status);

        return workItem is null
            ? NotFound()
            : Ok(workItem);
    }
}