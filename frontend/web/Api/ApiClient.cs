using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectPortal.Web.Auth;

namespace ProjectPortal.Web.Api;

/// <summary>Calls the backend's /api/v1 endpoints as the signed-in user.</summary>
public sealed class ApiClient(HttpClient http, IConfiguration config, AuthSession auth)
{
    // Matches the API: camelCase properties and enums written as camelCase strings ("available").
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string baseUrl = (config["Api:BaseUrl"]?.TrimEnd('/') ?? "") + "/api/v1/";

    public Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);

    public Task<T> PostAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Post, path, body);

    public Task<T> PutAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Put, path, body);

    public Task DeleteAsync(string path) => SendAsync<object?>(HttpMethod.Delete, path, null);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body)
    {
        var user = await auth.GetUserAsync()
            ?? throw new ApiException(HttpStatusCode.Unauthorized, "Your session has ended. Sign in again.");

        using var request = new HttpRequestMessage(method, baseUrl + path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: Json);
        }

        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            throw new ApiException(0, "Can't reach the server. Check that the API is running.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(response.StatusCode, await ReadProblemAsync(response));
            }
            if (response.StatusCode == HttpStatusCode.NoContent || typeof(T) == typeof(object))
            {
                return default!;
            }
            return (await response.Content.ReadFromJsonAsync<T>(Json))!;
        }
    }

    // Errors come back as problem details: field errors for validation failures, otherwise a detail or title.
    private static async Task<string> ReadProblemAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            if (problem.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in errors.EnumerateObject())
                {
                    foreach (var message in field.Value.EnumerateArray())
                    {
                        return message.GetString() ?? "";
                    }
                }
            }
            if (problem.TryGetProperty("detail", out var detail) && detail.GetString() is { Length: > 0 } detailText)
            {
                return detailText;
            }
        }
        catch (JsonException)
        {
        }
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Your session has ended. Sign in again.",
            HttpStatusCode.Forbidden => "You don't have permission to do that.",
            HttpStatusCode.NotFound => "That record no longer exists.",
            _ => $"The server returned an error ({(int)response.StatusCode})."
        };
    }
}

public sealed class ApiException(HttpStatusCode status, string message) : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
}
