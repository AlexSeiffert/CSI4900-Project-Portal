using System.Net;
using uoProjects.Models;
using uoProjects.Services;
using uoProjects.Tests.Support;

namespace uoProjects.Tests.Services;

public class ProjectsServiceTests : ServiceTestBase
{
    private readonly ProjectsService service;

    public ProjectsServiceTests() => service = new ProjectsService(Http, Notifications);

    [Fact]
    public async Task DeleteAsync_WhenApiReturnsNoContent_Succeeds()
    {
        RespondWith(HttpStatusCode.NoContent);

        Assert.True(await service.DeleteAsync(Guid.NewGuid()));
        Snackbar.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetProjectsAsync_WhenFetchWorks_ReturnsProjects()
    {
        RespondWith(new[] { new { title = "Test project", status = "available" } });

        var projects = await service.GetProjectsAsync();

        Assert.NotNull(projects);
        Assert.Equal("Test project", Assert.Single(projects).Title);
        Snackbar.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetProjectsAsync_WhenNoProjects_ReturnsEmptyList()
    {
        RespondWith(Array.Empty<Project>());

        var projects = await service.GetProjectsAsync();

        Assert.NotNull(projects);
        Assert.Empty(projects);
        Snackbar.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetProjectsAsync_WhenFetchFails_ReturnsNullAndNotifies()
    {
        RespondWith(HttpStatusCode.InternalServerError);

        var projects = await service.GetProjectsAsync();

        Assert.Null(projects);
        VerifyErrorNotification("An error occurred when fetching projects.");
    }

    [Fact]
    public async Task GetProjectsAsync_WhenRequestThrows_ReturnsNullAndNotifies()
    {
        ThrowOnRequest(new HttpRequestException("Connection failed"));

        var projects = await service.GetProjectsAsync();

        Assert.Null(projects);
        VerifyErrorNotification("An error occurred when fetching projects.");
    }

    [Fact]
    public async Task GetProjectsAsync_WhenRequestTimesOut_ReturnsNullAndNotifies()
    {
        ThrowOnRequest(new TaskCanceledException("Timed out"));

        var projects = await service.GetProjectsAsync();

        Assert.Null(projects);
        VerifyErrorNotification("timed out");
    }
}
