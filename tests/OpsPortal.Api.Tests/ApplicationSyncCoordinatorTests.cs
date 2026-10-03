using OpsPortal.Api.Synchronization;

namespace OpsPortal.Api.Tests;

public sealed class ApplicationSyncCoordinatorTests
{

    [Fact]
    public async Task Acquire_gate_rejects_second_entrant_and_allows_entry_after_release()
    {
        var coordinator = new ApplicationSyncCoordinator();
        Assert.True(await coordinator.TryEnterAsync(CancellationToken.None));
        Assert.False(await coordinator.TryEnterAsync(CancellationToken.None));
        coordinator.Exit();

        Assert.True(await coordinator.TryEnterAsync(CancellationToken.None));
        coordinator.Exit();
    }
}