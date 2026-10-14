using API.DTOs;
using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations;
using API.Services.Interfaces;
using Moq;

namespace API.Tests.Orchestration;

public class SupervisorOrchestrationTests
{
    private readonly Mock<ISupervisorService> _supervisors = new();
    private readonly Mock<ITopicService> _topics = new();

    private SupervisorOrchestration CreateOrchestration() => new(_supervisors.Object, _topics.Object);

    [Fact]
    public async Task UpdateSupervisor_CallsServicesAndReturnsResult()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor { TopicIds = [Guid.NewGuid()] };
        var expected = new Supervisor { UserId = userId };

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.UpdateSupervisor(userId, request, CancellationToken.None))
            .ReturnsAsync(expected);

        var orchestration = CreateOrchestration();

        var result = await orchestration.UpdateSupervisor(userId, request, CancellationToken.None);

        Assert.Same(expected, result);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.UpdateSupervisor(userId, request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateSupervisor_WrapsTopicsServiceException(ErrorType type)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor { TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("ValidateTopicsExist", type, "Service failed", new InvalidOperationException("Original cause"));

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateSupervisor(userId, request, CancellationToken.None));

        Assert.Equal(nameof(SupervisorOrchestration.UpdateSupervisor), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateSupervisor_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor { TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateSupervisor(userId, request, CancellationToken.None));

        Assert.Equal(nameof(SupervisorOrchestration.UpdateSupervisor), exception.Operation);
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
    public async Task UpdateSupervisor_WrapsSupervisorsServiceException(ErrorType type)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor { TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("UpdateSupervisor", type, "Service failed", new InvalidOperationException("Original cause"));

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.UpdateSupervisor(userId, request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateSupervisor(userId, request, CancellationToken.None));

        Assert.Equal(nameof(SupervisorOrchestration.UpdateSupervisor), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.UpdateSupervisor(userId, request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateSupervisor_WrapsSupervisorsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor { TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _supervisors.Setup(service => service.UpdateSupervisor(userId, request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateSupervisor(userId, request, CancellationToken.None));

        Assert.Equal(nameof(SupervisorOrchestration.UpdateSupervisor), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _supervisors.Verify(service => service.UpdateSupervisor(userId, request, CancellationToken.None), Times.Once);
    }
}
