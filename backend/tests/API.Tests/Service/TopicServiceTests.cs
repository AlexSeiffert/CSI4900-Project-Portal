using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services;
using Moq;

namespace API.Tests.Service;

public class TopicServiceTests
{
    private readonly Mock<ITopicRepository> _repository = new();

    private TopicService CreateService()
    {
        return new(_repository.Object);
    }

    [Fact]
    public async Task GetTopics_ReturnsMappedTopics()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };

        _repository.Setup(repository => repository.GetTopics(CancellationToken.None))
            .ReturnsAsync(new List<Topic> { topic });

        var service = CreateService();

        var result = Assert.Single(await service.GetTopics(CancellationToken.None));

        Assert.Equal(topic.Id, result.Id);
        Assert.Equal(topic.Name, result.Name);
        _repository.Verify(repository => repository.GetTopics(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetTopics_PropagatesRepositoryFailure()
    {
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = new RepositoryException("dependency", new InvalidOperationException("Database failed"));

        _repository.Setup(repository => repository.GetTopics(token))
            .ThrowsAsync(original);

        var service = CreateService();

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => service.GetTopics(token));

        Assert.Same(original, exception);
        _repository.Verify(repository => repository.GetTopics(token), Times.Once);
    }
}
