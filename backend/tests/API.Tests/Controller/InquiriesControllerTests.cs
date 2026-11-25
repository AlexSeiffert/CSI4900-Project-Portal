using API.Tests.Support;
using API.Controllers;
using API.DTOs;
using API.DTOs.Request;
using API.Services;
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

        var callerId = Guid.NewGuid();
        _service.Setup(service => service.GetInquiryById(inquiry.Id, new Caller(callerId, false), CancellationToken.None))
            .ReturnsAsync(inquiry);

        var controller = CreateController().SignedInAs(callerId);

        var result = await controller.GetInquiryById(inquiry.Id, CancellationToken.None);

        Assert.Same(inquiry, result.Value);
    }

    [Fact]
    public async Task CreateInquiry_ReturnsCreatedWithInquiry()
    {
        var studentId = Guid.NewGuid();
        var request = new CreateInquiry { StudentId = Guid.NewGuid(), ProjectId = Guid.NewGuid() };

        var inquiry = new Inquiry { Id = Guid.NewGuid(), StudentId = studentId, ProjectId = request.ProjectId };

        _service.Setup(service => service.CreateInquiry(It.Is<CreateInquiry>(r => r.StudentId == studentId), CancellationToken.None))
            .ReturnsAsync(inquiry);

        var controller = CreateController().SignedInAs(studentId);

        var result = await controller.CreateInquiry(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);

        Assert.Same(inquiry, created.Value);
        Assert.Equal(nameof(InquiriesController.GetInquiryById), created.ActionName);
        Assert.Equal(inquiry.Id, created.RouteValues!["inquiryId"]);
    }

    [Fact]
    public async Task AddMember_PassesTheSignedInCreator()
    {
        var studentId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid() };
        var request = new AddInquiryMember { Email = "friend@example.test" };
        _service.Setup(service => service.AddMember(inquiry.Id, request, new Caller(studentId, false), CancellationToken.None))
            .ReturnsAsync(inquiry);

        var result = await CreateController().SignedInAs(studentId, "student").AddMember(inquiry.Id, request, CancellationToken.None);

        Assert.Same(inquiry, result.Value);
    }

    [Fact]
    public async Task Respond_PassesTheAnswerAndCaller()
    {
        var studentId = Guid.NewGuid();
        var inquiryId = Guid.NewGuid();
        var outcome = new RespondResult(API.Models.InquiryStatus.Matched, [Guid.NewGuid()]);
        _service.Setup(service => service.Respond(inquiryId, true, new Caller(studentId, false), CancellationToken.None))
            .ReturnsAsync(outcome);

        var result = await CreateController().SignedInAs(studentId).Respond(inquiryId, new RespondToInquiry { Accept = true }, CancellationToken.None);

        Assert.Same(outcome, result.Value);
    }

    [Fact]
    public async Task Offer_MarksAdminsAsAdmins()
    {
        var adminId = Guid.NewGuid();
        var inquiry = new Inquiry { Id = Guid.NewGuid() };
        _service.Setup(service => service.Offer(inquiry.Id, new Caller(adminId, true), CancellationToken.None))
            .ReturnsAsync(inquiry);

        var result = await CreateController().SignedInAs(adminId, "admin").Offer(inquiry.Id, CancellationToken.None);

        Assert.Same(inquiry, result.Value);
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent()
    {
        var studentId = Guid.NewGuid();
        var inquiryId = Guid.NewGuid();

        var result = await CreateController().SignedInAs(studentId).RemoveMember(inquiryId, studentId, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _service.Verify(service => service.RemoveMember(inquiryId, studentId, new Caller(studentId, false), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task RegisterAttachment_ReturnsCreated()
    {
        var studentId = Guid.NewGuid();
        var inquiryId = Guid.NewGuid();
        var request = new RegisterAttachment { StoragePath = "p", Kind = API.Models.AttachmentKind.Resume, SizeBytes = 10 };
        var attachment = new InquiryAttachment { Id = Guid.NewGuid() };
        _service.Setup(service => service.RegisterAttachment(inquiryId, request, new Caller(studentId, false), CancellationToken.None))
            .ReturnsAsync(attachment);

        var result = await CreateController().SignedInAs(studentId).RegisterAttachment(inquiryId, request, CancellationToken.None);

        Assert.Same(attachment, Assert.IsType<CreatedAtActionResult>(result.Result).Value);
    }
}
