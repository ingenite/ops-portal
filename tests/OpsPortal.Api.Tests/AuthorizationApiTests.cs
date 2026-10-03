using System.Net;
using System.Net.Http.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using OpsPortal.Api.Storage;
using OpsPortal.Api.Integrations.Atlassian;

namespace OpsPortal.Api.Tests;

public sealed class AuthorizationApiTests
{
    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Reader", HttpStatusCode.OK)]
    [InlineData("Test user", "Admin", HttpStatusCode.OK)]
    public async Task GetAll_enforces_read_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus)
    {
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.GetAsync("/api/work-items");

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Reader", HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Admin", HttpStatusCode.Created)]
    public async Task Create_enforces_write_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus)
    {
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title = $"Integration test item {Guid.NewGuid():N}" });

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Reader", HttpStatusCode.OK)]
    [InlineData("Test user", "Admin", HttpStatusCode.OK)]
    public async Task GetById_enforces_read_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus)
    {
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var store = factory.Services.GetRequiredService<IWorkItemStore>();
        var workItem = await store.CreateAsync("Authorization test item");

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.GetAsync(
            $"/api/work-items/{workItem.Id}");

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Reader", HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Admin", HttpStatusCode.OK)]
    public async Task Update_enforces_write_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus)
    {
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var store = factory.Services.GetRequiredService<IWorkItemStore>();
        var workItem = await store.CreateAsync("Authorization test item");

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.PatchAsJsonAsync(
            $"/api/work-items/{workItem.Id}",
            new { status = "Completed" });

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Reader", HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Admin", HttpStatusCode.NoContent)]
    public async Task Delete_enforces_write_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus)
    {
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        var store = factory.Services.GetRequiredService<IWorkItemStore>();
        var workItem = await store.CreateAsync("Authorization test item");

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.DeleteAsync(
            $"/api/work-items/{workItem.Id}");

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Reader", HttpStatusCode.OK)]
    [InlineData("Test user", "Admin", HttpStatusCode.OK)]
    public async Task GetWorkItemStatusReport_enforces_read_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus)
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database, null);

        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.GetAsync("/api/reports/workItemStatus");

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden)]
    [InlineData("Test user", "Reader", HttpStatusCode.OK)]
    [InlineData("Test user", "Admin", HttpStatusCode.OK)]
    public async Task GetApplications_enforces_read_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus)
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        var provider = new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse());
        using var applicationFactory = factory.WithDatabase(database, provider);

        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.GetAsync("/api/applications");

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData(null, null, HttpStatusCode.Unauthorized, 0)]
    [InlineData("Test user", null, HttpStatusCode.Forbidden, 0)]
    [InlineData("Test user", "Reader", HttpStatusCode.Forbidden, 0)]
    [InlineData("Test user", "Admin", HttpStatusCode.NoContent, 1)]
    public async Task SyncApplications_enforces_write_access(
        string? username,
        string? role,
        HttpStatusCode expectedStatus,
        int expectedProviderCalls)
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        var provider = new FakeAtlassianAssetsClient(new AtlassianAssetsQueryResponse());
        using var applicationFactory = factory.WithDatabase(database, provider);

        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        if (username is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-User", username);
        }

        if (role is not null)
        {
            client.DefaultRequestHeaders.Add("X-Test-Role", role);
        }

        using var response = await client.PostAsync("/api/applications/sync", null);

        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal(expectedProviderCalls, provider.CallCount);
    }
}