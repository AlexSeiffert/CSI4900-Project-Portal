using API.Controllers;
using API.DTOs;
using API.Orchestrations.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controller;

public class SavedProjectsControllerTests
{
    private readonly Mock<ISavedProjectOrchestration> _orchestration = new();

    private SavedProjectsController CreateController()
    {
        return new(_orchestration.Object);
    }

    [Fact]
    public async Task GetSavedProjects_ReturnsOkWithProjects()
    {
        var studentId = Guid.NewGuid();

        var projects = new List<Project> { new() { Id = Guid.NewGuid() } };

        _orchestration.Setup(orchestration => orchestration.GetSavedProjects(studentId, CancellationToken.None))
            .ReturnsAsync(projects);

        var controller = CreateController();

        var result = await controller.GetSavedProjects(studentId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);

        Assert.Same(projects, ok.Value);
    }

    [Fact]
    public async Task SaveProject_ReturnsNoContent()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _orchestration.Setup(orchestration => orchestration.SaveProject(studentId, projectId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        var result = await controller.SaveProject(studentId, projectId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _orchestration.Verify(orchestration => orchestration.SaveProject(studentId, projectId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RemoveSavedProject_ReturnsNoContent()
    {
        var studentId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _orchestration.Setup(orchestration => orchestration.RemoveSavedProject(studentId, projectId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        var result = await controller.RemoveSavedProject(studentId, projectId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _orchestration.Verify(orchestration => orchestration.RemoveSavedProject(studentId, projectId, CancellationToken.None), Times.Once);
    }
}
