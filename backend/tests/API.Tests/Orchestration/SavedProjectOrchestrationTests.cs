using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Orchestration;

public class SavedProjectOrchestrationTests
{
    [Fact]
    public async Task GetSavedProjects_RejectsMissingStudentBeforeAccessingSavedProjects()
    {
        var c = new ServiceTestContext();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.SavedProjectOrchestration.GetSavedProjects(Guid.NewGuid()), "GetSavedProjects", ErrorType.NotFound);
        Assert.Empty(c.SavedProjects.Calls);
    }

    [Fact]
    public async Task SaveProject_RejectsMissingStudentBeforeAccessingSavedProjects()
    {
        var c = new ServiceTestContext();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.SavedProjectOrchestration.SaveProject(Guid.NewGuid(), c.Project.Id), "SaveProject", ErrorType.NotFound);
        Assert.Empty(c.SavedProjects.Calls);
    }

    [Fact]
    public async Task RemoveSavedProject_RejectsMissingStudentBeforeAccessingSavedProjects()
    {
        var c = new ServiceTestContext();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.SavedProjectOrchestration.RemoveSavedProject(Guid.NewGuid(), c.Project.Id), "RemoveSavedProject", ErrorType.NotFound);
        Assert.Empty(c.SavedProjects.Calls);
    }

    [Fact]
    public async Task SaveProject_RejectsMissingProjectBeforeSaving()
    {
        var c = new ServiceTestContext();
        await OrchestrationFailureCases.AssertDomainFailure(() => c.SavedProjectOrchestration.SaveProject(c.Student.UserId, Guid.NewGuid()), "SaveProject", ErrorType.NotFound);
        Assert.Empty(c.SavedProjects.Calls);
    }

    [Fact]
    public async Task GetSavedProjects_WrapsStudentsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Students.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.SavedProjectOrchestration.GetSavedProjects(c.Student.UserId), "GetSavedProjects", original);
        Assert.DoesNotContain("GetSavedProjects", c.SavedProjects.Calls);
    }

    [Fact]
    public async Task SaveProject_WrapsStudentsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Students.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.SavedProjectOrchestration.SaveProject(c.Student.UserId, c.Project.Id), "SaveProject", original);
        Assert.DoesNotContain("SaveProject", c.SavedProjects.Calls);
    }

    [Fact]
    public async Task SaveProject_WrapsProjectsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Projects.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.SavedProjectOrchestration.SaveProject(c.Student.UserId, c.Project.Id), "SaveProject", original);
        Assert.DoesNotContain("SaveProject", c.SavedProjects.Calls);
    }

    [Fact]
    public async Task RemoveSavedProject_WrapsStudentsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Students.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.SavedProjectOrchestration.RemoveSavedProject(c.Student.UserId, c.Project.Id), "RemoveSavedProject", original);
        Assert.DoesNotContain("RemoveSavedProject", c.SavedProjects.Calls);
    }

    [Fact]
    public async Task GetSavedProjects_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var result = await c.SavedProjectOrchestration.GetSavedProjects(studentId, cancellation.Token);
        Assert.Same(c.SavedProjects.Items, result);
        Assert.Equal(new[] { "Students.GetStudentById", "SavedProjects.GetSavedProjects" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
        Assert.Equal(studentId, c.Students.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.SavedProjects.LastToken);
        Assert.Equal(studentId, c.SavedProjects.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetSavedProjects_WrapsStudentsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("GetStudentById", kind, "Service failed", root);
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.GetSavedProjects(studentId, cancellation.Token));
        Assert.Equal("GetSavedProjects", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetSavedProjects_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.GetSavedProjects(studentId, cancellation.Token));
        Assert.Equal("GetSavedProjects", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetSavedProjects_WrapsSavedProjectsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("GetSavedProjects", kind, "Service failed", root);
        c.SavedProjects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.GetSavedProjects(studentId, cancellation.Token));
        Assert.Equal("GetSavedProjects", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "SavedProjects.GetSavedProjects" }, c.Calls);
        Assert.Equal(cancellation.Token, c.SavedProjects.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetSavedProjects_WrapsSavedProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.SavedProjects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.GetSavedProjects(studentId, cancellation.Token));
        Assert.Equal("GetSavedProjects", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "SavedProjects.GetSavedProjects" }, c.Calls);
    }

    [Fact]
    public async Task SaveProject_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        await c.SavedProjectOrchestration.SaveProject(studentId, projectId, cancellation.Token);

        Assert.Equal(new[] { "Students.GetStudentById", "Projects.GetProjectById", "SavedProjects.SaveProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
        Assert.Equal(studentId, c.Students.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
        Assert.Equal(projectId, c.Projects.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.SavedProjects.LastToken);
        Assert.Equal(studentId, c.SavedProjects.LastArguments[0]);
        Assert.Equal(projectId, c.SavedProjects.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task SaveProject_WrapsStudentsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("GetStudentById", kind, "Service failed", root);
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.SaveProject(studentId, projectId, cancellation.Token));
        Assert.Equal("SaveProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.SaveProject(studentId, projectId, cancellation.Token));
        Assert.Equal("SaveProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task SaveProject_WrapsProjectsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("GetProjectById", kind, "Service failed", root);
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.SaveProject(studentId, projectId, cancellation.Token));
        Assert.Equal("SaveProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "Projects.GetProjectById" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Projects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.SaveProject(studentId, projectId, cancellation.Token));
        Assert.Equal("SaveProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "Projects.GetProjectById" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task SaveProject_WrapsSavedProjectsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("SaveProject", kind, "Service failed", root);
        c.SavedProjects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.SaveProject(studentId, projectId, cancellation.Token));
        Assert.Equal("SaveProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "Projects.GetProjectById", "SavedProjects.SaveProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.SavedProjects.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_WrapsSavedProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.SavedProjects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.SaveProject(studentId, projectId, cancellation.Token));
        Assert.Equal("SaveProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "Projects.GetProjectById", "SavedProjects.SaveProject" }, c.Calls);
    }

    [Fact]
    public async Task RemoveSavedProject_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        await c.SavedProjectOrchestration.RemoveSavedProject(studentId, projectId, cancellation.Token);

        Assert.Equal(new[] { "Students.GetStudentById", "SavedProjects.RemoveSavedProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
        Assert.Equal(studentId, c.Students.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.SavedProjects.LastToken);
        Assert.Equal(studentId, c.SavedProjects.LastArguments[0]);
        Assert.Equal(projectId, c.SavedProjects.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task RemoveSavedProject_WrapsStudentsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("GetStudentById", kind, "Service failed", root);
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.RemoveSavedProject(studentId, projectId, cancellation.Token));
        Assert.Equal("RemoveSavedProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveSavedProject_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.RemoveSavedProject(studentId, projectId, cancellation.Token));
        Assert.Equal("RemoveSavedProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task RemoveSavedProject_WrapsSavedProjectsFailureAndStopsRemainingCalls(ErrorType kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("RemoveSavedProject", kind, "Service failed", root);
        c.SavedProjects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.RemoveSavedProject(studentId, projectId, cancellation.Token));
        Assert.Equal("RemoveSavedProject", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "SavedProjects.RemoveSavedProject" }, c.Calls);
        Assert.Equal(cancellation.Token, c.SavedProjects.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveSavedProject_WrapsSavedProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var studentId = c.Students.Item.UserId;
        var projectId = c.Projects.Item.Id;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.SavedProjects.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.SavedProjectOrchestration.RemoveSavedProject(studentId, projectId, cancellation.Token));
        Assert.Equal("RemoveSavedProject", exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Students.GetStudentById", "SavedProjects.RemoveSavedProject" }, c.Calls);
    }

    [Fact]
    public void Constructor_DependsOnlyOnTheServicesItCoordinates()
    {
        var constructor = Assert.Single(typeof(API.Orchestrations.SavedProjectOrchestration).GetConstructors());
        var dependencies = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToHashSet();
        Assert.True(dependencies.SetEquals(new[] { typeof(API.Services.Interfaces.IStudentService), typeof(API.Services.Interfaces.ISavedProjectService), typeof(API.Services.Interfaces.IProjectService) }));
    }
}
