namespace OpsPortal.Api.Contracts;

public sealed class WorkItemsByStatusReportRow {
    public required string Status { get; set; }

    public int Count { get; set; }
}
