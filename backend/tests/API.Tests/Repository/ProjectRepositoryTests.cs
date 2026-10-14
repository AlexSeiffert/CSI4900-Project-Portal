using API.Exceptions;
using API.Models;
using API.Repositories;
using API.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repository;

public class ProjectRepositoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetProjects_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjects(token));
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
    public async Task GetProjectsBySupervisorId_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectsBySupervisorId(c.Supervisor.UserId, token));
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
    public async Task GetProjectsByTopicId_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectsByTopicId(c.Topic.Id, token));
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
    public async Task GetProjectsByStatus_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectsByStatus(ProjectStatus.Available, token));
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
    public async Task GetProjectById_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.GetProjectById(c.Project.Id, token));
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
    public async Task CreateProject_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateProject(new Project { Id = Guid.NewGuid(), SupervisorId = c.Supervisor.UserId, Title = "New", Description = "Research" }, new[] { c.Topic.Id }, token));
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
    public async Task UpdateProject_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateProject(c.Project, new[] { c.Topic.Id }, token));
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
    public async Task DeleteProject_WrapsCancellationAndContextFailures(bool cancelled)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
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
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.DeleteProject(c.Project.Id, token));
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
    public async Task GetProjects_ReturnsNewestFirstWithTopicsWithoutTracking()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var result = await repository.GetProjects();
        Assert.Equal(new[] { c.Project.Id, c.OtherProject.Id }, result.Select(x => x.Id));
        Assert.Equal(c.Topic.Id, Assert.Single(result[0].Topics).Id);
        Assert.Empty(c.Db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_ReturnsOnlyMatches()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var result = Assert.Single(await repository.GetProjectsBySupervisorId(c.Supervisor.UserId));
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Empty(await repository.GetProjectsBySupervisorId(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetProjectsByTopicId_ReturnsOnlyMatches()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var result = Assert.Single(await repository.GetProjectsByTopicId(c.Topic.Id));
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Empty(await repository.GetProjectsByTopicId(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetProjectsByStatus_ReturnsOnlyMatches()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var result = Assert.Single(await repository.GetProjectsByStatus(ProjectStatus.Available));
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Empty(await repository.GetProjectsByStatus(ProjectStatus.Unavailable));
    }

    [Fact]
    public async Task GetProjectById_LoadsTrackedProjectAndTopicsOrReturnsNull()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var result = await repository.GetProjectById(c.Project.Id);
        Assert.NotNull(result);
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Equal(EntityState.Unchanged, c.Db.Entry(result).State);
        Assert.Null(await repository.GetProjectById(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateProject_PersistsProjectAndDistinctTopics()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = c.Supervisor.UserId, Title = "New", Description = "Research" };
        Assert.Same(project, await repository.CreateProject(project, [c.Topic.Id, c.Topic.Id]));
        c.Db.ChangeTracker.Clear();
        var persisted = await c.Db.Projects.Include(x => x.Topics).SingleAsync(x => x.Id == project.Id);
        Assert.Equal("New", persisted.Title);
        Assert.Equal(c.Topic.Id, Assert.Single(persisted.Topics).Id);
    }

    [Fact]
    public async Task UpdateProject_PersistsChangesAndReplacesTopics()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var project = (await repository.GetProjectById(c.Project.Id))!;
        project.Title = "Updated";
        Assert.Same(project, await repository.UpdateProject(project, [c.OtherTopic.Id]));
        c.Db.ChangeTracker.Clear();
        var persisted = (await repository.GetProjectById(project.Id))!;
        Assert.Equal("Updated", persisted.Title);
        Assert.Equal(c.OtherTopic.Id, Assert.Single(persisted.Topics).Id);
    }

    [Fact]
    public async Task DeleteProject_RemovesProjectAndSavesButPreservesInquiry()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        await new SavedProjectRepository(c.Db).SaveProject(c.Student.UserId, c.Project.Id);
        c.Db.ChangeTracker.Clear();
        Assert.True(await repository.DeleteProject(c.Project.Id));
        c.Db.ChangeTracker.Clear();
        Assert.Null(await c.Db.Projects.FindAsync(c.Project.Id));
        var inquiry = await c.Db.Inquiries.SingleAsync(x => x.Id == c.Inquiry.Id);
        Assert.Null(inquiry.ProjectId);
        Assert.Equal(c.Student.UserId, inquiry.StudentId);
        Assert.Empty(await new SavedProjectRepository(c.Db).GetSavedProjects(c.Student.UserId));
        Assert.False(await repository.DeleteProject(c.Project.Id));
    }

    [Fact]
    public async Task CreateProject_RejectsTopicsRemovedBeforeWrite()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var project = (await repository.GetProjectById(c.Project.Id))!;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateProject(project, [Guid.NewGuid()]));
        Assert.Equal(ErrorType.Conflict, exception.Kind);
        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    [Fact]
    public async Task UpdateProject_RejectsTopicsRemovedBeforeWrite()
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var project = (await repository.GetProjectById(c.Project.Id))!;
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateProject(project, [Guid.NewGuid()]));
        Assert.Equal(ErrorType.Conflict, exception.Kind);
        Assert.IsType<DbUpdateConcurrencyException>(exception.InnerException);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CreateProject_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);

        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateProject(new Project { Id = Guid.NewGuid(), SupervisorId = c.Supervisor.UserId, Title = "New", Description = "Research" }, new[] { c.Topic.Id }));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorType.Conflict : ErrorType.Unexpected, exception.Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateProject_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);
        var entity = (await repository.GetProjectById(c.Project.Id))!;
entity.Title = "Updated";
        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.UpdateProject(entity, new[] { c.Topic.Id }));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorType.Conflict : ErrorType.Unexpected, exception.Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeleteProject_PreservesSaveFailureAndClassifiesConflict(bool conflict)
    {
        await using var c = await RepositoryTestContext.CreateAsync();
        var repository = new ProjectRepository(c.Db);

        Exception original = conflict ? new DbUpdateConcurrencyException("Concurrent write") : new InvalidOperationException("Save failed");
        c.FailSavesWith(original);
        var exception = await Assert.ThrowsAsync<RepositoryException>(() => repository.DeleteProject(c.Project.Id));
        Assert.Same(original, exception.InnerException);
        Assert.Equal(conflict ? ErrorType.Conflict : ErrorType.Unexpected, exception.Kind);
    }
}
