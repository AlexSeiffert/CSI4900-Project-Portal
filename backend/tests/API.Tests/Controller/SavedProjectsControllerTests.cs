using API.Controllers;
using API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using System.Net;
using API.Models.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using API.Tests.Support;
using static API.Tests.Support.ApiTestData;

namespace API.Tests.Controller;

public class SavedProjectsControllerTests
{
    [Fact]
    public async Task SimultaneousSavesBothSucceedAndCreateOnlyOneRelationship()
    {
        await using var factory = new ConcurrentSaveFactory();
        using var client = await factory.CreateSeededClient();
        var project = await CreateProject(client);
        var path = $"/api/v1/students/{PortalApiFactory.StudentId}/saved-projects/{project.Id}";
        var responses = await Task.WhenAll(client.PutAsync(path, null), client.PutAsync(path, null));
        Assert.All(responses, response => Assert.Equal(HttpStatusCode.NoContent, response.StatusCode));
        var saved = await Read<List<DTOs.Project>>(await client.GetAsync($"/api/v1/students/{PortalApiFactory.StudentId}/saved-projects"));
        Assert.Equal(project.Id, Assert.Single(saved).Id);
    }

    [Fact]
    public async Task SavedProjectsCanBeRemovedIdempotentlyAndRequireExistingProfilesAndProjects()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var project = await CreateProject(client);
        var path = $"/api/v1/students/{PortalApiFactory.StudentId}/saved-projects/{project.Id}";
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsync(path, null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(path)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(path)).StatusCode);
        Assert.Empty(await Read<List<DTOs.Project>>(await client.GetAsync($"/api/v1/students/{PortalApiFactory.StudentId}/saved-projects")));
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync($"/api/v1/students/{PortalApiFactory.StudentId}/saved-projects/{Guid.NewGuid()}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsync($"/api/v1/students/{Guid.NewGuid()}/saved-projects/{project.Id}", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/students/{Guid.NewGuid()}/saved-projects")).StatusCode);
    }

    private class ConcurrentSaveFactory : PortalApiFactory
    {
        private readonly SaveBarrier _barrier = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.AddDbContext<ApplicationDbContext>(options =>
                options.AddInterceptors(_barrier)));
        }
    }

    private class SaveBarrier : SaveChangesInterceptor
    {
        private int _waiting;
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context!.ChangeTracker.Entries().Any(entry =>
                    entry.Metadata.GetTableName() == "student_saved_projects" && entry.State == EntityState.Added))
            {
                if (Interlocked.Increment(ref _waiting) == 2)
                {
                    _release.SetResult();
                }
                await _release.Task.WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);
            }
            return result;
        }
    }

    [Fact]
    public async Task GetSavedProjects_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new SavedProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SavedProjectsController(service);
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var result = await controller.GetSavedProjects(studentId, cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetSavedProjects", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(studentId, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task GetSavedProjects_PropagatesOrchestrationExceptionsToTheHandler(ErrorKind kind)
    {
        var service = new SavedProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SavedProjectsController(service);
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var original = new OrchestrationException("GetSavedProjects", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.GetSavedProjects(studentId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task SaveProject_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new SavedProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SavedProjectsController(service);
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var result = await controller.SaveProject(studentId, projectId, cancellation.Token);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("SaveProject", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Equal(2, service.LastArguments.Length);
        Assert.Equal(studentId, service.LastArguments[0]);
        Assert.Equal(projectId, service.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task SaveProject_PropagatesOrchestrationExceptionsToTheHandler(ErrorKind kind)
    {
        var service = new SavedProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SavedProjectsController(service);
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var original = new OrchestrationException("SaveProject", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.SaveProject(studentId, projectId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task RemoveSavedProject_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new SavedProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SavedProjectsController(service);
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var result = await controller.RemoveSavedProject(studentId, projectId, cancellation.Token);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("RemoveSavedProject", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Equal(2, service.LastArguments.Length);
        Assert.Equal(studentId, service.LastArguments[0]);
        Assert.Equal(projectId, service.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task RemoveSavedProject_PropagatesOrchestrationExceptionsToTheHandler(ErrorKind kind)
    {
        var service = new SavedProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SavedProjectsController(service);
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var original = new OrchestrationException("RemoveSavedProject", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.RemoveSavedProject(studentId, projectId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }
}
