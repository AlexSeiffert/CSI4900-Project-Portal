using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Repository;

public class SupervisorRepositoryTests
{
    [Fact]
    public async Task GetSupervisors_OrdersProfilesAndIncludesTopicsWithoutTracking()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(context.Db);

        var result = await repository.GetSupervisors();

        Assert.Equal(new[] { context.OtherSupervisor.UserId, context.Supervisor.UserId }, result.Select(x => x.UserId));
        Assert.Equal(context.Topic.Id, Assert.Single(result[1].TopicInterests).Id);
        Assert.Empty(context.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetSupervisors_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Supervisor>())
            .Throws(original);

        var repository = new SupervisorRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetSupervisors(CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Supervisor>(), Times.Once);
    }

    [Fact]
    public async Task GetSupervisorById_LoadsTrackedProfile()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(context.Db);

        var result = await repository.GetSupervisorById(context.Supervisor.UserId);

        Assert.NotNull(result);
        Assert.Equal(context.Supervisor.UserId, result.UserId);
        Assert.Equal(context.Topic.Id, Assert.Single(result.TopicInterests).Id);
        Assert.Equal(EntityState.Unchanged, context.Db.Entry(result).State);
    }

    [Fact]
    public async Task GetSupervisorById_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Supervisor>())
            .Throws(original);

        var repository = new SupervisorRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetSupervisorById(context.Supervisor.UserId, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Supervisor>(), Times.Once);
    }

    [Fact]
    public async Task UpdateSupervisor_PersistsFieldsAndReplacesTopics()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(context.Db);
        var profile = (await repository.GetSupervisorById(context.Supervisor.UserId))!;
        profile.DisplayName = "Updated";

        Assert.Same(profile, await repository.UpdateSupervisor(profile, [context.OtherTopic.Id, context.OtherTopic.Id]));
        context.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetSupervisorById(profile.UserId))!;
        Assert.Equal("Updated", persisted.DisplayName);
        Assert.Equal(context.OtherTopic.Id, Assert.Single(persisted.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateSupervisor_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Topic>())
            .Throws(original);

        var repository = new SupervisorRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateSupervisor(context.Supervisor, new[] { context.Topic.Id }, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Topic>(), Times.Once);
    }

    [Fact]
    public async Task UpdateSupervisor_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(context.Db);
        var entity = (await repository.GetSupervisorById(context.Supervisor.UserId))!;
        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateSupervisor(entity, new[] { context.Topic.Id }));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
