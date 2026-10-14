using System.ComponentModel.DataAnnotations;
using API.Exceptions;

namespace API.Tests.Support;

internal static class OrchestrationFailureCases
{
    internal static async Task AssertDomainFailure(Func<Task> action, string operation, ErrorKind kind)
    {
        var exception = await Assert.ThrowsAsync<OrchestrationException>(action);
        Assert.Equal(operation, exception.Operation);
        Assert.Equal(kind, exception.Kind);
        var service = Assert.IsType<ServiceException>(exception.InnerException);
        Assert.Equal(kind, service.Kind);
        if (kind == ErrorKind.NotFound)
        {
            Assert.IsType<KeyNotFoundException>(service.InnerException);
        }
        else
        {
            Assert.IsType<ValidationException>(service.InnerException);
        }
    }

    internal static async Task AssertRepositoryFailure(Func<Task> action, string operation, Exception original)
    {
        var exception = await Assert.ThrowsAsync<OrchestrationException>(action);
        Assert.Equal(operation, exception.Operation);
        var service = Assert.IsType<ServiceException>(exception.InnerException);
        Assert.Same(original, service.InnerException);
        Assert.Equal(service.Kind, exception.Kind);
    }
}
