using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repository;

public class SupervisorRepositoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetSupervisors_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetSupervisors(token));
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
    public async Task GetSupervisorById_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetSupervisorById(c.Supervisor.UserId, token));
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
    public async Task UpdateSupervisor_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateSupervisor(c.Supervisor, new[] { c.Topic.Id }, token));
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
    public async Task GetSupervisors_OrdersProfilesAndIncludesTopicsWithoutTracking()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
        var result = await repository.GetSupervisors();
        Assert.Equal(new[] { c.OtherSupervisor.UserId, c.Supervisor.UserId }, result.Select(x => x.UserId));
        Assert.Equal(c.Topic.Id, Assert.Single(result[1].TopicInterests).Id);
        Assert.Empty(c.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetSupervisorById_LoadsTrackedProfileOrReturnsNull()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
        var result = await repository.GetSupervisorById(c.Supervisor.UserId);
        Assert.NotNull(result);
        Assert.Equal(c.Supervisor.UserId, result.UserId);
        Assert.Equal(c.Topic.Id, Assert.Single(result.TopicInterests).Id);
        Assert.Equal(EntityState.Unchanged, c.Db.Entry(result).State);
        Assert.Null(await repository.GetSupervisorById(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateSupervisor_PersistsFieldsAndReplacesTopics()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
        var profile = (await repository.GetSupervisorById(c.Supervisor.UserId))!;
        profile.DisplayName = "Updated";
        Assert.Same(profile, await repository.UpdateSupervisor(profile, [c.OtherTopic.Id, c.OtherTopic.Id]));
        c.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetSupervisorById(profile.UserId))!;
        Assert.Equal("Updated", persisted.DisplayName);
        Assert.Equal(c.OtherTopic.Id, Assert.Single(persisted.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateSupervisor_RejectsTopicsRemovedBeforeWrite()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
        var profile = (await repository.GetSupervisorById(c.Supervisor.UserId))!;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateSupervisor(profile, [Guid.NewGuid()]));
        Assert.Equal(ErrorKind.Conflict, exception.Kind);
        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateSupervisor_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SupervisorRepository(c.Db);
        var entity = (await repository.GetSupervisorById(c.Supervisor.UserId))!;
        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateSupervisor(entity, new[] { c.Topic.Id }));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorKind.Conflict : ErrorKind.Unexpected, exception.Kind);
    }
}
