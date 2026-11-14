using System.Net;
using MudBlazor;
using uoProjects.Exceptions;

namespace uoProjects.Services;

public class NotificationService(ISnackbar snackbar, ILogger<NotificationService> logger)
{
    public void Error(string message, ServiceException exception)
    {
        var level = exception.StatusCode switch
        {
            null or >= HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.Conflict => LogLevel.Warning,
            _ => LogLevel.Information
        };
        logger.Log(level, exception, "API operation {Operation} failed. Status: {StatusCode}. Trace ID: {TraceId}",
            exception.Operation, exception.StatusCode, exception.TraceId);

        message = $"{message} {exception.Message}";
        if (exception.StatusCode == HttpStatusCode.BadRequest && exception.Problem is { } problem)
        {
            message = string.Join(" ", new[] { message }.Concat(problem.Errors.Values.SelectMany(errors => errors)).Distinct());
        }
        if (!string.IsNullOrWhiteSpace(exception.TraceId))
        {
            message += $" Trace ID: {exception.TraceId}";
        }

        Error(message, $"api-error:{exception.Operation}");
    }

    public void Error(string message, string? key = null)
    {
        snackbar.Add(message, Severity.Error, options =>
        {
            options.CloseAfterNavigation = true;
        }, key: key);
    }
}
