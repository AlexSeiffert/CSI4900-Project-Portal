using System.Security.Claims;
using System.Text.Json;
using API.Auth;
using API.Tests.Support;

namespace API.Tests.Auth;

// Tests for reading the user from a token and for building the claims sent to the database.
public class AuthTests
{
    [Fact]
    public void GetUserId_ReadsSubClaim()
    {
        var id = Guid.NewGuid();

        Assert.Equal(id, TestUser.Principal(id).GetUserId());
    }

    [Fact]
    public void GetUserId_ReturnsNullWithoutValidSub()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "not-a-guid")], "test"));

        Assert.Null(principal.GetUserId());
        Assert.Null(((ClaimsPrincipal?)null).GetUserId());
        Assert.Throws<InvalidOperationException>(() => principal.GetRequiredUserId());
    }

    [Fact]
    public void BuildClaimsJson_UsesAuthenticatedRoleAndSubject()
    {
        var id = Guid.NewGuid();

        var json = RlsConnectionInterceptor.BuildClaimsJson(TestUser.Principal(id, "supervisor"));

        using var document = JsonDocument.Parse(json!);
        Assert.Equal(id.ToString(), document.RootElement.GetProperty("sub").GetString());
        Assert.Equal("authenticated", document.RootElement.GetProperty("role").GetString());
        Assert.Equal("supervisor", document.RootElement.GetProperty("user_role").GetString());
    }

    [Fact]
    public void BuildClaimsJson_ReturnsNullForAnonymousCaller()
    {
        Assert.Null(RlsConnectionInterceptor.BuildClaimsJson(new ClaimsPrincipal(new ClaimsIdentity())));
        Assert.Null(RlsConnectionInterceptor.BuildClaimsJson(null));
    }
}
