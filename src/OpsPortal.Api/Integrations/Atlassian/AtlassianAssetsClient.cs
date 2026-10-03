using Microsoft.Extensions.Options;

namespace OpsPortal.Api.Integrations.Atlassian;

public sealed class AtlassianAssetsClient : IAtlassianAssetsClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<AtlassianOptions> _options;

    public AtlassianAssetsClient(
        HttpClient httpClient,
        IOptions<AtlassianOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(
       CancellationToken cancellationToken)
    {
        var options = _options.Value;

        var requestUri =
            $"ex/jira/{Uri.EscapeDataString(options.CloudId)}" +
            $"/jsm/assets/workspace/{Uri.EscapeDataString(options.WorkspaceId)}" +
            $"/v1/object/aql?startAt=0" +
            $"&maxResults={options.ApplicationsMaxResults}" +
            "&includeAttributes=true";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(new
            {
                qlQuery = options.ApplicationsAqlQuery
            })
        };

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<AtlassianAssetsQueryResponse>(
                cancellationToken: cancellationToken);

        return result ?? throw new InvalidOperationException(
            "Atlassian Assets returned an empty response body.");
    }
}