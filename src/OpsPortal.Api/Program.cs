using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using OpsPortal.Api.Data;
using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Reporting;
using OpsPortal.Api.Storage;
using OpsPortal.Api.Synchronization;

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

builder.Services.AddScoped<IWorkItemStore, SqlWorkItemStore>();
builder.Services.AddScoped<WorkItemReportQuery>();
builder.Services.AddScoped<ApplicationSyncService>();
builder.Services.AddScoped<ApplicationQuery>();

// Coordinate manual and background application sync
builder.Services.AddSingleton<ApplicationSyncCoordinator>();

// Integrations.Atlassian
builder.Services.AddOptions<AtlassianOptions>()
    .Bind(builder.Configuration.GetSection("Atlassian"))
    .Validate(options => !string.IsNullOrWhiteSpace(options.CloudId),
        "Atlassian:CloudId is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.WorkspaceId),
        "Atlassian:WorkspaceId is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Email),
        "Atlassian:Email is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApiToken),
        "Atlassian:ApiToken is required")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ApplicationsAqlQuery),
        "Atlassian:ApplicationsAqlQuery is required")
    .Validate(options => options.ApplicationsMaxResults > 0,
        "Atlassian:ApplicationsMaxResults must be greater than zero")
    .ValidateOnStart();

builder.Services.AddHttpClient<IAtlassianAssetsClient, AtlassianAssetsClient>(
    (services, client) => {
        var options = services
            .GetRequiredService<IOptions<AtlassianOptions>>()
            .Value;

        client.BaseAddress = new Uri("https://api.atlassian.com/");
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{options.Email}:{options.ApiToken}"));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    });

// Worker Service
if (builder.Configuration.GetValue<bool>("ApplicationSync:Enabled")) {
    builder.Services.AddHostedService<ApplicationSyncWorker>();
}

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(jwtOptions =>
{
	jwtOptions.Authority = builder.Configuration.GetValue<string>("Api:Authority");
	jwtOptions.Audience = builder.Configuration.GetValue<string>("Api:Audience");
    if (builder.Environment.IsDevelopment()) jwtOptions.RequireHttpsMetadata = false;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.MapOpenApi();
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/openapi/v1.json", "OpsPortal API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose top-level API entry point to test assembly
public partial class Program { }
