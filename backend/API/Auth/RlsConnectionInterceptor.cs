using System.Data.Common;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace API.Auth;

// Tells the database who is calling, so its row level security rules (db/migrations) apply to that user.
// Each time the API opens a database connection, this runs two settings on it:
//   - role = authenticated: the connection acts as a normal logged-in user, not an admin.
//   - request.jwt.claims: the caller's id and role, which the database reads with auth.uid().
// This only works with a direct or session-mode connection (port 5432). A transaction-mode
// pooler (port 6543) shares connections between requests, so the user's identity would be lost.
public class RlsConnectionInterceptor(IHttpContextAccessor accessor) : DbConnectionInterceptor
{
    private const string Sql = "select set_config('role', 'authenticated', false), set_config('request.jwt.claims', @claims, false)";

    // Runs right after a connection is opened (synchronous version).
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        if (CreateCommand(connection) is { } command)
        {
            using (command)
            {
                command.ExecuteNonQuery();
            }
        }
    }

    // Same as above for the async version, which is the one the API normally uses.
    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        if (CreateCommand(connection) is { } command)
        {
            await using (command)
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
    }

    // Builds the command that sets the role and claims, or returns null if there is nothing to set.
    private NpgsqlCommand? CreateCommand(DbConnection connection)
    {
        var claims = BuildClaimsJson(accessor.HttpContext?.User);
        if (claims == null || connection is not NpgsqlConnection npgsql)
        {
            // No logged-in user (for example, startup work like seeding): leave the connection as it is.
            return null;
        }
        var command = npgsql.CreateCommand();
        command.CommandText = Sql;
        command.Parameters.AddWithValue("claims", claims);
        return command;
    }

    // Turns the caller's token info into the JSON text the database expects. Returns null if not logged in.
    internal static string? BuildClaimsJson(System.Security.Claims.ClaimsPrincipal? user)
    {
        if (user?.Identity?.IsAuthenticated != true || user.GetUserId() is not { } id)
        {
            return null;
        }
        var claims = new Dictionary<string, string>
        {
            [ClaimsPrincipalExtensions.SubjectClaim] = id.ToString(),
            ["role"] = "authenticated"
        };
        foreach (var name in new[] { "email", ClaimsPrincipalExtensions.UserRoleClaim })
        {
            if (user.FindFirst(name)?.Value is { Length: > 0 } value)
            {
                claims[name] = value;
            }
        }
        return JsonSerializer.Serialize(claims);
    }
}
