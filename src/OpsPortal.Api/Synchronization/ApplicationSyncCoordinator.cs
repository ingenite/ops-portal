namespace OpsPortal.Api.Synchronization;

public sealed class ApplicationSyncCoordinator
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public Task<bool> TryEnterAsync(CancellationToken cancellationToken) =>
        _gate.WaitAsync(0, cancellationToken);

    public void Exit() => _gate.Release();
}