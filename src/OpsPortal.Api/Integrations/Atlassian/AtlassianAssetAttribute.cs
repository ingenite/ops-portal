namespace OpsPortal.Api.Integrations.Atlassian;

public class AtlassianAssetAttribute {
    // Asset Object "Property" Id
    public string ObjectTypeAttributeId {get; set;} = string.Empty;

    public List<AtlassianAssetAttributeValue> ObjectAttributeValues { get; set; } = [];
}
