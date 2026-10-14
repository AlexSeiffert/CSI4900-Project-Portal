using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repository;

public class TopicRepositoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetTopics_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new TopicRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetTopics(token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetTopicsByIds_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new TopicRepository(c.Db);
        using var cancellation = new CancellationTokenSource();
        if (cancelled)
        {
            cancellation.Cancel();
        }
        else
        {
            await c.Db.DisposeAsync();
        }
        var token = cancellation.Token;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetTopicsByIds(new[] { c.Topic.Id }, token));
        if (cancelled)
        {
            Assert.Equal(ErrorKind.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorKind.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Fact]
    public async Task GetTopics_ReturnsAlphabeticalTopicsWithoutTracking()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new TopicRepository(c.Db);
        var result = await repository.GetTopics();
        Assert.Equal(new[] { c.OtherTopic.Id, c.Topic.Id }, result.Select(x => x.Id));
        Assert.Empty(c.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetTopicsByIds_ReturnsOnlyExistingDistinctMatches()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new TopicRepository(c.Db);
        var result = await repository.GetTopicsByIds([c.Topic.Id, c.Topic.Id, Guid.NewGuid()]);
        Assert.Equal(c.Topic.Id, Assert.Single(result).Id);
        Assert.Empty(await repository.GetTopicsByIds([]));
        Assert.Empty(c.Db.ChangeTracker.Entries());
    }
}
