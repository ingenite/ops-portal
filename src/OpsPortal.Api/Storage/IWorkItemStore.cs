using OpsPortal.Api.Models;

namespace OpsPortal.Api.Storage;

public interface IWorkItemStore {
    Task<IReadOnlyCollection<WorkItem>> GetAllAsync();

    Task<WorkItem?> GetByIdAsync(Guid id);

    Task<WorkItem> CreateAsync(string title);

    Task<WorkItem?> UpdateAsync(Guid id, string? title, string? status);

    Task<bool> DeleteAsync(Guid id);
}