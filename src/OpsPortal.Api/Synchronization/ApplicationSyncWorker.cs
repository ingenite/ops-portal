namespace OpsPortal.Api.Synchronization;
public sealed class ApplicationSyncWorker : BackgroundService {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApplicationSyncWorker> _logger;

    public ApplicationSyncWorker(IServiceScopeFactory scopeFactory,
        ILogger<ApplicationSyncWorker> logger) {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        do {
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider
                .GetRequiredService<ApplicationSyncService>();

            try {
                await syncService.SyncAsync(stoppingToken);
            } catch (HttpRequestException ex) {
                _logger.LogError(ex, "Application Sync Worker could not retrieve data.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
