using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repository;

public class SavedProjectRepositoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetSavedProjects_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetSavedProjects(c.Student.UserId, token));
        if (cancelled)
        {
            Assert.Equal(ErrorType.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorType.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.SaveProject(c.Student.UserId, c.Project.Id, token));
        if (cancelled)
        {
            Assert.Equal(ErrorType.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorType.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveSavedProject_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.RemoveSavedProject(c.Student.UserId, c.Project.Id, token));
        if (cancelled)
        {
            Assert.Equal(ErrorType.Cancelled, exception.Kind);
            var original = Assert.IsAssignableFrom<OperationCanceledException>(exception.InnerException);
            Assert.Equal(token, original.CancellationToken);
        }
        else
        {
            Assert.Equal(ErrorType.Unexpected, exception.Kind);
            Assert.IsType<ObjectDisposedException>(exception.InnerException);
        }
    }

    [Fact]
    public async Task GetSavedProjects_IsolatesStudentsOrdersTitlesAndIncludesTopics()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);
        await repository.SaveProject(c.Student.UserId, c.Project.Id);
        await repository.SaveProject(c.Student.UserId, c.OtherProject.Id);
        c.Db.ChangeTracker.Clear();
        var result = await repository.GetSavedProjects(c.Student.UserId);
        Assert.Equal(new[] { c.OtherProject.Id, c.Project.Id }, result.Select(x => x.Id));
        Assert.Equal(c.Topic.Id, Assert.Single(result[1].Topics).Id);
        Assert.Empty(await repository.GetSavedProjects(c.OtherStudent.UserId));
        Assert.Empty(await repository.GetSavedProjects(Guid.NewGuid()));
        Assert.Empty(c.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task SaveProject_IsIdempotentAndPersistsSavedTimestamp()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);
        await repository.SaveProject(c.Student.UserId, c.Project.Id);
        await repository.SaveProject(c.Student.UserId, c.Project.Id);
        c.Db.ChangeTracker.Clear();
        var relationship = Assert.Single(await c.Db.Set<Dictionary<string, object>>("student_saved_projects").ToListAsync());
        Assert.Equal(c.Student.UserId, relationship["student_id"]);
        Assert.Equal(c.Project.Id, relationship["project_id"]);
        Assert.IsType<DateTime>(relationship["saved_at"]);
        Assert.Equal(c.Project.Id, Assert.Single(await repository.GetSavedProjects(c.Student.UserId)).Id);
    }

    [Fact]
    public async Task RemoveSavedProject_RemovesOnlyRequestedRelationshipAndIsIdempotent()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);
        await repository.SaveProject(c.Student.UserId, c.Project.Id);
        await repository.SaveProject(c.Student.UserId, c.OtherProject.Id);
        await repository.RemoveSavedProject(c.Student.UserId, c.Project.Id);
        await repository.RemoveSavedProject(c.Student.UserId, c.Project.Id);
        c.Db.ChangeTracker.Clear();
        Assert.Equal(c.OtherProject.Id, Assert.Single(await repository.GetSavedProjects(c.Student.UserId)).Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveProject_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);

        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.SaveProject(c.Student.UserId, c.Project.Id));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorType.Conflict : ErrorType.Unexpected, exception.Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RemoveSavedProject_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(c.Db);
        await repository.SaveProject(c.Student.UserId, c.Project.Id);
        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.RemoveSavedProject(c.Student.UserId, c.Project.Id));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorType.Conflict : ErrorType.Unexpected, exception.Kind);
    }
}
