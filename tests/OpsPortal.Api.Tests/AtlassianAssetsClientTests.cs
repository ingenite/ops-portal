using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OpsPortal.Api.Integrations.Atlassian;

using Xunit.Sdk;

namespace OpsPortal.Api.Tests;

public sealed class AtlassianAssetsClientTests {
    [Fact]
    public async Task GetApplicationsAsync_sends_the_configured_AQL_request_and_deserializes_the_response() {
        // Arrange
        var options = new AtlassianOptions {
            CloudId = "cloud-123",
            WorkspaceId = "workspace-456",
            ApplicationsAqlQuery = "objectType = Application",
            ApplicationsMaxResults = 25
        };

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

        var handler = new FakeHttpMessageHandler(json);

        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://example.test/")
        };

        var client = new AtlassianAssetsClient(
            httpClient,
            Options.Create(options)
        );

        // Act
        var result = await client.GetApplicationsAsync(
            CancellationToken.None
        );

        // Assert
        var asset = Assert.Single(result.Values);

        Assert.Equal("APM-22", asset.ObjectKey);
        Assert.Equal(HttpMethod.Post, handler.RequestMethod);

        Assert.NotNull(handler.RequestUri);
        Assert.Equal(
            "/ex/jira/cloud-123/jsm/assets/workspace/workspace-456/v1/object/aql",
            handler.RequestUri.AbsolutePath
        );

        Assert.Equal(
            "?startAt=0&maxResults=25&includeAttributes=true",
            handler.RequestUri.Query
        );

        Assert.NotNull(handler.RequestBody);
        using var requestBody = JsonDocument.Parse(handler.RequestBody);

        Assert.Equal(
            options.ApplicationsAqlQuery,
            requestBody.RootElement.GetProperty("qlQuery").GetString()
        );
    }

    [Fact]
    public async Task GetApplicationsAsync_throws_when_provider_returns_forbidden() {
        // Arrange
        var options = new AtlassianOptions {
            CloudId = "cloud-123",
            WorkspaceId = "workspace-456",
            ApplicationsAqlQuery = "objectType = Application",
            ApplicationsMaxResults = 25
        };

        var handler = new FakeHttpMessageHandler(
            "{}",
            HttpStatusCode.Forbidden);

        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://example.test/")
        };

        var client = new AtlassianAssetsClient(
            httpClient,
            Options.Create(options)
        );

        // Act and Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetApplicationsAsync(CancellationToken.None)
        );

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public async Task GetApplicationsAsync_throws_when_response_is_json_null()
    {
        // Arrange
        var options = new AtlassianOptions {
            CloudId = "cloud-123",
            WorkspaceId = "workspace-456",
            ApplicationsAqlQuery = "objectType = Application",
            ApplicationsMaxResults = 25
        };

        var handler = new FakeHttpMessageHandler("null");

        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://example.test/")
        };
        var client = new AtlassianAssetsClient(
            httpClient,
            Options.Create(options)
        );

        // Act and Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetApplicationsAsync(CancellationToken.None)
        );

        Assert.Equal(
            "Atlassian Assets returned an empty response body.",
            exception.Message);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler {
        private readonly string _json;
        private readonly HttpStatusCode _statusCode;

        public FakeHttpMessageHandler(
            string json,
            HttpStatusCode statusCode = HttpStatusCode.OK) {
            _json = json;
            _statusCode = statusCode;
        }

        public HttpMethod? RequestMethod { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {

            RequestMethod = request.Method;
            RequestUri = request.RequestUri;
            RequestBody = await request.Content!
                .ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(_statusCode) {
                Content = new StringContent(_json)
            };

            return response;
        }
    }
}
