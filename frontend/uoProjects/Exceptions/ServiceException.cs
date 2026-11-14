using System.Net;

namespace uoProjects.Exceptions;

public class ServiceException(string operation, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    public string Operation { get; } = operation;
    public HttpStatusCode? StatusCode { get; init; }
    public ApiProblem? Problem { get; init; }

    public string? TraceId => Problem?.TraceId;

    public static ServiceException FromResponse(string operation, HttpStatusCode status, ApiProblem? problem)
    {
        var message = (int)status switch
        {
            400 => "One or more validation errors occurred.",
            401 => "Authentication is required to complete this request.",
            403 => "You do not have permission to perform this operation.",
            404 => "The requested resource was not found.",
            409 => "The resource changed or already exists. Refresh and retry.",
            408 or 504 => "The request timed out. Please try again.",
            429 => "Too many requests. Please wait before trying again.",
            499 => "The operation was cancelled.",
            502 or 503 => "The service is temporarily unavailable. Please try again.",
            _ => "The request could not be completed. Please try again."
        };

        // Only display details for expected API errors; never expose server failure details.
        if ((int)status is 400 or 404 or 409
            && !string.IsNullOrWhiteSpace(problem?.Detail))
        {
            message = problem.Detail;
        }

        return new ServiceException(operation, message) { StatusCode = status, Problem = problem };
    }
}
