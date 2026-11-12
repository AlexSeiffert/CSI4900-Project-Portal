using System.Security.Claims;

namespace API.Auth;

public static class ClaimsPrincipalExtensions
{
    public const string SubjectClaim = "sub";

    public const string UserRoleClaim = "user_role";

    public static Guid? GetUserId(this ClaimsPrincipal? user)
    {
        return Guid.TryParse(user?.FindFirstValue(SubjectClaim), out var id) ? id : null;
    }

    public static bool IsAdmin(this ClaimsPrincipal? user)
    {
        return user?.FindFirstValue(UserRoleClaim) == "admin";
    }

    public static Guid GetRequiredUserId(this ClaimsPrincipal? user)
    {
        return user.GetUserId() ?? throw new InvalidOperationException("The authenticated token has no valid 'sub' claim.");
    }
}
