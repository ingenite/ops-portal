
namespace OpsPortal.Api.Tests;

internal static class ApplicationTestData
{
    internal static OpsPortal.Api.Models.Application Create(
        string objectId, string name) => new()
        {
            Id = Guid.NewGuid(),
            WorkspaceId = "test-cloud-id",
            ObjectId = objectId,
            Key = $"APM-{objectId}",
            Name = name,
            DeploymentMethod = "Cloud",
            Status = "Test",
            LastSyncedAtUtc = new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero)
        };
}