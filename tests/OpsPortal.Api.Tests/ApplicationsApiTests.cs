using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Synchronization;

namespace OpsPortal.Api.Tests;

public sealed class ApplicationsApiTests
{
    [Fact]
    public async Task Sync_returns_no_content_after_persisting_applications()
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database,
            new FakeAtlassianAssetsClient(
                new AtlassianAssetsQueryResponse
                {
                    Values = [new() { Id = "22", ObjectKey = "APM-22", Label = "Demo CRM" }]
                }));

        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        using var response = await client.PostAsync("/api/applications/sync", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        using var verification = database.CreateContext();
        var row = Assert.Single(await verification.Applications.ToListAsync());
        Assert.Equal("22", row.ObjectId);
        Assert.Equal("Demo CRM", row.Name);
    }

    [Fact]
    public async Task Manual_sync_returns_409_Conflict_while_the_gate_is_held()
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        const string providerError = "Private fake provider diagnostic";
        var provider = new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse())
        {
            Failure = new Exception(providerError)
        };

        using var applicationFactory = factory.WithDatabase(database, provider);
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var coordinator = applicationFactory.Services.GetRequiredService<ApplicationSyncCoordinator>();
        Assert.True(await coordinator.TryEnterAsync(CancellationToken.None));

        using var response = await client.PostAsync("/api/applications/sync", null);
        coordinator.Exit();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task Sync_returns_bad_gateway_without_exposing_provider_diagnostics()
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        const string providerError = "Private fake provider diagnostic";
        var provider = new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse())
        {
            Failure = new HttpRequestException(providerError, null, HttpStatusCode.Forbidden)
        };

        using var applicationFactory = factory.WithDatabase(database, provider);
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        using var response = await client.PostAsync("/api/applications/sync", null);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        Assert.Equal(502, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Problem contacting external provider", document.RootElement.GetProperty("title").GetString());
        Assert.Equal("Unable to synchronize applications from the provider API.",
            document.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain(providerError, body);
        using var verification = database.CreateContext();
        Assert.Empty(await verification.Applications.ToListAsync());
    }

    [Fact]
    public async Task GetAll_returns_stored_rows_for_configured_workspace_with_freshness()
    {
        using var database = new TestDatabase();
        var stored = ApplicationTestData.Create("22", "Demo CRM");
        using (var context = database.CreateContext())
        {
            var other = ApplicationTestData.Create("22", "Other workspace CRM");
            other.WorkspaceId = "other-workspace";
            context.Applications.AddRange(stored, other);
            await context.SaveChangesAsync();
        }

        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database,
            new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse()));

        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        using var response = await client.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = Assert.Single(document.RootElement.EnumerateArray());
        Assert.Equal("22", row.GetProperty("externalId").GetString());
        Assert.Equal("APM-22", row.GetProperty("key").GetString());
        Assert.Equal("Demo CRM", row.GetProperty("name").GetString());
        Assert.Equal("Cloud", row.GetProperty("deploymentMethod").GetString());
        Assert.Equal("Test", row.GetProperty("status").GetString());
        Assert.Equal(stored.Id.ToString(), row.GetProperty("id").GetString());
        Assert.Equal(stored.LastSyncedAtUtc, row.GetProperty("lastSyncedAtUtc").GetDateTimeOffset());
        Assert.Equal(7, row.EnumerateObject().Count());
    }

    [Fact]
    public async Task GetAll_returns_empty_array_when_database_is_empty_even_if_provider_has_assets()
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database,
            new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse
            {
                Values = [new() { Id = "22", ObjectKey = "APM-22", Label = "Provider-only CRM" }]
            }));
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        using var response = await client.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.Empty(document.RootElement.EnumerateArray());
    }

    [Fact]
    public async Task GetAll_returns_stored_rows_without_calling_failing_provider()
    {
        using var database = new TestDatabase();
        var stored = ApplicationTestData.Create("22", "Stored CRM");
        using (var context = database.CreateContext())
        {
            context.Applications.Add(stored);
            await context.SaveChangesAsync();
        }

        var provider = new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse())
        {
            Failure = new Exception("Private fake provider diagnostic")
        };

        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database, provider);
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        using var response = await client.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var row = Assert.Single(document.RootElement.EnumerateArray());
        Assert.Equal("22", row.GetProperty("externalId").GetString());
        Assert.Equal("Stored CRM", row.GetProperty("name").GetString());
        Assert.Equal(stored.LastSyncedAtUtc, row.GetProperty("lastSyncedAtUtc").GetDateTimeOffset());
        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task GetAll_orders_by_name_then_object_id_and_preserves_null_fields()
    {
        using var database = new TestDatabase();
        var nullableApplication = ApplicationTestData.Create("20", "Alpha");
        nullableApplication.DeploymentMethod = null;
        nullableApplication.Status = null;
        using (var context = database.CreateContext())
        {
            context.Applications.AddRange(
                ApplicationTestData.Create("10", "Zulu"),
                ApplicationTestData.Create("30", "Alpha"),
                nullableApplication);
            await context.SaveChangesAsync();
        }

        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database,
            new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse()));

        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        using var response = await client.GetAsync("/api/applications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var rows = document.RootElement.EnumerateArray().ToArray();
        Assert.Equal(new[] { "20", "30", "10" },
            rows.Select(row => row.GetProperty("externalId").GetString()));
        Assert.Equal(JsonValueKind.Null, rows[0].GetProperty("deploymentMethod").ValueKind);
        Assert.Equal(JsonValueKind.Null, rows[0].GetProperty("status").ValueKind);
    }
}