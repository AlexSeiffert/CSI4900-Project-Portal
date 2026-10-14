using API.DTOs;
using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations;
using API.Services.Interfaces;
using Moq;

namespace API.Tests.Orchestration;

public class StudentOrchestrationTests
{
    private readonly Mock<IStudentService> _students = new();
    private readonly Mock<ITopicService> _topics = new();

    private StudentOrchestration CreateOrchestration() => new(_students.Object, _topics.Object);

    [Fact]
    public async Task UpdateStudent_CallsServicesAndReturnsResult()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStudent { TopicIds = [Guid.NewGuid()] };
        var expected = new Student { UserId = userId };

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _students.Setup(service => service.UpdateStudent(userId, request, CancellationToken.None))
            .ReturnsAsync(expected);

        var orchestration = CreateOrchestration();

        var result = await orchestration.UpdateStudent(userId, request, CancellationToken.None);

        Assert.Same(expected, result);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _students.Verify(service => service.UpdateStudent(userId, request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateStudent_WrapsTopicsServiceException(ErrorType type)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStudent { TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("ValidateTopicsExist", type, "Service failed", new InvalidOperationException("Original cause"));

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateStudent(userId, request, CancellationToken.None));

        Assert.Equal(nameof(StudentOrchestration.UpdateStudent), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateStudent_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStudent { TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateStudent(userId, request, CancellationToken.None));

        Assert.Equal(nameof(StudentOrchestration.UpdateStudent), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateStudent_WrapsStudentsServiceException(ErrorType type)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStudent { TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("UpdateStudent", type, "Service failed", new InvalidOperationException("Original cause"));

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _students.Setup(service => service.UpdateStudent(userId, request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateStudent(userId, request, CancellationToken.None));

        Assert.Equal(nameof(StudentOrchestration.UpdateStudent), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _students.Verify(service => service.UpdateStudent(userId, request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateStudent_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStudent { TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _students.Setup(service => service.UpdateStudent(userId, request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateStudent(userId, request, CancellationToken.None));

        Assert.Equal(nameof(StudentOrchestration.UpdateStudent), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _students.Verify(service => service.UpdateStudent(userId, request, CancellationToken.None), Times.Once);
    }
}
