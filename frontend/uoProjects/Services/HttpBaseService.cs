using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using uoProjects.Exceptions;

namespace uoProjects.Services;

public abstract class HttpBaseService(HttpClient httpClient, NotificationService notifications)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
    };

    // Null means the request failed and was reported; an empty collection is a successful result.
    protected Task<T?> GetAsync<T>(string endpoint, string errorMessage, CancellationToken cancellationToken = default,
        [CallerMemberName] string operation = "") where T : class
    {
        return SendAsync<T>(HttpMethod.Get, endpoint, null, errorMessage, cancellationToken, operation);
    }

    protected Task<T?> SendAsync<T>(HttpMethod method, string endpoint, object? body, string errorMessage,
        CancellationToken cancellationToken = default, [CallerMemberName] string operation = "") where T : class
    {
        return SendRequestAsync<T>(method, endpoint, body, errorMessage, operation, cancellationToken,
                async (response, token) => await response.Content.ReadFromJsonAsync<T>(Json, token)
                    ?? throw new JsonException("The API returned no response body."));
    }

    protected Task<bool> SendAsync(HttpMethod method, string endpoint, string errorMessage,
        CancellationToken cancellationToken = default, [CallerMemberName] string operation = "")
    {
        return SendRequestAsync(method, endpoint, null, errorMessage, operation, cancellationToken,
                (_, _) => Task.FromResult(true));
    }

    private async Task<T?> SendRequestAsync<T>(HttpMethod method, string endpoint, object? body, string errorMessage,
        string operation, CancellationToken cancellationToken, Func<HttpResponseMessage, CancellationToken, Task<T>> read)
    {
        using var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestCancellation.CancelAfter(httpClient.Timeout);
        var requestToken = requestCancellation.Token;
        try
        {
            using var request = new HttpRequestMessage(method, endpoint);
            if (body is not null)
            {
                request.Content = JsonContent.Create(body, options: Json);
            }
            using var response = await httpClient.SendAsync(request, requestToken);
            if (!response.IsSuccessStatusCode)
            {
                ApiProblem? problem = null;
                try
                {
                    problem = await response.Content.ReadFromJsonAsync<ApiProblem>(requestToken);
                }
                catch (JsonException)
                {
                    // Empty, HTML, or malformed error bodies still retain the HTTP status.
                }

                throw ServiceException.FromResponse(operation, response.StatusCode, problem);
            }

            return await read(response, requestToken);
        }
        catch (ServiceException exception)
        {
            notifications.Error(errorMessage, exception);
            return default;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Caller cancellation is expected and must not produce a notification.
            throw;
        }
        catch (Exception exception) when (exception is OperationCanceledException or HttpRequestException or IOException or JsonException)
        {
            var message = exception switch
            {
                OperationCanceledException => "The request timed out. Please try again.",
                JsonException => "The service returned an invalid response. Please try again.",
                _ => "The service could not be reached. Please try again."
            };
            notifications.Error(errorMessage, new ServiceException(operation, message, exception));
            return default;
        }
    }
}
