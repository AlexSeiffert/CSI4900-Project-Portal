using API.Controllers;
using API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using System.Net;
using System.Net.Http.Json;
using API.DTOs.Request;
using API.Tests.Support;
using static API.Tests.Support.ApiTestData;

namespace API.Tests.Controller;

public class SupervisorsControllerTests
{
    [Fact]
    public async Task UpdateSupervisor_ReplacesTopicsAndReturnsNoContent()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var path = $"/api/v1/supervisors/{PortalApiFactory.SupervisorId}";
        var request = new UpdateSupervisor { DisplayName = "Updated", ContactEmail = "updated@example.test", AcceptingInquiries = false, TopicIds = [PortalApiFactory.TopicId] };
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync(path, request)).StatusCode);
        var profile = await Read<DTOs.Supervisor>(await client.GetAsync(path));
        Assert.Equal(request.DisplayName, profile.DisplayName);
        Assert.Equal(PortalApiFactory.TopicId, Assert.Single(profile.TopicInterests).Id);
        request.TopicIds = [PortalApiFactory.OtherTopicId];
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync(path, request)).StatusCode);
        profile = await Read<DTOs.Supervisor>(await client.GetAsync(path));
        Assert.Equal(PortalApiFactory.OtherTopicId, Assert.Single(profile.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateSupervisor_ReturnsValidationProblemForInvalidRequestOrUnknownTopics()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var path = $"/api/v1/supervisors/{PortalApiFactory.SupervisorId}";
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(path, new UpdateSupervisor { DisplayName = "Valid", ContactEmail = "bad-email" })).StatusCode);
        var request = new UpdateSupervisor { DisplayName = "Updated", ContactEmail = "updated@example.test", AcceptingInquiries = false, TopicIds = [PortalApiFactory.TopicId] };
        request.TopicIds = [Guid.NewGuid()];
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(path, request)).StatusCode);
    }

    [Fact]
    public async Task GetSupervisorById_ReturnsNotFoundForMissingProfile()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/supervisors/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task UpdateSupervisor_ReturnsNotFoundForMissingProfile()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/v1/supervisors/{Guid.NewGuid()}", new UpdateSupervisor { DisplayName = "Updated", ContactEmail = "updated@example.test", AcceptingInquiries = false, TopicIds = [PortalApiFactory.TopicId] })).StatusCode);
    }

    [Fact]
    public async Task GetSupervisors_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new SupervisorServiceStub();
        service.Items.Add(service.Item);
        var controller = new SupervisorsController(service, new SupervisorOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var result = await controller.GetSupervisors(cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetSupervisors", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Empty(service.LastArguments);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetSupervisors_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new SupervisorServiceStub();
        service.Items.Add(service.Item);
        var controller = new SupervisorsController(service, new SupervisorOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var original = new ServiceException("GetSupervisors", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetSupervisors(cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetSupervisorById_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new SupervisorServiceStub();
        service.Items.Add(service.Item);
        var controller = new SupervisorsController(service, new SupervisorOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var result = await controller.GetSupervisorById(userId, cancellation.Token);
        Assert.Same(service.Item, result.Value);
        Assert.Equal("GetSupervisorById", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(userId, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetSupervisorById_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new SupervisorServiceStub();
        service.Items.Add(service.Item);
        var controller = new SupervisorsController(service, new SupervisorOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var original = new ServiceException("GetSupervisorById", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetSupervisorById(userId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task UpdateSupervisor_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new SupervisorOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SupervisorsController(new SupervisorServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor();
        var result = await controller.UpdateSupervisor(userId, request, cancellation.Token);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("UpdateSupervisor", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Equal(2, service.LastArguments.Length);
        Assert.Equal(userId, service.LastArguments[0]);
        Assert.Equal(request, service.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateSupervisor_PropagatesOrchestrationExceptionsToTheHandler(ErrorType kind)
    {
        var service = new SupervisorOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new SupervisorsController(new SupervisorServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var request = new UpdateSupervisor();
        var original = new OrchestrationException("UpdateSupervisor", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.UpdateSupervisor(userId, request, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }
}
