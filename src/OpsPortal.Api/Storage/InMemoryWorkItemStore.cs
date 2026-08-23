using OpsPortal.Api.Models;

namespace OpsPortal.Api.Storage;

public sealed class InMemoryWorkItemStore : IWorkItemStore
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, WorkItem> _workItems = new();

    public InMemoryWorkItemStore()
    {
        Create("Review integration documentation");
        Create("Design the reporting page");
    }

    public IReadOnlyCollection<WorkItem> GetAll()
    {
        lock (_syncRoot)
        {
            return _workItems.Values
                .OrderByDescending(workItem => workItem.UpdatedAtUtc)
                .ToArray();
        }
    }

    public WorkItem? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            return _workItems.GetValueOrDefault(id);
        }
    }

    public WorkItem Create(string title)
    {
        var now = DateTimeOffset.UtcNow;

        var workItem = new WorkItem
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Status = "Open",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        lock (_syncRoot)
        {
            _workItems.Add(workItem.Id, workItem);
        }

        return workItem;
    }

    public WorkItem? Update(Guid id, string? title, string? status)
    {
        lock (_syncRoot)
        {
            if (!_workItems.TryGetValue(id, out var workItem))
            {
                return null;
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

            return workItem;
        }
    }
}