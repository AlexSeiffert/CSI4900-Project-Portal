using System.Security.Claims;

namespace API.Auth;

// Helpers for reading who is calling the API from their login token.
// A token carries "claims", which are facts about the user. Inside a controller, User holds them.
public static class ClaimsPrincipalExtensions
{
    // "sub" is the user's id. Supabase sets it when the user signs up, and it is the same id
    // used in the users and profile tables in the database.
    public const string SubjectClaim = "sub";

    // "user_role" is student, supervisor or admin. The login hook in db/migrations adds it to the token.
    public const string UserRoleClaim = "user_role";

    // Returns the user's id, or null if the token has no valid id.
    public static Guid? GetUserId(this ClaimsPrincipal? user)
    {
        return Guid.TryParse(user?.FindFirstValue(SubjectClaim), out var id) ? id : null;
    }

    // Same as GetUserId, but throws an error instead of returning null.
    // Use it where a user must be logged in, such as when creating a project.
    public static Guid GetRequiredUserId(this ClaimsPrincipal? user)
    {
        return user.GetUserId() ?? throw new InvalidOperationException("The authenticated token has no valid 'sub' claim.");
    }
}
