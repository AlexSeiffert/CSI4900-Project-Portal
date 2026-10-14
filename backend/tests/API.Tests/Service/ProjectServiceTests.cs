using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using Moq;

namespace API.Tests.Service;

public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _repository = new();

    private ProjectService CreateService() => new(_repository.Object);

    [Fact]
    public async Task GetProjects_ReturnsMatchingProjectDtos()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };

        _repository.Setup(repository => repository.GetProjects(CancellationToken.None))
            .ReturnsAsync(new List<Project> { project });

        var service = CreateService();

        var result = Assert.Single(await service.GetProjects(CancellationToken.None));

        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.Title, result.Title);
        Assert.Equal(topic.Id, Assert.Single(result.Topics).Id);
        _repository.Verify(repository => repository.GetProjects(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetProjects_WrapsRepositoryFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjects(token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetProjects(token));

        Assert.Equal(nameof(ProjectService.GetProjects), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjects(token), Times.Once);
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_ReturnsMatchingProjectDtos()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };

        _repository.Setup(repository => repository.GetProjectsBySupervisorId(supervisorId, CancellationToken.None))
            .ReturnsAsync(new List<Project> { project });

        var service = CreateService();

        var result = Assert.Single(await service.GetProjectsBySupervisorId(supervisorId, CancellationToken.None));

        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.Title, result.Title);
        Assert.Equal(topic.Id, Assert.Single(result.Topics).Id);
        _repository.Verify(repository => repository.GetProjectsBySupervisorId(supervisorId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_WrapsRepositoryFailure()
    {
        var supervisorId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjectsBySupervisorId(supervisorId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetProjectsBySupervisorId(supervisorId, token));

        Assert.Equal(nameof(ProjectService.GetProjectsBySupervisorId), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjectsBySupervisorId(supervisorId, token), Times.Once);
    }

    [Fact]
    public async Task GetProjectsByTopicId_ReturnsMatchingProjectDtos()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };

        _repository.Setup(repository => repository.GetProjectsByTopicId(topic.Id, CancellationToken.None))
            .ReturnsAsync(new List<Project> { project });

        var service = CreateService();

        var result = Assert.Single(await service.GetProjectsByTopicId(topic.Id, CancellationToken.None));

        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.Title, result.Title);
        Assert.Equal(topic.Id, Assert.Single(result.Topics).Id);
        _repository.Verify(repository => repository.GetProjectsByTopicId(topic.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetProjectsByTopicId_WrapsRepositoryFailure()
    {
        var topicId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjectsByTopicId(topicId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetProjectsByTopicId(topicId, token));

        Assert.Equal(nameof(ProjectService.GetProjectsByTopicId), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjectsByTopicId(topicId, token), Times.Once);
    }

    [Fact]
    public async Task GetProjectsByStatus_ReturnsMatchingProjectDtos()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };

        _repository.Setup(repository => repository.GetProjectsByStatus(ProjectStatus.Available, CancellationToken.None))
            .ReturnsAsync(new List<Project> { project });

        var service = CreateService();

        var result = Assert.Single(await service.GetProjectsByStatus(ProjectStatus.Available, CancellationToken.None));

        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.Title, result.Title);
        Assert.Equal(topic.Id, Assert.Single(result.Topics).Id);
        _repository.Verify(repository => repository.GetProjectsByStatus(ProjectStatus.Available, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetProjectsByStatus_WrapsRepositoryFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjectsByStatus(ProjectStatus.Available, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetProjectsByStatus(ProjectStatus.Available, token));

        Assert.Equal(nameof(ProjectService.GetProjectsByStatus), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjectsByStatus(ProjectStatus.Available, token), Times.Once);
    }

    [Fact]
    public async Task GetProjectById_ReturnsMappedProject()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };

        _repository.Setup(repository => repository.GetProjectById(project.Id, CancellationToken.None))
            .ReturnsAsync(project);

        var service = CreateService();

        var result = await service.GetProjectById(project.Id, CancellationToken.None);

        Assert.Equal(project.Id, result.Id);
        Assert.Equal(project.SupervisorId, result.SupervisorId);
        Assert.Equal(project.CreatedAt, result.CreatedAt);
        Assert.Equal(topic.Id, Assert.Single(result.Topics).Id);
        _repository.Verify(repository => repository.GetProjectById(project.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetProjectById_WrapsRepositoryFailure()
    {
        var projectId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjectById(projectId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetProjectById(projectId, token));

        Assert.Equal(nameof(ProjectService.GetProjectById), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjectById(projectId, token), Times.Once);
    }

    [Fact]
    public async Task CreateProject_TrimsTextAndGeneratesIdentity()
    {
        var topicId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var request = new CreateProject { SupervisorId = supervisorId, Title = "  New portal  ", Description = "  Research  ", TopicIds = [topicId, topicId] };

        _repository.Setup(repository => repository.CreateProject(It.IsAny<Project>(), request.TopicIds, CancellationToken.None))
            .ReturnsAsync((Project value, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken) => value);

        var service = CreateService();

        var result = await service.CreateProject(request, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(projectId, result.Id);
        Assert.Equal("New portal", result.Title);
        Assert.Equal("Research", result.Description);
        Assert.Equal(request.SupervisorId, result.SupervisorId);
        _repository.Verify(repository => repository.CreateProject(It.IsAny<Project>(), request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task CreateProject_WrapsRepositoryFailure()
    {
        var topicId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var request = new CreateProject { SupervisorId = supervisorId, Title = "  New portal  ", Description = "  Research  ", TopicIds = [topicId, topicId] };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.CreateProject(It.IsAny<Project>(), request.TopicIds, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.CreateProject(request, token));

        Assert.Equal(nameof(ProjectService.CreateProject), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.CreateProject(It.IsAny<Project>(), request.TopicIds, token), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_ChangesEditableFieldsAndPreservesOwnershipAndCreationTime()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor", ContactEmail = "supervisor@example.test", TopicInterests = [topic] };
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisor.UserId, Title = "Portal", Description = "Research", Topics = [topic] };
        var createdAt = project.CreatedAt;
        var supervisorId = project.SupervisorId;
        var request = new UpdateProject { Title = "  Updated  ", Description = "  Description  ", Status = ProjectStatus.Archived, TopicIds = [topic.Id] };
        request.ExternalUrl = "https://example.test";

        _repository.Setup(repository => repository.GetProjectById(project.Id, CancellationToken.None))
            .ReturnsAsync(project);
        _repository.Setup(repository => repository.UpdateProject(project, request.TopicIds, CancellationToken.None))
            .ReturnsAsync(project);

        var service = CreateService();

        var result = await service.UpdateProject(project.Id, request, CancellationToken.None);

        Assert.Equal("Updated", result.Title);
        Assert.Equal("Description", result.Description);
        Assert.Equal(request.ExternalUrl, result.ExternalUrl);
        Assert.Equal(ProjectStatus.Archived, result.Status);
        Assert.Equal(project.Id, result.Id);
        Assert.Equal(supervisorId, result.SupervisorId);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.True(result.UpdatedAt >= createdAt);
        _repository.Verify(repository => repository.GetProjectById(project.Id, CancellationToken.None), Times.Once);
        _repository.Verify(repository => repository.UpdateProject(project, request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_WrapsRepositoryFailure()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };
        var request = new UpdateProject { Title = "  Updated  ", Description = "  Description  ", Status = ProjectStatus.Archived, TopicIds = [topic.Id] };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjectById(project.Id, token))
            .ReturnsAsync(project);
        _repository.Setup(repository => repository.UpdateProject(project, request.TopicIds, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateProject(project.Id, request, token));

        Assert.Equal(nameof(ProjectService.UpdateProject), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjectById(project.Id, token), Times.Once);
        _repository.Verify(repository => repository.UpdateProject(project, request.TopicIds, token), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_WrapsLookupFailure()
    {
        var topicId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var request = new UpdateProject { Title = "  Updated  ", Description = "  Description  ", Status = ProjectStatus.Archived, TopicIds = [topicId] };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjectById(projectId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateProject(projectId, request, token));

        Assert.Equal(nameof(ProjectService.UpdateProject), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjectById(projectId, token), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_DeletesExistingProject()
    {
        var projectId = Guid.NewGuid();

        _repository.Setup(repository => repository.DeleteProject(projectId, CancellationToken.None))
            .ReturnsAsync(true);

        var service = CreateService();

        await service.DeleteProject(projectId, CancellationToken.None);

        _repository.Verify(repository => repository.DeleteProject(projectId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_WrapsRepositoryFailure()
    {
        var projectId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.DeleteProject(projectId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.DeleteProject(projectId, token));

        Assert.Equal(nameof(ProjectService.DeleteProject), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.DeleteProject(projectId, token), Times.Once);
    }

    [Fact]
    public async Task ValidateProjectSupervisor_AcceptsExistingReferences()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisorId = Guid.NewGuid();
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId, Title = "Portal", Description = "Research", Topics = [topic] };

        _repository.Setup(repository => repository.GetProjectById(project.Id, CancellationToken.None))
            .ReturnsAsync(project);

        var service = CreateService();

        await service.ValidateProjectSupervisor(project.Id, supervisorId, CancellationToken.None);

        _repository.Verify(repository => repository.GetProjectById(project.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ValidateProjectSupervisor_WrapsRepositoryFailure()
    {
        var supervisorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetProjectById(projectId, cancellation.Token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.ValidateProjectSupervisor(projectId, supervisorId, cancellation.Token));

        Assert.Equal(nameof(ProjectService.ValidateProjectSupervisor), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetProjectById(projectId, cancellation.Token), Times.Once);
    }
}
