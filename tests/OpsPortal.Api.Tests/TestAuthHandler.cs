using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace OpsPortal.Api.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var username = Request.Headers["X-Test-User"].ToString();

        if (string.IsNullOrWhiteSpace(username)) {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, username)
        };

        var role = Request.Headers["X-Test-Role"].ToString();

        if (!string.IsNullOrWhiteSpace(role)) {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
