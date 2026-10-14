using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Orchestration;

public class SupervisorOrchestrationTests
{
    [Fact]
    public async Task UpdateSupervisor_RejectsUnknownTopicsBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.SupervisorUpdate();
        request.TopicIds = [Guid.NewGuid()];
        await OrchestrationFailureCases.AssertDomainFailure(() => c.SupervisorOrchestration.UpdateSupervisor(c.Supervisor.UserId, request), "UpdateSupervisor", ErrorType.Validation);
        Assert.DoesNotContain("UpdateSupervisor", c.Supervisors.Calls);
    }

    [Fact]
    public async Task UpdateSupervisor_WrapsTopicsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Topics.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.SupervisorOrchestration.UpdateSupervisor(c.Supervisor.UserId, c.SupervisorUpdate()), "UpdateSupervisor", original);
        Assert.DoesNotContain("UpdateSupervisor", c.Supervisors.Calls);
    }

    [Fact]
    public async Task UpdateSupervisor_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.SupervisorUpdate();
        var id = c.Supervisors.Item.UserId;
        var result = await c.SupervisorOrchestration.UpdateSupervisor(id, request, cancellation.Token);
        Assert.Same(c.Supervisors.Item, result);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Supervisors.UpdateSupervisor" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
        Assert.Equal(request.TopicIds, c.Topics.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Supervisors.LastToken);
        Assert.Equal(id, c.Supervisors.LastArguments[0]);
        Assert.Equal(request, c.Supervisors.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateSupervisor_WrapsTopicsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.SupervisorUpdate();
        var id = c.Supervisors.Item.UserId;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateTopicsExist", kind, "Service failed", root);
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SupervisorOrchestration.UpdateSupervisor(id, request, cancellation.Token));
        Assert.Equal("UpdateSupervisor", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateSupervisor_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.SupervisorUpdate();
        var id = c.Supervisors.Item.UserId;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SupervisorOrchestration.UpdateSupervisor(id, request, cancellation.Token));
        Assert.Equal("UpdateSupervisor", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateSupervisor_WrapsSupervisorsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.SupervisorUpdate();
        var id = c.Supervisors.Item.UserId;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("UpdateSupervisor", kind, "Service failed", root);
        c.Supervisors.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SupervisorOrchestration.UpdateSupervisor(id, request, cancellation.Token));
        Assert.Equal("UpdateSupervisor", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Supervisors.UpdateSupervisor" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Supervisors.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateSupervisor_WrapsSupervisorsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.SupervisorUpdate();
        var id = c.Supervisors.Item.UserId;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Supervisors.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SupervisorOrchestration.UpdateSupervisor(id, request, cancellation.Token));
        Assert.Equal("UpdateSupervisor", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Supervisors.UpdateSupervisor" }, c.Calls);
    }

    [Fact]
    public void Constructor_DependsOnlyOnTheServicesItCoordinates()
    {
        var constructor = Assert.Single(typeof(API.Orchestrations.SupervisorOrchestration).GetConstructors());
        var dependencies = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToHashSet();
        Assert.True(dependencies.SetEquals(new[] { typeof(API.Services.Interfaces.ITopicService), typeof(API.Services.Interfaces.ISupervisorService) }));
    }
}
