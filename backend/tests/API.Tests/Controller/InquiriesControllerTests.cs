using API.Controllers;
using API.DTOs;
using API.DTOs.Request;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controller;

public class InquiriesControllerTests
{
    private readonly Mock<IInquiryService> _service = new();

    private InquiriesController CreateController()
    {
        return new(_service.Object);
    }

    [Fact]
    public async Task GetInquiries_ReturnsOkWithInquiries()
    {
        var inquiries = new List<Inquiry> { new() { Id = Guid.NewGuid() } };

        _service.Setup(service => service.GetInquiries(CancellationToken.None))
            .ReturnsAsync(inquiries);

        var controller = CreateController();

        var result = await controller.GetInquiries(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(inquiries, ok.Value);
    }

    [Fact]
    public async Task GetInquiriesByStudentId_ReturnsOkWithInquiries()
    {
        var studentId = Guid.NewGuid();

        var inquiries = new List<Inquiry> { new() { Id = Guid.NewGuid(), StudentId = studentId } };

        _service.Setup(service => service.GetInquiriesByStudentId(studentId, CancellationToken.None))
            .ReturnsAsync(inquiries);

        var controller = CreateController();

        var result = await controller.GetInquiriesByStudentId(studentId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(inquiries, ok.Value);
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_ReturnsOkWithInquiries()
    {
        var supervisorId = Guid.NewGuid();

        var inquiries = new List<Inquiry> { new() { Id = Guid.NewGuid(), SupervisorId = supervisorId } };

        _service.Setup(service => service.GetInquiriesBySupervisorId(supervisorId, CancellationToken.None))
            .ReturnsAsync(inquiries);

        var controller = CreateController();

        var result = await controller.GetInquiriesBySupervisorId(supervisorId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(inquiries, ok.Value);
    }

    [Fact]
    public async Task GetInquiriesByProjectId_ReturnsOkWithInquiries()
    {
        var projectId = Guid.NewGuid();

        var inquiries = new List<Inquiry> { new() { Id = Guid.NewGuid(), ProjectId = projectId } };

        _service.Setup(service => service.GetInquiriesByProjectId(projectId, CancellationToken.None))
            .ReturnsAsync(inquiries);

        var controller = CreateController();

        var result = await controller.GetInquiriesByProjectId(projectId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(inquiries, ok.Value);
    }

    [Fact]
    public async Task GetInquiryById_ReturnsInquiry()
    {
        var inquiry = new Inquiry { Id = Guid.NewGuid() };

        _service.Setup(service => service.GetInquiryById(inquiry.Id, CancellationToken.None))
            .ReturnsAsync(inquiry);

        var controller = CreateController();

        var result = await controller.GetInquiryById(inquiry.Id, CancellationToken.None);

        Assert.Same(inquiry, result.Value);
    }

    [Fact]
    public async Task CreateInquiry_ReturnsCreatedWithInquiry()
    {
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = Guid.NewGuid() };

        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = request.StudentId, SupervisorId = request.SupervisorId };

        _service.Setup(service => service.CreateInquiry(request, CancellationToken.None))
            .ReturnsAsync(inquiry);

        var controller = CreateController();

        var result = await controller.CreateInquiry(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Same(inquiry, created.Value);
        Assert.Equal(nameof(InquiriesController.GetInquiryById), created.ActionName);
        Assert.Equal(inquiry.Id, created.RouteValues!["inquiryId"]);
    }

    [Fact]
    public async Task UpdateInquiry_ReturnsNoContent()
    {
        var inquiryId = Guid.NewGuid();

        var request = new UpdateInquiry();

        _service.Setup(service => service.UpdateInquiry(inquiryId, request, CancellationToken.None))
            .ReturnsAsync(new Inquiry { Id = inquiryId });
            
        var controller = CreateController();

        var result = await controller.UpdateInquiry(inquiryId, request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _service.Verify(service => service.UpdateInquiry(inquiryId, request, CancellationToken.None), Times.Once);
    }
}
