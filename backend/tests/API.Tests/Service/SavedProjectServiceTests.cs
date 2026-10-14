using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using Moq;

namespace API.Tests.Service;

public class SavedProjectServiceTests
{
    private readonly Mock<ISavedProjectRepository> _repository = new();

    private SavedProjectService CreateService() => new(_repository.Object);

    [Fact]
    public async Task GetSavedProjects_ReturnsMappedProjects()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var studentId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };

        _repository.Setup(repository => repository.GetSavedProjects(studentId, CancellationToken.None))
            .ReturnsAsync(new List<Project> { project });

        var service = CreateService();

        var result = Assert.Single(await service.GetSavedProjects(studentId, CancellationToken.None));

        Assert.Equal(project.Id, result.Id);
        Assert.Equal(topic.Id, Assert.Single(result.Topics).Id);
        _repository.Verify(repository => repository.GetSavedProjects(studentId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetSavedProjects_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetSavedProjects(studentId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetSavedProjects(studentId, token));

        Assert.Equal(nameof(SavedProjectService.GetSavedProjects), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetSavedProjects(studentId, token), Times.Once);
    }

    [Fact]
    public async Task SaveProject_CallsOwnRepository()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _repository.Setup(repository => repository.SaveProject(studentId, projectId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        await service.SaveProject(studentId, projectId, CancellationToken.None);

        _repository.Verify(repository => repository.SaveProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SaveProject_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.SaveProject(studentId, projectId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.SaveProject(studentId, projectId, token));

        Assert.Equal(nameof(SavedProjectService.SaveProject), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.SaveProject(studentId, projectId, token), Times.Once);
    }

    [Fact]
    public async Task RemoveSavedProject_RemovesRelationship()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _repository.Setup(repository => repository.RemoveSavedProject(studentId, projectId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        await service.RemoveSavedProject(studentId, projectId, CancellationToken.None);

        _repository.Verify(repository => repository.RemoveSavedProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RemoveSavedProject_WrapsRepositoryFailure()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.RemoveSavedProject(studentId, projectId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.RemoveSavedProject(studentId, projectId, token));

        Assert.Equal(nameof(SavedProjectService.RemoveSavedProject), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.RemoveSavedProject(studentId, projectId, token), Times.Once);
    }
}
