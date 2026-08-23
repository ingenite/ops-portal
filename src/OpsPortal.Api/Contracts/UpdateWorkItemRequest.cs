using System.ComponentModel.DataAnnotations;

namespace OpsPortal.Api.Contracts;

public sealed class UpdateWorkItemRequest {
    [StringLength(200)]
    public string? Title { get; init; }

    [RegularExpression("^(Open|InProgress|Completed)$")]
    public string? Status { get; init; }
}