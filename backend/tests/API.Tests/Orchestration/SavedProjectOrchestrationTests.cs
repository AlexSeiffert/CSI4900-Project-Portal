using API.DTOs;
using API.Exceptions;
using API.Orchestrations;
using API.Services.Interfaces;
using Moq;

namespace API.Tests.Orchestration;

public class SavedProjectOrchestrationTests
{
    private readonly Mock<ISavedProjectService> _savedProjects = new();
    private readonly Mock<IStudentService> _students = new();
    private readonly Mock<IProjectService> _projects = new();

    private SavedProjectOrchestration CreateOrchestration() => new(_savedProjects.Object, _students.Object, _projects.Object);

    [Fact]
    public async Task GetSavedProjects_CallsServicesAndReturnsResult()
    {
        var studentId = Guid.NewGuid();
        var expected = new List<Project> { new() { Id = Guid.NewGuid() } };

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _savedProjects.Setup(service => service.GetSavedProjects(studentId, CancellationToken.None))
            .ReturnsAsync(expected);

        var orchestration = CreateOrchestration();

        var result = await orchestration.GetSavedProjects(studentId, CancellationToken.None);

        Assert.Same(expected, result);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.GetSavedProjects(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetSavedProjects_WrapsStudentsServiceException(ErrorType type)
    {
        var studentId = Guid.NewGuid();
        var original = new ServiceException("GetStudentById", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.GetSavedProjects(studentId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.GetSavedProjects), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetSavedProjects_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var studentId = Guid.NewGuid();
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.GetSavedProjects(studentId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.GetSavedProjects), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetSavedProjects_WrapsSavedProjectsServiceException(ErrorType type)
    {
        var studentId = Guid.NewGuid();
        var original = new ServiceException("GetSavedProjects", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _savedProjects.Setup(service => service.GetSavedProjects(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.GetSavedProjects(studentId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.GetSavedProjects), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.GetSavedProjects(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetSavedProjects_WrapsSavedProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var studentId = Guid.NewGuid();
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _savedProjects.Setup(service => service.GetSavedProjects(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.GetSavedProjects(studentId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.GetSavedProjects), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.GetSavedProjects(studentId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SaveProject_CallsServicesAndCompletes()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _projects.Setup(service => service.GetProjectById(projectId, CancellationToken.None))
            .ReturnsAsync(new Project { Id = projectId });
        _savedProjects.Setup(service => service.SaveProject(studentId, projectId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var orchestration = CreateOrchestration();

        await orchestration.SaveProject(studentId, projectId, CancellationToken.None);

        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.GetProjectById(projectId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.SaveProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task SaveProject_WrapsStudentsServiceException(ErrorType type)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("GetStudentById", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.SaveProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.SaveProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.SaveProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.SaveProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task SaveProject_WrapsProjectsServiceException(ErrorType type)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("GetProjectById", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _projects.Setup(service => service.GetProjectById(projectId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.SaveProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.SaveProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.GetProjectById(projectId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_WrapsProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _projects.Setup(service => service.GetProjectById(projectId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.SaveProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.SaveProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.GetProjectById(projectId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task SaveProject_WrapsSavedProjectsServiceException(ErrorType type)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("SaveProject", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _projects.Setup(service => service.GetProjectById(projectId, CancellationToken.None))
            .ReturnsAsync(new Project { Id = projectId });
        _savedProjects.Setup(service => service.SaveProject(studentId, projectId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.SaveProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.SaveProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.GetProjectById(projectId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.SaveProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_WrapsSavedProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _projects.Setup(service => service.GetProjectById(projectId, CancellationToken.None))
            .ReturnsAsync(new Project { Id = projectId });
        _savedProjects.Setup(service => service.SaveProject(studentId, projectId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.SaveProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.SaveProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _projects.Verify(service => service.GetProjectById(projectId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.SaveProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RemoveSavedProject_CallsServicesAndCompletes()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _savedProjects.Setup(service => service.RemoveSavedProject(studentId, projectId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var orchestration = CreateOrchestration();

        await orchestration.RemoveSavedProject(studentId, projectId, CancellationToken.None);

        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.RemoveSavedProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task RemoveSavedProject_WrapsStudentsServiceException(ErrorType type)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("GetStudentById", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.RemoveSavedProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.RemoveSavedProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveSavedProject_WrapsStudentsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.RemoveSavedProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.RemoveSavedProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task RemoveSavedProject_WrapsSavedProjectsServiceException(ErrorType type)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("RemoveSavedProject", type, "Service failed", new InvalidOperationException("Original cause"));

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _savedProjects.Setup(service => service.RemoveSavedProject(studentId, projectId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.RemoveSavedProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.RemoveSavedProject), exception.Operation);
        Assert.Equal(type, exception.Type);
        Assert.Same(original, exception.InnerException);
        Assert.Equal(original.Message, exception.Message);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.RemoveSavedProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveSavedProject_WrapsSavedProjectsUnexpectedFailureAndCancellation(bool cancelled)
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        Exception original = cancelled
            ? new OperationCanceledException()
            : new InvalidOperationException("Unexpected failure");

        _students.Setup(service => service.GetStudentById(studentId, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = studentId });
        _savedProjects.Setup(service => service.RemoveSavedProject(studentId, projectId, CancellationToken.None))
            .ThrowsAsync(original);

        var orchestration = CreateOrchestration();

        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => orchestration.RemoveSavedProject(studentId, projectId, CancellationToken.None));

        Assert.Equal(nameof(SavedProjectOrchestration.RemoveSavedProject), exception.Operation);
        Assert.Equal(cancelled ? ErrorType.Cancelled : ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _students.Verify(service => service.GetStudentById(studentId, CancellationToken.None), Times.Once);
        _savedProjects.Verify(service => service.RemoveSavedProject(studentId, projectId, CancellationToken.None), Times.Once);
    }
}
