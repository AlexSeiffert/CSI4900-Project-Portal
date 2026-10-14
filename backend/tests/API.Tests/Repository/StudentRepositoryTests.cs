using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repository;

public class StudentRepositoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetStudents_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetStudents(token));
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
    public async Task GetStudentById_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetStudentById(c.Student.UserId, token));
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
    public async Task UpdateStudent_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateStudent(c.Student, new[] { c.Topic.Id }, token));
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
    public async Task GetStudents_OrdersProfilesAndIncludesTopicsWithoutTracking()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
        var result = await repository.GetStudents();
        Assert.Equal(new[] { c.OtherStudent.UserId, c.Student.UserId }, result.Select(x => x.UserId));
        Assert.Equal(c.Topic.Id, Assert.Single(result[1].TopicInterests).Id);
        Assert.Empty(c.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetStudentById_LoadsTrackedProfileOrReturnsNull()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
        var result = await repository.GetStudentById(c.Student.UserId);
        Assert.NotNull(result);
        Assert.Equal(c.Student.UserId, result.UserId);
        Assert.Equal(c.Topic.Id, Assert.Single(result.TopicInterests).Id);
        Assert.Equal(EntityState.Unchanged, c.Db.Entry(result).State);
        Assert.Null(await repository.GetStudentById(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateStudent_PersistsFieldsAndReplacesTopics()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
        var profile = (await repository.GetStudentById(c.Student.UserId))!;
        profile.DisplayName = "Updated";
        Assert.Same(profile, await repository.UpdateStudent(profile, [c.OtherTopic.Id, c.OtherTopic.Id]));
        c.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetStudentById(profile.UserId))!;
        Assert.Equal("Updated", persisted.DisplayName);
        Assert.Equal(c.OtherTopic.Id, Assert.Single(persisted.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateStudent_RejectsTopicsRemovedBeforeWrite()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
        var profile = (await repository.GetStudentById(c.Student.UserId))!;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateStudent(profile, [Guid.NewGuid()]));
        Assert.Equal(ErrorKind.Conflict, exception.Kind);
        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateStudent_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(c.Db);
        var entity = (await repository.GetStudentById(c.Student.UserId))!;
        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateStudent(entity, new[] { c.Topic.Id }));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorKind.Conflict : ErrorKind.Unexpected, exception.Kind);
    }
}
