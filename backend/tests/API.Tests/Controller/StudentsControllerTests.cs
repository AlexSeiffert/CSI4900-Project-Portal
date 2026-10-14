using API.Controllers;
using API.DTOs;
using API.DTOs.Request;
using API.Orchestrations.Interfaces;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controller;

public class StudentsControllerTests
{
    private readonly Mock<IStudentService> _service = new();
    private readonly Mock<IStudentOrchestration> _orchestration = new();

    private StudentsController CreateController()
    {
        return new(_service.Object, _orchestration.Object);
    }

    [Fact]
    public async Task GetStudents_ReturnsOkWithStudents()
    {
        var students = new List<Student> { new() { UserId = Guid.NewGuid() } };

        _service.Setup(service => service.GetStudents(CancellationToken.None))
            .ReturnsAsync(students);

        var controller = CreateController();

        var result = await controller.GetStudents(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(students, ok.Value);
    }

    [Fact]
    public async Task GetStudentById_ReturnsStudent()
    {
        var student = new Student { UserId = Guid.NewGuid() };

        _service.Setup(service => service.GetStudentById(student.UserId, CancellationToken.None))
            .ReturnsAsync(student);

        var controller = CreateController();

        var result = await controller.GetStudentById(student.UserId, CancellationToken.None);

        Assert.Same(student, result.Value);
    }

    [Fact]
    public async Task UpdateStudent_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateStudent();

        _orchestration.Setup(orchestration => orchestration.UpdateStudent(userId, request, CancellationToken.None))
            .ReturnsAsync(new Student { UserId = userId });
            
        var controller = CreateController();

        var result = await controller.UpdateStudent(userId, request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _orchestration.Verify(orchestration => orchestration.UpdateStudent(userId, request, CancellationToken.None), Times.Once);
    }
}
