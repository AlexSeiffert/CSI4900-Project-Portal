using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using Moq;

namespace API.Tests.Service;

public class SupervisorServiceTests
{
    private readonly Mock<ISupervisorRepository> _repository = new();

    private SupervisorService CreateService() => new(_repository.Object);

    [Fact]
    public async Task GetSupervisors_MapsProfilesAndTopics()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor", ContactEmail = "supervisor@example.test", TopicInterests = [topic] };

        _repository.Setup(repository => repository.GetSupervisors(CancellationToken.None))
            .ReturnsAsync(new List<Supervisor> { supervisor });

        var service = CreateService();

        var result = Assert.Single(await service.GetSupervisors(CancellationToken.None));

        Assert.Equal(supervisor.UserId, result.UserId);
        Assert.Equal(supervisor.DisplayName, result.DisplayName);
        Assert.Equal(topic.Id, Assert.Single(result.TopicInterests).Id);
        _repository.Verify(repository => repository.GetSupervisors(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetSupervisors_WrapsRepositoryFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetSupervisors(token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetSupervisors(token));

        Assert.Equal(nameof(SupervisorService.GetSupervisors), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetSupervisors(token), Times.Once);
    }

    [Fact]
    public async Task GetSupervisorById_ReturnsMatchingProfile()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor", ContactEmail = "supervisor@example.test", TopicInterests = [topic] };

        _repository.Setup(repository => repository.GetSupervisorById(supervisor.UserId, CancellationToken.None))
            .ReturnsAsync(supervisor);

        var service = CreateService();

        var result = await service.GetSupervisorById(supervisor.UserId, CancellationToken.None);

        Assert.Equal(supervisor.UserId, result.UserId);
        Assert.Equal(supervisor.DisplayName, result.DisplayName);
        Assert.Equal(topic.Id, Assert.Single(result.TopicInterests).Id);
        _repository.Verify(repository => repository.GetSupervisorById(supervisor.UserId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetSupervisorById_WrapsRepositoryFailure()
    {
        var supervisorId = Guid.NewGuid();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetSupervisorById(supervisorId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.GetSupervisorById(supervisorId, token));

        Assert.Equal(nameof(SupervisorService.GetSupervisorById), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetSupervisorById(supervisorId, token), Times.Once);
    }

    [Fact]
    public async Task UpdateSupervisor_UpdatesFieldsAndPreservesIdentity()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor", ContactEmail = "supervisor@example.test", TopicInterests = [topic] };
        var request = new UpdateSupervisor { DisplayName = "Updated", ContactEmail = "updated@example.test", AboutMe = "Research", ExternalUrl = "https://example.test", AcceptingInquiries = false, TopicIds = [topic.Id] };
        var before = supervisor.UpdatedAt;

        _repository.Setup(repository => repository.GetSupervisorById(supervisor.UserId, CancellationToken.None))
            .ReturnsAsync(supervisor);
        _repository.Setup(repository => repository.UpdateSupervisor(supervisor, request.TopicIds, CancellationToken.None))
            .ReturnsAsync(supervisor);

        var service = CreateService();

        var result = await service.UpdateSupervisor(supervisor.UserId, request, CancellationToken.None);

        Assert.Equal(supervisor.UserId, result.UserId);
        Assert.Equal(request.DisplayName, result.DisplayName);
        Assert.Equal(request.ContactEmail, result.ContactEmail);
        Assert.Equal(request.AboutMe, result.AboutMe);
        Assert.Equal(request.ExternalUrl, result.ExternalUrl);
        Assert.False(result.AcceptingInquiries);
        Assert.True(result.UpdatedAt >= before);
        _repository.Verify(repository => repository.GetSupervisorById(supervisor.UserId, CancellationToken.None), Times.Once);
        _repository.Verify(repository => repository.UpdateSupervisor(supervisor, request.TopicIds, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateSupervisor_WrapsRepositoryFailure()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor", ContactEmail = "supervisor@example.test", TopicInterests = [topic] };
        var request = new UpdateSupervisor { DisplayName = "Updated", ContactEmail = "updated@example.test", AboutMe = "Research", ExternalUrl = "https://example.test", AcceptingInquiries = false, TopicIds = [topic.Id] };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetSupervisorById(supervisor.UserId, token))
            .ReturnsAsync(supervisor);
        _repository.Setup(repository => repository.UpdateSupervisor(supervisor, request.TopicIds, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateSupervisor(supervisor.UserId, request, token));

        Assert.Equal(nameof(SupervisorService.UpdateSupervisor), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetSupervisorById(supervisor.UserId, token), Times.Once);
        _repository.Verify(repository => repository.UpdateSupervisor(supervisor, request.TopicIds, token), Times.Once);
    }

    [Fact]
    public async Task UpdateSupervisor_WrapsLookupFailure()
    {
        var topicId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var request = new UpdateSupervisor { DisplayName = "Updated", ContactEmail = "updated@example.test", AboutMe = "Research", ExternalUrl = "https://example.test", AcceptingInquiries = false, TopicIds = [topicId] };
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetSupervisorById(supervisorId, token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<ServiceException>(() => service.UpdateSupervisor(supervisorId, request, token));

        Assert.Equal(nameof(SupervisorService.UpdateSupervisor), exception.Operation);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        _repository.Verify(repository => repository.GetSupervisorById(supervisorId, token), Times.Once);
    }
}
