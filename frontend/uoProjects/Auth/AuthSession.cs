using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace uoProjects.Auth;

public sealed class AuthSession(HttpClient http, IConfiguration config, IJSRuntime js)
{
    private const string StorageKey = "projectPortal.session";
    private static readonly string[] Roles = ["student", "supervisor", "admin"];

    private readonly string authUrl = config["Supabase:AuthUrl"]?.TrimEnd('/') ?? "";
    private readonly string publishableKey = config["Supabase:PublishableKey"] ?? "";

    private SignedInUser? user;
    private Task? restore;

    public static string HomeFor(string role)
    {
        return $"/{role}-home";
    }

    public async Task<SignedInUser?> GetUserAsync()
    {
        await (restore ??= RestoreAsync());
        if (user is not null && user.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            await ClearAsync();
        }
        return user;
    }

    private async Task RestoreAsync()
    {
        try
        {
            var saved = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
            user = saved is null ? null : JsonSerializer.Deserialize<SignedInUser>(saved);
        }
        catch (JsonException)
        {
            await ClearAsync();
        }
        catch (JSException)
        {
            user = null;
        }
    }

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
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException or JsonException)
        {
            return "Can't reach the sign-in service. Check your connection and try again.";
        }

        try
        {
            var accessToken = token.GetProperty("access_token").GetString()!;
            var claims = ReadClaims(accessToken);
            var role = claims.TryGetProperty("user_role", out var r) ? r.GetString() : null;
            if (role is null || !Roles.Contains(role))
            {
                return "This account has no role yet. Contact an administrator.";
            }

            var id = Guid.Parse(claims.GetProperty("sub").GetString()!);
            var signedInEmail = claims.TryGetProperty("email", out var e) ? e.GetString() ?? email : email;
            var expires = DateTimeOffset.FromUnixTimeSeconds(claims.GetProperty("exp").GetInt64());
            if (expires <= DateTimeOffset.UtcNow)
            {
                return "The sign-in service returned an expired session.";
            }
            await (restore ??= RestoreAsync());
            user = new SignedInUser(id, signedInEmail, role, signedInEmail, accessToken, expires);
            try
            {
                await js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(user));
            }
            catch (JSException)
            {
                // Keep the session in memory if storage is unavailable.
            }

            return null;
        }
        catch (Exception exception) when (exception is JsonException or FormatException or InvalidOperationException
            or KeyNotFoundException or ArgumentException or IndexOutOfRangeException)
        {
            return "The sign-in service returned an invalid session.";
        }
    }

    public async Task SignOutAsync()
    {
        if (user is not null && authUrl.Length > 0)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{authUrl}/logout");
                request.Headers.Add("apikey", publishableKey);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
                using var response = await http.SendAsync(request);
            }
            catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException or JsonException)
            {
            }
        }
        await ClearAsync();
    }

    private async Task ClearAsync()
    {
        user = null;
        try
        {
            await js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
        }
        catch (JSException)
        {
            // The in-memory session is already cleared.
        }
    }

    private static JsonElement ReadClaims(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        return JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
    }
}
