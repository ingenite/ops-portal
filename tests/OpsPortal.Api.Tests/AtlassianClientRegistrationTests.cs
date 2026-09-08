using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Tests;

public sealed class AtlassianClientRegistrationTests {
    [Fact]
    public async Task Registered_client_sends_basic_auth_with_configured_email_and_api_token() {
        // Arrange - keep Program.cs registration; replace only the network transport.
        using var factory = new OpsPortalApiFactory();
        var handler = new CapturingHttpMessageHandler();
        using var applicationFactory = factory.WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.ConfigureHttpClientDefaults(client => {
                    client.ConfigurePrimaryHttpMessageHandler(() => handler);
                });
            });
        });
        using var scope = applicationFactory.Services.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<IAtlassianAssetsClient>();

        // Act
        await client.GetApplicationsAsync(CancellationToken.None);

        // Assert - credentials come from OpsPortalApiFactory's fake configuration.
        Assert.NotNull(handler.Authorization);
        Assert.Equal("Basic", handler.Authorization.Scheme);
        Assert.NotNull(handler.Authorization.Parameter);
        var credentials = Encoding.UTF8.GetString(
            Convert.FromBase64String(handler.Authorization.Parameter));
        Assert.Equal("test@example.com:not-a-real-token", credentials);

        Assert.NotNull(handler.RequestUri);
        Assert.Equal("https", handler.RequestUri.Scheme);
        Assert.Equal("api.atlassian.com", handler.RequestUri.Host);
        Assert.Contains("application/json", handler.AcceptMediaTypes);
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler {
        public AuthenticationHeaderValue? Authorization { get; private set; }
        public Uri? RequestUri { get; private set; }
        public string?[] AcceptMediaTypes { get; private set; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            Authorization = request.Headers.Authorization;
            RequestUri = request.RequestUri;
            AcceptMediaTypes = request.Headers.Accept.Select(value => value.MediaType).ToArray();

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent("""{"values": []}""", Encoding.UTF8, "application/json")
            });
        }
    }
}
