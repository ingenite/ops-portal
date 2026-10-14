using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Tests;

internal sealed class FakeAtlassianAssetsClient : IAtlassianAssetsClient
{
    private readonly AtlassianAssetsQueryResponse _response;
    public int CallCount { get; private set; }
    public Exception? Failure { get; init; }
    public CancellationToken? ReceivedToken { get; private set; }

    public FakeAtlassianAssetsClient(AtlassianAssetsQueryResponse response)
    {
        _response = response;
    }

    public Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(
        CancellationToken cancellationToken)
    {
        CallCount++;

        ReceivedToken = cancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        return Failure is null
            ? Task.FromResult(_response)
            : Task.FromException<AtlassianAssetsQueryResponse>(Failure);
    }
}