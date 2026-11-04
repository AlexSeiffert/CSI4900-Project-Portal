using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

public class ApiExceptionHandler(IProblemDetailsService problems, ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private readonly IProblemDetailsService _problems = problems;
    private readonly ILogger<ApiExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var problem = CreateProblemDetails(exception);
        if (problem.Status == StatusCodes.Status499ClientClosedRequest && context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            _logger.LogInformation(exception, "Request {Method} {Path} was cancelled.", context.Request.Method, context.Request.Path);
            return true;
        }

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        problem.Instance = context.Request.Path;
        problem.Extensions["traceId"] = traceId;
        context.Response.StatusCode = problem.Status!.Value;

        if (problem.Status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Request {Method} {Path} failed. Trace ID: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);
        }
        else if (problem.Status == StatusCodes.Status409Conflict)
        {
            _logger.LogWarning(exception, "Request {Method} {Path} conflicted. Trace ID: {TraceId}",
                context.Request.Method, context.Request.Path, traceId);
        }

        var written = await _problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problem
        });

        if (!written)
        {
            // Keep the error response consistent even when Accept excludes the default JSON writer.
            await context.Response.WriteAsJsonAsync((object)problem, options: null, contentType: "application/problem+json",
                cancellationToken: cancellationToken);
        }

        return true;
    }

    private static ProblemDetails CreateProblemDetails(Exception exception)
    {
        var type = exception switch
        {
            ServiceException service => service.Type,
            RepositoryException repository => repository.Type,
            ValidationException => ErrorType.Validation,
            OperationCanceledException => ErrorType.Cancelled,
            _ => ErrorType.Unexpected
        };

        return type switch
        {
            ErrorType.Validation => new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["request"] = [exception.Message]
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            },
            ErrorType.NotFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Detail = exception.Message
            },
            // The database or API refused the action for this user (for example, editing someone else's project).
            ErrorType.Forbidden => new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this operation."
            },
            ErrorType.Cancelled => new ProblemDetails
            {
                Status = StatusCodes.Status499ClientClosedRequest,
                Title = "The operation was cancelled",
                Detail = "The request was cancelled before it could complete."
            },
            ErrorType.Conflict => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "The operation conflicts with existing data",
                Detail = "The resource changed or the relationship already exists. Refresh and retry."
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred",
                Detail = "The request could not be completed. Use the trace ID when reporting this error."
            }
        };
    }
}
