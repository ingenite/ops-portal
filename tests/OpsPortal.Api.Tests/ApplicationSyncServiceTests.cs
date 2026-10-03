using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Models;
using OpsPortal.Api.Synchronization;

namespace OpsPortal.Api.Tests;

public sealed class ApplicationSyncServiceTests
{
    private const string WorkspaceId = "demo-workspace";
    private static readonly DateTimeOffset OldSync = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Sync_inserts_mapped_rows_with_one_utc_timestamp()
    {
        using var database = new TestDatabase();
        var before = DateTimeOffset.UtcNow;

        await Sync(database, new ProviderFake(Asset("22"), Asset("23")));

        using var verification = database.CreateContext();
        var rows = await verification.Applications.OrderBy(app => app.ObjectId).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(2, rows.Select(app => app.Id).Distinct().Count());
        Assert.All(rows, row =>
        {
            Assert.NotEqual(Guid.Empty, row.Id);
            Assert.Equal(WorkspaceId, row.WorkspaceId);
            Assert.Equal($"APM-{row.ObjectId}", row.Key);
            Assert.Equal("Demo CRM", row.Name);
            Assert.Equal("Cloud", row.DeploymentMethod);
            Assert.Equal("Test", row.Status);
            Assert.Equal(TimeSpan.Zero, row.LastSyncedAtUtc.Offset);
            Assert.InRange(row.LastSyncedAtUtc, before, DateTimeOffset.UtcNow);
        });
        Assert.Equal(new[] { "22", "23" }, rows.Select(app => app.ObjectId));
        Assert.Equal(rows[0].LastSyncedAtUtc, rows[1].LastSyncedAtUtc);
    }

    [Fact]
    public async Task Repeated_sync_preserves_local_identity_and_refreshes_timestamp()
    {
        using var database = new TestDatabase();
        var provider = new ProviderFake(Asset("22"));
        await Sync(database, provider);
        Guid originalId;
        using (var context = database.CreateContext())
        {
            var row = await context.Applications.SingleAsync();
            originalId = row.Id;
            // Avoid sleeps and clock-resolution assumptions when checking freshness.
            row.LastSyncedAtUtc = OldSync;
            await context.SaveChangesAsync();
        }

        await Sync(database, provider);

        using var verification = database.CreateContext();
        var actual = Assert.Single(await verification.Applications.ToListAsync());
        Assert.Equal(originalId, actual.Id);
        Assert.Equal("APM-22", actual.Key);
        Assert.True(actual.LastSyncedAtUtc > OldSync);
    }

    [Theory]
    [InlineData("Hybrid", "Production")]
    [InlineData(null, null)]
    public async Task Sync_updates_descriptive_fields_including_cleared_values(
        string? deploymentMethod, string? status)
    {
        using var database = new TestDatabase();
        var original = LocalApplication("22");
        await Seed(database, original);

        await Sync(database, new ProviderFake(Asset("22", "Renamed CRM", deploymentMethod, status)));

        using var verification = database.CreateContext();
        var actual = Assert.Single(await verification.Applications.ToListAsync());
        Assert.Equal(original.Id, actual.Id);
        Assert.Equal(original.WorkspaceId, actual.WorkspaceId);
        Assert.Equal(original.ObjectId, actual.ObjectId);
        Assert.Equal(original.Key, actual.Key);
        Assert.Equal("Renamed CRM", actual.Name);
        Assert.Equal(deploymentMethod, actual.DeploymentMethod);
        Assert.Equal(status, actual.Status);
        Assert.True(actual.LastSyncedAtUtc > OldSync);
    }

    [Fact]
    public async Task Sync_does_not_match_same_object_id_in_another_workspace()
    {
        using var database = new TestDatabase();
        var other = LocalApplication("22", "other-workspace");
        await Seed(database, other);

        await Sync(database, new ProviderFake(Asset("22")));

        using var verification = database.CreateContext();
        var rows = await verification.Applications.ToListAsync();
        Assert.Equal(2, rows.Count);
        var untouched = Assert.Single(rows, row => row.WorkspaceId == other.WorkspaceId);
        Assert.Equal(other.Id, untouched.Id);
        Assert.Equal(other.Name, untouched.Name);
        Assert.Equal(OldSync, untouched.LastSyncedAtUtc);
        var imported = Assert.Single(rows, row => row.WorkspaceId == WorkspaceId);
        Assert.NotEqual(other.Id, imported.Id);
        Assert.Equal("22", imported.ObjectId);
        Assert.Equal("Demo CRM", imported.Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Sync_preserves_rows_absent_from_empty_or_partial_response(bool includeIncoming)
    {
        using var database = new TestDatabase();
        var original = LocalApplication("99");
        await Seed(database, original);
        var provider = includeIncoming ? new ProviderFake(Asset("22")) : new ProviderFake();

        await Sync(database, provider);

        using var verification = database.CreateContext();
        var rows = await verification.Applications.ToListAsync();
        Assert.Equal(includeIncoming ? 2 : 1, rows.Count);
        var actual = Assert.Single(rows, row => row.ObjectId == "99");
        Assert.Equal(original.Id, actual.Id);
        Assert.Equal(original.Name, actual.Name);
        Assert.Equal(original.DeploymentMethod, actual.DeploymentMethod);
        Assert.Equal(original.Status, actual.Status);
        Assert.Equal(OldSync, actual.LastSyncedAtUtc);
    }

    [Fact]
    public async Task Provider_failure_leaves_persisted_data_unchanged()
    {
        using var database = new TestDatabase();
        var original = LocalApplication("22");
        await Seed(database, original);
        var failure = new HttpRequestException("Fake provider failure");

        var thrown = await Assert.ThrowsAsync<HttpRequestException>(() =>
            Sync(database, new ProviderFake { Failure = failure }));

        Assert.Same(failure, thrown);
        using var verification = database.CreateContext();
        var actual = Assert.Single(await verification.Applications.ToListAsync());
        Assert.Equal(original.Id, actual.Id);
        Assert.Equal(original.Name, actual.Name);
        Assert.Equal(original.DeploymentMethod, actual.DeploymentMethod);
        Assert.Equal(original.Status, actual.Status);
        Assert.Equal(OldSync, actual.LastSyncedAtUtc);
    }

    [Fact]
    public async Task Duplicate_source_ids_in_one_batch_create_only_one_row()
    {
        using var database = new TestDatabase();

        await Sync(database, new ProviderFake(Asset("22"), Asset("22")));

        using var verification = database.CreateContext();
        var actual = Assert.Single(await verification.Applications.ToListAsync());
        Assert.Equal("22", actual.ObjectId);
    }

    [Fact]
    public async Task Cancellation_reaches_provider_and_leaves_database_unchanged()
    {
        using var database = new TestDatabase();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var provider = new ProviderFake(Asset("22"));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Sync(database, provider, cancellation.Token));

        Assert.Equal(cancellation.Token, provider.ReceivedToken);
        using var verification = database.CreateContext();
        Assert.Empty(await verification.Applications.ToListAsync());
    }

    private static async Task Sync(TestDatabase database, IAtlassianAssetsClient provider,
        CancellationToken cancellationToken = default)
    {
        using var context = database.CreateContext();
        var service = new ApplicationSyncService(provider,
            Options.Create(new AtlassianOptions { WorkspaceId = WorkspaceId }), context);
        await service.SyncAsync(cancellationToken);
    }

    private static async Task Seed(TestDatabase database, Application application)
    {
        using var context = database.CreateContext();
        context.Applications.Add(application);
        await context.SaveChangesAsync();
    }

    private static Application LocalApplication(string objectId, string workspaceId = WorkspaceId) => new()
    {
        Id = Guid.NewGuid(),
        WorkspaceId = workspaceId,
        ObjectId = objectId,
        Key = $"APM-{objectId}",
        Name = "Old CRM",
        DeploymentMethod = "On-premises",
        Status = "Old status",
        LastSyncedAtUtc = OldSync
    };

    private static AtlassianAssetObject Asset(string id, string name = "Demo CRM",
        string? deploymentMethod = "Cloud", string? status = "Test") => new()
        {
            Id = id,
            ObjectKey = $"APM-{id}",
            Label = name,
            Attributes = [
            new() { ObjectTypeAttributeId = "594", ObjectAttributeValues =
                deploymentMethod is null ? [] : [new() { DisplayValue = deploymentMethod }] },
            new() { ObjectTypeAttributeId = "597", ObjectAttributeValues =
                status is null ? [] : [new() { DisplayValue = status }] }
        ]
        };

    private sealed class ProviderFake(params AtlassianAssetObject[] assets) : IAtlassianAssetsClient
    {
        public Exception? Failure { get; init; }
        public CancellationToken ReceivedToken { get; private set; }

        public Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(CancellationToken cancellationToken)
        {
            ReceivedToken = cancellationToken;
            cancellationToken.ThrowIfCancellationRequested();
            return Failure is null
                ? Task.FromResult(new AtlassianAssetsQueryResponse { Values = assets.ToList() })
                : Task.FromException<AtlassianAssetsQueryResponse>(Failure);
        }
    }
}