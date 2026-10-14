using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Repository;

public class SavedProjectRepositoryTests
{
    [Fact]
    public async Task GetSavedProjects_IsolatesStudentsOrdersTitlesAndIncludesTopics()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(context.Db);
        await repository.SaveProject(context.Student.UserId, context.Project.Id);
        await repository.SaveProject(context.Student.UserId, context.OtherProject.Id);
        context.Db.ChangeTracker.Clear();

        var result = await repository.GetSavedProjects(context.Student.UserId);

        Assert.Equal(new[] { context.OtherProject.Id, context.Project.Id }, result.Select(x => x.Id));
        Assert.Equal(context.Topic.Id, Assert.Single(result[1].Topics).Id);
        Assert.Empty(context.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetSavedProjects_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Student>())
            .Throws(original);

        var repository = new SavedProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetSavedProjects(context.Student.UserId, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Student>(), Times.Once);
    }

    [Fact]
    public async Task SaveProject_PersistsRelationshipAndTimestamp()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(context.Db);

        await repository.SaveProject(context.Student.UserId, context.Project.Id);
        context.Db.ChangeTracker.Clear();

        var relationship = Assert.Single(await context.Db.Set<Dictionary<string, object>>("student_saved_projects").ToListAsync());

        Assert.Equal(context.Student.UserId, relationship["student_id"]);
        Assert.Equal(context.Project.Id, relationship["project_id"]);
        Assert.IsType<DateTime>(relationship["saved_at"]);
        Assert.Equal(context.Project.Id, Assert.Single(await repository.GetSavedProjects(context.Student.UserId)).Id);
    }

    [Fact]
    public async Task SaveProject_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Dictionary<string, object>>("student_saved_projects"))
            .Throws(original);

        var repository = new SavedProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.SaveProject(context.Student.UserId, context.Project.Id, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Dictionary<string, object>>("student_saved_projects"), Times.Once);
    }

    [Fact]
    public async Task SaveProject_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(context.Db);

        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.SaveProject(context.Student.UserId, context.Project.Id));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RemoveSavedProject_RemovesRequestedRelationship()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(context.Db);

        await repository.SaveProject(context.Student.UserId, context.Project.Id);
        await repository.SaveProject(context.Student.UserId, context.OtherProject.Id);
        await repository.RemoveSavedProject(context.Student.UserId, context.Project.Id);
        
        context.Db.ChangeTracker.Clear();
        Assert.Equal(context.OtherProject.Id, Assert.Single(await repository.GetSavedProjects(context.Student.UserId)).Id);
    }

    [Fact]
    public async Task RemoveSavedProject_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Dictionary<string, object>>("student_saved_projects"))
            .Throws(original);

        var repository = new SavedProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.RemoveSavedProject(context.Student.UserId, context.Project.Id, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Dictionary<string, object>>("student_saved_projects"), Times.Once);
    }

    [Fact]
    public async Task RemoveSavedProject_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new SavedProjectRepository(context.Db);
        await repository.SaveProject(context.Student.UserId, context.Project.Id);
        context.DbMock.Invocations.Clear();
        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.RemoveSavedProject(context.Student.UserId, context.Project.Id));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
