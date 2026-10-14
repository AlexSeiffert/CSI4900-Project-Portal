using API.Controllers;
using API.DTOs;
using API.DTOs.Request;
using API.Orchestrations.Interfaces;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controller;

public class SupervisorsControllerTests
{
    private readonly Mock<ISupervisorService> _service = new();
    private readonly Mock<ISupervisorOrchestration> _orchestration = new();

    private SupervisorsController CreateController()
    {
        return new(_service.Object, _orchestration.Object);
    }

    [Fact]
    public async Task GetSupervisors_ReturnsOkWithSupervisors()
    {
        var supervisors = new List<Supervisor> { new() { UserId = Guid.NewGuid() } };

        _service.Setup(service => service.GetSupervisors(CancellationToken.None))
            .ReturnsAsync(supervisors);

        var controller = CreateController();

        var result = await controller.GetSupervisors(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(supervisors, ok.Value);
    }

    [Fact]
    public async Task GetSupervisorById_ReturnsSupervisor()
    {
        var supervisor = new Supervisor { UserId = Guid.NewGuid() };

        _service.Setup(service => service.GetSupervisorById(supervisor.UserId, CancellationToken.None))
            .ReturnsAsync(supervisor);

        var controller = CreateController();

        var result = await controller.GetSupervisorById(supervisor.UserId, CancellationToken.None);

        Assert.Same(supervisor, result.Value);
    }

    [Fact]
    public async Task UpdateSupervisor_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor();

        _orchestration.Setup(orchestration => orchestration.UpdateSupervisor(userId, request, CancellationToken.None))
            .ReturnsAsync(new Supervisor { UserId = userId });
            
        var controller = CreateController();

        var result = await controller.UpdateSupervisor(userId, request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _orchestration.Verify(orchestration => orchestration.UpdateSupervisor(userId, request, CancellationToken.None), Times.Once);
    }
}
