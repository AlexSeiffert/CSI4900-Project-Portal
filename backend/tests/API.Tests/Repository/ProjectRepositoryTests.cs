using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Repository;

public class ProjectRepositoryTests
{
    [Fact]
    public async Task GetProjects_ReturnsNewestFirstWithTopicsWithoutTracking()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);

        var result = await repository.GetProjects();

        Assert.Equal(new[] { context.Project.Id, context.OtherProject.Id }, result.Select(x => x.Id));
        Assert.Equal(context.Topic.Id, Assert.Single(result[0].Topics).Id);
        Assert.Empty(context.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetProjects_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Project>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjects(CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Project>(), Times.Once);
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_ReturnsOnlyMatches()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);

        var result = Assert.Single(await repository.GetProjectsBySupervisorId(context.Supervisor.UserId));

        Assert.Equal(context.Project.Id, result.Id);
        Assert.Equal(context.Topic.Id, Assert.Single(result.Topics).Id);
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Project>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectsBySupervisorId(context.Supervisor.UserId, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Project>(), Times.Once);
    }

    [Fact]
    public async Task GetProjectsByTopicId_ReturnsOnlyMatches()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);

        var result = Assert.Single(await repository.GetProjectsByTopicId(context.Topic.Id));

        Assert.Equal(context.Project.Id, result.Id);
        Assert.Equal(context.Topic.Id, Assert.Single(result.Topics).Id);
    }

    [Fact]
    public async Task GetProjectsByTopicId_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Project>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectsByTopicId(context.Topic.Id, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Project>(), Times.Once);
    }

    [Fact]
    public async Task GetProjectsByStatus_ReturnsOnlyMatches()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);

        var result = Assert.Single(await repository.GetProjectsByStatus(ProjectStatus.Available));

        Assert.Equal(context.Project.Id, result.Id);
        Assert.Equal(context.Topic.Id, Assert.Single(result.Topics).Id);
    }

    [Fact]
    public async Task GetProjectsByStatus_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Project>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectsByStatus(ProjectStatus.Available, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Project>(), Times.Once);
    }

    [Fact]
    public async Task GetProjectById_LoadsTrackedProjectAndTopics()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);

        var result = await repository.GetProjectById(context.Project.Id);

        Assert.NotNull(result);
        Assert.Equal(context.Project.Id, result.Id);
        Assert.Equal(context.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Equal(EntityState.Unchanged, context.Db.Entry(result).State);
    }

    [Fact]
    public async Task GetProjectById_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Project>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectById(context.Project.Id, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Project>(), Times.Once);
    }

    [Fact]
    public async Task CreateProject_PersistsProjectAndDistinctTopics()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = context.Supervisor.UserId, Title = "New", Description = "Research" };

        Assert.Same(project, await repository.CreateProject(project, [context.Topic.Id, context.Topic.Id]));
        context.Db.ChangeTracker.Clear();
        var persisted = await context.Db.Projects.Include(x => x.Topics).SingleAsync(x => x.Id == project.Id);
        Assert.Equal("New", persisted.Title);
        Assert.Equal(context.Topic.Id, Assert.Single(persisted.Topics).Id);
    }

    [Fact]
    public async Task CreateProject_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Topic>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateProject(new Project { Id = Guid.NewGuid(), SupervisorId = context.Supervisor.UserId, Title = "New", Description = "Research" }, new[] { context.Topic.Id }, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Topic>(), Times.Once);
    }

    [Fact]
    public async Task CreateProject_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);

        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateProject(new Project { Id = Guid.NewGuid(), SupervisorId = context.Supervisor.UserId, Title = "New", Description = "Research" }, new[] { context.Topic.Id }));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_PersistsChangesAndReplacesTopics()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);
        var project = (await repository.GetProjectById(context.Project.Id))!;
        project.Title = "Updated";

        Assert.Same(project, await repository.UpdateProject(project, [context.OtherTopic.Id]));
        context.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetProjectById(project.Id))!;
        Assert.Equal("Updated", persisted.Title);
        Assert.Equal(context.OtherTopic.Id, Assert.Single(persisted.Topics).Id);
    }

    [Fact]
    public async Task UpdateProject_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Topic>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateProject(context.Project, new[] { context.Topic.Id }, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Topic>(), Times.Once);
    }

    [Fact]
    public async Task UpdateProject_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);
        var entity = (await repository.GetProjectById(context.Project.Id))!;
        entity.Title = "Updated";
        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateProject(entity, new[] { context.Topic.Id }));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_RemovesProjectAndSavesButPreservesInquiry()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);
        await new SavedProjectRepository(context.Db).SaveProject(context.Student.UserId, context.Project.Id);
        context.Db.ChangeTracker.Clear();
        Assert.True(await repository.DeleteProject(context.Project.Id));
        context.Db.ChangeTracker.Clear();
        Assert.Null(await context.Db.Projects.FindAsync(context.Project.Id));
        var inquiry = await context.Db.Inquiries.SingleAsync(x => x.Id == context.Inquiry.Id);
        Assert.Null(inquiry.ProjectId);
        Assert.Equal(context.Student.UserId, inquiry.StudentId);
        Assert.Empty(await new SavedProjectRepository(context.Db).GetSavedProjects(context.Student.UserId));
    }

    [Fact]
    public async Task DeleteProject_WrapsContextFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var original = new InvalidOperationException("Context failed");

        context.DbMock.Setup(db => db.Set<Project>())
            .Throws(original);

        var repository = new ProjectRepository(context.Db);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.DeleteProject(context.Project.Id, CancellationToken.None));

        Assert.Equal(ErrorType.Unexpected, exception.Type);
        Assert.Same(original, exception.InnerException);
        context.DbMock.Verify(db => db.Set<Project>(), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_WrapsSaveFailure()
    {
        await using var context = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(context.Db);

        var original = new InvalidOperationException("Save failed");

        context.DbMock.Setup(db => db.SaveChangesAsync(CancellationToken.None))
            .ThrowsAsync(original);

        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.DeleteProject(context.Project.Id));

        Assert.Same(original, exception.InnerException);
        Assert.Equal(ErrorType.Unexpected, exception.Type);
        context.DbMock.Verify(db => db.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}
