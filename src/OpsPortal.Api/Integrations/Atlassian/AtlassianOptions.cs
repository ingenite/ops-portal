

namespace OpsPortal.Api.Integrations.Atlassian;

public sealed class AtlassianOptions {
    public string CloudId { get; set; } = string.Empty;

    public string WorkspaceId { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string ApplicationsAqlQuery { get; set; } = string.Empty;

    public int ApplicationsMaxResults { get; set; } = 25;
}
