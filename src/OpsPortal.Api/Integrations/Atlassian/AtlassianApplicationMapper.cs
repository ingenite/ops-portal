using OpsPortal.Api.Contracts;

namespace OpsPortal.Api.Integrations.Atlassian;

public static class AtlassianApplicationMapper {
    public static ApplicationListRow Map(AtlassianAssetObject asset) {
        // Find the display values for Deployment Method and Status
        var deploymentMethod = asset.Attributes.FirstOrDefault(
            attribute => attribute.ObjectTypeAttributeId == "594")
            ?.ObjectAttributeValues
            .FirstOrDefault()
            ?.DisplayValue;


        var status = asset.Attributes.FirstOrDefault(
            attribute => attribute.ObjectTypeAttributeId == "597")
            ?.ObjectAttributeValues
            .FirstOrDefault()
            ?.DisplayValue;

        return new ApplicationListRow {
            // Assign all five contract properties
            ExternalId = asset.Id,
            Key = asset.ObjectKey,
            Name = asset.Label,
            DeploymentMethod = deploymentMethod,
            Status = status
        };
    }
}
