using System.Data.Common;
using System.Text.Json;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

using OpsPortal.Api.Data;
using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Synchronization;

namespace OpsPortal.Api.Tests;

public sealed class ApplicationSyncWorkerTests
{
    public enum FailureCase
    {
        HttpRequestException, JsonException, OperationCanceledException,
        DbException, DbUpdateException
    }

    [Theory]
    [InlineData(FailureCase.HttpRequestException)]
    [InlineData(FailureCase.JsonException)]
    [InlineData(FailureCase.OperationCanceledException)]
    [InlineData(FailureCase.DbException)]
    [InlineData(FailureCase.DbUpdateException)]
    public async Task Worker_recovers_after_recoverable_failure(FailureCase failureCase)
    {
        // Arrange
        using var database = new TestDatabase();

        var message = "Invalid provider response";
        Exception providerException = failureCase switch
        {
            FailureCase.HttpRequestException => new HttpRequestException(message),
            FailureCase.JsonException => new JsonException(message),
            FailureCase.OperationCanceledException => new OperationCanceledException(message),
            FailureCase.DbException => new SqliteException(message, 0),
            FailureCase.DbUpdateException => new DbUpdateException(message),
            _ => throw new ArgumentOutOfRangeException()
        };

        var response = new AtlassianAssetsQueryResponse
        {
            Values = [ApplicationSyncServiceTests.Asset("22")]
        };

        var provider = new FailOnceProvider(providerException, response);
        using var serviceProvider = CreateServices(database, provider);

        var logger = new FailureLogger();
        var coordinator = new ApplicationSyncCoordinator();
        var timeProvider = new FakeTimeProvider();

        using var worker = new ApplicationSyncWorker(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger, coordinator, timeProvider);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act
        try
        {
            // Initial run fails
            await worker.StartAsync(CancellationToken.None);

            var loggedException = await logger.FailureLogged
                .WaitAsync(timeout.Token);

            Assert.Same(providerException, loggedException);
            Assert.Equal(1, provider.CallCount);

            // Fast-forward time, trigger next run
            timeProvider.Advance(TimeSpan.FromMinutes(5));

            await provider.SecondCall.WaitAsync(timeout.Token);

            // provider call happens before db work finishes
            // Wait for the worker to release the gate before inspecting data
            while (!await coordinator.TryEnterAsync(timeout.Token))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
            }

            try
            {
                // Assert - scheduled retry persisted returned application
                Assert.Equal(2, provider.CallCount);

                using var verification = database.CreateContext();
                var actual = Assert.Single(
                    await verification.Applications.ToListAsync(timeout.Token));

                Assert.Equal("demo-workspace", actual.WorkspaceId);
                Assert.Equal("22", actual.ObjectId);
                Assert.Equal("APM-22", actual.Key);
                Assert.Equal("Demo CRM", actual.Name);

                // worker is still running
                Assert.False(worker.ExecuteTask!.IsCompleted);
            }
            finally
            {
                coordinator.Exit();
            }
        }
        finally
        {
            using var shutdownTimeout =
                new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await worker.StopAsync(shutdownTimeout.Token);
        }
    }

    [Fact]
    public async Task Worker_stops_during_sync_and_releases_gate()
    {
        // Arrange
        using var database = new TestDatabase();
        var response = new AtlassianAssetsQueryResponse();
        var provider = new HeldGateProvider(response);
        using var serviceProvider = CreateServices(database, provider);

        var logger = new FailureLogger();
        var coordinator = new ApplicationSyncCoordinator();
        var timeProvider = new FakeTimeProvider();
        using var worker = new ApplicationSyncWorker(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            logger, coordinator, timeProvider);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act - start worker and await 'Started'
        try
        {
            await worker.StartAsync(CancellationToken.None);
            await provider.Started.WaitAsync(timeout.Token);

            // worker is still running
            Assert.False(worker.ExecuteTask!.IsCompleted);

            // gate is held
            Assert.False(await coordinator.TryEnterAsync(timeout.Token));

            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await worker.StopAsync(shutdown.Token);

            Assert.NotNull(worker.ExecuteTask);
            Assert.Equal(TaskStatus.RanToCompletion, worker.ExecuteTask.Status);

            // Gate is available after shutdown
            Assert.True(await coordinator.TryEnterAsync(timeout.Token));

            try
            {
                // No logged failures
                Assert.False(logger.FailureLogged.IsCompleted);
            }
            finally
            {
                coordinator.Exit();
            }
        }
        finally
        {
            using var shutdownTimeout =
                new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await worker.StopAsync(shutdownTimeout.Token);
        }
    }
    private class FailOnceProvider : IAtlassianAssetsClient
    {
        private readonly Exception _exception;
        private readonly AtlassianAssetsQueryResponse _response;
        private readonly TaskCompletionSource<bool> _secondCall =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }
        public Task SecondCall => _secondCall.Task;

        public FailOnceProvider(Exception exception,
            AtlassianAssetsQueryResponse response)
        {
            _exception = exception;
            _response = response;
        }

        public Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(
            CancellationToken cancellationToken)
        {
            CallCount++;
            cancellationToken.ThrowIfCancellationRequested();

            if (CallCount == 1)
            {
                return Task.FromException<AtlassianAssetsQueryResponse>(_exception);
            }
            else
            {
                _secondCall.TrySetResult(true);

                return Task.FromResult(_response);
            }
        }
    }

    private class HeldGateProvider : IAtlassianAssetsClient
    {
        private readonly AtlassianAssetsQueryResponse _response;
        private readonly TaskCompletionSource<bool> _started =
           new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Started => _started.Task;

        public HeldGateProvider(AtlassianAssetsQueryResponse response)
        {
            _response = response;
        }

        public async Task<AtlassianAssetsQueryResponse> GetApplicationsAsync(CancellationToken cancellationToken)
        {
            _started.TrySetResult(true);

            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

            return await Task.FromResult(_response);
        }
    }
    private static ServiceProvider CreateServices(
            TestDatabase testDatabase,
            IAtlassianAssetsClient provider)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IAtlassianAssetsClient>(provider);
        services.AddScoped<OpsPortalDbContext>(_ =>
            testDatabase.CreateContext());
        services.AddScoped<ApplicationSyncService>();
        services.Configure<AtlassianOptions>(options =>
            options.WorkspaceId = "demo-workspace");

        return services.BuildServiceProvider();
    }

    private class FailureLogger : ILogger<ApplicationSyncWorker>
    {
        private readonly TaskCompletionSource<Exception> _failure =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<Exception> FailureLogged
        {
            get { return _failure.Task; }
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Error && exception is not null)
            {
                _failure.TrySetResult(exception);
            }
        }
    }
}