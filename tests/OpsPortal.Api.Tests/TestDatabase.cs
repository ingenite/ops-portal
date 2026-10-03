using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using OpsPortal.Api.Data;

namespace OpsPortal.Api.Tests;
// One open connection owns one isolated database; every operation can use a fresh context.
internal sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly DbContextOptions<OpsPortalDbContext> _options;

    public TestDatabase()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<OpsPortalDbContext>().UseSqlite(_connection).Options;
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public OpsPortalDbContext CreateContext() => new(_options);
    public void Dispose() => _connection.Dispose();
}