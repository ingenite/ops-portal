using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using OpsPortal.Api.Data;
using OpsPortal.Api.Models;
using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Synchronization;

public class ApplicationSyncService
{
    private readonly IAtlassianAssetsClient _atlassianAssetsClient;
    private readonly IOptions<AtlassianOptions> _options;
    private readonly OpsPortalDbContext _dbContext;

    public ApplicationSyncService(
        IAtlassianAssetsClient atlassianAssetsClient,
        IOptions<AtlassianOptions> options,
        OpsPortalDbContext dbContext)
    {
        _atlassianAssetsClient = atlassianAssetsClient;
        _options = options;
        _dbContext = dbContext;
    }

    public async Task SyncAsync(CancellationToken cancellationToken)
    {
        var response = await _atlassianAssetsClient.GetApplicationsAsync(cancellationToken);

        var incomingApplications = response.Values
            .Select(AtlassianApplicationMapper.Map)
            .ToList();

        var workspaceId = _options.Value.WorkspaceId;

        var existingApplications = await _dbContext.Applications
            .Where(app => app.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);

        var syncedAtUtc = DateTimeOffset.UtcNow;

        foreach (var incomingApplication in incomingApplications)
        {
            var existingApplication = existingApplications.SingleOrDefault(app =>
                app.ObjectId == incomingApplication.ExternalId);

            if (existingApplication is null)
            {
                var application = new Application
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    ObjectId = incomingApplication.ExternalId,
                    Key = incomingApplication.Key,
                    Name = incomingApplication.Name,
                    DeploymentMethod = incomingApplication.DeploymentMethod,
                    Status = incomingApplication.Status,
                    LastSyncedAtUtc = syncedAtUtc
                };

                _dbContext.Applications.Add(application);

                existingApplications.Add(application);
            }
            else
            {
                // update
                existingApplication.Name = incomingApplication.Name;
                existingApplication.DeploymentMethod = incomingApplication.DeploymentMethod;
                existingApplication.Status = incomingApplication.Status;
                existingApplication.LastSyncedAtUtc = syncedAtUtc;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}