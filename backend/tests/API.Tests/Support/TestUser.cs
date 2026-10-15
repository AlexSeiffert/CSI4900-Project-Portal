using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.Support;

// Test helper: makes a fake logged-in user so controller tests can run without a real token.
public static class TestUser
{
    // A fake user with the given id (and optional role), like one read from a token.
    public static ClaimsPrincipal Principal(Guid id, string? userRole = null)
    {
        var claims = new List<Claim> { new("sub", id.ToString()) };
        if (userRole != null)
        {
            claims.Add(new Claim("user_role", userRole));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    // Makes the controller act as if that user is logged in.
    public static T SignedInAs<T>(this T controller, Guid id) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = Principal(id) }
        };
        return controller;
    }
}
