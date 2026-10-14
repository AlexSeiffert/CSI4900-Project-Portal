using API.Controllers;
using API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using API.DTOs.Request;
using API.Models;
using API.Tests.Support;
using static API.Tests.Support.ApiTestData;

namespace API.Tests.Controller;

public class InquiriesControllerTests
{

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"status\":null}")]
    [InlineData("{\"status\":\"invalid\"}")]
    [InlineData("{\"status\":999}")]
    public async Task InvalidInquiryStatusReturnsBadRequest(string body)
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var response = await client.PutAsync($"/api/v1/inquiries/{Guid.NewGuid()}",
            new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InquiryCreationRejectsUnknownProfilesAndMismatchedProjectOwner()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var project = await CreateProject(client);
        var requests = new[]
        {
            new CreateInquiry { StudentId = Guid.NewGuid(), SupervisorId = PortalApiFactory.SupervisorId },
            new CreateInquiry { StudentId = PortalApiFactory.StudentId, SupervisorId = Guid.NewGuid() },
            new CreateInquiry { StudentId = PortalApiFactory.StudentId, SupervisorId = PortalApiFactory.SupervisorId, ProjectId = Guid.NewGuid() },
            new CreateInquiry { StudentId = PortalApiFactory.StudentId, SupervisorId = PortalApiFactory.OtherSupervisorId, ProjectId = project.Id }
        };
        foreach (var request in requests)
        {
            var response = await client.PostAsJsonAsync("/api/v1/inquiries", request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        Assert.Empty(await Read<List<DTOs.Inquiry>>(await client.GetAsync("/api/v1/inquiries")));
    }

    [Fact]
    public async Task GetInquiries_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var result = await controller.GetInquiries(cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetInquiries", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Empty(service.LastArguments);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetInquiries_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var original = new ServiceException("GetInquiries", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetInquiries(cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetInquiriesByStudentId_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var result = await controller.GetInquiriesByStudentId(studentId, cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetInquiriesByStudentId", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(studentId, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetInquiriesByStudentId_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var studentId = Guid.NewGuid();
        var original = new ServiceException("GetInquiriesByStudentId", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetInquiriesByStudentId(studentId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetInquiriesBySupervisorId_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var supervisorId = Guid.NewGuid();
        var result = await controller.GetInquiriesBySupervisorId(supervisorId, cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetInquiriesBySupervisorId", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(supervisorId, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetInquiriesBySupervisorId_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var supervisorId = Guid.NewGuid();
        var original = new ServiceException("GetInquiriesBySupervisorId", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetInquiriesBySupervisorId(supervisorId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetInquiriesByProjectId_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var result = await controller.GetInquiriesByProjectId(projectId, cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetInquiriesByProjectId", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(projectId, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetInquiriesByProjectId_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("GetInquiriesByProjectId", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetInquiriesByProjectId(projectId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetInquiryById_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var inquiryId = Guid.NewGuid();
        var result = await controller.GetInquiryById(inquiryId, cancellation.Token);
        Assert.Same(service.Item, result.Value);
        Assert.Equal("GetInquiryById", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(inquiryId, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetInquiryById_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var inquiryId = Guid.NewGuid();
        var original = new ServiceException("GetInquiryById", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetInquiryById(inquiryId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task CreateInquiry_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new InquiryOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(new InquiryServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var request = new CreateInquiry();
        var result = await controller.CreateInquiry(request, cancellation.Token);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Same(service.Item, created.Value);
        Assert.Equal("GetInquiryById", created.ActionName);
        Assert.Equal(service.Item.Id, created.RouteValues!["inquiryId"]);
        Assert.Equal("CreateInquiry", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(request, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task CreateInquiry_PropagatesOrchestrationExceptionsToTheHandler(ErrorType kind)
    {
        var service = new InquiryOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(new InquiryServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var request = new CreateInquiry();
        var original = new OrchestrationException("CreateInquiry", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.CreateInquiry(request, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task UpdateInquiry_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var inquiryId = Guid.NewGuid();
        var request = new UpdateInquiry();
        var result = await controller.UpdateInquiry(inquiryId, request, cancellation.Token);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("UpdateInquiry", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Equal(2, service.LastArguments.Length);
        Assert.Equal(inquiryId, service.LastArguments[0]);
        Assert.Equal(request, service.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateInquiry_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new InquiryServiceStub();
        service.Items.Add(service.Item);
        var controller = new InquiriesController(service, new InquiryOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var inquiryId = Guid.NewGuid();
        var request = new UpdateInquiry();
        var original = new ServiceException("UpdateInquiry", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.UpdateInquiry(inquiryId, request, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }
}
