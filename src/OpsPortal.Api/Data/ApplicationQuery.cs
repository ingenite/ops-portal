using Microsoft.EntityFrameworkCore;
using OpsPortal.Api.Contracts;

namespace OpsPortal.Api.Data;

public sealed class ApplicationQuery {
    private readonly OpsPortalDbContext _dbContext;

    public ApplicationQuery(OpsPortalDbContext dbContext) {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ApplicationListRow>> GetAllAsync(
        string workspaceId,
        CancellationToken cancellationToken) {
        var applications = await _dbContext.Applications
            .Where(app => app.WorkspaceId == workspaceId)
            .OrderBy(app => app.Name)
            .ThenBy(app => app.ObjectId)
            .Select(app => new ApplicationListRow {
                ExternalId = app.ObjectId,
                Key = app.Key,
                Name = app.Name,
                DeploymentMethod = app.DeploymentMethod,
                Status = app.Status,
                LastSyncedAtUtc = app.LastSyncedAtUtc
            })
            .ToArrayAsync(cancellationToken);

        return applications;
    }
}
