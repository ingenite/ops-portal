namespace OpsPortal.Api.Models;

public sealed class WorkItem
{
    public Guid Id { get; init; }

    public required string Title { get; set; }

    public required string Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; init; }
    
    public DateTimeOffset UpdatedAtUtc { get; set; }
}