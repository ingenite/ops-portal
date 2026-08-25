using Microsoft.EntityFrameworkCore;
using OpsPortal.Api.Data;
using OpsPortal.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("OpsPortalDatabase")
    ?? throw new InvalidOperationException(
        "Connection string 'OpsPortalDatabase' was not found.");

builder.Services.AddDbContext<OpsPortalDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton<IWorkItemStore, InMemoryWorkItemStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose top-level API entry point to test assembly
public partial class Program { }