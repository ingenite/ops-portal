namespace OpsPortal.Api.Models;

public sealed class Application
{
    public Guid Id { get; init; }
    public required string WorkspaceId { get; set; }
    public required string ObjectId { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? DeploymentMethod { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset LastSyncedAtUtc { get; set; }
}