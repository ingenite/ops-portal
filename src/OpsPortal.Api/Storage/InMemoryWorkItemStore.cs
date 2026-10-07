using OpsPortal.Api.Models;

namespace OpsPortal.Api.Storage;

public sealed class InMemoryWorkItemStore : IWorkItemStore
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, WorkItem> _workItems = new();

    public InMemoryWorkItemStore()
    {
        CreateWorkItem("Review integration documentation", null);
        CreateWorkItem("Design the reporting page", null);
    }

    public Task<WorkItem> CreateAsync(string title, Guid? applicationId)
    {
        return Task.FromResult(CreateWorkItem(title, applicationId));
    }

    private WorkItem CreateWorkItem(string title, Guid? applicationId)
    {
        var now = DateTimeOffset.UtcNow;

        var workItem = new WorkItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Status = "Open",
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            ApplicationId = applicationId
        };

        lock (_syncRoot)
        {
            _workItems.Add(workItem.Id, workItem);
        }

        return workItem;
    }

    public Task<IReadOnlyCollection<WorkItem>> GetAllAsync()
    {
        lock (_syncRoot)
        {
            var workItems = _workItems.Values
                .OrderByDescending(workItem => workItem.UpdatedAtUtc)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<WorkItem>>(workItems);
        }
    }

    public Task<WorkItem?> GetByIdAsync(Guid id)
    {
        lock (_syncRoot)
        {
            return Task.FromResult(_workItems.GetValueOrDefault(id));
        }
    }

    public Task<WorkItem?> UpdateAsync(Guid id, string? title, string? status)
    {
        lock (_syncRoot)
        {
            if (!_workItems.TryGetValue(id, out var workItem))
            {
                return Task.FromResult<WorkItem?>(null);
            }

            if (title is not null)
            {
                workItem.Title = title.Trim();
            }

            if (status is not null)
            {
                workItem.Status = status;
            }

            workItem.UpdatedAtUtc = DateTimeOffset.UtcNow;

            return Task.FromResult<WorkItem?>(workItem);
        }
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        lock (_syncRoot)
        {
            return Task.FromResult(_workItems.Remove(id));
        }
    }
}
