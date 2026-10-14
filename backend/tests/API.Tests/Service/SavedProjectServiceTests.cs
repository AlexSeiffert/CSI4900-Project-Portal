using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Service;

public class SavedProjectServiceTests
{
    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetSavedProjects_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.SavedProjects.Failure = original;
        c.SavedProjects.FailureMethod = "GetSavedProjects";
        await ServiceFailureCases.AssertWrapped(() => c.SavedProjectService.GetSavedProjects(c.Student.UserId, token), "GetSavedProjects", original);
        Assert.Equal(token, c.SavedProjects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task SaveProject_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.SavedProjects.Failure = original;
        c.SavedProjects.FailureMethod = "SaveProject";
        await ServiceFailureCases.AssertWrapped(() => c.SavedProjectService.SaveProject(c.Student.UserId, c.Project.Id, token), "SaveProject", original);
        Assert.Equal(token, c.SavedProjects.LastToken);
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task RemoveSavedProject_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.SavedProjects.Failure = original;
        c.SavedProjects.FailureMethod = "RemoveSavedProject";
        await ServiceFailureCases.AssertWrapped(() => c.SavedProjectService.RemoveSavedProject(c.Student.UserId, c.Project.Id, token), "RemoveSavedProject", original);
        Assert.Equal(token, c.SavedProjects.LastToken);
    }

    [Fact]
    public async Task GetSavedProjects_ReturnsMappedProjects()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.SavedProjectService.GetSavedProjects(c.Student.UserId));
        Assert.Equal(c.Project.Id, result.Id);
        Assert.Equal(c.Topic.Id, Assert.Single(result.Topics).Id);
        Assert.Equal("GetSavedProjects", Assert.Single(c.SavedProjects.Calls));
    }

    [Fact]
    public async Task SaveProject_CallsOwnRepository()
    {
        var c = new ServiceTestContext();
        await c.SavedProjectService.SaveProject(c.Student.UserId, c.Project.Id);
        Assert.Equal("SaveProject", Assert.Single(c.SavedProjects.Calls));
    }

    [Fact]
    public async Task RemoveSavedProject_RemovesRelationship()
    {
        var c = new ServiceTestContext();
        await c.SavedProjectService.RemoveSavedProject(c.Student.UserId, c.Project.Id);
        Assert.Equal("RemoveSavedProject", Assert.Single(c.SavedProjects.Calls));
        Assert.Empty(c.Projects.Calls);
    }


    [Fact]
    public void Constructor_DependsOnlyOnOwnRepository()
    {
        var constructor = Assert.Single(typeof(API.Services.SavedProjectService).GetConstructors());
        var dependency = Assert.Single(constructor.GetParameters());
        Assert.Equal(typeof(API.Repositories.Interfaces.ISavedProjectRepository), dependency.ParameterType);
    }
}
