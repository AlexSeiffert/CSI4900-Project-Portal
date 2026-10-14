using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Service;

public class ProjectServiceTests
{
    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetProjects_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "GetProjects";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.GetProjects(token), "GetProjects", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetProjectsBySupervisorId_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "GetProjectsBySupervisorId";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.GetProjectsBySupervisorId(c.Supervisor.UserId, token), "GetProjectsBySupervisorId", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetProjectsByTopicId_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "GetProjectsByTopicId";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.GetProjectsByTopicId(c.Topic.Id, token), "GetProjectsByTopicId", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetProjectsByStatus_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "GetProjectsByStatus";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.GetProjectsByStatus(ProjectStatus.Available, token), "GetProjectsByStatus", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetProjectById_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "GetProjectById";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.GetProjectById(c.Project.Id, token), "GetProjectById", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task CreateProject_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "CreateProject";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.CreateProject(c.NewProject(), token), "CreateProject", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task UpdateProject_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "UpdateProject";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.UpdateProject(c.Project.Id, c.ProjectUpdate(), token), "UpdateProject", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task DeleteProject_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        c.Projects.FailureMethod = "DeleteProject";
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.DeleteProject(c.Project.Id, token), "DeleteProject", original);
        Assert.Equal(token, c.Projects.LastToken);
    }

    [Fact]
    public async Task GetProjects_ReturnsMatchingProjectDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.ProjectService.GetProjects());
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Project.Title, result.Title);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Equal("GetProjects", Assert.Single(c.Projects.Calls));
    }

    [Fact]
    public async Task GetProjectsBySupervisorId_ReturnsMatchingProjectDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.ProjectService.GetProjectsBySupervisorId(c.Supervisor.UserId));
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Project.Title, result.Title);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Equal("GetProjectsBySupervisorId", Assert.Single(c.Projects.Calls));
    }

    [Fact]
    public async Task GetProjectsByTopicId_ReturnsMatchingProjectDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.ProjectService.GetProjectsByTopicId(c.Topic.Id));
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Project.Title, result.Title);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Equal("GetProjectsByTopicId", Assert.Single(c.Projects.Calls));
    }

    [Fact]
    public async Task GetProjectsByStatus_ReturnsMatchingProjectDtos()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.ProjectService.GetProjectsByStatus(ProjectStatus.Available));
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Project.Title, result.Title);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Equal("GetProjectsByStatus", Assert.Single(c.Projects.Calls));
    }

    [Fact]
    public async Task GetProjectById_ReturnsMappedProject()
    {
        var c = new ServiceTestContext();
        var result = await c.ProjectService.GetProjectById(c.Project.Id);
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Project.SupervisorId, result.SupervisorId);
        Assert.Equal(c.Project.CreatedAt, result.CreatedAt);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
    }

    [Fact]
    public async Task CreateProject_TrimsTextAndGeneratesIdentity()
    {
        var c = new ServiceTestContext();
        var request = c.NewProject();
        var result = await c.ProjectService.CreateProject(request);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(c.Project.Id, result.Id);
        Assert.Equal("New portal", result.Title);
        Assert.Equal("Research", result.Description);
        Assert.Equal(request.SupervisorId, result.SupervisorId);
        Assert.Equal("CreateProject", Assert.Single(c.Projects.Calls));
    }

    [Fact]
    public async Task UpdateProject_ChangesEditableFieldsAndPreservesOwnershipAndCreationTime()
    {
        var c = new ServiceTestContext();
        var createdAt = c.Project.CreatedAt;
        var supervisorId = c.Project.SupervisorId;
        var request = c.ProjectUpdate();
        request.ExternalUrl = "https://example.test";
        var result = await c.ProjectService.UpdateProject(c.Project.Id, request);
        Assert.Equal("Updated", result.Title);
        Assert.Equal("Description", result.Description);
        Assert.Equal(request.ExternalUrl, result.ExternalUrl);
        Assert.Equal(ProjectStatus.Archived, result.Status);
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(supervisorId, result.SupervisorId);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.True(result.UpdatedAt >= createdAt);
        Assert.Equal(new[] { "GetProjectById", "UpdateProject" }, c.Projects.Calls);
    }

    [Fact]
    public async Task DeleteProject_DeletesExistingProject()
    {
        var c = new ServiceTestContext();
        await c.ProjectService.DeleteProject(c.Project.Id);
        Assert.Empty(c.Projects.Items);
        Assert.Equal("DeleteProject", Assert.Single(c.Projects.Calls));
    }

    [Fact]
    public async Task GetProjectById_ThrowsForMissingProject()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.GetProjectById(Guid.NewGuid()), "GetProjectById", ErrorType.NotFound);
        Assert.DoesNotContain("UpdateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task UpdateProject_ThrowsForMissingProject()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.UpdateProject(Guid.NewGuid(), c.ProjectUpdate()), "UpdateProject", ErrorType.NotFound);
        Assert.DoesNotContain("UpdateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task DeleteProject_ThrowsForMissingProject()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.DeleteProject(Guid.NewGuid()), "DeleteProject", ErrorType.NotFound);
        Assert.DoesNotContain("UpdateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task GetProjectsByStatus_RejectsUndefinedStatusBeforeRepositoryCall()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.GetProjectsByStatus((ProjectStatus)999), "GetProjectsByStatus", ErrorType.Validation);
        Assert.Empty(c.Projects.Calls);
    }

    [Fact]
    public async Task CreateProject_RejectsInvalidRequestBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.NewProject();
        request.Title = " ";
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.CreateProject(request), "CreateProject", ErrorType.Validation);
        Assert.DoesNotContain("CreateProject", c.Projects.Calls);
    }

    [Fact]
    public async Task UpdateProject_RejectsInvalidRequestBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.ProjectUpdate();
        request.Title = " ";
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.UpdateProject(c.Project.Id, request), "UpdateProject", ErrorType.Validation);
        Assert.DoesNotContain("UpdateProject", c.Projects.Calls);
    }


    [Fact]
    public void Constructor_DependsOnlyOnOwnRepository()
    {
        var constructor = Assert.Single(typeof(API.Services.ProjectService).GetConstructors());
        var dependency = Assert.Single(constructor.GetParameters());
        Assert.Equal(typeof(API.Repositories.Interfaces.IProjectRepository), dependency.ParameterType);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task ValidateProjectSupervisor_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var original = ServiceFailureCases.Create(kind);
        c.Projects.Failure = original;
        await ServiceFailureCases.AssertWrapped(() => c.ProjectService.ValidateProjectSupervisor(c.Project.Id, c.Supervisor.UserId, cancellation.Token), "ValidateProjectSupervisor", original);
        Assert.Equal(cancellation.Token, c.Projects.LastToken);
    }

    [Fact]
    public async Task ValidateProjectSupervisor_AcceptsExistingReferences()
    {
        var c = new ServiceTestContext();
        await c.ProjectService.ValidateProjectSupervisor(c.Project.Id, c.Supervisor.UserId);
        Assert.Equal("GetProjectById", Assert.Single(c.Projects.Calls));
    }

    [Fact]
    public async Task ValidateProjectSupervisor_RejectsMissingReferences()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.ValidateProjectSupervisor(Guid.NewGuid(), c.Supervisor.UserId), "ValidateProjectSupervisor", ErrorType.Validation);
    }

    [Fact]
    public async Task ValidateProjectSupervisor_AcceptsGeneralInquiryWithoutProjectLookup()
    {
        var c = new ServiceTestContext();
        await c.ProjectService.ValidateProjectSupervisor(null, c.Supervisor.UserId);
        Assert.Empty(c.Projects.Calls);
    }

    [Fact]
    public async Task ValidateProjectSupervisor_RejectsDifferentOwner()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.ProjectService.ValidateProjectSupervisor(c.Project.Id, Guid.NewGuid()), "ValidateProjectSupervisor", ErrorType.Validation);
    }
}
