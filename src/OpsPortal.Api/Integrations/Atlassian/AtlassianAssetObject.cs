namespace OpsPortal.Api.Integrations.Atlassian;

public class AtlassianAssetObject {
    // Asset Object Id
    public required string Id { get; set; } = string.Empty;

    // Asset Object Key, e.g. APM-8
    public required string ObjectKey { get; set; } = string.Empty;

    // Asset Object DisplayName, e.g. "Salesforce CRM"
    public required string Label { get; set; } = string.Empty;

    public List<AtlassianAssetAttribute> Attributes { get; set; } = [];
}
