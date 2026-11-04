using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace ProjectPortal.Web.Auth;

/// <summary>
/// The signed-in user. Signs in against Supabase Auth (the API checks the same token), then looks up
/// the user's display name from the API. Kept in localStorage so a page refresh keeps the session.
/// </summary>
public sealed class AuthSession(HttpClient http, IConfiguration config, IJSRuntime js)
{
    private const string StorageKey = "projectPortal.session";
    private static readonly string[] Roles = ["student", "supervisor", "admin"];

    private readonly string authUrl = config["Supabase:AuthUrl"]?.TrimEnd('/') ?? "";
    private readonly string publishableKey = config["Supabase:PublishableKey"] ?? "";
    private readonly string apiUrl = config["Api:BaseUrl"]?.TrimEnd('/') ?? "";

    private SignedInUser? user;
    private bool restored;

    /// <summary>The page each role lands on after signing in.</summary>
    public static string HomeFor(string role) => $"/{role}-home";

    /// <summary>The signed-in user, or null when signed out or the session has expired.</summary>
    public async Task<SignedInUser?> GetUserAsync()
    {
        if (!restored)
        {
            restored = true;
            var saved = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            user = saved is null ? null : JsonSerializer.Deserialize<SignedInUser>(saved);
        }
        // Tokens last an hour; after that the API rejects them, so treat the session as over.
        if (user is not null && user.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await ClearAsync();
        }
        return user;
    }

    /// <summary>Signs in. Returns null on success, otherwise a message to show the user.</summary>
    public async Task<string?> SignInAsync(string email, string password)
    {
        if (authUrl.Length == 0 || publishableKey.Length == 0)
        {
            return "Sign-in isn't configured. Set Supabase:AuthUrl and Supabase:PublishableKey.";
        }

        JsonElement token;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{authUrl}/token?grant_type=password")
            {
                Content = JsonContent.Create(new { email, password })
            };
            request.Headers.Add("apikey", publishableKey);
            using var response = await http.SendAsync(request);
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
            {
                return "Incorrect email or password.";
            }
            if (!response.IsSuccessStatusCode)
            {
                return "Sign-in is unavailable right now. Try again later.";
            }
            token = await response.Content.ReadFromJsonAsync<JsonElement>();
        }
        catch (HttpRequestException)
        {
            return "Can't reach the sign-in service. Check your connection and try again.";
        }

        var accessToken = token.GetProperty("access_token").GetString()!;
        var claims = ReadClaims(accessToken);
        var role = claims.TryGetProperty("user_role", out var r) ? r.GetString() : null;
        if (role is null || !Roles.Contains(role))
        {
            return "This account has no role yet. Contact an administrator.";
        }

        var id = Guid.Parse(claims.GetProperty("sub").GetString()!);
        var signedInEmail = claims.TryGetProperty("email", out var e) ? e.GetString() ?? email : email;
        user = new SignedInUser(
            id,
            signedInEmail,
            role,
            await LookUpNameAsync(id, role, accessToken) ?? signedInEmail,
            accessToken,
            DateTimeOffset.FromUnixTimeSeconds(claims.GetProperty("exp").GetInt64()));
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(user));
        return null;
    }

    public async Task SignOutAsync()
    {
        if (user is not null && authUrl.Length > 0)
        {
            try
            {
                // Ends the session on Supabase too. The local sign-out below happens either way.
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{authUrl}/logout");
                request.Headers.Add("apikey", publishableKey);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
                await http.SendAsync(request);
            }
            catch (HttpRequestException)
            {
            }
        }
        await ClearAsync();
    }

    // Students and supervisors have a profile with a display name. Admins don't, so they get null.
    private async Task<string?> LookUpNameAsync(Guid id, string role, string accessToken)
    {
        if (role == "admin" || apiUrl.Length == 0)
        {
            return null;
        }
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}/api/v1/{role}s/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
            return profile.TryGetProperty("displayName", out var name) ? name.GetString() : null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private async Task ClearAsync()
    {
        user = null;
        await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }

    // The middle part of a JWT is base64url-encoded JSON. Supabase already verified it; this only reads it.
    private static JsonElement ReadClaims(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        return JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
    }
}

public sealed record SignedInUser(Guid Id, string Email, string Role, string Name, string AccessToken, DateTimeOffset ExpiresAt);
