using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using OpsPortal.Api.Models;

namespace OpsPortal.Api.Tests;

public sealed class WorkItemsApiTests
{
    [Fact]
    // async Task - test can await HTTP without blocking a thread
    public async Task GetAll_returns_ok_with_a_json_array()
    {
        // Arrange - create an app factory/client and prepare input
        // `using var` disposes the factory/client automatically at the end of the test
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // Act - send an HTTP request
        // await - pause test until HTTP response arrives
        var response = await client.GetAsync("/api/work-items");

        // Assert - response
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
    }

    [Fact]
    public async Task GetById_returns_not_found_when_the_work_item_does_not_exist()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // Act - GetById
        var getResponse = await client.GetAsync($"/api/work-items/{Guid.NewGuid():N}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

    }
    [Fact]
    public async Task Create_then_get_returns_created_work_item()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // Unique title - avoids collisions/accidental test overlap
        var title = $"Integration test item {Guid.NewGuid():N}";

        // Act
        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title });

        // Assert - POST contract
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // read the response back from POST
        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(title, created.Title);
        Assert.Equal("Open", created.Status);
        Assert.NotEqual(default, created.CreatedAtUtc);
        Assert.NotEqual(default, created.UpdatedAtUtc);

        // Act - follow the location returned from POST
        var location = Assert.IsType<Uri>(createResponse.Headers.Location);
        var getResponse = await client.GetAsync(location);

        // Assert - GET contract and persistence
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Title, fetched.Title);
        Assert.Equal(created.Status, fetched.Status);
    }
    [Fact]
    public async Task Create_without_title_returns_bad_request()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // Act - POST without title
        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { });

        // Assert - HTTP/400
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
    }

    [Fact]
    public async Task Create_with_valid_application_returns_and_persists_application_association()
    {
        // Arrange
        using var database = new TestDatabase();
        var application = ApplicationTestData.Create("22", "Demo CRM");
        using (var context = database.CreateContext())
        {
            context.Applications.Add(application);
            await context.SaveChangesAsync();
        }

        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database, null, true);

        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Integration test item {Guid.NewGuid():N}";
        var applicationId = application.Id;

        // Act
        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title, applicationId });

        // Assert - Work Item is created
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();
        Assert.NotNull(created);
        Assert.Equal(applicationId, created.ApplicationId);

        var location = Assert.IsType<Uri>(createResponse.Headers.Location);
        var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItem>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(applicationId, fetched.ApplicationId);

        using var verification = database.CreateContext();
        var row = Assert.Single(await verification.WorkItems.ToListAsync());

        Assert.Equal(fetched.Id, row.Id);
        Assert.Equal(applicationId, row.ApplicationId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Create_without_application_returns_and_persists_unassociated_work_item(
        bool includeExplicitNull)
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database, null, true);
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var requestBody = new Dictionary<string, object?>
        {
            ["title"] = $"Integration test item {Guid.NewGuid():N}"
        };

        if (includeExplicitNull)
        {
            requestBody["applicationId"] = null;
        }

        // Act
        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            requestBody);

        // Assert - Work Item is created
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();
        Assert.NotNull(created);
        Assert.Null(created.ApplicationId);

        using var verification = database.CreateContext();
        var row = Assert.Single(await verification.WorkItems.ToListAsync());

        Assert.Equal(created.Id, row.Id);
        Assert.Null(row.ApplicationId);
    }

    [Fact]
    public async Task Create_with_nonexistent_application_returns_bad_request_without_persisting_work_item()
    {
        using var database = new TestDatabase();
        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database, null, true);
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Integration test item {Guid.NewGuid():N}";
        var applicationId = Guid.NewGuid();

        // Act
        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title, applicationId });

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);

        using var verification = database.CreateContext();
        Assert.Empty(await verification.WorkItems.ToListAsync());
    }

    [Fact]
    public async Task Create_with_application_from_another_workspace_returns_bad_request_without_persisting_work_item()
    {
        using var database = new TestDatabase();
        var application = ApplicationTestData.Create("22", "Demo CRM");
        application.WorkspaceId = "other-workspace-id";
        using (var context = database.CreateContext())
        {
            context.Applications.Add(application);
            await context.SaveChangesAsync();
        }

        using var factory = new OpsPortalApiFactory();
        using var applicationFactory = factory.WithDatabase(database, null, true);
        using var client = applicationFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Integration test item {Guid.NewGuid():N}";
        var applicationId = application.Id;

        // Act
        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title, applicationId });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);

        using var verification = database.CreateContext();
        Assert.Empty(await verification.WorkItems.ToListAsync());
    }

    [Fact]
    public async Task Update_status_then_get_returns_persisted_changes()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Work Item to update {Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(created);

        // Act - PATCH
        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/work-items/{created.Id}",
            new { status = "Completed" });

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        // Assert - PATCH results
        var updated = await patchResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(title, updated.Title);
        Assert.Equal("Completed", updated.Status);

        // Assert - change persisted
        var getResponse = await client.GetAsync($"/api/work-items/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(fetched);
        Assert.Equal(created.Title, fetched.Title);
        Assert.Equal("Completed", fetched.Status);
    }

    [Fact]
    public async Task Update_title_does_not_modify_status()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Work Item to update {Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(created);

        // Act - PATCH
        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/work-items/{created.Id}",
            new { title = "Updated Title" });

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        // Assert - Title is updated, Status is not updated
        var updated = await patchResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.NotEqual(title, updated.Title);
        Assert.Equal(created.Status, updated.Status);
    }

    [Fact]
    public async Task Update_with_no_fields_returns_bad_request()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // Act - PATCH contains empty body
        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/work-items/{Guid.NewGuid():N}",
            new { });

        // Assert - HTTP/400 Bad Request
        Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);
    }
    [Fact]
    public async Task Update_status_fails_validation_returns_bad_request()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Update Status Work Item {Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(created);

        // Act - PATCH unsupported Status value
        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/work-items/{created.Id}",
            new { status = "NotStarted" });

        // Assert - HTTP/400
        Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);
    }
    [Fact]
    public async Task Update_returns_not_found_when_the_work_item_does_not_exist()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // Act - PATCH with invalid or nonexistent id
        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/work-items/{Guid.NewGuid():N}",
            new { status = "Completed" });

        // Assert - HTTP/404 Not Found
        Assert.Equal(HttpStatusCode.NotFound, patchResponse.StatusCode);
    }

    [Theory]
    [InlineData("title", "")]
    [InlineData("title", "  ")]
    [InlineData("status", "")]
    [InlineData("status", "  ")]
    public async Task Update_returns_bad_request_when_empty_or_whitespace_value_provided(
        string field,
        string invalidValue
    )
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Update Status Work Item {Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title });

        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(created);

        // Act - PATCH Title with whitespace value
        var patchBody = new Dictionary<string, string>
        {
            [field] = invalidValue
        };

        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/work-items/{created.Id}",
            patchBody);

        // Assert - HTTP/400
        Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/work-items/{created.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<WorkItem>();

        // Assert fields are not updated
        Assert.NotNull(fetched);
        Assert.Equal(created.Title, fetched.Title);
        Assert.Equal(created.Status, fetched.Status);
        Assert.Equal(created.UpdatedAtUtc, fetched.UpdatedAtUtc);
    }

    [Fact]
    public async Task Create_work_item_then_Delete_then_getById_returns_not_found()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var title = $"Integration test item {Guid.NewGuid():N}";

        var createResponse = await client.PostAsJsonAsync(
            "/api/work-items",
            new { title });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<WorkItem>();

        Assert.NotNull(created);

        // Act
        var deleteResponse = await client.DeleteAsync(
            $"/api/work-items/{created.Id}");

        // Assert - Delete - HTTP/204 No Content
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Get with same id - HTTP/404 Not Found
        var getResponse = await client.GetAsync($"/api/work-items/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
    [Fact]
    public async Task Delete_returns_not_found_when_the_work_item_does_not_exist()
    {
        // Arrange
        using var factory = new OpsPortalApiFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        client.DefaultRequestHeaders.Add("X-Test-User", "Test admin");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/work-items/{Guid.NewGuid():N}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }
}