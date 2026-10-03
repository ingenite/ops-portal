using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using OpsPortal.Api.Data;
using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Storage;
using OpsPortal.Api.Synchronization;

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
                    ["Atlassian:ApplicationsMaxResults"] = "25",
                    ["ApplicationSync:Enabled"] = "false"
                });
        });

        builder.ConfigureTestServices(services => {
            services.RemoveAll<IWorkItemStore>();
            services.AddSingleton<IWorkItemStore, InMemoryWorkItemStore>();

            var worker = services.SingleOrDefault(service =>
                service.ServiceType == typeof(IHostedService) &&
                service.ImplementationType == typeof(ApplicationSyncWorker));

            if (worker is not null) {
                services.Remove(worker);
            }

            services.AddAuthentication(options => {
               options.DefaultAuthenticateScheme = "Test";
               options.DefaultChallengeScheme = "Test";
               options.DefaultForbidScheme = "Test";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                "Test",
                options => { });
        });
    }

    internal WebApplicationFactory<Program> WithDatabase(
        TestDatabase database,
        IAtlassianAssetsClient? provider) {
        return WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.RemoveAll<OpsPortalDbContext>();
                services.AddScoped<OpsPortalDbContext>(_ => database.CreateContext());
                if (provider is not null) {
                    services.RemoveAll<IAtlassianAssetsClient>();
                    services.AddSingleton<IAtlassianAssetsClient>(provider);
                }
            });
        });
    }
}
