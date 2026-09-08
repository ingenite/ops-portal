using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Tests;

public sealed class ApplicationsApiTests {
    [Fact]
    public async Task GetAll_returns_mapped_application_rows() {
        // Arrange
        var providerResponse = new AtlassianAssetsQueryResponse {
            Values = [
                new AtlassianAssetObject {
                    Id = "22",
                    ObjectKey = "APM-22",
                    Label = "Demo CRM",
                    Attributes = [
                        new AtlassianAssetAttribute {
                            ObjectTypeAttributeId = "594",
                            ObjectAttributeValues = [new() { DisplayValue = "Cloud" }]
                        },
                        new AtlassianAssetAttribute {
                            ObjectTypeAttributeId = "597",
                            ObjectAttributeValues = [new() { DisplayValue = "Test" }]
                        }
                    ]
                }
            ]
        };

        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = WithProviderResponse(factory, providerResponse);
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions {
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.GetAsync("/api/applications");

        // Assert - inspect the public JSON contract, including property names.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = Assert.Single(document.RootElement.EnumerateArray());
        Assert.Equal("22", row.GetProperty("externalId").GetString());
        Assert.Equal("APM-22", row.GetProperty("key").GetString());
        Assert.Equal("Demo CRM", row.GetProperty("name").GetString());
        Assert.Equal("Cloud", row.GetProperty("deploymentMethod").GetString());
        Assert.Equal("Test", row.GetProperty("status").GetString());
        Assert.Equal(5, row.EnumerateObject().Count());
    }

    [Fact]
    public async Task GetAll_returns_ok_with_an_empty_array_when_provider_has_no_assets() {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = WithProviderResponse(factory, new AtlassianAssetsQueryResponse());
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions {
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.GetAsync("/api/applications");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Empty(document.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task GetAll_returns_bad_gateway_problem_without_provider_error_details() {
        // Arrange
        const string providerError = "Fake provider diagnostic that must remain private";
        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.RemoveAll<IAtlassianAssetsClient>();
                services.AddSingleton<IAtlassianAssetsClient>(
                    new FailingAtlassianAssetsClient(providerError));
            });
        });
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions {
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.GetAsync("/api/applications");

        // Assert - the provider's 403 becomes our 502 with a generic problem body.
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var problem = document.RootElement;
        Assert.Equal(502, problem.GetProperty("status").GetInt32());
        Assert.Equal("Problem contacting external provider", problem.GetProperty("title").GetString());
        Assert.Equal("Unable to retrieve Applications from provider API", problem.GetProperty("detail").GetString());
        Assert.DoesNotContain(providerError, body);
    }

    private sealed class FailingAtlassianAssetsClient : IAtlassianAssetsClient {
        private readonly string _message;

        public FailingAtlassianAssetsClient(string message) {
            _message = message;
        }

        public Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(
            CancellationToken cancellationToken) {
            return Task.FromException<AtlassianAssetsQueryResponse>(
                new HttpRequestException(_message, null, HttpStatusCode.Forbidden));
        }
    }

    private static WebApplicationFactory<Program> WithProviderResponse(
        OpsPortalApiFactory factory,
        AtlassianAssetsQueryResponse response) {
        return factory.WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.RemoveAll<IAtlassianAssetsClient>();
                services.AddSingleton<IAtlassianAssetsClient>(new FakeAtlassianAssetsClient(response));
            });
        });
    }

    private sealed class FakeAtlassianAssetsClient : IAtlassianAssetsClient {
        private readonly AtlassianAssetsQueryResponse _response;

        public FakeAtlassianAssetsClient(AtlassianAssetsQueryResponse response) {
            _response = response;
        }

        public Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(
            CancellationToken cancellationToken) {
            return Task.FromResult(_response);
        }
    }
}
