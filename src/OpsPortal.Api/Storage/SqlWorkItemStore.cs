using Microsoft.EntityFrameworkCore;

using OpsPortal.Api.Data;
using OpsPortal.Api.Models;

namespace OpsPortal.Api.Storage;

public sealed class SqlWorkItemStore : IWorkItemStore
{
    private readonly OpsPortalDbContext _dbContext;

    public SqlWorkItemStore(OpsPortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkItem> CreateAsync(string title, Guid? applicationId)
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

        _dbContext.WorkItems.Add(workItem);
        await _dbContext.SaveChangesAsync();

        return workItem;
    }

    public async Task<IReadOnlyCollection<WorkItem>> GetAllAsync()
    {
        return await _dbContext.WorkItems // start EF query against WorkItems table
            .AsNoTracking() //read-only
            .OrderByDescending(workItem => workItem.UpdatedAtUtc)
            .ToArrayAsync(); // execute the query
    }

    public async Task<WorkItem?> GetByIdAsync(Guid id)
    {
        return await _dbContext.WorkItems
            .AsNoTracking()
            .SingleOrDefaultAsync(workItem => workItem.Id == id);
    }

    public async Task<WorkItem?> UpdateAsync(Guid id, string? title, string? status)
    {
        var workItem = await _dbContext.WorkItems.FindAsync(id);

        // avoid the ternary temptation
        if (workItem is null)
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

        await _dbContext.SaveChangesAsync();

        return workItem;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var workItem = await _dbContext.WorkItems.FindAsync(id);

        if (workItem is null)
        {
            return false;
        }

        _dbContext.WorkItems.Remove(workItem);

        await _dbContext.SaveChangesAsync();

        return true;
    }
}