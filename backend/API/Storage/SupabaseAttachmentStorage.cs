using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Exceptions;
using Microsoft.Net.Http.Headers;

namespace API.Storage;

// Signs URLs for the inquiry-attachments bucket (migration 0010) through the Supabase Storage API.
// It sends the caller's own login token, so Storage's row level security decides what they may
// upload or read, the same way the database does for the API's queries.
public class SupabaseAttachmentStorage(HttpClient http, IHttpContextAccessor accessor, IConfiguration config) : IAttachmentStorage
{
    public const string Bucket = "inquiry-attachments";
    private static readonly TimeSpan DownloadLifetime = TimeSpan.FromMinutes(5);

    public async Task<string> CreateUploadUrl(string storagePath, CancellationToken cancellationToken = default)
    {
        var json = await Send($"object/upload/sign/{Bucket}/{storagePath}", new { }, nameof(CreateUploadUrl), cancellationToken);
        return StorageUrl(json.GetProperty("url").GetString()!);
    }

    public async Task<string> CreateDownloadUrl(string storagePath, CancellationToken cancellationToken = default)
    {
        var json = await Send($"object/sign/{Bucket}/{storagePath}", new { expiresIn = (int)DownloadLifetime.TotalSeconds },
            nameof(CreateDownloadUrl), cancellationToken);
        return StorageUrl(json.GetProperty("signedURL").GetString()!);
    }

    private async Task<JsonElement> Send(string path, object body, string operation, CancellationToken cancellationToken)
    {
        var token = accessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
        if (BaseUrl() is not { } baseUrl || ApiKey() is not { Length: > 0 } apiKey
            || string.IsNullOrEmpty(token) || !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw ServiceException.Unavailable(operation,
                "File storage needs Supabase (Supabase:Url or Auth:Authority, and Supabase:PublishableKey) and a signed-in Supabase token.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/storage/v1/{path}")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("apikey", apiKey);
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
        using var response = await http.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            var type = (int)response.StatusCode is 400 or 401 or 403 ? ErrorType.Forbidden : ErrorType.Unavailable;
            throw new ServiceException(operation, type, "Storage refused the request.",
                new HttpRequestException($"Storage returned {(int)response.StatusCode}: {detail}"));
        }
        return await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
    }

    private string StorageUrl(string relative)
    {
        return $"{BaseUrl()}/storage/v1/{relative.TrimStart('/')}";
    }

    private string? BaseUrl()
    {
        var url = config["Supabase:Url"];
        if (string.IsNullOrEmpty(url) && config["Auth:Authority"] is { Length: > 0 } authority)
        {
            url = authority.TrimEnd('/');
            url = url.EndsWith("/auth/v1", StringComparison.OrdinalIgnoreCase) ? url[..^"/auth/v1".Length] : url;
        }
        return string.IsNullOrEmpty(url) ? null : url.TrimEnd('/');
    }

    private string? ApiKey()
    {
        return config["Supabase:PublishableKey"] is { Length: > 0 } key ? key : config["SUPABASE_PUBLISHABLE_KEY"];
    }
}
