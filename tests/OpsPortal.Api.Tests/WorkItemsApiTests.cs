using System.Net;
using System.Net.Http.Json;
using OpsPortal.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OpsPortal.Api.Tests;

public sealed class WorkItemsApiTests {
    [Fact]
    // async Task - test can await HTTP without blocking a thread
    public async Task GetAll_returns_ok_with_a_json_array() {
        // Arrange - create an app factory/client and prepare input
        // `using var` disposes the factory/client automatically at the end of the test
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions {
            BaseAddress = new Uri("https://localhost")
        });

        // Act - send an HTTP request
        // await - pause test until HTTP response arrives
        var response = await client.GetAsync("/api/work-items");

        // Assert - check observable results
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Create_then_get_returns_created_work_item() {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions {
            BaseAddress = new Uri("https://localhost")
        });

        // Unique title - avoids collisions/accidental test overlap
        var title = $"Integration test item (Guid.NewGuid():N)";

        // Act
        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title });

        // Assert - POST contract
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        // read the response back from POST
        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(title, created.Title);
        Assert.Equal("Open", created.Status);
        Assert.NotEqual(default, created.CreatedAtUtc);
        Assert.NotEqual(default, created.UpdatedAtUtc);

        // Act - follow the location returned from POST
        var getResponse = await client.GetAsync(createResponse.Headers.Location!);

        // Assert - GET contract and persistence
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Title, fetched.Title);
        Assert.Equal(created.Status, fetched.Status);
    }
}