namespace OpsPortal.Api.Contracts;

public sealed class ApplicationListRow {
    // Atlassian's Object Id
    public required string ExternalId { get; set; }

    // Atlassian's Key, e.g. APM-8
    public required string Key { get; set; }

    public required string Name { get; set; }

    public string? DeploymentMethod { get; set; }

    public string? Status { get; set; }

    public DateTimeOffset LastSyncedAtUtc { get; set; }
}
