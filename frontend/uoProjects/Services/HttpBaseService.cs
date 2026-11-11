using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using uoProjects.Exceptions;

namespace uoProjects.Services;

public abstract class HttpBaseService(HttpClient httpClient, NotificationService notifications)
{
    // Null means the request failed and was reported; an empty collection is a successful result.
    protected async Task<T?> GetAsync<T>(string endpoint, string errorMessage, CancellationToken cancellationToken = default,
        [CallerMemberName] string operation = "") where T : class
    {
        try
        {
            return await SendGetAsync<T>(endpoint, operation, cancellationToken);
        }
        catch (ServiceException exception)
        {
            notifications.Error(errorMessage, exception);
            return null;
        }
    }

    private async Task<T> SendGetAsync<T>(string endpoint, string operation, CancellationToken cancellationToken) where T : class
    {
        using var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        requestCancellation.CancelAfter(httpClient.Timeout);
        var requestToken = requestCancellation.Token;
        try
        {
            using var response = await httpClient.GetAsync(endpoint, requestToken);
            if (!response.IsSuccessStatusCode)
            {
                ValidationProblemDetails? problem = null;
                try
                {
                    problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(requestToken);
                }
                catch (JsonException)
                {
                    // Empty, HTML, or malformed error bodies still retain the HTTP status.
                }

                throw ServiceException.FromResponse(operation, response.StatusCode, problem);
            }

            return await response.Content.ReadFromJsonAsync<T>(requestToken)
                ?? throw new JsonException("The API returned no response body.");
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
            throw new ServiceException(operation, message, exception);
        }
    }
}
