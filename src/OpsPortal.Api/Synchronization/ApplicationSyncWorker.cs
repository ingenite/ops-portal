namespace OpsPortal.Api.Synchronization;
public sealed class ApplicationSyncWorker : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApplicationSyncWorker> _logger;
    private readonly ApplicationSyncCoordinator _coordinator;

    public ApplicationSyncWorker(IServiceScopeFactory scopeFactory,
        ILogger<ApplicationSyncWorker> logger,
        ApplicationSyncCoordinator coordinator) {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _coordinator = coordinator;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        do {
            var entered = await _coordinator.TryEnterAsync(stoppingToken);

            if (!entered) continue;
            try {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider
                    .GetRequiredService<ApplicationSyncService>();

                await syncService.SyncAsync(stoppingToken);
            } catch (HttpRequestException ex) {
                _logger.LogError(ex, "Application Sync Worker could not retrieve data.");
            } finally {
                _coordinator.Exit();
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
