using API.Tests.Support;
using API.Controllers;
using API.DTOs;
using API.DTOs.Request;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProjectStatus = API.Models.ProjectStatus;

namespace API.Tests.Controller;

public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> _service = new();

    private ProjectsController CreateController()
    {
        return new(_service.Object);
    }

    [Fact]
    public async Task GetProjects_ReturnsOkWithProjects()
    {
        var projects = new List<Project> { new() { Id = Guid.NewGuid() } };

        _service.Setup(service => service.GetProjects(CancellationToken.None))
            .ReturnsAsync(projects);

        var controller = CreateController();

        var result = await controller.GetProjects(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(projects, ok.Value);
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_ReturnsOkWithProjects()
    {
        var supervisorId = Guid.NewGuid();

        var projects = new List<Project> { new() { Id = Guid.NewGuid(), SupervisorId = supervisorId } };
        
        _service.Setup(service => service.GetProjectsBySupervisorId(supervisorId, CancellationToken.None))
            .ReturnsAsync(projects);

        var controller = CreateController();

        var result = await controller.GetProjectsBySupervisorId(supervisorId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(projects, ok.Value);
    }

    [Fact]
    public async Task GetProjectsByTopicId_ReturnsOkWithProjects()
    {
        var topicId = Guid.NewGuid();

        var projects = new List<Project> { new() { Id = Guid.NewGuid() } };

        _service.Setup(service => service.GetProjectsByTopicId(topicId, CancellationToken.None))
            .ReturnsAsync(projects);

        var controller = CreateController();

        var result = await controller.GetProjectsByTopicId(topicId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(projects, ok.Value);
    }

    [Fact]
    public async Task GetProjectsByStatus_ReturnsOkWithProjects()
    {
        var status = ProjectStatus.Available;

        var projects = new List<Project> { new() { Id = Guid.NewGuid(), Status = status } };

        _service.Setup(service => service.GetProjectsByStatus(status, CancellationToken.None))
            .ReturnsAsync(projects);

        var controller = CreateController();

        var result = await controller.GetProjectsByStatus(status, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(projects, ok.Value);
    }

    [Fact]
    public async Task GetProjectById_ReturnsProject()
    {
        var project = new Project { Id = Guid.NewGuid() };

        _service.Setup(service => service.GetProjectById(project.Id, CancellationToken.None))
            .ReturnsAsync(project);

        var controller = CreateController();

        var result = await controller.GetProjectById(project.Id, CancellationToken.None);

        Assert.Same(project, result.Value);
    }

    [Fact]
    public async Task CreateProject_ReturnsCreatedWithProject()
    {
        var supervisorId = Guid.NewGuid();
        var request = new CreateProject { SupervisorId = Guid.NewGuid() };

        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisorId };

        _service.Setup(service => service.CreateProject(It.Is<CreateProject>(r => r.SupervisorId == supervisorId), CancellationToken.None))
            .ReturnsAsync(project);

        var controller = CreateController().SignedInAs(supervisorId);

        var result = await controller.CreateProject(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Same(project, created.Value);
        Assert.Equal(nameof(ProjectsController.GetProjectById), created.ActionName);
        Assert.Equal(project.Id, created.RouteValues!["projectId"]);
    }

    [Fact]
    public async Task UpdateProject_ReturnsNoContent()
    {
        var projectId = Guid.NewGuid();

        var request = new UpdateProject();

        _service.Setup(service => service.UpdateProject(projectId, request, CancellationToken.None))
            .ReturnsAsync(new Project { Id = projectId });

        var controller = CreateController();

        var result = await controller.UpdateProject(projectId, request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _service.Verify(service => service.UpdateProject(projectId, request, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_ReturnsNoContent()
    {
        var projectId = Guid.NewGuid();

        _service.Setup(service => service.DeleteProject(projectId, CancellationToken.None))
            .Returns(Task.CompletedTask);

        var controller = CreateController();

        var result = await controller.DeleteProject(projectId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _service.Verify(service => service.DeleteProject(projectId, CancellationToken.None), Times.Once);
    }
}
