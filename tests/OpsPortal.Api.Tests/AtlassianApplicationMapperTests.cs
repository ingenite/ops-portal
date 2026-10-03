using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Tests;

public sealed class AtlassianApplicationMapperTests
{
    [Fact]
    public void Map_populated_asset_returns_expected_application_fields()
    {
        // Arrange
        var asset = CreatePopulatedAsset();

        // Act
        var map = AtlassianApplicationMapper.Map(asset);

        // Assert
        Assert.NotNull(map);
        Assert.Equal(asset.Id, map.ExternalId);
        Assert.Equal(asset.ObjectKey, map.Key);
        Assert.Equal(asset.Label, map.Name);
        Assert.Equal("Cloud", map.DeploymentMethod);
        Assert.Equal("Test", map.Status);
    }

    [Fact]
    public void Map_asset_with_missing_optional_attributes_returns_null_for_optional_fields()
    {
        // Arrange
        var asset = CreatePopulatedAsset();
        asset.Attributes.Clear();

        // Act
        var map = AtlassianApplicationMapper.Map(asset);

        // Assert
        Assert.NotNull(map);
        Assert.Equal(asset.Id, map.ExternalId);
        Assert.Equal(asset.ObjectKey, map.Key);
        Assert.Equal(asset.Label, map.Name);
        Assert.Null(map.DeploymentMethod);
        Assert.Null(map.Status);
    }

    [Fact]
    public void Map_asset_with_empty_optional_attribute_values_returns_null_for_optional_fields()
    {
        // Arrange
        var asset = CreatePopulatedAsset();
        foreach (var attribute in asset.Attributes)
        {
            attribute.ObjectAttributeValues.Clear();
        }

        // Act
        var map = AtlassianApplicationMapper.Map(asset);

        // Assert
        Assert.NotNull(map);
        Assert.Equal(asset.Id, map.ExternalId);
        Assert.Equal(asset.ObjectKey, map.Key);
        Assert.Equal(asset.Label, map.Name);
        Assert.Null(map.DeploymentMethod);
        Assert.Null(map.Status);
    }

    private static AtlassianAssetObject CreatePopulatedAsset()
    {
        return new AtlassianAssetObject
        {
            Id = "1",
            ObjectKey = "TEST-1",
            Label = "Test label",
            Attributes =
            [
                new AtlassianAssetAttribute {
                    ObjectTypeAttributeId = "594",
                    ObjectAttributeValues =
                    [
                        new AtlassianAssetAttributeValue {
                            DisplayValue = "Cloud"
                        }
                    ]
                },
                new AtlassianAssetAttribute {
                    ObjectTypeAttributeId = "597",
                    ObjectAttributeValues =
                    [
                        new AtlassianAssetAttributeValue {
                            DisplayValue = "Test"
                        }
                    ]
                }
            ]
        };
    }
}