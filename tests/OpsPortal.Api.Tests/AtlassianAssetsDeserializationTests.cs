using System.Text.Json;

using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Tests;

public sealed class AtlassianAssetsDeserializationTests
{
    [Fact]
    public void Atlassian_Assets_response_deserializes_into_types()
    {
        // Arrange - static Assets API response
        var json = """
            {
                "values": [
                    {
                    "id": "22",
                    "objectKey": "APM-22",
                    "label": "Salesforce CRM",
                    "attributes": [
                        {
                        "objectTypeAttributeId": "250",
                        "objectAttributeValues": [
                            {
                            "displayValue": "Salesforce CRM",
                            "searchValue": "Salesforce CRM",
                            "referencedType": false,
                            "value": "Salesforce CRM"
                            }
                        ]
                        },
                        {
                        "objectTypeAttributeId": "594",
                        "objectAttributeValues": [
                            {
                            "displayValue": "Cloud",
                            "searchValue": "Cloud",
                            "referencedType": false,
                            "value": "Cloud"
                            }
                        ]
                        },
                        {
                        "objectTypeAttributeId": "597",
                        "objectAttributeValues": [
                            {
                            "displayValue": "Test",
                            "searchValue": "Test",
                            "referencedType": false,
                            "status": {
                                "workspaceId": "25517fe1-b9bd-4c76-9a12-41fcd81b9473",
                                "globalId": "25517fe1-b9bd-4c76-9a12-41fcd81b9473:28",
                                "id": "28",
                                "name": "Test",
                                "displayName": "Test",
                                "category": 2,
                                "objectSchemaId": "7"
                            }
                            }
                        ]
                        }
                    ]
                    }
                ]
                }
        """;

        // Act - Deserialize json response
        var response = JsonSerializer.Deserialize<AtlassianAssetsQueryResponse>(
            json,
            JsonSerializerOptions.Web);

        // Assert
        Assert.NotNull(response);

        var asset = Assert.Single(response.Values);
        Assert.Equal("APM-22", asset.ObjectKey);
        // Assert each attribute id is returned with the correct value
        // Name - Id 250 = Salesforce CRM
        var nameAttribute = Assert.Single(asset.Attributes,
            attribute => attribute.ObjectTypeAttributeId == "250");

        var nameValue = Assert.Single(nameAttribute.ObjectAttributeValues);

        Assert.Equal("Salesforce CRM", nameValue.DisplayValue);

        // Deployment Method - Id 594 = Cloud
        var deploymentMethodAttribute = Assert.Single(asset.Attributes,
            attribute => attribute.ObjectTypeAttributeId == "594");

        var deploymentMethodValue = Assert.Single(
            deploymentMethodAttribute.ObjectAttributeValues);

        Assert.Equal("Cloud", deploymentMethodValue.DisplayValue);

        // Status - Id 597 = Test
        var statusAttribute = Assert.Single(asset.Attributes,
            attribute => attribute.ObjectTypeAttributeId == "597");

        var statusValue = Assert.Single(statusAttribute.ObjectAttributeValues);

        Assert.Equal("Test", statusValue.DisplayValue);
    }
}