using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Service;

public class SupervisorServiceTests
{
    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetSupervisors_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Supervisors.Failure = original;
        c.Supervisors.FailureMethod = "GetSupervisors";
        await ServiceFailureCases.AssertWrapped(() => c.SupervisorService.GetSupervisors(token), "GetSupervisors", original);
        Assert.Equal(token, c.Supervisors.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetSupervisorById_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Supervisors.Failure = original;
        c.Supervisors.FailureMethod = "GetSupervisorById";
        await ServiceFailureCases.AssertWrapped(() => c.SupervisorService.GetSupervisorById(c.Supervisor.UserId, token), "GetSupervisorById", original);
        Assert.Equal(token, c.Supervisors.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task UpdateSupervisor_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Supervisors.Failure = original;
        c.Supervisors.FailureMethod = "UpdateSupervisor";
        await ServiceFailureCases.AssertWrapped(() => c.SupervisorService.UpdateSupervisor(c.Supervisor.UserId, c.SupervisorUpdate(), token), "UpdateSupervisor", original);
        Assert.Equal(token, c.Supervisors.LastToken);
    }

    [Fact]
    public async Task GetSupervisors_MapsProfilesAndTopics()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.SupervisorService.GetSupervisors());
        Assert.Equal(c.Supervisor.UserId, result.UserId);
        Assert.Equal(c.Supervisor.DisplayName, result.DisplayName);
        Assert.Equal(c.Topic.Id, Assert.Single(result.TopicInterests).Id);
    }

    [Fact]
    public async Task GetSupervisorById_ReturnsMatchingProfile()
    {
        var c = new ServiceTestContext();
        var result = await c.SupervisorService.GetSupervisorById(c.Supervisor.UserId);
        Assert.Equal(c.Supervisor.UserId, result.UserId);
        Assert.Equal(c.Supervisor.DisplayName, result.DisplayName);
        Assert.Equal(c.Topic.Id, Assert.Single(result.TopicInterests).Id);
    }

    [Fact]
    public async Task UpdateSupervisor_UpdatesFieldsAndPreservesIdentity()
    {
        var c = new ServiceTestContext();
        var request = c.SupervisorUpdate();
        var before = c.Supervisor.UpdatedAt;
        var result = await c.SupervisorService.UpdateSupervisor(c.Supervisor.UserId, request);
        Assert.Equal(c.Supervisor.UserId, result.UserId);
        Assert.Equal(request.DisplayName, result.DisplayName);
        Assert.Equal(request.ContactEmail, result.ContactEmail);
        Assert.Equal(request.AboutMe, result.AboutMe);
        Assert.Equal(request.ExternalUrl, result.ExternalUrl);
        Assert.False(result.AcceptingInquiries);
        Assert.True(result.UpdatedAt >= before);
        Assert.Contains("UpdateSupervisor", c.Supervisors.Calls);
    }

    [Fact]
    public async Task GetSupervisorById_ThrowsForMissingProfile()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.SupervisorService.GetSupervisorById(Guid.NewGuid()), "GetSupervisorById", ErrorKind.NotFound);
        Assert.DoesNotContain("UpdateSupervisor", c.Supervisors.Calls);
    }

    [Fact]
    public async Task UpdateSupervisor_ThrowsForMissingProfile()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.SupervisorService.UpdateSupervisor(Guid.NewGuid(), c.SupervisorUpdate()), "UpdateSupervisor", ErrorKind.NotFound);
        Assert.DoesNotContain("UpdateSupervisor", c.Supervisors.Calls);
    }

    [Fact]
    public async Task UpdateSupervisor_RejectsInvalidRequestBeforeWriting()
    {
        var c = new ServiceTestContext();
        var request = c.SupervisorUpdate();
        request.DisplayName = " ";
        await ServiceFailureCases.AssertDomainFailure(() => c.SupervisorService.UpdateSupervisor(c.Supervisor.UserId, request), "UpdateSupervisor", ErrorKind.Validation);
        Assert.DoesNotContain("UpdateSupervisor", c.Supervisors.Calls);
    }


    [Fact]
    public void Constructor_DependsOnlyOnOwnRepository()
    {
        var constructor = Assert.Single(typeof(API.Services.SupervisorService).GetConstructors());
        var dependency = Assert.Single(constructor.GetParameters());
        Assert.Equal(typeof(API.Repositories.Interfaces.ISupervisorRepository), dependency.ParameterType);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task ValidateSupervisorExists_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var original = ServiceFailureCases.Create(kind);
        c.Supervisors.Failure = original;
        await ServiceFailureCases.AssertWrapped(() => c.SupervisorService.ValidateSupervisorExists(c.Supervisor.UserId, cancellation.Token), "ValidateSupervisorExists", original);
        Assert.Equal(cancellation.Token, c.Supervisors.LastToken);
    }

    [Fact]
    public async Task ValidateSupervisorExists_AcceptsExistingReferences()
    {
        var c = new ServiceTestContext();
        await c.SupervisorService.ValidateSupervisorExists(c.Supervisor.UserId);
        Assert.Equal("GetSupervisorById", Assert.Single(c.Supervisors.Calls));
    }

    [Fact]
    public async Task ValidateSupervisorExists_RejectsMissingReferences()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.SupervisorService.ValidateSupervisorExists(Guid.NewGuid()), "ValidateSupervisorExists", ErrorKind.Validation);
    }
}
