using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpsPortal.Api.Storage;

namespace OpsPortal.Api.Tests;

public sealed class OpsPortalApiFactory : WebApplicationFactory<Program> {
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureAppConfiguration((_, configuration) => {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?> {
                    ["connectionStrings:OpsPortalDatabase"] =
                        "Server=unused;Database=unused;User Id=unused;Password=unused",
                    ["Atlassian:CloudId"] = "test-cloud-id",
                    ["Atlassian:WorkspaceId"] = "test-cloud-id",
                    ["Atlassian:Email"] = "test@example.com",
                    ["Atlassian:ApiToken"] = "not-a-real-token",
                    ["Atlassian:ApplicationsAqlQuery"] = "objectType = Application",
                    ["Atlassian:ApplicationsMaxResults"] = "25"
                });
        });

        builder.ConfigureTestServices(services => {
            services.RemoveAll<IWorkItemStore>();
            services.AddSingleton<IWorkItemStore, InMemoryWorkItemStore>();
        });
    }
}
