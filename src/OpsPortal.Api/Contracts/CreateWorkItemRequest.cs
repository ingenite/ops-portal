using System.ComponentModel.DataAnnotations;

namespace OpsPortal.Api.Contracts;

public sealed class CreateWorkItemRequest {
    [Required]
    [StringLength(200)]
    public string? Title { get; init; }
}