using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Orchestration;

public class ProjectOrchestrationTests
{
    [Fact]
    public async Task CreateProject_RejectsUnknownTopicsBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.NewProject();
        request.TopicIds = [Guid.NewGuid()];
        await OrchestrationFailureCases.AssertDomainFailure(() => c.ProjectOrchestration.CreateProject(request), "CreateProject", ErrorType.Validation);
        Assert.DoesNotContain("CreateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task CreateProject_RejectsUnknownSupervisorBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.NewProject();
        request.SupervisorId = Guid.NewGuid();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.ProjectOrchestration.CreateProject(request), "CreateProject", ErrorType.Validation);
        Assert.DoesNotContain("CreateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task UpdateProject_RejectsUnknownTopicsBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.ProjectUpdate();
        request.TopicIds = [Guid.NewGuid()];
        await OrchestrationFailureCases.AssertDomainFailure(() => c.ProjectOrchestration.UpdateProject(c.Project.Id, request), "UpdateProject", ErrorType.Validation);
        Assert.DoesNotContain("UpdateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task CreateProject_WrapsSupervisorsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Supervisors.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.ProjectOrchestration.CreateProject(c.NewProject()), "CreateProject", original);
        Assert.DoesNotContain("CreateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task CreateProject_WrapsTopicsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Topics.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.ProjectOrchestration.CreateProject(c.NewProject()), "CreateProject", original);
        Assert.DoesNotContain("CreateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task UpdateProject_WrapsTopicsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Topics.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.ProjectOrchestration.UpdateProject(c.Project.Id, c.ProjectUpdate()), "UpdateProject", original);
        Assert.DoesNotContain("UpdateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task CreateProject_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewProject();
        var result = await c.ProjectOrchestration.CreateProject(request, cancellation.Token);
        Assert.Same(c.Projects.Item, result);
        Assert.Equal(new[] { "Supervisors.ValidateSupervisorExists", "Topics.ValidateTopicsExist", "Projects.CreateProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Supervisors.LastToken);
        Assert.Equal(request.SupervisorId, c.Supervisors.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
        Assert.Equal(request.TopicIds, c.Topics.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
        Assert.Equal(request, c.Projects.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateProject_WrapsSupervisorsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewProject();
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateSupervisorExists", kind, "Service failed", root);
        c.Supervisors.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.CreateProject(request, cancellation.Token));
        Assert.Equal("CreateProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Supervisors.ValidateSupervisorExists" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Supervisors.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateProject_WrapsSupervisorsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewProject();
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Supervisors.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.CreateProject(request, cancellation.Token));
        Assert.Equal("CreateProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Supervisors.ValidateSupervisorExists" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateProject_WrapsTopicsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewProject();
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateTopicsExist", kind, "Service failed", root);
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.CreateProject(request, cancellation.Token));
        Assert.Equal("CreateProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Supervisors.ValidateSupervisorExists", "Topics.ValidateTopicsExist" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateProject_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewProject();
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.CreateProject(request, cancellation.Token));
        Assert.Equal("CreateProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Supervisors.ValidateSupervisorExists", "Topics.ValidateTopicsExist" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateProject_WrapsProjectsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewProject();
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("CreateProject", kind, "Service failed", root);
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.CreateProject(request, cancellation.Token));
        Assert.Equal("CreateProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Supervisors.ValidateSupervisorExists", "Topics.ValidateTopicsExist", "Projects.CreateProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateProject_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.NewProject();
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.CreateProject(request, cancellation.Token));
        Assert.Equal("CreateProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Supervisors.ValidateSupervisorExists", "Topics.ValidateTopicsExist", "Projects.CreateProject" }, c.Calls);
    }

    [Fact]
    public async Task UpdateProject_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.ProjectUpdate();
        var id = c.Projects.Item.Id;
        var result = await c.ProjectOrchestration.UpdateProject(id, request, cancellation.Token);
        Assert.Same(c.Projects.Item, result);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Projects.UpdateProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
        Assert.Equal(request.TopicIds, c.Topics.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
        Assert.Equal(id, c.Projects.LastArguments[0]);
        Assert.Equal(request, c.Projects.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateProject_WrapsTopicsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.ProjectUpdate();
        var id = c.Projects.Item.Id;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateTopicsExist", kind, "Service failed", root);
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.UpdateProject(id, request, cancellation.Token));
        Assert.Equal("UpdateProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateProject_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.ProjectUpdate();
        var id = c.Projects.Item.Id;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.UpdateProject(id, request, cancellation.Token));
        Assert.Equal("UpdateProject", exception.Operation);
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
    public async Task UpdateProject_WrapsProjectsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.ProjectUpdate();
        var id = c.Projects.Item.Id;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("UpdateProject", kind, "Service failed", root);
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.UpdateProject(id, request, cancellation.Token));
        Assert.Equal("UpdateProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Projects.UpdateProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateProject_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.ProjectUpdate();
        var id = c.Projects.Item.Id;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.ProjectOrchestration.UpdateProject(id, request, cancellation.Token));
        Assert.Equal("UpdateProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Projects.UpdateProject" }, c.Calls);
    }

    [Fact]
    public void Constructor_DependsOnlyOnTheServicesItCoordinates()
    {
        var constructor = Assert.Single(typeof(API.Orchestrations.ProjectOrchestration).GetConstructors());
        var dependencies = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToHashSet();
        Assert.True(dependencies.SetEquals(new[] { typeof(API.Services.Interfaces.ISupervisorService), typeof(API.Services.Interfaces.ITopicService), typeof(API.Services.Interfaces.IProjectService) }));
    }
}
