using System.ComponentModel.DataAnnotations;

namespace OpsPortal.Api.Contracts;

public sealed class UpdateWorkItemRequest : IValidatableObject
{
    [StringLength(200)]
    public string? Title { get; init; }

    [RegularExpression("^(Open|InProgress|Completed)$")]
    public string? Status { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Title is not null && string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult(
                "Title cannot be empty or whitespace.",
                new[] { nameof(Title) });
        }

        if (Status is not null && string.IsNullOrWhiteSpace(Status))
        {
            yield return new ValidationResult(
                "Status cannot be empty or whitespace.",
                new[] { nameof(Status) });
        }
    }
}