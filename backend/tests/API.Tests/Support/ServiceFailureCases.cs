using System.ComponentModel.DataAnnotations;
using API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Support;

public static class ServiceFailureCases
{
    public static IEnumerable<object[]> All => new[] { "repository", "conflict", "validation", "cancelled", "unexpected", "service" }.Select(kind => new object[] { kind });

    internal static Exception Create(string kind) => kind switch
    {
        "repository" => new RepositoryException("dependency", new InvalidOperationException("Database failed")),
        "conflict" => new RepositoryException("dependency", new DbUpdateConcurrencyException("Concurrent update")),
        "validation" => new ValidationException("Invalid dependency result"),
        "cancelled" => new OperationCanceledException("Cancelled"),
        "service" => new ServiceException("dependency", ErrorKind.Validation, "Already wrapped", new ValidationException("Invalid")),
        _ => new InvalidOperationException("Unexpected failure")
    };

    internal static async Task AssertWrapped(Func<Task> action, string operation, Exception original)
    {
        var exception = await Assert.ThrowsAsync<ServiceException>(action);
        if (original is ServiceException)
        {
            Assert.Same(original, exception);
            return;
        }
        Assert.Equal(operation, exception.Operation);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original switch
        {
            RepositoryException repository => repository.Kind,
            ValidationException => ErrorKind.Validation,
            OperationCanceledException => ErrorKind.Cancelled,
            _ => ErrorKind.Unexpected
        }, exception.Kind);
    }

    internal static async Task AssertDomainFailure(Func<Task> action, string operation, ErrorKind kind)
    {
        var exception = await Assert.ThrowsAsync<ServiceException>(action);
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(kind, exception.Kind);
        if (kind == ErrorKind.NotFound)
        {
            Assert.IsType<KeyNotFoundException>(exception.InnerException);
        }
        else
        {
            Assert.IsType<ValidationException>(exception.InnerException);
        }
    }
}
