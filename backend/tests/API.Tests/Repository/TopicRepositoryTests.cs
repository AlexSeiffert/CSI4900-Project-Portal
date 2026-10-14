using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Repository;

public class TopicRepositoryTests
{
    [Fact]
    public async Task GetTopics_ReturnsAlphabeticalTopicsWithoutTracking()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new TopicRepository(context.Db);

        var result = await repository.GetTopics();

        Assert.Equal(new[] { context.OtherTopic.Id, context.Topic.Id }, result.Select(x => x.Id));
        Assert.Equal(context.Project.Id, Assert.Single(result[1].Projects).Id);
        Assert.Equal(context.Student.UserId, Assert.Single(result[1].Students).UserId);
        Assert.Equal(context.Supervisor.UserId, Assert.Single(result[1].Supervisors).UserId);
        Assert.Empty(context.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetTopics_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Topic>())
            .Throws(original);

        var repository = new TopicRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetTopics(CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Topic>(), Times.Once);
    }
}
