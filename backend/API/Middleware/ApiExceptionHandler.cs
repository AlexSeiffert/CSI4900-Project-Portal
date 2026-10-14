using System.Diagnostics;
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
        var kind = exception switch
        {
            ServiceException service => service.Kind,
            OrchestrationException orchestration => orchestration.Kind,
            _ => ErrorKind.Unexpected
        };

        return kind switch
        {
            ErrorKind.Validation => new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["request"] = [exception.Message]
            })
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            },
            ErrorKind.NotFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Detail = exception.Message
            },
            ErrorKind.Cancelled => new ProblemDetails
            {
                Status = StatusCodes.Status499ClientClosedRequest,
                Title = "The operation was cancelled",
                Detail = "The request was cancelled before it could complete."
            },
            ErrorKind.Conflict => new ProblemDetails
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
