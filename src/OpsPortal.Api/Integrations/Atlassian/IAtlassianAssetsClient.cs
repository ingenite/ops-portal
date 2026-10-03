namespace OpsPortal.Api.Integrations.Atlassian;

public interface IAtlassianAssetsClient
{
    Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(
        CancellationToken cancellationToken);
}