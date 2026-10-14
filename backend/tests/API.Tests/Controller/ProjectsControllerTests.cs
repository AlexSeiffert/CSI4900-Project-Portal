using API.Controllers;
using API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.DTOs.Request;
using API.Models;
using API.Tests.Support;
using static API.Tests.Support.ApiTestData;

namespace API.Tests.Controller;

public class ProjectsControllerTests
{

    [Fact]
    public async Task SeparateSupervisorAndTopicEndpointsReturnOnlyMatchingProjects()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var first = await CreateProject(client);
        var other = await CreateProject(client, NewProject(PortalApiFactory.OtherSupervisorId, [PortalApiFactory.OtherTopicId]));
        var bySupervisor = await Read<List<DTOs.Project>>(await client.GetAsync($"/api/v1/projects/supervisors/{first.SupervisorId}"));
        Assert.Equal(first.Id, Assert.Single(bySupervisor).Id);
        var byTopic = await Read<List<DTOs.Project>>(await client.GetAsync($"/api/v1/projects/topics/{PortalApiFactory.OtherTopicId}"));
        Assert.Equal(other.Id, Assert.Single(byTopic).Id);
        var empty = await Read<List<DTOs.Project>>(await client.GetAsync($"/api/v1/projects/supervisors/{Guid.NewGuid()}"));
        Assert.Empty(empty);
        var all = await Read<List<DTOs.Project>>(await client.GetAsync("/api/v1/projects"));
        Assert.Equal(2, all.Count);
        var available = await Read<List<DTOs.Project>>(await client.GetAsync("/api/v1/projects/statuses/available"));
        Assert.Equal(2, available.Count);
        Assert.Empty(await Read<List<DTOs.Project>>(await client.GetAsync("/api/v1/projects/statuses/archived")));
    }

    [Theory]
    [InlineData("{\"title\":\" \",\"description\":\"Valid\"}")]
    [InlineData("{\"title\":\"Valid\",\"description\":\"Valid\",\"topicIds\":null}")]
    [InlineData("{\"title\":\"Valid\",\"description\":\"Valid\",\"status\":\"invalid\"}")]
    [InlineData("{\"title\":\"Valid\",\"description\":\"Valid\",\"status\":42}")]
    [InlineData("{\"title\":\"Valid\",\"description\":\"Valid\",\"externalUrl\":\"bad URL\"}")]
    public async Task InvalidProjectRequestsReturnValidationProblems(string body)
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var response = await client.PostAsync("/api/v1/projects", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("not-a-status")]
    [InlineData("999")]
    public async Task InvalidStatusFilterReturnsBadRequest(string status)
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/v1/projects/statuses/{status}")).StatusCode);
    }

    [Fact]
    public async Task GetProjectsByStatus_SelectsOnlyMatchingProjects()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var availableRequest = NewProject();
        availableRequest.Title = "Accessible Portal";
        var available = await CreateProject(client, availableRequest);
        var archivedRequest = NewProject(PortalApiFactory.OtherSupervisorId);
        archivedRequest.Title = "Archived Dataset";
        archivedRequest.Description = "Historical data analysis";
        archivedRequest.Status = ProjectStatus.Archived;
        var archived = await CreateProject(client, archivedRequest);

        var byStatus = await Read<List<DTOs.Project>>(await client.GetAsync("/api/v1/projects/statuses/archived"));
        Assert.Equal(archived.Id, Assert.Single(byStatus).Id);
        var byAvailableStatus = await Read<List<DTOs.Project>>(await client.GetAsync("/api/v1/projects/statuses/available"));
        Assert.Equal(available.Id, Assert.Single(byAvailableStatus).Id);
        Assert.Equal(2, (await Read<List<DTOs.Project>>(await client.GetAsync("/api/v1/projects"))).Count);
    }

    [Fact]
    public async Task ProjectUpdatesCannotOverwriteOwnershipIdentityOrCreationTime()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var project = await CreateProject(client);
        var response = await client.PutAsJsonAsync($"/api/v1/projects/{project.Id}", new
        {
            id = Guid.NewGuid(),
            supervisorId = PortalApiFactory.OtherSupervisorId,
            title = "Updated title",
            description = "Updated description",
            createdAt = "2000-01-01T00:00:00Z",
            status = "available",
            topicIds = Array.Empty<Guid>()
        });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var updated = await Read<DTOs.Project>(await client.GetAsync($"/api/v1/projects/{project.Id}"));
        Assert.Equal(project.Id, updated.Id);
        Assert.Equal(project.SupervisorId, updated.SupervisorId);
        Assert.Equal(project.CreatedAt, updated.CreatedAt);
        Assert.Equal("Updated title", updated.Title);
    }

    [Fact]
    public async Task UnknownRelationshipIdsAreRejectedWithoutChangingTheProject()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var invalid = NewProject(Guid.NewGuid());
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/projects", invalid, ApiTestData.JsonOptions)).StatusCode);
        invalid = NewProject(topicIds: [Guid.NewGuid()]);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/projects", invalid, ApiTestData.JsonOptions)).StatusCode);
        var created = await CreateProject(client);
        var response = await client.PutAsJsonAsync($"/api/v1/projects/{created.Id}", invalid, ApiTestData.JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
        Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("request", out _));
        var unchanged = await Read<DTOs.Project>(await client.GetAsync($"/api/v1/projects/{created.Id}"));
        Assert.Equal(created.Title, unchanged.Title);
        Assert.Equal(PortalApiFactory.TopicId, Assert.Single(unchanged.Topics).Id);
    }

    [Fact]
    public async Task GetProjects_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var result = await controller.GetProjects(cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetProjects", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Empty(service.LastArguments);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetProjects_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var original = new ServiceException("GetProjects", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetProjects(cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var supervisorId = Guid.NewGuid();
        var result = await controller.GetProjectsBySupervisorId(supervisorId, cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetProjectsBySupervisorId", service.LastMethod);
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
    public async Task GetProjectsBySupervisorId_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var supervisorId = Guid.NewGuid();
        var original = new ServiceException("GetProjectsBySupervisorId", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetProjectsBySupervisorId(supervisorId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetProjectsByTopicId_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var topicId = Guid.NewGuid();
        var result = await controller.GetProjectsByTopicId(topicId, cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetProjectsByTopicId", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(topicId, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetProjectsByTopicId_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var topicId = Guid.NewGuid();
        var original = new ServiceException("GetProjectsByTopicId", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetProjectsByTopicId(topicId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetProjectsByStatus_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var status = ProjectStatus.Archived;
        var result = await controller.GetProjectsByStatus(status, cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetProjectsByStatus", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Single(service.LastArguments);
        Assert.Equal(status, service.LastArguments[0]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task GetProjectsByStatus_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var status = ProjectStatus.Archived;
        var original = new ServiceException("GetProjectsByStatus", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetProjectsByStatus(status, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task GetProjectById_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var result = await controller.GetProjectById(projectId, cancellation.Token);
        Assert.Same(service.Item, result.Value);
        Assert.Equal("GetProjectById", service.LastMethod);
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
    public async Task GetProjectById_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("GetProjectById", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetProjectById(projectId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task CreateProject_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(new ProjectServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var request = new CreateProject();
        var result = await controller.CreateProject(request, cancellation.Token);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Same(service.Item, created.Value);
        Assert.Equal("GetProjectById", created.ActionName);
        Assert.Equal(service.Item.Id, created.RouteValues!["projectId"]);
        Assert.Equal("CreateProject", service.LastMethod);
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
    public async Task CreateProject_PropagatesOrchestrationExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(new ProjectServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var request = new CreateProject();
        var original = new OrchestrationException("CreateProject", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.CreateProject(request, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task UpdateProject_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(new ProjectServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var request = new UpdateProject();
        var result = await controller.UpdateProject(projectId, request, cancellation.Token);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("UpdateProject", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Equal(2, service.LastArguments.Length);
        Assert.Equal(projectId, service.LastArguments[0]);
        Assert.Equal(request, service.LastArguments[1]);
    }

    [Theory]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Cancelled)]
    [InlineData(ErrorType.Unexpected)]
    public async Task UpdateProject_PropagatesOrchestrationExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectOrchestrationStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(new ProjectServiceStub(), service);
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var request = new UpdateProject();
        var original = new OrchestrationException("UpdateProject", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<OrchestrationException>(() => controller.UpdateProject(projectId, request, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }

    [Fact]
    public async Task DeleteProject_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var result = await controller.DeleteProject(projectId, cancellation.Token);
        Assert.IsType<NoContentResult>(result);
        Assert.Equal("DeleteProject", service.LastMethod);
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
    public async Task DeleteProject_PropagatesServiceExceptionsToTheHandler(ErrorType kind)
    {
        var service = new ProjectServiceStub();
        service.Items.Add(service.Item);
        var controller = new ProjectsController(service, new ProjectOrchestrationStub());
        using var cancellation = new CancellationTokenSource();
        var projectId = Guid.NewGuid();
        var original = new ServiceException("DeleteProject", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.DeleteProject(projectId, cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }
}
