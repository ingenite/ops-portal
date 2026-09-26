using OpsPortal.Api.Synchronization;

namespace OpsPortal.Api.Tests;

public sealed class ApplicationSyncCoordinatorTests {

    [Fact]
    public async Task Acquire_gate_rejects_second_entrant_and_allows_entry_after_release() {
        var coordinator = new ApplicationSyncCoordinator();
        var firstEntry = await coordinator.TryEnterAsync(CancellationToken.None);
        Assert.True(firstEntry);

        var secondEntry = await coordinator.TryEnterAsync(CancellationToken.None);
        Assert.False(secondEntry);

        coordinator.Exit();
        var thirdEntry = await coordinator.TryEnterAsync(CancellationToken.None);
        Assert.True(thirdEntry);

        coordinator.Exit();
    }
}
