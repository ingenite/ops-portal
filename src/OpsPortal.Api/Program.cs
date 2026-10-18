using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

using OpsPortal.Api.Data;
using OpsPortal.Api.Integrations.Atlassian;
using OpsPortal.Api.Reporting;
using OpsPortal.Api.Storage;
using OpsPortal.Api.Synchronization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

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
// Allow for tests to adjust time via TimeProvider
builder.Services.AddSingleton<ApplicationSyncCoordinator>();
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

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
    (services, client) =>
    {
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
if (builder.Configuration.GetValue<bool>("ApplicationSync:Enabled"))
{
    builder.Services.AddHostedService<ApplicationSyncWorker>();
}

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(jwtOptions =>
{
    jwtOptions.Authority = builder.Configuration.GetValue<string>("Api:Authority");
    jwtOptions.Audience = builder.Configuration.GetValue<string>("Api:Audience");
    if (builder.Environment.IsDevelopment()) jwtOptions.RequireHttpsMetadata = false;
    jwtOptions.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var realmAccess = context.Principal?.FindFirst("realm_access");

            if (context.Principal?.Identity is not ClaimsIdentity identity || realmAccess is null) return Task.CompletedTask;

            using var document = JsonDocument.Parse(realmAccess.Value);

            if (document.RootElement.TryGetProperty("roles", out var roles))
            {
                foreach (var role in roles.EnumerateArray())
                {
                    var roleName = role.GetString();

                    if (!string.IsNullOrWhiteSpace(roleName))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                    }
                }
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanRead", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Reader", "Admin");
    })
    .AddPolicy("CanWrite", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpsPortal API v1");
        options.OAuthClientId("opsportal-swagger");
        options.OAuthScopes("openid");
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider,
    IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var authority = configuration["Api:Authority"]?.TrimEnd('/')
                ?? throw new InvalidOperationException("Api:Authority is required.");
            var securitySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                },
                ["Keycloak"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(
                                $"{authority}/protocol/openid-connect/auth"),
                            TokenUrl = new Uri(
                                $"{authority}/protocol/openid-connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "Sign in with Keycloak"
                            }
                        }
                    }
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = securitySchemes;

            foreach (var path in document.Paths.Values)
            {
                if (path.Operations is null)
                {
                    continue;
                }

                foreach (var operation in path.Operations.Values)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Keycloak", document)] = ["openid"]
                    });
                }
            }
        }
    }
}