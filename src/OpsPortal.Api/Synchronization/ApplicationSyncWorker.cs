using System.Data.Common;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

namespace OpsPortal.Api.Synchronization;

public sealed class ApplicationSyncWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApplicationSyncWorker> _logger;
    private readonly ApplicationSyncCoordinator _coordinator;
    private readonly TimeProvider _timeProvider;

    public ApplicationSyncWorker(IServiceScopeFactory scopeFactory,
        ILogger<ApplicationSyncWorker> logger,
        ApplicationSyncCoordinator coordinator,
        TimeProvider timeProvider)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _coordinator = coordinator;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5), _timeProvider);

        do
        {
            var entered = await _coordinator.TryEnterAsync(stoppingToken);

            if (!entered) continue;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var syncService = scope.ServiceProvider
                    .GetRequiredService<ApplicationSyncService>();

                await syncService.SyncAsync(stoppingToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Application Sync Worker could not retrieve data.");
            }
            // Cover cases when response cannot be deserialized and/or is malformed
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Provider response could not be read.");
            }
            // Allow host shutdown requested during sync
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            // Cover cases like HTTP timeouts
            catch (OperationCanceledException ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Application synchronization was canceled before completion.");
            }
            // Common (SQL) database errors or EF Core failures while saving
            catch (Exception ex) when (ex is DbException or DbUpdateException)
            {
                _logger.LogError(ex, "Application synchronization could not read or save application data");
            }
            finally
            {
                _coordinator.Exit();
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}