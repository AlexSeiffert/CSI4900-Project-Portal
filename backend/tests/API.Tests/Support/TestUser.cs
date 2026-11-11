using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.Support;

public static class TestUser
{
    public static ClaimsPrincipal Principal(Guid id, string? userRole = null)
    {
        var claims = new List<Claim> { new("sub", id.ToString()) };
        if (userRole != null)
        {
            claims.Add(new Claim("user_role", userRole));
        }
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    public static T SignedInAs<T>(this T controller, Guid id, string? userRole = null) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = Principal(id, userRole) }
        };
        return controller;
    }
}
