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

public class StudentsControllerTests
{
    [Fact]
    public async Task UpdateStudent_ReplacesTopicsAndReturnsNoContent()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var path = $"/api/v1/students/{PortalApiFactory.StudentId}";
        var request = new UpdateStudent { DisplayName = "Updated", Program = "CS", TopicIds = [PortalApiFactory.TopicId] };
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync(path, request)).StatusCode);
        var profile = await Read<DTOs.Student>(await client.GetAsync(path));
        Assert.Equal(request.DisplayName, profile.DisplayName);
        Assert.Equal(PortalApiFactory.TopicId, Assert.Single(profile.TopicInterests).Id);
        request.TopicIds = [PortalApiFactory.OtherTopicId];
        Assert.Equal(HttpStatusCode.NoContent, (await client.PutAsJsonAsync(path, request)).StatusCode);
        profile = await Read<DTOs.Student>(await client.GetAsync(path));
        Assert.Equal(PortalApiFactory.OtherTopicId, Assert.Single(profile.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateStudent_ReturnsValidationProblemForInvalidRequestOrUnknownTopics()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var path = $"/api/v1/students/{PortalApiFactory.StudentId}";
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(path, new UpdateStudent { DisplayName = " " })).StatusCode);
        var request = new UpdateStudent { DisplayName = "Updated", Program = "CS", TopicIds = [PortalApiFactory.TopicId] };
        request.TopicIds = [Guid.NewGuid()];
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(path, request)).StatusCode);
    }

    [Fact]
    public async Task GetStudentById_ReturnsNotFoundForMissingProfile()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/students/{Guid.NewGuid()}")).StatusCode);
    }

    [Fact]
    public async Task UpdateStudent_ReturnsNotFoundForMissingProfile()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/v1/students/{Guid.NewGuid()}", new UpdateStudent { DisplayName = "Updated", Program = "CS", TopicIds = [PortalApiFactory.TopicId] })).StatusCode);
    }

    [Fact]
    public async Task GetStudents_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new StudentServiceStub();
        service.Items.Add(service.Item);
        var controller = new StudentsController(service, new StudentOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var result = await controller.GetStudents(cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetStudents", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Empty(service.LastArguments);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetStudents_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new StudentServiceStub();
        service.Items.Add(service.Item);
        var controller = new StudentsController(service, new StudentOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var original = new ServiceException("GetStudents", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetStudents(cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetStudentById_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new StudentServiceStub();
        service.Items.Add(service.Item);
        var controller = new StudentsController(service, new StudentOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var result = await controller.GetStudentById(userId, cancellation.Token);
        Assert.Same(service.Item, result.Value);
        Assert.Equal("GetStudentById", service.LastMethod);
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
    public async Task GetStudentById_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new StudentServiceStub();
        service.Items.Add(service.Item);
        var controller = new StudentsController(service, new StudentOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var original = new ServiceException("GetStudentById", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetStudentById(userId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task UpdateStudent_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new StudentOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new StudentsController(new StudentServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var request = new UpdateStudent();
        var result = await controller.UpdateStudent(userId, request, cancellation.Token);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("UpdateStudent", service.LastMethod);
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
    public async Task UpdateStudent_PropagatesOrchestrationExceptionsToTheHandler(ErrorType kind)
    {
        var service = new StudentOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new StudentsController(new StudentServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var userId = Guid.NewGuid();
        var request = new UpdateStudent();
        var original = new OrchestrationException("UpdateStudent", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.UpdateStudent(userId, request, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }
}
