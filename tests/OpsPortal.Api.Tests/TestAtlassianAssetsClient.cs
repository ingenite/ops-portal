using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Tests;

internal sealed class FakeAtlassianAssetsClient : IAtlassianAssetsClient {
    private readonly AtlassianAssetsQueryResponse _response;
    public int CallCount { get; private set; }

    public FakeAtlassianAssetsClient(AtlassianAssetsQueryResponse response) {
        _response = response;
    }

    public Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(
        CancellationToken cancellationToken) {
        CallCount++;
        return Task.FromResult(_response);
    }
}
