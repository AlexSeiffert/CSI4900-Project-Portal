using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace API.Auth;

// Lets you use the API in Development without logging in to Supabase.
// Send a request with an X-Dev-User-Id header (a user id), and the API treats you as that user.
// X-Dev-User-Role is optional and sets the role, for example "student" or "supervisor".
// This is only turned on in Development (see Program.cs), so it can't be used in Production.
public class DevelopmentAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Development";
    public const string UserIdHeader = "X-Dev-User-Id";
    public const string UserRoleHeader = "X-Dev-User-Role";

    // Runs on each request. Builds a user from the headers if the id is valid.
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // No valid id in the header: do nothing, so the request is treated as not logged in.
        if (!Guid.TryParse(Request.Headers[UserIdHeader], out var id))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }
        var claims = new List<Claim> { new(ClaimsPrincipalExtensions.SubjectClaim, id.ToString()) };
        if (Request.Headers[UserRoleHeader].ToString() is { Length: > 0 } role)
        {
            claims.Add(new Claim(ClaimsPrincipalExtensions.UserRoleClaim, role));
        }
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
