
using OpsPortal.Api.Storage;

using Microsoft.EntityFrameworkCore;

namespace OpsPortal.Api.Tests;

public sealed class SqlWorkItemStoreTests
{
    [Fact]
    public async Task Create_with_nonexistent_application_throws_without_persisting_work_item()
    {
        // Arrange
        using var database = new TestDatabase();
        using var context = database.CreateContext();

        var store = new SqlWorkItemStore(context);
        var title = "Work item with a nonexistent application";
        var applicationId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            store.CreateAsync(title, applicationId));

        using var verification = database.CreateContext();
        Assert.Empty(await verification.WorkItems.ToListAsync());
    }

    [Fact]
    public async Task Delete_referenced_application_throws_and_preserves_application_and_work_item()
    {
        // Arrange
        using var database = new TestDatabase();
        var application = ApplicationTestData.Create("22", "Demo CRM");
        Guid workItemId;
        using (var context = database.CreateContext())
        {
            context.Applications.Add(application);
            await context.SaveChangesAsync();

            var store = new SqlWorkItemStore(context);
            var workItem = await store.CreateAsync(
                "WorkItem with linked application",
                application.Id);
            workItemId = workItem.Id;
        }

        // Act
        using var deletionContext = database.CreateContext();

        var applicationToDelete = await deletionContext.Applications
            .SingleAsync(app => app.Id == application.Id);

        deletionContext.Applications.Remove(applicationToDelete);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            deletionContext.SaveChangesAsync());

        using var verification = database.CreateContext();
        var applicationRow = Assert.Single(await verification.Applications.ToListAsync());
        var workItemRow = Assert.Single(await verification.WorkItems.ToListAsync());
        Assert.Equal(application.Id, applicationRow.Id);
        Assert.Equal(workItemId, workItemRow.Id);
        Assert.Equal(application.Id, workItemRow.ApplicationId);
    }

    [Fact]
    public async Task Update_status_preserves_application_association()
    {
        // Arrange
        using var database = new TestDatabase();
        var application = ApplicationTestData.Create("22", "Demo CRM");
        Guid workItemId;
        string title;
        using (var context = database.CreateContext())
        {
            context.Applications.Add(application);
            await context.SaveChangesAsync();

            var store = new SqlWorkItemStore(context);
            var workItem = await store.CreateAsync(
                "WorkItem with linked application",
                application.Id);
            workItemId = workItem.Id;
            title = workItem.Title;
        }

        // Act
        using var updateContext = database.CreateContext();
        var updateStore = new SqlWorkItemStore(updateContext);
        var updated = await updateStore.UpdateAsync(workItemId, null, "Completed");

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(workItemId, updated.Id);
        Assert.Equal(title, updated.Title);
        Assert.Equal("Completed", updated.Status);
        Assert.Equal(application.Id, updated.ApplicationId);

        using var verification = database.CreateContext();
        var row = Assert.Single(await verification.WorkItems.ToListAsync());
        Assert.Equal(workItemId, row.Id);
        Assert.Equal(title, row.Title);
        Assert.Equal("Completed", row.Status);
        Assert.Equal(application.Id, row.ApplicationId);
    }
}