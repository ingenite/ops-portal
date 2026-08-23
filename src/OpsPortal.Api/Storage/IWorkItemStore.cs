using OpsPortal.Api.Models;

namespace OpsPortal.Api.Storage;

public interface IWorkItemStore
{
    IReadOnlyCollection<WorkItem> GetAll();

    WorkItem? GetById(Guid id);

    WorkItem Create(string title);

    WorkItem? Update(Guid id, string? title, string? status);
}