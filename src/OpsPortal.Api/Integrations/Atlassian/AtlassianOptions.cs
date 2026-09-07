

namespace OpsPortal.Api.Integrations.Atlassian;

public sealed class AtlassianOptions {
    public string CloudId { get; set; } = string.Empty;

    public string WorkspaceId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string ApiToken { get; set; } = string.Empty;
}
