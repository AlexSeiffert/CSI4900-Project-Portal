using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Repository;

public class StudentRepositoryTests
{
    [Fact]
    public async Task GetStudents_OrdersProfilesAndIncludesTopicsWithoutTracking()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(context.Db);

        var result = await repository.GetStudents();

        Assert.Equal(new[] { context.OtherStudent.UserId, context.Student.UserId }, result.Select(x => x.UserId));
        Assert.Equal(context.Topic.Id, Assert.Single(result[1].TopicInterests).Id);
        Assert.Equal(context.Inquiry.Id, Assert.Single(result[1].Inquiries).Id);
        Assert.Empty(context.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetStudents_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Student>())
            .Throws(original);

        var repository = new StudentRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetStudents(CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Student>(), Times.Once);
    }

    [Fact]
    public async Task GetStudentById_LoadsTrackedProfile()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(context.Db);

        var result = await repository.GetStudentById(context.Student.UserId);

        Assert.NotNull(result);
        Assert.Equal(context.Student.UserId, result.UserId);
        Assert.Equal(context.Topic.Id, Assert.Single(result.TopicInterests).Id);
        Assert.Equal(context.Inquiry.Id, Assert.Single(result.Inquiries).Id);
        Assert.Equal(EntityState.Unchanged, context.Db.Entry(result).State);
    }

    [Fact]
    public async Task GetStudentById_LoadsSavedProjects()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        await new SavedProjectRepository(context.Db).SaveProject(context.Student.UserId, context.Project.Id);
        context.Db.ChangeTracker.Clear();

        var result = await new StudentRepository(context.Db).GetStudentById(context.Student.UserId);

        Assert.Equal(context.Project.Id, Assert.Single(result!.SavedProjects).Id);
    }

    [Fact]
    public async Task GetStudentById_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Student>())
            .Throws(original);

        var repository = new StudentRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetStudentById(context.Student.UserId, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Student>(), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_PersistsFieldsAndReplacesTopics()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(context.Db);
        var profile = (await repository.GetStudentById(context.Student.UserId))!;
        profile.DisplayName = "Updated";

        Assert.Same(profile, await repository.UpdateStudent(profile, [context.OtherTopic.Id, context.OtherTopic.Id]));
        context.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetStudentById(profile.UserId))!;
        Assert.Equal("Updated", persisted.DisplayName);
        Assert.Equal(context.OtherTopic.Id, Assert.Single(persisted.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateStudent_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Topic>())
            .Throws(original);

        var repository = new StudentRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateStudent(context.Student, new[] { context.Topic.Id }, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Topic>(), Times.Once);
    }

    [Fact]
    public async Task UpdateStudent_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new StudentRepository(context.Db);
        var entity = (await repository.GetStudentById(context.Student.UserId))!;
        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateStudent(entity, new[] { context.Topic.Id }));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
