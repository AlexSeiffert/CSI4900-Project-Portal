using API.DTOs;
using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations;
using API.Services.Interfaces;
using Moq;

namespace API.Tests.Orchestration;

public class ProjectOrchestrationTests
{
    private readonly Mock<IProjectService> _projects = new();
    private readonly Mock<ISupervisorService> _supervisors = new();
    private readonly Mock<ITopicService> _topics = new();

    private ProjectOrchestration CreateOrchestration() => new(_projects.Object, _supervisors.Object, _topics.Object);

    [Fact]
    public async Task CreateProject_CallsServicesAndReturnsResult()
    {
        var request = new CreateProject { SupervisorId = Guid.NewGuid(), TopicIds = [Guid.NewGuid()] };
        var expected = new Project { Id = Guid.NewGuid() };

        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.CreateProject(request, CancellationToken.None))
            .ReturnsAsync(expected);

        var orchestration = CreateOrchestration();

        var result = await orchestration.CreateProject(request, CancellationToken.None);

        Assert.Same(expected, result);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.CreateProject(request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateProject_WrapsSupervisorsServiceException(ErrorType type)
    {
        var request = new CreateProject { SupervisorId = Guid.NewGuid(), TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("ValidateSupervisorExists", type, "Service failed", new InvalidOperationException("Original cause"));

        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateProject(request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.CreateProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateProject_WrapsSupervisorsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var request = new CreateProject { SupervisorId = Guid.NewGuid(), TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateProject(request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.CreateProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateProject_WrapsTopicsServiceException(ErrorType type)
    {
        var request = new CreateProject { SupervisorId = Guid.NewGuid(), TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("ValidateTopicsExist", type, "Service failed", new InvalidOperationException("Original cause"));

        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateProject(request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.CreateProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateProject_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var request = new CreateProject { SupervisorId = Guid.NewGuid(), TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateProject(request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.CreateProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateProject_WrapsProjectsServiceException(ErrorType type)
    {
        var request = new CreateProject { SupervisorId = Guid.NewGuid(), TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("CreateProject", type, "Service failed", new InvalidOperationException("Original cause"));

        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.CreateProject(request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateProject(request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.CreateProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.CreateProject(request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateProject_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var request = new CreateProject { SupervisorId = Guid.NewGuid(), TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _supervisors.Setup(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.CreateProject(request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.CreateProject(request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.CreateProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _supervisors.Verify(service => service.ValidateSupervisorExists(request.SupervisorId, CancellationToken.None), Times.Once);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.CreateProject(request, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_CallsServicesAndReturnsResult()
    {
        var projectId = Guid.NewGuid();
        var request = new UpdateProject { TopicIds = [Guid.NewGuid()] };
        var expected = new Project { Id = projectId };

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.UpdateProject(projectId, request, CancellationToken.None))
            .ReturnsAsync(expected);

        var orchestration = CreateOrchestration();

        var result = await orchestration.UpdateProject(projectId, request, CancellationToken.None);

        Assert.Same(expected, result);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.UpdateProject(projectId, request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateProject_WrapsTopicsServiceException(ErrorType type)
    {
        var projectId = Guid.NewGuid();
        var request = new UpdateProject { TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("ValidateTopicsExist", type, "Service failed", new InvalidOperationException("Original cause"));

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateProject(projectId, request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.UpdateProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateProject_WrapsTopicsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var projectId = Guid.NewGuid();
        var request = new UpdateProject { TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateProject(projectId, request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.UpdateProject), exception.Operation);
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
    public async Task UpdateProject_WrapsProjectsServiceException(ErrorType type)
    {
        var projectId = Guid.NewGuid();
        var request = new UpdateProject { TopicIds = [Guid.NewGuid()] };
        var original = new ServiceException("UpdateProject", type, "Service failed", new InvalidOperationException("Original cause"));

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.UpdateProject(projectId, request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateProject(projectId, request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.UpdateProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.UpdateProject(projectId, request, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateProject_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var projectId = Guid.NewGuid();
        var request = new UpdateProject { TopicIds = [Guid.NewGuid()] };
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _topics.Setup(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None))
            .Returns(Task.CompletedTask);
        _projects.Setup(service => service.UpdateProject(projectId, request, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.UpdateProject(projectId, request, CancellationToken.None));

        Assert.Equal(nameof(ProjectOrchestration.UpdateProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _topics.Verify(service => service.ValidateTopicsExist(request.TopicIds, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.UpdateProject(projectId, request, CancellationToken.None), Times.Once);
    }
}
