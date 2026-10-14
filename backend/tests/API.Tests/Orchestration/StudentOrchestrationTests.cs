using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Orchestration;

public class StudentOrchestrationTests
{
    [Fact]
    public async Task UpdateStudent_RejectsUnknownTopicsBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.StudentUpdate();
        request.TopicIds = [Guid.NewGuid()];
        await OrchestrationFailureCases.AssertDomainFailure(() => c.StudentOrchestration.UpdateStudent(c.Student.UserId, request), "UpdateStudent", ErrorKind.Validation);
        Assert.DoesNotContain("UpdateStudent", c.Students.Calls);
    }

    [Fact]
    public async Task UpdateStudent_WrapsTopicsLookupFailureBeforeWriting()
    {
        var c = new ServiceTestContext();
        var original = new RepositoryException("lookup", new InvalidOperationException("Database failed"));
        c.Topics.Failure = original;
        await OrchestrationFailureCases.AssertRepositoryFailure(() => c.StudentOrchestration.UpdateStudent(c.Student.UserId, c.StudentUpdate()), "UpdateStudent", original);
        Assert.DoesNotContain("UpdateStudent", c.Students.Calls);
    }

    [Fact]
    public async Task UpdateStudent_CoordinatesServicesInOrderAndForwardsArguments()
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.StudentUpdate();
        var id = c.Students.Item.UserId;
        var result = await c.StudentOrchestration.UpdateStudent(id, request, cancellation.Token);
        Assert.Same(c.Students.Item, result);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Students.UpdateStudent" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
        Assert.Equal(request.TopicIds, c.Topics.LastArguments[0]);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
        Assert.Equal(id, c.Students.LastArguments[0]);
        Assert.Equal(request, c.Students.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task UpdateStudent_WrapsTopicsFailureAndStopsRemainingCalls(ErrorKind kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.StudentUpdate();
        var id = c.Students.Item.UserId;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("ValidateTopicsExist", kind, "Service failed", root);
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.StudentOrchestration.UpdateStudent(id, request, cancellation.Token));
        Assert.Equal("UpdateStudent", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateStudent_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.StudentUpdate();
        var id = c.Students.Item.UserId;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Topics.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.StudentOrchestration.UpdateStudent(id, request, cancellation.Token));
        Assert.Equal("UpdateStudent", exception.Operation);
        Assert.Equal(cancelled ? ErrorKind.Cancelled : ErrorKind.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist" }, c.Calls);
    }

    [Theory]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task UpdateStudent_WrapsStudentsFailureAndStopsRemainingCalls(ErrorKind kind)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.StudentUpdate();
        var id = c.Students.Item.UserId;
        var root = new InvalidOperationException("Original failure");
        var original = new ServiceException("UpdateStudent", kind, "Service failed", root);
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.StudentOrchestration.UpdateStudent(id, request, cancellation.Token));
        Assert.Equal("UpdateStudent", exception.Operation);
        Assert.Equal(kind, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Same(root, exception.InnerException!.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Students.UpdateStudent" }, c.Calls);
        Assert.Equal(cancellation.Token, c.Students.LastToken);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateStudent_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var c = new OrchestrationTestContext();
        using var cancellation = new CancellationTokenSource();
        var request = c.StudentUpdate();
        var id = c.Students.Item.UserId;
        Exception original = cancelled ? new OperationCanceledException(cancellation.Token) : new InvalidOperationException("Unexpected failure");
        c.Students.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => c.StudentOrchestration.UpdateStudent(id, request, cancellation.Token));
        Assert.Equal("UpdateStudent", exception.Operation);
        Assert.Equal(cancelled ? ErrorKind.Cancelled : ErrorKind.Unexpected, exception.Kind);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(new[] { "Topics.ValidateTopicsExist", "Students.UpdateStudent" }, c.Calls);
    }

    [Fact]
    public void Constructor_DependsOnlyOnTheServicesItCoordinates()
    {
        var constructor = Assert.Single(typeof(API.Orchestrations.StudentOrchestration).GetConstructors());
        var dependencies = constructor.GetParameters().Select(parameter => parameter.ParameterType).ToHashSet();
        Assert.True(dependencies.SetEquals(new[] { typeof(API.Services.Interfaces.ITopicService), typeof(API.Services.Interfaces.IStudentService) }));
    }
}
